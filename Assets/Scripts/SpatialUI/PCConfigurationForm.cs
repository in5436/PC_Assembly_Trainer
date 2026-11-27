using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PCConfigurationForm : MonoBehaviour
{
    [Header("Form elements")]
    [SerializeField] private TMP_InputField pcNameInput;
    [SerializeField] private Slider ramSlider;
    [SerializeField] private TMP_Text ramValueLabel;
    [SerializeField] private Toggle ssdToggle;
    [SerializeField] private Toggle gpuToggle;
    [SerializeField] private TMP_Text configurationStatus;

    private readonly int[] ramOptions = { 8, 16, 32, 64 };

    private string SavePath => Path.Combine(
        Application.persistentDataPath,
        "pc_configuration.json"
    );

    [Serializable]
    private class PCConfiguration
    {
        public string pcName;
        public int ramGB;
        public bool hasSSD;
        public bool hasGPU;
    }

    private void Awake()
    {
        ramSlider.onValueChanged.AddListener(UpdateRamLabel);

        ResetForm();
        LoadConfiguration();
    }

    private void OnDestroy()
    {
        ramSlider.onValueChanged.RemoveListener(UpdateRamLabel);
    }

    private int GetRamIndex()
    {
        return Mathf.Clamp(
            Mathf.RoundToInt(ramSlider.value) - 1,
            0,
            ramOptions.Length - 1
        );
    }

    private void UpdateRamLabel(float value)
    {
        ramValueLabel.text =
            $"Оперативна пам'ять: {ramOptions[GetRamIndex()]} ГБ";
    }

    public void SaveConfiguration()
    {
        string pcName = pcNameInput.text.Trim();

        if (string.IsNullOrEmpty(pcName))
        {
            configurationStatus.text =
                "Введіть назву конфігурації!";
            return;
        }

        PCConfiguration configuration = new PCConfiguration
        {
            pcName = pcName,
            ramGB = ramOptions[GetRamIndex()],
            hasSSD = ssdToggle.isOn,
            hasGPU = gpuToggle.isOn
        };

        try
        {
            string json = JsonUtility.ToJson(configuration, true);
            File.WriteAllText(SavePath, json);

            configurationStatus.text =
                "Конфігурацію збережено.";

            Debug.Log("Конфігурацію збережено: " + SavePath);
            Debug.Log(json);
        }
        catch (Exception exception)
        {
            configurationStatus.text =
                "Помилка збереження конфігурації.";

            Debug.LogError(exception.Message);
        }
    }

    public void ResetForm()
    {
        pcNameInput.text = "";
        ramSlider.value = 2;
        ssdToggle.isOn = false;
        gpuToggle.isOn = false;

        UpdateRamLabel(ramSlider.value);

        configurationStatus.text =
            "Значення форми скинуто.";
    }

    private void LoadConfiguration()
    {
        if (!File.Exists(SavePath))
        {
            configurationStatus.text =
                "Налаштуйте параметри збірки.";
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);

            PCConfiguration configuration =
                JsonUtility.FromJson<PCConfiguration>(json);

            if (configuration == null)
                throw new InvalidDataException();

            pcNameInput.text = configuration.pcName;

            int ramIndex = Array.IndexOf(
                ramOptions, configuration.ramGB
            );

            ramSlider.value =
                ramIndex >= 0 ? ramIndex + 1 : 2;

            ssdToggle.isOn = configuration.hasSSD;
            gpuToggle.isOn = configuration.hasGPU;

            UpdateRamLabel(ramSlider.value);

            configurationStatus.text =
                "Конфігурацію відновлено.";
        }
        catch (Exception exception)
        {
            ResetForm();

            configurationStatus.text =
                "Помилка завантаження конфігурації.";

            Debug.LogWarning(exception.Message);
        }
    }
}
