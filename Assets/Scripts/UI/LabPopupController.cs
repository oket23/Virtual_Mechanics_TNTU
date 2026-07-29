using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public Button     linkBtn;

    [Header("Вимірювання")]
    public GameObject     experimentPanel;
    public TMP_Text       progressText;
    public TMP_Text       measurementContextText; // "α = 30°"
    public TMP_Text       measuredValueText;
    public TMP_InputField inputField;
    public Button         measureBtn;
    public Button         saveBtn;
    public Button         checkBtn;
    public Button         experimentBackBtn;

    [Header("Ліва панель вимірювань")]
    public TMP_Text expNameText;
    public TMP_Text expDescText;
    public TMP_Text expFormulaText;
    public TMP_Text expHintsText;

    [Header("Жива таблиця")]
    public TMP_Text liveTableText;

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
        if (linkBtn)     linkBtn.onClick.AddListener(OpenLink);

        if (measureBtn)        measureBtn.onClick.AddListener(DoMeasure);
        if (saveBtn)           saveBtn.onClick.AddListener(SaveMeasurement);
        if (checkBtn)          checkBtn.onClick.AddListener(ShowResults);
        if (experimentBackBtn) experimentBackBtn.onClick.AddListener(() => { ResetExperiment(); SetPanel(mainPanel); });

        if (inputField)     inputField.onValueChanged.AddListener(_ => RefreshSaveBtn());
        if (resultsBackBtn) resultsBackBtn.onClick.AddListener(() => { ResetExperiment(); SetPanel(mainPanel); });
    }

    // ── Публічне API ─────────────────────────────────────────────────────────

    public void Open(LabData data)
    {
        currentData = data;
        ResetExperiment();

        nameText.text = currentData.instrumentName;
        descText.text = currentData.description;

        if (expNameText)    expNameText.text    = currentData.instrumentName;
        if (expDescText)    expDescText.text    = currentData.description;
        if (expFormulaText) expFormulaText.text = currentData.formula;
        if (expHintsText)   expHintsText.text   = currentData.hints;

        if (linkBtn)
            linkBtn.gameObject.SetActive(!string.IsNullOrEmpty(currentData.externalUrl));

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
        string ctx = experiment.GetCurrentLabel();

        progressText.text = $"Вимір {experiment.CurrentCount + 1} з {experiment.TotalCount}";

        if (measurementContextText != null)
            measurementContextText.text = ctx;

        measuredValueText.text = "—";
        inputField.text        = "";
        inputField.gameObject.SetActive(true);
        hasMeasured          = false;
        saveBtn.interactable = false;
        saveBtn.gameObject.SetActive(true);
        measureBtn.gameObject.SetActive(true);
        checkBtn.gameObject.SetActive(false);
    }

    void DoMeasure()
    {
        lastGenerated          = experiment.GenerateMeasurement();
        measuredValueText.text = $"{lastGenerated:F3} {currentData.unit}";
        hasMeasured            = true;
        inputField.text        = "";
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
        RefreshLiveTable();

        if (experiment.IsComplete)
        {
            progressText.text = $"Усі {experiment.TotalCount} замірів виконані";
            if (measurementContextText != null) measurementContextText.text = "";
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

        // Якщо експеримент має власну таблицю — використовуємо її (напр. Lab2 з cos α)
        string customTable = experiment.BuildResultsTable(r);
        if (customTable != null)
        {
            tableText.text = customTable;
        }
        else
        {
            bool hasLabels = r.Labels != null && r.Labels.Count > 0;
            var sb = new StringBuilder();
            if (hasLabels)
                sb.AppendLine($"{"Умова",-10} {"Покази",-10} {"Записано",-10} {"Відхил.",-8}");
            else
                sb.AppendLine($"{"№",-4} {"Покази приладу",-16} {"Записано",-12} {"Відхил.",-8}");
            sb.AppendLine(new string('─', 42));
            for (int i = 0; i < r.Measurements.Count; i++)
            {
                string lbl = hasLabels ? r.Labels[i] : $"{i + 1}";
                sb.AppendLine($"{lbl,-10} {r.Generated[i],-10:F3} {r.Measurements[i],-10:F3} {r.Deviations[i],-8:F3}");
            }
            tableText.text = sb.ToString();
        }

        summaryText.text = $"Середнє: {r.Mean:F3} {r.Unit}      Похибка: ±{r.Uncertainty:F3} {r.Unit}";
    }

    void Close()
    {
        gameObject.SetActive(false);
        if (uiManager != null) uiManager.CloseMenu();
    }

    // ── Хелпери ──────────────────────────────────────────────────────────────

    void ResetExperiment()
    {
        experiment = currentData.experimentType == LabData.ExperimentType.PhotoEffect
            ? (LabExperimentBase)new Lab2PhotoExperiment(currentData)
            : new DefaultLabExperiment(currentData);

        if (liveTableText != null)
            liveTableText.text = "Результати з'являться після першого запису...";
    }

    void RefreshLiveTable()
    {
        if (liveTableText == null) return;
        var sb = new StringBuilder();

        string firstLabel = experiment.GetLabel(0);
        bool hasLabels = !string.IsNullOrEmpty(firstLabel) && firstLabel != "1";

        if (hasLabels)
            sb.AppendLine($"{"Умова",-10} {"Показ",-10} {"Запис",-8}");
        else
            sb.AppendLine($"{"№",-3} {"Показ",-10} {"Запис",-8}");

        sb.AppendLine(new string('─', 28));

        for (int i = 0; i < experiment.MeasuredCount; i++)
            sb.AppendLine($"{experiment.GetLabel(i),-10} {experiment.GetGenerated(i),-10:F3} {experiment.GetMeasurement(i),-8:F3}");

        liveTableText.text = sb.ToString();
    }

    void OpenLink()
    {
        if (currentData != null && !string.IsNullOrEmpty(currentData.externalUrl))
            Application.OpenURL(currentData.externalUrl);
    }

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
