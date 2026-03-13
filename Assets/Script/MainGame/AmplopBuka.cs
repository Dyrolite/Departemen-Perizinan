using UnityEngine;
using UnityEngine.InputSystem;

public class AmplopBuka : MonoBehaviour
{
    [Header("Referensi")]
    public ManagerInspect managerInspect;
    
    [Header("Pengaturan Input")]
    public InputAction clickAction;
    public InputAction pointerPositionAction;

    private Camera mainCamera;
    private bool sudahDiklik = false; // Mencegah player spam klik amplop

    private void Awake()
    {
        mainCamera = Camera.main;
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

    private void OnClick(InputAction.CallbackContext context)
    {
        if (sudahDiklik) return; // Kalau sudah diklik, hentikan proses

        Vector2 screenPosition = pointerPositionAction.ReadValue<Vector2>();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            Debug.Log("Amplop kedua ditekan! Mengeluarkan berkas...");
            sudahDiklik = true; // Kunci agar tidak keluar berkas dobel

            // Panggil animasi berkas keluar dari Manager
            managerInspect.PanggilBerkasDariAmplop();

            // Opsional: Matikan collider atau ganti sprite amplop menjadi "amplop kosong" di sini
            // GetComponent<Collider2D>().enabled = false; 
        }
    }

    // Reset status klik jika klien baru datang (bisa dipanggil dari ManagerInspect saat siklus reset)
    public void ResetAmplop()
    {
        sudahDiklik = false;
    }
}