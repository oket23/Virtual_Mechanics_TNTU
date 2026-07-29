using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AtwoodLabController : MonoBehaviour
{
    public UIManager uiManager;

    // ── Shared ────────────────────────────────────────────────────────────────
    public TMP_Text phaseTitleText;
    public TMP_Text hintBarText;
    public Button   headerCloseBtn;

    // ── Sub-panels ────────────────────────────────────────────────────────────
    public GameObject quizPanel;
    public GameObject measurePanel;
    public GameObject conclusionPanel;

    // ── Quiz ──────────────────────────────────────────────────────────────────
    public TMP_Text       quizQText;
    public TMP_InputField quizAnswerInput;
    public TMP_Text       quizFeedbackText;
    public Button         quizNextBtn;
    public TMP_Text       quizNextLabel;

    static readonly string[] QUESTIONS = new[]
    {
        "Що таке прискорення?",
        "В яких одиницях вимірюють прискорення в системі СІ?",
        "Сформулюйте другий закон Ньютона.",
        "Чому дорівнює систематична похибка міліметрової лінійки?"
    };

    static readonly string[] ANSWERS = new[]
    {
        "Прискорення — векторна фізична величина, що дорівнює відношенню зміни швидкості до часу, за який ця зміна відбулася: a = Δv/Δt.",
        "м/с² (метр на секунду в квадраті).",
        "Прискорення тіла прямо пропорційне рівнодійній прикладених до нього сил і обернено пропорційне масі тіла: a = F/m.",
        "0,5 мм (половина ціни найменшого поділу шкали лінійки)."
    };

    // ── Measure / enter panel ─────────────────────────────────────────────────
    public TMP_Text       measContextText;
    public GameObject     measGenerateGroup;   // contains btn + raw value + rounding instr
    public Button         measGenerateBtn;
    public TMP_Text       measRawValueText;
    public TMP_Text       measRoundInstrText;
    public TMP_InputField measValueInput;
    public TMP_Text       measCellQuestionText;
    public TMP_InputField measCellInput;
    public TMP_Text       measErrorText;
    public Button         measInsertBtn;

    // ── Scheme image (left panel) ─────────────────────────────────────────────
    public UnityEngine.UI.RawImage schemeRawImage;

    // ── Conclusion ────────────────────────────────────────────────────────────
    public TMP_Text conclusionText;
    public Button   conclusionCloseBtn;

    // ── Table 1 cells (right panel) ───────────────────────────────────────────
    // Cells: A1-A3 = S₁[0..2], B1 = S₂, C1-C3 = t₂[0..2]
    // E1 = ā, H1 = Δā, I1 = ε  (Table 1 calculated)
    public Image[]    t1BgA  = new Image[3];
    public TMP_Text[] t1TxtA = new TMP_Text[3];
    public Image[]    t1BgB  = new Image[3];
    public TMP_Text[] t1TxtB = new TMP_Text[3];
    public Image[]    t1BgC  = new Image[3];
    public TMP_Text[] t1TxtC = new TMP_Text[3];
    public Image      t1BgE;    public TMP_Text t1TxtE;
    public Image      t1BgH;    public TMP_Text t1TxtH;
    public Image      t1BgI;    public TMP_Text t1TxtI;

    // ── Table 2 cells (right panel) ───────────────────────────────────────────
    // D1 = m, F1 = m₁, G1 = g, J1 = ā, K1 = Δā, L1 = ε
    public Image  t2BgD;  public TMP_Text t2TxtD;
    public Image  t2BgF;  public TMP_Text t2TxtF;
    public Image  t2BgG;  public TMP_Text t2TxtG;
    public Image  t2BgJ;  public TMP_Text t2TxtJ;
    public Image  t2BgK;  public TMP_Text t2TxtK;
    public Image  t2BgL;  public TMP_Text t2TxtL;

    // ── Table 1 — Сер.Д row auto-display cells ────────────────────────────────
    public TMP_Text t1TxtAvgA;  // averaged S₁ (auto-filled)
    public TMP_Text t1TxtAvgB;  // averaged S₂ (auto-filled)
    public TMP_Text t1TxtAvgC;  // averaged t₂ (auto-filled)

    // ── Table 1 — pre-filled error columns ────────────────────────────────────
    public TMP_Text[] t1TxtDS1 = new TMP_Text[4];  // ΔS₁ rows 1,2,3 + avg
    public TMP_Text[] t1TxtDT2 = new TMP_Text[4];  // Δt₂ rows 1,2,3 + avg
    public TMP_Text   t1TxtDS2;                      // ΔS₂ in avg row

    // ── Table 2 — pre-filled error columns ────────────────────────────────────
    public TMP_Text t2TxtDM;
    public TMP_Text t2TxtDM1;
    public TMP_Text t2TxtDG;

    // ── Links ─────────────────────────────────────────────────────────────────
    public Button linkCpslBtn;
    public Button linkBookBtn;

    // ── Experiment ID ─────────────────────────────────────────────────────────
    public TMP_Text experimentIdText;

    // ── Cell colours ──────────────────────────────────────────────────────────
    static readonly Color CELL_NORMAL = new Color(0.11f, 0.13f, 0.20f, 1f);
    static readonly Color CELL_ACTIVE = new Color(0.80f, 0.45f, 0.05f, 1f);
    static readonly Color CELL_DONE   = new Color(0.10f, 0.30f, 0.18f, 1f);
    static readonly Color BTN_QUIZ    = new Color(0.20f, 0.23f, 0.35f, 1f);

    // ── Step descriptor ───────────────────────────────────────────────────────
    struct Step
    {
        public string cell;
        public bool   needsGenerate;
        public string title;
        public string hint;
    }

    // ── State ─────────────────────────────────────────────────────────────────
    enum Phase { Quiz, Measure, Conclusion }
    Phase    phase;
    int      quizSub;
    bool     quizShowingFeedback;
    string[] savedAnswers = new string[4];

    Step[] allSteps;
    int    stepIdx;
    float  lastRaw;
    bool   hasGenerated;
    string activeCellName = "";

    float[] valA = new float[3];
    float[] valB = new float[3];
    float[] valC = new float[3];

    AtwoodLabData data;

    // ── Awake ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        quizNextBtn.onClick.AddListener(AdvanceQuiz);
        quizAnswerInput.onValueChanged.AddListener(_ => RefreshQuizBtn());
        measGenerateBtn.onClick.AddListener(DoGenerate);
        measValueInput.onValueChanged.AddListener(_ => RefreshInsertBtn());
        measCellInput.onValueChanged.AddListener(_ => RefreshInsertBtn());
        measInsertBtn.onClick.AddListener(DoInsert);
        conclusionCloseBtn.onClick.AddListener(Close);
        if (headerCloseBtn) headerCloseBtn.onClick.AddListener(Close);
        if (linkCpslBtn) linkCpslBtn.onClick.AddListener(() => Application.OpenURL("https://physics.tntu.edu.ua/labs/cpsl/"));
        if (linkBookBtn) linkBookBtn.onClick.AddListener(() => Application.OpenURL("https://elartu.tntu.edu.ua/handle/lib/50011"));

        if (headerCloseBtn) headerCloseBtn.GetComponentInChildren<TMP_Text>().text = "× Закрити";
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Open(AtwoodLabData labData)
    {
        data          = labData;
        valA          = new float[3];
        valB          = new float[3];
        valC          = new float[3];
        stepIdx       = 0;
        hasGenerated  = false;
        activeCellName = "";

        if (schemeRawImage != null && data.schemeImage != null)
        {
            schemeRawImage.texture = data.schemeImage;
            
            // Вираховуємо пропорції картинки (ширина поділена на висоту)
            var fitter = schemeRawImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                fitter.aspectRatio = (float)data.schemeImage.width / data.schemeImage.height;
            }
        }

        BuildSteps();
        ClearAllCells();
        FillPrefilledCells();

        if (experimentIdText != null)
            experimentIdText.text = "ID: " + GenerateExperimentId();

        quizSub       = 0;
        savedAnswers  = new string[QUESTIONS.Length];
        ToPhase(Phase.Quiz);
    }

    // ── Steps ─────────────────────────────────────────────────────────────────
    void BuildSteps()
    {
        allSteps = new Step[]
        {
            Stp("A1", true,  "Вимірювання S<sub>1</sub>  (1 з 3)",
                $"S<sub>1</sub> — шлях рівноприскореного руху.  ΔS<sub>1</sub> = {U(data.s1SysError,"F1")} мм.  Округліть до 0,5 мм"),
            Stp("A2", true,  "Вимірювання S<sub>1</sub>  (2 з 3)",
                $"S<sub>1</sub> — шлях рівноприскореного руху.  ΔS<sub>1</sub> = {U(data.s1SysError,"F1")} мм.  Округліть до 0,5 мм"),
            Stp("A3", true,  "Вимірювання S<sub>1</sub>  (3 з 3)",
                $"S<sub>1</sub> — шлях рівноприскореного руху.  ΔS<sub>1</sub> = {U(data.s1SysError,"F1")} мм.  Округліть до 0,5 мм"),
            Stp("B1", true,  "Вимірювання S<sub>2</sub>  (1 з 3)",
                $"S<sub>2</sub> — шлях рівномірного руху.  ΔS<sub>2</sub> = {U(data.s2SysError,"F1")} мм.  Округліть до 0,5 мм"),
            Stp("B2", true,  "Вимірювання S<sub>2</sub>  (2 з 3)",
                $"S<sub>2</sub> — шлях рівномірного руху.  ΔS<sub>2</sub> = {U(data.s2SysError,"F1")} мм.  Округліть до 0,5 мм"),
            Stp("B3", true,  "Вимірювання S<sub>2</sub>  (3 з 3)",
                $"S<sub>2</sub> — шлях рівномірного руху.  ΔS<sub>2</sub> = {U(data.s2SysError,"F1")} мм.  Округліть до 0,5 мм"),
            Stp("C1", true,  "Вимірювання t<sub>2</sub>  (1 з 3)",
                $"t<sub>2</sub> — час руху.  Δt<sub>2</sub> = {U(data.t2SysError,"F4")} с.  Округліть до 4 знаків"),
            Stp("C2", true,  "Вимірювання t<sub>2</sub>  (2 з 3)",
                $"t<sub>2</sub> — час руху.  Δt<sub>2</sub> = {U(data.t2SysError,"F4")} с.  Округліть до 4 знаків"),
            Stp("C3", true,  "Вимірювання t<sub>2</sub>  (3 з 3)",
                $"t<sub>2</sub> — час руху.  Δt<sub>2</sub> = {U(data.t2SysError,"F4")} с.  Округліть до 4 знаків"),
            Stp("D1", false, "Введення m — Таблиця 2",
                $"Маса тягарця  m = {U(data.massM,"F1")} г"),
            Stp("F1", false, "Введення m<sub>1</sub> — Таблиця 2",
                $"Маса перевантаження  m<sub>1</sub> = {U(data.massM1,"F1")} г"),
            Stp("G1", false, "Введення g — Таблиця 2",
                $"Прискорення вільного падіння  g = {U(data.gravG,"F2")} м/с<sup>2</sup>"),
        };
    }

    static Step Stp(string cell, bool gen, string title, string hint) =>
        new Step { cell = cell, needsGenerate = gen, title = title, hint = hint };

    void BeginMeasurements()
    {
        stepIdx = 0;
        ToPhase(Phase.Measure);
        Canvas.ForceUpdateCanvases();
        ShowStep();
    }

    // ── Phase ─────────────────────────────────────────────────────────────────
    void ToPhase(Phase p)
    {
        phase = p;
        quizPanel.SetActive(p == Phase.Quiz);
        measurePanel.SetActive(p == Phase.Measure);
        conclusionPanel.SetActive(p == Phase.Conclusion);

        if (p == Phase.Quiz) ShowQuiz();
    }

    // ── Quiz ──────────────────────────────────────────────────────────────────
    void ShowQuiz()
    {
        phaseTitleText.text          = $"Опитування  {quizSub + 1} / {QUESTIONS.Length}";
        hintBarText.text             = "Введіть відповідь у текстове поле";
        quizQText.text               = QUESTIONS[quizSub];
        quizAnswerInput.text         = "";
        quizAnswerInput.interactable = true;
        if (quizFeedbackText) quizFeedbackText.text = "";
        quizShowingFeedback          = false;
        quizNextLabel.text           = "Перевірити  >>";
        RefreshQuizBtn();
        quizAnswerInput.Select();
    }

    void RefreshQuizBtn() =>
        quizNextBtn.interactable = quizShowingFeedback ||
                                   !string.IsNullOrWhiteSpace(quizAnswerInput.text);

    void AdvanceQuiz()
    {
        if (!quizShowingFeedback)
        {
            savedAnswers[quizSub]        = quizAnswerInput.text.Trim();
            quizAnswerInput.interactable = false;
            if (quizFeedbackText)
                quizFeedbackText.text = $"Правильна відповідь:\n{ANSWERS[quizSub]}";
            bool last          = quizSub >= QUESTIONS.Length - 1;
            quizNextLabel.text = last ? "Розпочати лабораторну  >>" : "Далі  >>";
            quizNextBtn.interactable  = true;
            quizShowingFeedback       = true;
        }
        else
        {
            quizSub++;
            if (quizSub >= QUESTIONS.Length) BeginMeasurements();
            else ToPhase(Phase.Quiz);
        }
    }

    // ── Measure step ──────────────────────────────────────────────────────────
    void ShowStep()
    {
        var st = allSteps[stepIdx];
        phaseTitleText.text = st.title;
        hintBarText.text    = st.hint;
        measErrorText.text  = "";
        measValueInput.text = "";
        measCellInput.text  = "";
        hasGenerated        = false;

        measContextText.text = BuildContextText(st.cell);
        measGenerateGroup.SetActive(st.needsGenerate);

        if (st.needsGenerate)
            measRawValueText.text = "—";

        activeCellName = st.cell;
        RefreshInsertBtn();
    }

    string BuildContextText(string cell)
    {
        switch (cell[0])
        {
            case 'A':
                return
                    $"Систематична похибка:  ΔS<sub>1</sub> = {U(data.s1SysError,"F1")} мм\n" +
                    "Одиниці запису: мм";
            case 'B':
                return
                    $"Систематична похибка:  ΔS<sub>2</sub> = {U(data.s2SysError,"F1")} мм\n" +
                    "Одиниці запису: мм";
            case 'C':
                return
                    $"Систематична похибка:  Δt<sub>2</sub> = {U(data.t2SysError,"F4")} с\n" +
                    "Округліть до 4 знаків після коми.\n" +
                    "Одиниці запису: с";
            case 'D':
                return $"Маса тягарця\n\nm = {U(data.massM,"F1")} г\n\nОдиниці запису: г";
            case 'F':
                return $"Маса перевантаження\n\nm<sub>1</sub> = {U(data.massM1,"F1")} г\n\nОдиниці запису: г";
            case 'G':
                return $"Прискорення вільного падіння\n\ng = {U(data.gravG,"F2")} м/с<sup>2</sup>\n\nОдиниці запису: м/с<sup>2</sup>";
            default: return "";
        }
    }

    void DoGenerate()
    {
        char col = allSteps[stepIdx].cell[0];
        if (col == 'A')
        {
            lastRaw = Random.Range(data.s1Min, data.s1Max);
            measRawValueText.text = $"{U(lastRaw,"F3")} мм";
        }
        else if (col == 'B')
        {
            lastRaw = Random.Range(data.s2Min, data.s2Max);
            measRawValueText.text = $"{U(lastRaw,"F3")} мм";
        }
        else
        {
            lastRaw = Random.Range(data.t2Min, data.t2Max);
            measRawValueText.text = $"{U(lastRaw,"F6")} с";
        }
        hasGenerated = true;
        measValueInput.text = "";
        measValueInput.Select();
        RefreshInsertBtn();
    }

    void DoInsert()
    {
        var st = allSteps[stepIdx];

        string enteredCell = measCellInput.text.Trim().ToUpper();
        if (enteredCell != st.cell)
        {
            measErrorText.text = "Неправильна клітинка таблиці!";
            return;
        }

        string raw = measValueInput.text.Trim().Replace(',', '.');
        if (!float.TryParse(raw,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float val))
        {
            measErrorText.text = "Введіть числове значення";
            return;
        }

        string errMsg = ValidateValue(st.cell, val);
        if (errMsg != null) { measErrorText.text = errMsg; return; }

        StoreValue(st.cell, val);
        HighlightCell(st.cell, CELL_DONE);
        SetCellText(st.cell, FormatCell(st.cell, val));
        activeCellName = "";

        stepIdx++;
        if (stepIdx >= allSteps.Length)
            ShowConclusion();
        else
            ShowStep();
    }

    // Returns null if OK, error message otherwise
    string ValidateValue(string cell, float val)
    {
        switch (cell[0])
        {
            case 'A':
            case 'B':
            {
                float rounded = RoundHalf(lastRaw);
                if (Mathf.Abs(val - rounded) > 0.05f)
                    return $"Перевірте округлення до 0,5 мм. Правильно: {U(rounded,"F1")}";
                return null;
            }
            case 'C':
            {
                float rounded = Round4dp(lastRaw);
                if (Mathf.Abs(val - rounded) > 0.00006f)
                    return $"Округліть до 4 знаків після коми. Правильно: {U(rounded,"F4")}";
                return null;
            }
            case 'D':
                if (Mathf.Abs(val - data.massM) > 0.15f)
                    return $"Очікується m = {U(data.massM,"F1")} г";
                return null;
            case 'F':
                if (Mathf.Abs(val - data.massM1) > 0.15f)
                    return $"Очікується m₁ = {U(data.massM1,"F1")} г";
                return null;
            case 'G':
                if (Mathf.Abs(val - data.gravG) > 0.015f)
                    return $"Очікується g = {U(data.gravG,"F2")} м/с²";
                return null;
        }
        return null;
    }

    void StoreValue(string cell, float val)
    {
        switch (cell)
        {
            case "A1": valA[0] = val; UpdateAvgS1(); break;
            case "A2": valA[1] = val; UpdateAvgS1(); break;
            case "A3": valA[2] = val; UpdateAvgS1(); break;
            case "B1": valB[0] = val; UpdateAvgS2(); break;
            case "B2": valB[1] = val; UpdateAvgS2(); break;
            case "B3": valB[2] = val; UpdateAvgS2(); break;
            case "C1": valC[0] = val; UpdateAvgT2(); break;
            case "C2": valC[1] = val; UpdateAvgT2(); break;
            case "C3": valC[2] = val; UpdateAvgT2(); break;
        }
    }

    void FillPrefilledCells() { }

    void UpdateAvgS1()
    {
        if (t1TxtAvgA == null) return;
        t1TxtAvgA.text = (valA[0] > 0 && valA[1] > 0 && valA[2] > 0)
            ? S1Avg().ToString("F1", ukUA) : "";
    }

    void UpdateAvgS2()
    {
        if (t1TxtAvgB == null) return;
        t1TxtAvgB.text = (valB[0] > 0 && valB[1] > 0 && valB[2] > 0)
            ? S2Avg().ToString("F1", ukUA) : "";
    }

    void UpdateAvgT2()
    {
        if (t1TxtAvgC == null) return;
        t1TxtAvgC.text = (valC[0] > 0 && valC[1] > 0 && valC[2] > 0)
            ? T2Avg().ToString("F4", ukUA) : "";
    }

    static string FormatCell(string cell, float val)
    {
        switch (cell[0])
        {
            case 'A': case 'B':             return U(val, "F1");
            case 'C':                        return U(val, "F4");
            case 'D': case 'F':             return U(val, "F1");
            case 'G':                        return U(val, "F2");
            case 'E': case 'J':             return U(val, "F4");
            case 'H': case 'K':             return U(val, "F5");
            case 'I': case 'L':             return U(val, "F2");
            default:                         return val.ToString("G4", ukUA);
        }
    }

    void RefreshInsertBtn()
    {
        if (stepIdx >= allSteps.Length) return;
        bool genOk  = !allSteps[stepIdx].needsGenerate || hasGenerated;
        bool valOk  = !string.IsNullOrWhiteSpace(measValueInput.text);
        bool cellOk = !string.IsNullOrWhiteSpace(measCellInput.text);
        measInsertBtn.interactable = genOk && valOk && cellOk;
    }

    // ── Conclusion ────────────────────────────────────────────────────────────
    void ShowConclusion()
    {
        ToPhase(Phase.Conclusion);
        phaseTitleText.text = "Інструкція до звіту";
        hintBarText.text    = "Вимірювання завершені — зробіть скріншот таблиць";
        StartCoroutine(ApplyConclusionText());
    }

    IEnumerator ApplyConclusionText()
    {
        // Wait one frame so the layout system establishes container widths
        // before TMP_Text renders — otherwise word-wrap has no width to work with
        yield return null;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Вимірювання завершено!\n");
        sb.AppendLine(
            "Розрахунок випадкових похибок вимірювань, середнього значення " +
            "вимірюваної величини та похибок експерименту проведіть за формулами, " +
            "наведеними у методичних вказівках до лабораторної роботи.\n\n" +
            "Скріншот заповнених таблиць віртуального експерименту є лише додатком " +
            "до звіту, в якому повинні бути таблиці з повними результатами вимірювань " +
            "та обчислень.");
        sb.AppendLine("\n──────────────────────────────");
        sb.AppendLine("ВІДПОВІДІ НА ЗАПИТАННЯ\n");
        for (int i = 0; i < QUESTIONS.Length; i++)
        {
            sb.AppendLine($"{i + 1}. {QUESTIONS[i]}");
            sb.AppendLine($"   {savedAnswers[i]}\n");
        }
        conclusionText.text = sb.ToString();
    }

    float S1Avg() => (valA[0] + valA[1] + valA[2]) / 3f;
    float S2Avg() => (valB[0] + valB[1] + valB[2]) / 3f;
    float T2Avg() => (valC[0] + valC[1] + valC[2]) / 3f;

    static readonly System.Globalization.CultureInfo ukUA = System.Globalization.CultureInfo.GetCultureInfo("uk-UA");
    static string U(float v, string fmt) => v.ToString(fmt, ukUA);

    static string GenerateExperimentId()
    {
        var now = System.DateTime.Now;
        int rand = Random.Range(1000, 10000);
        return $"{now.Day:D2}{now.Month:D2}-{now.Year}-{rand}";
    }

    static float RoundHalf(float v) => (float)(System.Math.Round((double)v * 2.0, System.MidpointRounding.AwayFromZero) / 2.0);
    static float Round4dp(float v)  => (float)System.Math.Round((double)v, 4, System.MidpointRounding.AwayFromZero);

    // ── Cell management ───────────────────────────────────────────────────────
    void ClearAllCells()
    {
        for (int i = 0; i < 3; i++) SetCell(t1BgA[i], t1TxtA[i], CELL_NORMAL, $"A{i + 1}");
        for (int i = 0; i < 3; i++) SetCell(t1BgB[i], t1TxtB[i], CELL_NORMAL, $"B{i + 1}");
        for (int i = 0; i < 3; i++) SetCell(t1BgC[i], t1TxtC[i], CELL_NORMAL, $"C{i + 1}");
        SetCell(t1BgE,  t1TxtE,  CELL_NORMAL, "");
        SetCell(t1BgH,  t1TxtH,  CELL_NORMAL, "");
        SetCell(t1BgI,  t1TxtI,  CELL_NORMAL, "");
        SetCell(t2BgD,  t2TxtD,  CELL_NORMAL, "D1");
        SetCell(t2BgF,  t2TxtF,  CELL_NORMAL, "F1");
        SetCell(t2BgG,  t2TxtG,  CELL_NORMAL, "G1");
        SetCell(t2BgJ,  t2TxtJ,  CELL_NORMAL, "");
        SetCell(t2BgK,  t2TxtK,  CELL_NORMAL, "");
        SetCell(t2BgL,  t2TxtL,  CELL_NORMAL, "");
        if (t1TxtAvgA != null) t1TxtAvgA.text = "";
        if (t1TxtAvgB != null) t1TxtAvgB.text = "";
        if (t1TxtAvgC != null) t1TxtAvgC.text = "";
    }

    void HighlightCell(string cell, Color color)
    {
        switch (cell)
        {
            case "A1": t1BgA[0].color = color; break;
            case "A2": t1BgA[1].color = color; break;
            case "A3": t1BgA[2].color = color; break;
            case "B1": t1BgB[0].color = color; break;
            case "B2": t1BgB[1].color = color; break;
            case "B3": t1BgB[2].color = color; break;
            case "C1": t1BgC[0].color = color; break;
            case "C2": t1BgC[1].color = color; break;
            case "C3": t1BgC[2].color = color; break;
            case "E1": t1BgE.color    = color; break;
            case "H1": t1BgH.color    = color; break;
            case "I1": t1BgI.color    = color; break;
            case "D1": t2BgD.color    = color; break;
            case "F1": t2BgF.color    = color; break;
            case "G1": t2BgG.color    = color; break;
            case "J1": t2BgJ.color    = color; break;
            case "K1": t2BgK.color    = color; break;
            case "L1": t2BgL.color    = color; break;
        }
    }

    void SetCellText(string cell, string text)
    {
        switch (cell)
        {
            case "A1": t1TxtA[0].text = text; break;
            case "A2": t1TxtA[1].text = text; break;
            case "A3": t1TxtA[2].text = text; break;
            case "B1": t1TxtB[0].text = text; break;
            case "B2": t1TxtB[1].text = text; break;
            case "B3": t1TxtB[2].text = text; break;
            case "C1": t1TxtC[0].text = text; break;
            case "C2": t1TxtC[1].text = text; break;
            case "C3": t1TxtC[2].text = text; break;
            case "E1": t1TxtE.text    = text; break;
            case "H1": t1TxtH.text    = text; break;
            case "I1": t1TxtI.text    = text; break;
            case "D1": t2TxtD.text    = text; break;
            case "F1": t2TxtF.text    = text; break;
            case "G1": t2TxtG.text    = text; break;
            case "J1": t2TxtJ.text    = text; break;
            case "K1": t2TxtK.text    = text; break;
            case "L1": t2TxtL.text    = text; break;
        }
    }

    static void SetCell(Image bg, TMP_Text txt, Color color, string text)
    {
        if (bg  != null) bg.color  = color;
        if (txt != null) txt.text  = text;
    }

    void Close()
    {
        gameObject.SetActive(false);
        uiManager?.CloseMenu();
    }
}
