using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ComponentTooltip : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private GameObject card;
    [SerializeField] private TMP_Text componentName;
    [SerializeField] private TMP_Text componentDescription;
    [SerializeField] private Transform head;
    [SerializeField] private GameObject workshopMenu;

    [Header("Scene objects")]
    [SerializeField] private XRBaseInteractable ram;
    [SerializeField] private XRBaseInteractable pcCase;
    [SerializeField] private XRBaseInteractable motherboard;

    private void Awake()
    {
        card.SetActive(false);
    }

    private void LateUpdate()
    {
        if (workshopMenu.activeInHierarchy)
        {
            card.SetActive(false);
            return;
        }

        if (IsHoveredByUser(ram))
        {
            ShowCard(
                "Оперативна пам'ять",
                "Тимчасово зберігає дані, необхідні для роботи програм. " +
                "Під час встановлення сумістіть контакти модуля зі слотом."
            );
        }
        else if (IsHoveredByUser(motherboard))
        {
            ShowCard(
                "Материнська плата",
                "Встановіть модуль оперативної пам'яті у позначений слот.\n" +
                "Синій — слот вільний.\n" +
                "Зелений — модуль встановлено."
            );
        }
        else if (IsHoveredByUser(pcCase))
        {
            ShowCard(
                "Корпус ПК",
                "Призначений для розміщення та захисту комплектуючих ПК. " +
                "Відкрита бічна сторона забезпечує доступ до компонентів."
            );
        }
        else
        {
            card.SetActive(false);
        }
    }

    private bool IsHoveredByUser(XRBaseInteractable target)
    {
        foreach (var interactor in target.interactorsHovering)
        {
            if (!(interactor is XRSocketInteractor))
                return true;
        }

        return false;
    }

    private void ShowCard(string title, string description)
    {
        componentName.text = title;
        componentDescription.text = description;

        transform.position = head.position
            + head.forward * 0.9f
            + head.right * 0.18f
            + head.up * 0.32f;

        transform.rotation = head.rotation;

        card.SetActive(true);
    }
}
