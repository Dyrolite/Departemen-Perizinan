using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Petunjuk : MonoBehaviour
{
    [Header("ref")]
    public ManagerInspect manager;
    public Animator fotoMuncul;

    [Header("Pengaturan Input System Baru")]
    public InputAction clickAction;
    public InputAction pointerPositionAction;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        //if (fotoMuncul != null) fotoMuncul.SetActive(false);
    }

    private void OnEnable()
    {
        clickAction.Enable();
        pointerPositionAction.Enable();
        clickAction.performed += OnClick;
    }

    private void OnDisable()
    {
        clickAction.Disable();
        pointerPositionAction.Disable();
        clickAction.performed -= OnClick;
    }

    // --- BAGIAN YANG DIPINDAH: Mengecek crosshair setiap saat (Hover) ---
    //private void Update()
    //{
    //    Vector2 screenPosition = pointerPositionAction.ReadValue<Vector2>();
    //    Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
    //    RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

    //    bool kenaMouse = (hit.collider != null && hit.collider.gameObject == this.gameObject);

    //    // Nyalakan foto kalau kena crosshair, matikan kalau crosshair pergi
    //    if (fotoMuncul != null && fotoMuncul.activeSelf != kenaMouse)
    //    {
    //        fotoMuncul.SetActive(kenaMouse);
    //    }
    //}

    // --- FUNGSI KLIK: Hanya untuk pindah kamera ---
    private void OnClick(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = pointerPositionAction.ReadValue<Vector2>();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        // Kalau diklik pas crosshair ada di kotak ini, pindah kamera
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            fotoMuncul.SetTrigger("SlideIn");
            //manager.IsKamreaAtas = true;
        }
    }
    public void SlideOut()
    {
        fotoMuncul.SetTrigger("SlideOut");
    }
}
