using UnityEngine;
using UnityEngine.InputSystem;

public class BukaUlang : MonoBehaviour
{
    [Header("ref")]
    public ManagerInspect manager;

    [Header("Pengaturan Input System")]
    public InputAction clickAction;
    public InputAction pointerPositionAction;

    private Collider2D myCollider;
    private Camera mainCamera;
    private Animator anim;
    private SpriteRenderer mySpriteRenderer;

    void Start()
    {
        anim = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        if (mySpriteRenderer != null && mySpriteRenderer.sortingOrder >= 10)
        {
            mySpriteRenderer.sortingOrder = 5;
        }
    }

    private void OnEnable()
    {
        clickAction.Enable();
        pointerPositionAction.Enable();
        clickAction.started += OnTargetClicked;
    }

    private void OnDisable()
    {
        clickAction.started -= OnTargetClicked;
        clickAction.Disable();
        pointerPositionAction.Disable();
    }

    private void OnTargetClicked(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = pointerPositionAction.ReadValue<Vector2>();
        Vector2 pointerPos = mainCamera.ScreenToWorldPoint(screenPosition);

        // Jika kursor sama sekali tidak menyentuh area amplop, abaikan.
        if (!myCollider.OverlapPoint(pointerPos)) return;

        // --- SOLUSI Z-INDEX: MENCARI OBJEK TERATAS ---
        Collider2D[] hits = Physics2D.OverlapPointAll(pointerPos);
        int maxSortingOrder = int.MinValue;
        Collider2D topmostCollider = null;

        // Cari siapa yang punya angka Sorting Order paling tinggi di titik mouse ini
        foreach (Collider2D hit in hits)
        {
            SpriteRenderer sr = hit.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sortingOrder > maxSortingOrder)
            {
                maxSortingOrder = sr.sortingOrder;
                topmostCollider = hit;
            }
        }

        // --- PENGECEKAN KASTA ---
        // Jika objek yang paling atas BUKAN Amplop ini (berarti terhalang Dokumen)
        if (topmostCollider != null && topmostCollider != myCollider)
        {
            // BATALKAN FUNGSI AMPLOP! Biarkan dokumen saja yang merespons klik/drag.
            return;
        }

        // --- LOGIKA AMPLOP ASLI (Jika tidak terhalang apa-apa) ---
        DragFile[] semuaBerkas = FindObjectsByType<DragFile>(FindObjectsSortMode.None);
        bool adaYangDikeluarkan = false;

        foreach (DragFile berkas in semuaBerkas)
        {
            // Jika statusnya sedang disimpan di dalam amplop
            if (berkas.isStored)
            {
                berkas.KeluarkanKeMeja();
                adaYangDikeluarkan = true;
            }
        }
        if (adaYangDikeluarkan)
        {
            if (manager != null && !manager.AmplopKebuka)
            {
                anim.SetTrigger("TriggOpen");
                manager.AmplopKebuka = true;
            }
        }
    }
}