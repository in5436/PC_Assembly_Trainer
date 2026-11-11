using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class RAMSlotIndicator : MonoBehaviour
{
    [SerializeField] private Renderer slotMarker;
    [SerializeField] private Color emptyColor = Color.blue;
    [SerializeField] private Color installedColor = Color.green;

    private XRSocketInteractor socket;
    private Material markerMaterial;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();

        if (slotMarker != null)
            markerMaterial = slotMarker.material;
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnInstalled);
        socket.selectExited.AddListener(OnRemoved);
    }

    private void Start()
    {
        UpdateIndicator(socket.hasSelection);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnInstalled);
        socket.selectExited.RemoveListener(OnRemoved);
    }

    private void OnInstalled(SelectEnterEventArgs args)
    {
        UpdateIndicator(true);
        Debug.Log("RAM: модуль встановлено у слот.");
    }

    private void OnRemoved(SelectExitEventArgs args)
    {
        UpdateIndicator(false);
        Debug.Log("RAM: модуль вилучено зі слота.");
    }

    private void UpdateIndicator(bool installed)
    {
        if (markerMaterial != null)
            markerMaterial.color = installed
                ? installedColor
                : emptyColor;
    }
}
