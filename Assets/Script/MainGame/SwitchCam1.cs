using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class SwitchCam1 : MonoBehaviour
{

    [Header("Pengaturan Cinemachine")]
    [Tooltip("Kamera asal yang sedang aktif")]
    public CinemachineVirtualCamera vcamAwal; // Ganti tipe menjadi CinemachineVirtualCamera jika memakai Cinemachine 2.x
    [Tooltip("Kamera tujuan")]
    public CinemachineVirtualCamera vcamTujuan; // Ganti tipe menjadi CinemachineVirtualCamera jika memakai Cinemachine 2.x

    [Header("Pengaturan Input System Baru")]
    public InputAction clickAction;
    public InputAction pointerPositionAction;

    private Camera mainCamera;
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
        Debug.Log("1. Input klik terdeteksi!"); // Mengecek apakah klik mouse terbaca

        Vector2 screenPosition = pointerPositionAction.ReadValue<Vector2>();
        Debug.Log("2. Posisi layar: " + screenPosition); // Mengecek apakah posisi kursor terbaca

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("3. Raycast mengenai objek: " + hit.collider.gameObject.name); // Mengecek apa yang tertabrak

            if (hit.collider.gameObject == this.gameObject)
            {
                Debug.Log("4. Target cocok! Memindahkan kamera sekarang.");
                SwitchCameraPriority();
            }
        }
        else
        {
            Debug.Log("3. Raycast meleset, tidak mengenai Collider apapun.");
        }
    }

    private void SwitchCameraPriority()
    {
        if (vcamAwal != null && vcamTujuan != null)
        {
            // Turunkan prioritas kamera awal
            vcamAwal.Priority = 0;

            // Naikkan prioritas kamera tujuan agar mengambil alih Main Camera
            vcamTujuan.Priority = 10;

            Debug.Log("Prioritas diubah! Cinemachine sedang memindahkan kamera.");
        }
        else
        {
            Debug.LogWarning("Virtual Cameras belum dimasukkan ke Inspector!");
        }
        
    }
}
