using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragFile : MonoBehaviour
{
    //[Header("ref")]
    //public ManagerInspect manager;

    [Header("Pengaturan Input System Baru")]
    public InputAction clickAction;
    public InputAction pointerPositionAction;

    [Header("Pengaturan Efek")]
    public float shrinkSpeed = 5f;
    public float absorbSpeed = 10f;

    private Vector3 offset;
    private Vector3 originalScale;
    private Vector3 homePosition;

    private bool isDragging = false;
    private bool isOverTarget = false;
    private bool isAbsorbing = false;
    public bool isStored;

    private Transform targetTransform;
    private Collider2D myCollider;
    private Camera mainCamera;

    private SpriteRenderer mySpriteRenderer;
    private Canvas myCanvas;
    private static int globalSortingOrder = 10;

    void Start()
    {
        myCanvas = GetComponentInChildren<Canvas>();
        originalScale = transform.localScale;
        myCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        if (mySpriteRenderer != null)
        {
            mySpriteRenderer.sortingOrder = ++globalSortingOrder;
        }
        UpdateSortingTumpukan();
    }
    private void UpdateSortingTumpukan()
    {
        globalSortingOrder += 2; // Kita naikkan 2 angka sekaligus

        // 1. Majukan kertasnya
        if (mySpriteRenderer != null)
        {
            mySpriteRenderer.sortingOrder = globalSortingOrder;
        }

        // 2. Majukan Canvas/Teksnya agar selalu ADA DI ATAS kertas (+1)
        if (myCanvas != null)
        {
            myCanvas.sortingOrder = globalSortingOrder + 1;
        }
    }
    private void OnEnable()
    {
        clickAction.Enable();
        pointerPositionAction.Enable();
        clickAction.started += OnClickStarted;
        clickAction.canceled += OnClickCanceled;
    }

    private void OnDisable()
    {
        clickAction.started -= OnClickStarted;
        clickAction.canceled -= OnClickCanceled;
        clickAction.Disable();
        pointerPositionAction.Disable();
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        if (isAbsorbing || isStored) return;

        Vector2 pointerPos = GetPointerWorldPos();

        // --- LOGIKA MENCARI OBJEK PALING ATAS ---
        // 1. Dapatkan semua collider yang tertusuk kursor
        Collider2D[] hits = Physics2D.OverlapPointAll(pointerPos);

        Collider2D topmostCollider = null;
        int maxSortingOrder = int.MinValue;

        // 2. Cari mana yang punya Sorting Order paling tinggi
        foreach (Collider2D hit in hits)
        {
            // Cek apakah yang tertusuk adalah objek berkas (punya script ini)
            DragFile draggable = hit.GetComponent<DragFile>();
            if (draggable != null)
            {
                SpriteRenderer sr = hit.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sortingOrder > maxSortingOrder)
                {
                    maxSortingOrder = sr.sortingOrder;
                    topmostCollider = hit;
                }
            }
        }

        // 3. Jika "Aku" adalah objek yang paling atas, maka izinkan drag!
        if (topmostCollider == myCollider)
        {
            isDragging = true;
            offset = transform.position - (Vector3)pointerPos;

            // Bawa objek ini ke tumpukan paling depan!
            if (mySpriteRenderer != null)
            {
                UpdateSortingTumpukan();
            }
        }
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        if (!isDragging || isAbsorbing) return;

        isDragging = false;

        // Proses sedot berjalan jika saat dilepas kursor berada di atas target
        if (isOverTarget && targetTransform != null)
        {
            StartCoroutine(AbsorbRoutine());
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    void Update()
    {
        if (isAbsorbing) return;
        if (!isDragging && !isStored)
        {
            homePosition = transform.position;
        }
        if (isDragging)
        {
            Vector2 pointerPos = GetPointerWorldPos();
            transform.position = (Vector3)pointerPos + offset;

            // --- SOLUSI JITTER: Cek Target berdasarkan titik Pointer secara terus menerus ---
            CheckTargetUnderPointer(pointerPos);

            if (isOverTarget && targetTransform != null)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale * 0.5f, Time.deltaTime * shrinkSpeed);
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * shrinkSpeed);
            }
        }
    }

    // --- FUNGSI BARU PENGGANTI ONTRIGGER ---
    private void CheckTargetUnderPointer(Vector2 pointerPos)
    {
        isOverTarget = false;
        targetTransform = null;

        // Mengambil semua collider yang berada tepat di titik mouse/pointer
        Collider2D[] hits = Physics2D.OverlapPointAll(pointerPos);
        foreach (Collider2D hit in hits)
        {
            // Jika salah satu collider di bawah mouse memiliki Tag "Target"
            if (hit.CompareTag("Target"))
            {
                isOverTarget = true;
                targetTransform = hit.transform;
                break; // Target ditemukan, hentikan pencarian
            }
        }
    }

    private Vector2 GetPointerWorldPos()
    {
        Vector2 screenPosition = pointerPositionAction.ReadValue<Vector2>();
        return mainCamera.ScreenToWorldPoint(screenPosition);
    }

    private IEnumerator AbsorbRoutine()
    {
        isAbsorbing = true;
        myCollider.enabled = false;

        while (transform.localScale.x > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * absorbSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * absorbSpeed);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        isStored = true;
        isAbsorbing = false;
        // --- JEBAKAN DETEKTIF 1 ---
        Debug.Log($"Berkas {gameObject.name} selesai disedot. Mencari Manager...");

        // Menggunakan FindFirstObjectByType (standar Unity terbaru pengganti FindObjectOfType)
        ManagerInspect manager = Object.FindFirstObjectByType<ManagerInspect>();

        if (manager != null)
        {
            Debug.Log("Manager ketemu! Menyuruh Manager mengecek sisa berkas...");
            manager.CekAnimasiTutupAmplop();
        }
        else
        {
            Debug.LogError("Waduh, ManagerInspect tidak ditemukan di Scene!");
        }
    }
    public void KeluarkanKeMeja()
    {
        if (!isStored) return;
        if (mySpriteRenderer != null)
        {
            UpdateSortingTumpukan();
        }
        StartCoroutine(KeluarkanRoutine());
    }
    private IEnumerator KeluarkanRoutine()
    {
        isStored = false;

        while (transform.localScale.x < originalScale.x - 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, homePosition, Time.deltaTime * absorbSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * absorbSpeed);
            yield return null;
        }

        transform.position = homePosition;
        transform.localScale = originalScale;
        myCollider.enabled = true; // Nyalakan collider agar bisa di-drag lagi
    }

}