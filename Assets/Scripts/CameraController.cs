
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float mouseSensitivity = 2f;

    private float verticalRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = Vector3.ProjectOnPlane(
            transform.forward, Vector3.up
        ).normalized;

        Vector3 right = Vector3.ProjectOnPlane(
            transform.right, Vector3.up
        ).normalized;

        Vector3 direction = (right * horizontal +
                             forward * vertical).normalized;

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X")
                           * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y")
                           * mouseSensitivity;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(
                verticalRotation, -80f, 80f
            );

            transform.eulerAngles = new Vector3(
                verticalRotation,
                transform.eulerAngles.y + mouseX,
                0f
            );
        }
    }
}
