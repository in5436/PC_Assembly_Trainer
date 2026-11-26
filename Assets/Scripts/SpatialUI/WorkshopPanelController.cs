using UnityEngine;
using UnityEngine.InputSystem;

public class WorkshopPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform head;
    [SerializeField] private float distance = 0.65f;
    [SerializeField] private float verticalOffset = -0.1f;

    private InputAction menuAction;

    private void Awake()
    {
        menuAction = new InputAction(
            "WorkshopMenu",
            InputActionType.Button,
            "<XRController>{LeftHand}/secondaryButton"
        );

        panel.SetActive(false);
    }

    private void OnEnable()
    {
        menuAction.performed += OnMenuPressed;
        menuAction.Enable();
    }

    private void OnDisable()
    {
        menuAction.performed -= OnMenuPressed;
        menuAction.Disable();
    }

    private void OnDestroy()
    {
        menuAction.Dispose();
    }

    private void OnMenuPressed(InputAction.CallbackContext context)
    {
        if (panel.activeSelf)
            ClosePanel();
        else
            OpenPanel();
    }

    public void OpenPanel()
    {
        Vector3 direction = Vector3.ProjectOnPlane(
            head.forward, Vector3.up
        ).normalized;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.forward;

        transform.position = head.position
            + direction * distance
            + Vector3.up * verticalOffset;

        transform.rotation = Quaternion.LookRotation(
            direction, Vector3.up
        );

        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }
}
