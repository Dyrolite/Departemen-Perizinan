using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class SwitchCam : MonoBehaviour
{
    [Header("Ref")]
    public ManagerInspect managerInspect;
    public Animator anim;

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
        }
    }

    private void SwitchCameraPriority()
    {
        // Turunkan prioritas kamera awal
        vcamAwal.Priority = 0;

        // Naikkan prioritas kamera tujuan agar mengambil alih Main Camera
        vcamTujuan.Priority = 10;

        if (managerInspect.AmplopKebuka == false)
        {
            StartCoroutine(WaitSwitch());
            managerInspect.AmplopKebuka = true;
        }
    }
    IEnumerator WaitSwitch()
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetTrigger("TriggOpen");
    }
}
