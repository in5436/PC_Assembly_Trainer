using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AssemblyTrainingController : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject configurationPage;
    [SerializeField] private GameObject trainingPage;
    [SerializeField] private GameObject resultsPage;

    [Header("Training interface")]
    [SerializeField] private TMP_Text trainingTitle;
    [SerializeField] private TMP_Text trainingInstruction;
    [SerializeField] private TMP_Text trainingFeedback;

    [SerializeField] private Button answerButton1;
    [SerializeField] private Button answerButton2;
    [SerializeField] private Button answerButton3;

    [Header("Results interface")]
    [SerializeField] private TMP_Text resultsText;

    [Header("Scene references")]
    [SerializeField] private WorkshopPanelController panelController;
    [SerializeField] private XRSocketInteractor ramSocket;
    [SerializeField] private XRGrabInteractable ramModule;

    private enum TrainingState
    {
        Introduction,
        PurposeQuestion,
        SafetyQuestion,
        Installation,
        Completed
    }

    private TrainingState state;

    private int correctAnswers;
    private int errors;
    private float startTime;
    private bool installedDuringTraining;

    private TrainingResult lastResult;

    private string SavePath => Path.Combine(
        Application.persistentDataPath,
        "training_result.json"
    );

    [Serializable]
    private class TrainingResult
    {
        public string sessionId;
        public string completedAtUtc;
        public int correctAnswers;
        public int totalQuestions;
        public int errors;
        public int durationSeconds;
        public bool ramInstalled;
        public bool passed;
    }

    private void Awake()
    {
        LoadResult();
    }

    private void OnEnable()
    {
        ramSocket.selectEntered.AddListener(OnRamInstalled);
    }

    private void OnDisable()
    {
        ramSocket.selectEntered.RemoveListener(OnRamInstalled);
    }

    private void ShowPage(GameObject page)
    {
        mainMenu.SetActive(page == mainMenu);
        configurationPage.SetActive(page == configurationPage);
        trainingPage.SetActive(page == trainingPage);
        resultsPage.SetActive(page == resultsPage);
    }

    public void OpenTraining()
    {
        state = TrainingState.Introduction;

        correctAnswers = 0;
        errors = 0;
        installedDuringTraining = false;

        ShowPage(trainingPage);
        RenderStep();
    }

    public void OpenResults()
    {
        ShowPage(resultsPage);
        RenderResult();
    }

    public void ReturnToMenu()
    {
        state = TrainingState.Introduction;
        ShowPage(mainMenu);
    }

    private void ConfigureButton(
        Button button,
        string label,
        bool visible
    )
    {
        button.gameObject.SetActive(visible);

        if (visible)
        {
            button.GetComponentInChildren<TMP_Text>(true).text =
                label;
        }
    }

    private void RenderStep(string feedback = "")
    {
        trainingFeedback.text = feedback;

        switch (state)
        {
            case TrainingState.Introduction:

                trainingTitle.text = "Навчання зі складання";

                trainingInstruction.text =
                    "Пройдіть два теоретичні питання " +
                    "та практичне завдання зі встановлення RAM.";

                ConfigureButton(
                    answerButton1, "Розпочати навчання", true
                );

                ConfigureButton(answerButton2, "", false);
                ConfigureButton(answerButton3, "", false);
                break;

            case TrainingState.PurposeQuestion:

                trainingTitle.text = "Крок 1 із 3";

                trainingInstruction.text =
                    "Для чого призначена оперативна пам'ять?";

                ConfigureButton(
                    answerButton1,
                    "Постійне зберігання файлів",
                    true
                );

                ConfigureButton(
                    answerButton2,
                    "Тимчасове зберігання даних програм",
                    true
                );

                ConfigureButton(
                    answerButton3,
                    "Живлення комплектуючих",
                    true
                );

                break;

            case TrainingState.SafetyQuestion:

                trainingTitle.text = "Крок 2 із 3";

                trainingInstruction.text =
                    "Що потрібно зробити перед встановленням " +
                    "RAM у реальний ПК?";

                ConfigureButton(
                    answerButton1,
                    "Вимкнути ПК та від'єднати живлення",
                    true
                );

                ConfigureButton(
                    answerButton2,
                    "Увімкнути комп'ютер",
                    true
                );

                ConfigureButton(
                    answerButton3,
                    "Запустити операційну систему",
                    true
                );

                break;

            case TrainingState.Installation:

                trainingTitle.text = "Крок 3 із 3";

                trainingInstruction.text =
                    "Встановіть RAM у слот материнської плати. " +
                    "Якщо модуль уже в слоті, вийміть його. ";

                ConfigureButton(
                    answerButton1,
                    "Перевірити встановлення",
                    true
                );

                ConfigureButton(
                    answerButton2,
                    "Приховати панель",
                    true
                );

                ConfigureButton(answerButton3, "", false);
                break;
        }
    }

    public void SelectOption(int option)
    {
        switch (state)
        {
            case TrainingState.Introduction:

                if (option != 1)
                    return;

                startTime = Time.realtimeSinceStartup;

                state = TrainingState.PurposeQuestion;
                RenderStep();

                break;

            case TrainingState.PurposeQuestion:

                bool purposeCorrect = option == 2;
                RegisterAnswer(purposeCorrect);

                state = TrainingState.SafetyQuestion;

                RenderStep(
                    purposeCorrect
                        ? "Попередня відповідь правильна."
                        : "Попередня відповідь неправильна."
                );

                break;

            case TrainingState.SafetyQuestion:

                bool safetyCorrect = option == 1;
                RegisterAnswer(safetyCorrect);

                installedDuringTraining = false;
                state = TrainingState.Installation;

                RenderStep(
                    safetyCorrect
                        ? "Попередня відповідь правильна."
                        : "Попередня відповідь неправильна."
                );

                break;

            case TrainingState.Installation:

                if (option == 1)
                    CheckInstallation();

                else if (option == 2)
                    panelController.ClosePanel();

                break;
        }
    }

    private void RegisterAnswer(bool correct)
    {
        if (correct)
            correctAnswers++;
        else
            errors++;
    }

    private void OnRamInstalled(SelectEnterEventArgs args)
    {
        if (state == TrainingState.Installation &&
            ReferenceEquals(args.interactableObject, ramModule))
        {
            installedDuringTraining = true;
        }
    }

    private void CheckInstallation()
    {
        if (!installedDuringTraining ||
            !ramSocket.IsSelecting(ramModule))
        {
            errors++;

            trainingFeedback.text =
                "Спочатку встановіть RAM у слот!";

            return;
        }

        CompleteTraining();
    }

    private void CompleteTraining()
    {
        state = TrainingState.Completed;

        lastResult = new TrainingResult
        {
            sessionId = Guid.NewGuid().ToString(),
            completedAtUtc = DateTime.UtcNow.ToString("o"),
            correctAnswers = correctAnswers,
            totalQuestions = 2,
            errors = errors,
            durationSeconds = Mathf.RoundToInt(
                Time.realtimeSinceStartup - startTime
            ),
            ramInstalled = true,
            passed = correctAnswers == 2
        };

        bool saved = SaveResult();

        OpenResults();

        resultsText.text += saved
            ? "\nРезультат збережено."
            : "\nПомилка збереження результату.";
    }

    private bool SaveResult()
    {
        try
        {
            string json = JsonUtility.ToJson(lastResult, true);
            File.WriteAllText(SavePath, json);

            Debug.Log(
                "Результат навчання збережено: " + SavePath
            );

            Debug.Log(json);

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.Message);
            return false;
        }
    }

    private void LoadResult()
    {
        if (!File.Exists(SavePath))
            return;

        try
        {
            string json = File.ReadAllText(SavePath);

            lastResult =
                JsonUtility.FromJson<TrainingResult>(json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(exception.Message);
        }
    }

    private void RenderResult()
    {
        if (lastResult == null)
        {
            resultsText.text = "Навчання ще не пройдено.";
            return;
        }

        string outcome = lastResult.passed
            ? "Навчання зараховано."
            : "Навчання не зараховано.";

        resultsText.text =
            outcome + "\n\n" +
            $"Правильні відповіді: {lastResult.correctAnswers}" +
            $" / {lastResult.totalQuestions}\n" +
            $"Кількість помилок: {lastResult.errors}\n" +
            $"Час проходження: {lastResult.durationSeconds} с\n" +
            "Встановлення RAM: " +
            (lastResult.ramInstalled ? "успішно" : "не виконано");
    }
}
