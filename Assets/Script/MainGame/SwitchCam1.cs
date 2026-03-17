using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class SwitchCam1 : MonoBehaviour
{
    [Header("ref")]
    public ManagerInspect manager;

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
        Vector2 screenPosition = pointerPositionAction.ReadValue<Vector2>();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            SwitchCameraPriority();
            manager.IsKamreaAtas = true;
        }
        
    }

    private void SwitchCameraPriority()
    {
        vcamAwal.Priority = 0;
        vcamTujuan.Priority = 10;        
    }
}
