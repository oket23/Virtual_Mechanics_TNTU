using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Керує попапом лабораторної роботи.
// Стани: MainMenu → Info / Experiment → Results
public class LabPopupController : MonoBehaviour
{
    [Header("Посилання")]
    public UIManager uiManager;

    [Header("Головне меню")]
    public GameObject mainPanel;
    public TMP_Text   nameText;
    public Button     infoBtn;
    public Button     startBtn;
    public Button     closeBtn;

    [Header("Інформація")]
    public GameObject infoPanel;
    public TMP_Text   descText;
    public Button     infoBackBtn;

    [Header("Вимірювання")]
    public GameObject    experimentPanel;
    public TMP_Text      progressText;
    public TMP_Text      measuredValueText;
    public TMP_InputField inputField;
    public Button        measureBtn;
    public Button        saveBtn;
    public Button        checkBtn;
    public Button        experimentBackBtn;

    [Header("Результати")]
    public GameObject resultsPanel;
    public TMP_Text   tableText;
    public TMP_Text   summaryText;
    public Button     resultsBackBtn;

    LabData           currentData;
    LabExperimentBase experiment;
    float             lastGenerated;
    bool              hasMeasured;

    void Awake()
    {
        if (infoBtn)  infoBtn.onClick.AddListener(() => SetPanel(infoPanel));
        if (startBtn) startBtn.onClick.AddListener(StartExperiment);
        if (closeBtn) closeBtn.onClick.AddListener(Close);

        if (infoBackBtn) infoBackBtn.onClick.AddListener(() => SetPanel(mainPanel));

        if (measureBtn)        measureBtn.onClick.AddListener(DoMeasure);
        if (saveBtn)           saveBtn.onClick.AddListener(SaveMeasurement);
        if (checkBtn)          checkBtn.onClick.AddListener(ShowResults);
        if (experimentBackBtn) experimentBackBtn.onClick.AddListener(() => { ResetExperiment(); SetPanel(mainPanel); });

        if (inputField) inputField.onValueChanged.AddListener(_ => RefreshSaveBtn());

        if (resultsBackBtn) resultsBackBtn.onClick.AddListener(() => { ResetExperiment(); SetPanel(mainPanel); });
    }

    // ── Публічне API ─────────────────────────────────────────────────────────

    public void Open(LabData data)
    {
        currentData = data;
        ResetExperiment();
        nameText.text = currentData.instrumentName;
        SetPanel(mainPanel);
    }

    // ── Стани ────────────────────────────────────────────────────────────────

    void StartExperiment()
    {
        SetPanel(experimentPanel);
        RefreshExperimentUI();
    }

    void RefreshExperimentUI()
    {
        progressText.text = $"Вимір {experiment.CurrentCount + 1} з {experiment.TotalCount}";
        measuredValueText.text = "—";
        inputField.text = "";
        inputField.gameObject.SetActive(true);
        hasMeasured = false;
        saveBtn.interactable = false;
        saveBtn.gameObject.SetActive(true);
        measureBtn.gameObject.SetActive(true);
        checkBtn.gameObject.SetActive(false);
    }

    void DoMeasure()
    {
        lastGenerated = experiment.GenerateMeasurement();
        measuredValueText.text = $"{lastGenerated:F3} {currentData.unit}";
        hasMeasured = true;
        inputField.text = "";
        inputField.Select();
        RefreshSaveBtn();
    }

    void SaveMeasurement()
    {
        string raw = inputField.text.Replace(',', '.');
        if (!float.TryParse(raw,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float val))
            return;

        experiment.AddMeasurementPair(lastGenerated, val);

        if (experiment.IsComplete)
        {
            progressText.text = $"Усі {experiment.TotalCount} замірів виконані";
            measuredValueText.text = "";
            inputField.gameObject.SetActive(false);
            measureBtn.gameObject.SetActive(false);
            saveBtn.gameObject.SetActive(false);
            checkBtn.gameObject.SetActive(true);
        }
        else
        {
            RefreshExperimentUI();
        }
    }

    void ShowResults()
    {
        SetPanel(resultsPanel);

        LabResults r = experiment.Calculate();

        var sb = new StringBuilder();
        sb.AppendLine($"{"№",-4} {"Покази приладу",-18} {"Записано",-14} {"Відхилення",-12}");
        sb.AppendLine(new string('─', 50));
        for (int i = 0; i < r.Measurements.Count; i++)
            sb.AppendLine($"{i + 1,-4} {r.Generated[i],-18:F3} {r.Measurements[i],-14:F3} {r.Deviations[i],-12:F3}");

        tableText.text = sb.ToString();
        summaryText.text =
            $"Середнє: {r.Mean:F3} {r.Unit}      Похибка: ±{r.Uncertainty:F3} {r.Unit}";
    }

    void Close()
    {
        gameObject.SetActive(false);
        if (uiManager != null)
            uiManager.CloseMenu();
    }

    // ── Хелпери ──────────────────────────────────────────────────────────────

    void ResetExperiment() => experiment = new DefaultLabExperiment(currentData);

    void RefreshSaveBtn() =>
        saveBtn.interactable = hasMeasured && !string.IsNullOrWhiteSpace(inputField.text);

    void SetPanel(GameObject target)
    {
        mainPanel.SetActive(target == mainPanel);
        infoPanel.SetActive(target == infoPanel);
        experimentPanel.SetActive(target == experimentPanel);
        resultsPanel.SetActive(target == resultsPanel);
    }
}
