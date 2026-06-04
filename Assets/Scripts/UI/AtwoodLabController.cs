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
    public TMP_Text   quizQText;
    public Button[]   quizOptBtns   = new Button[4];
    public TMP_Text[] quizOptLabels = new TMP_Text[4];
    public TMP_Text   quizFeedText;
    public Button     quizNextBtn;
    public TMP_Text   quizNextLabel;

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
    public Image      t1BgB;    public TMP_Text t1TxtB;
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
    enum Phase { Quiz, QuizFeedback, Measure, Conclusion }
    Phase phase;
    int   quizSub;
    int   quizSel;

    Step[] allSteps;
    int    stepIdx;
    float  lastRaw;
    bool   hasGenerated;
    string activeCellName = "";

    float[] valA = new float[3];
    float   valB;
    float[] valC = new float[3];

    AtwoodLabData data;

    // ── Awake ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        for (int i = 0; i < 4; i++) { int k = i; quizOptBtns[i].onClick.AddListener(() => SelectOption(k)); }
        quizNextBtn.onClick.AddListener(AdvanceQuiz);
        measGenerateBtn.onClick.AddListener(DoGenerate);
        measValueInput.onValueChanged.AddListener(_ => RefreshInsertBtn());
        measCellInput.onValueChanged.AddListener(_ => RefreshInsertBtn());
        measInsertBtn.onClick.AddListener(DoInsert);
        conclusionCloseBtn.onClick.AddListener(Close);
        if (headerCloseBtn) headerCloseBtn.onClick.AddListener(Close);
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Open(AtwoodLabData labData)
    {
        data          = labData;
        valA          = new float[3];
        valB          = 0f;
        valC          = new float[3];
        stepIdx       = 0;
        hasGenerated  = false;
        activeCellName = "";

        if (schemeRawImage != null)
            schemeRawImage.texture = data.schemeImage;

        BuildSteps();
        ClearAllCells();

        quizSub = 0;
        if (data.quizQuestions?.Length > 0)
            ToPhase(Phase.Quiz);
        else
            BeginMeasurements();
    }

    // ── Steps ─────────────────────────────────────────────────────────────────
    void BuildSteps()
    {
        allSteps = new Step[]
        {
            Stp("A1", true,  "Вимірювання S₁  (1 з 3)",
                $"S₁ — малий шлях.  ΔS = {data.s1SysError:F1} мм.  Округліть до 0.5 мм"),
            Stp("A2", true,  "Вимірювання S₁  (2 з 3)",
                $"S₁ — малий шлях.  ΔS = {data.s1SysError:F1} мм.  Округліть до 0.5 мм"),
            Stp("A3", true,  "Вимірювання S₁  (3 з 3)",
                $"S₁ — малий шлях.  ΔS = {data.s1SysError:F1} мм.  Округліть до 0.5 мм"),
            Stp("B1", true,  "Вимірювання S₂",
                $"S₂ — повний шлях.  ΔS = {data.s2SysError:F1} мм.  Округліть до 0.5 мм"),
            Stp("C1", true,  "Вимірювання t₂  (1 з 3)",
                $"t₂ — час руху.  Δt = {data.t2SysError:F4} с.  Округліть до 4 знаків"),
            Stp("C2", true,  "Вимірювання t₂  (2 з 3)",
                $"t₂ — час руху.  Δt = {data.t2SysError:F4} с.  Округліть до 4 знаків"),
            Stp("C3", true,  "Вимірювання t₂  (3 з 3)",
                $"t₂ — час руху.  Δt = {data.t2SysError:F4} с.  Округліть до 4 знаків"),
            Stp("E1", false, "Обчислення ā — Таблиця 1",
                "ā = S₂² / (2·S₁сер·t₂сер²)   |   S — у метрах, t — у секундах"),
            Stp("I1", false, "Обчислення ε — Таблиця 1",
                "ε(%) = (2·ΔS₂/S₂ + ΔS₁/S₁ + 2·Δt₂/t₂сер)·100   |   S у метрах"),
            Stp("H1", false, "Обчислення Δā — Таблиця 1",
                "Δā = ā · ε / 100   |   ā та ε вже введені вище"),
            Stp("D1", false, "Введення m — Таблиця 2",
                $"Маса тягарця  m = {data.massM:F1} г"),
            Stp("F1", false, "Введення m₁ — Таблиця 2",
                $"Маса перевантаження  m₁ = {data.massM1:F1} г"),
            Stp("G1", false, "Введення g — Таблиця 2",
                $"Прискорення вільного падіння  g = {data.gravG:F2} м/с²"),
            Stp("J1", false, "Обчислення ā — Таблиця 2",
                "ā = m₁·g / (2m + m₁)   |   m у кілограмах"),
            Stp("L1", false, "Обчислення ε — Таблиця 2",
                "ε(%) = (Δm₁·2m/(m₁·(2m+m₁)) + Δg/g + 2·Δm/(2m+m₁))·100"),
            Stp("K1", false, "Обчислення Δā — Таблиця 2",
                "Δā = ā · ε / 100   |   ā та ε вже введені вище"),
        };
    }

    static Step Stp(string cell, bool gen, string title, string hint) =>
        new Step { cell = cell, needsGenerate = gen, title = title, hint = hint };

    void BeginMeasurements()
    {
        stepIdx = 0;
        ToPhase(Phase.Measure);
        ShowStep();
    }

    // ── Phase ─────────────────────────────────────────────────────────────────
    void ToPhase(Phase p)
    {
        phase = p;
        quizPanel.SetActive(p == Phase.Quiz || p == Phase.QuizFeedback);
        measurePanel.SetActive(p == Phase.Measure);
        conclusionPanel.SetActive(p == Phase.Conclusion);

        if (p == Phase.Quiz) ShowQuiz();
    }

    // ── Quiz ──────────────────────────────────────────────────────────────────
    void ShowQuiz()
    {
        phaseTitleText.text = $"Вікторина  {quizSub + 1} / {data.quizQuestions.Length}";
        hintBarText.text    = "Оберіть одну правильну відповідь";
        var q = data.quizQuestions[quizSub];
        quizQText.text = q.question;
        for (int i = 0; i < 4; i++)
        {
            quizOptLabels[i].text       = q.options[i];
            quizOptBtns[i].interactable = true;
            quizOptBtns[i].GetComponent<Image>().color = BTN_QUIZ;
        }
        quizFeedText.gameObject.SetActive(false);
        quizNextBtn.gameObject.SetActive(false);
    }

    void SelectOption(int idx)
    {
        quizSel = idx;
        var q = data.quizQuestions[quizSub];
        for (int i = 0; i < 4; i++) quizOptBtns[i].interactable = false;
        quizOptBtns[q.correctIndex].GetComponent<Image>().color = new Color(0.14f, 0.52f, 0.24f, 1f);
        if (idx != q.correctIndex)
            quizOptBtns[idx].GetComponent<Image>().color = new Color(0.60f, 0.14f, 0.14f, 1f);
        phase = Phase.QuizFeedback;
        bool ok = idx == q.correctIndex;
        quizFeedText.gameObject.SetActive(true);
        quizFeedText.text = ok
            ? "<color=#55ff88>Правильно!</color>"
            : $"<color=#ff6655>Неправильно.</color>  Правильна: {q.options[q.correctIndex]}";
        bool last = quizSub >= data.quizQuestions.Length - 1;
        quizNextBtn.gameObject.SetActive(true);
        quizNextLabel.text = last ? "Розпочати лабораторну  >>" : "Наступне  >>";
        hintBarText.text   = "";
    }

    void AdvanceQuiz()
    {
        quizSub++;
        if (quizSub >= data.quizQuestions.Length) BeginMeasurements();
        else { ToPhase(Phase.Quiz); }
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

        HighlightCell(st.cell, CELL_ACTIVE);
        activeCellName = st.cell;
        RefreshInsertBtn();
    }

    string BuildContextText(string cell)
    {
        switch (cell[0])
        {
            case 'A':
                return
                    $"Вимірювання малого шляху S₁\n\n" +
                    $"Діапазон: {data.s1Min:F1} – {data.s1Max:F1} мм\n" +
                    $"Систематична похибка:  ΔS₁ = {data.s1SysError:F1} мм\n" +
                    "Одиниці запису: мм";
            case 'B':
                return
                    $"Вимірювання повного шляху S₂\n\n" +
                    $"Діапазон: {data.s2Min:F1} – {data.s2Max:F1} мм\n" +
                    $"Систематична похибка:  ΔS₂ = {data.s2SysError:F1} мм\n" +
                    "Одиниці запису: мм";
            case 'C':
                return
                    $"Вимірювання часу t₂\n\n" +
                    $"Діапазон: {data.t2Min:F4} – {data.t2Max:F4} с\n" +
                    $"Систематична похибка:  Δt₂ = {data.t2SysError:F4} с\n" +
                    "Одиниці запису: с";
            case 'D':
                return $"Маса тягарця\n\nm = {data.massM:F1} г\n\nОдиниці запису: г";
            case 'E':
            {
                float s1m = S1Avg() / 1000f, s2m = valB / 1000f, t2 = T2Avg();
                return
                    "Дослідне прискорення  ā (Таблиця 1)\n\n" +
                    "Формула:  ā = S₂² / (2·S₁сер·t₂сер²)\n\n" +
                    $"S₁сер = {S1Avg():F2} мм = {s1m:F5} м\n" +
                    $"S₂    = {valB:F2} мм = {s2m:F5} м\n" +
                    $"t₂сер = {T2Avg():F4} с\n\n" +
                    "Одиниці: м/с²";
            }
            case 'F':
                return $"Маса перевантаження\n\nm₁ = {data.massM1:F1} г\n\nОдиниці запису: г";
            case 'G':
                return $"Прискорення вільного падіння\n\ng = {data.gravG:F2} м/с²\n\nОдиниці запису: м/с²";
            case 'H':
            {
                float[] t1 = CalcTable1();
                return
                    "Абсолютна похибка  Δā (Таблиця 1)\n\n" +
                    "Формула:  Δā = ā · ε / 100\n\n" +
                    $"ā  = {t1[0]:F4} м/с²\n" +
                    $"ε  = {t1[2]:F2} %\n\n" +
                    "Одиниці: м/с²";
            }
            case 'I':
            {
                float s1m = S1Avg()/1000f, s2m = valB/1000f, t2 = T2Avg();
                return
                    "Відносна похибка  ε% (Таблиця 1)\n\n" +
                    "ε = (2·ΔS₂/S₂ + ΔS₁/S₁ + 2·Δt₂/t₂сер)·100\n\n" +
                    $"ΔS₁={data.s1SysError/1000f:F4} м,  S₁={s1m:F5} м\n" +
                    $"ΔS₂={data.s2SysError/1000f:F4} м,  S₂={s2m:F5} м\n" +
                    $"Δt₂={data.t2SysError:F4} с,  t₂={t2:F4} с\n\n" +
                    "Одиниці: %";
            }
            case 'J':
            {
                float m = data.massM/1000f, m1 = data.massM1/1000f;
                return
                    "Теоретичне прискорення  ā (Таблиця 2)\n\n" +
                    "Формула:  ā = m₁·g / (2m + m₁)\n\n" +
                    $"m  = {data.massM:F1} г = {m:F4} кг\n" +
                    $"m₁ = {data.massM1:F1} г = {m1:F4} кг\n" +
                    $"g  = {data.gravG:F2} м/с²\n\n" +
                    "Одиниці: м/с²";
            }
            case 'K':
            {
                float[] t2 = CalcTable2();
                return
                    "Абсолютна похибка  Δā (Таблиця 2)\n\n" +
                    "Формула:  Δā = ā · ε / 100\n\n" +
                    $"ā  = {t2[0]:F4} м/с²\n" +
                    $"ε  = {t2[2]:F2} %\n\n" +
                    "Одиниці: м/с²";
            }
            case 'L':
            {
                float m = data.massM/1000f, m1 = data.massM1/1000f;
                float dm = 0.0001f, dm1 = 0.0001f, dg = 0.01f;
                return
                    "Відносна похибка  ε% (Таблиця 2)\n\n" +
                    "ε = (Δm₁·2m/(m₁·(2m+m₁)) + Δg/g + 2·Δm/(2m+m₁))·100\n\n" +
                    $"Δm={dm:F4} кг,  Δm₁={dm1:F4} кг,  Δg={dg:F2} м/с²\n" +
                    $"m={m:F4} кг,  m₁={m1:F4} кг,  g={data.gravG:F2}\n\n" +
                    "Одиниці: %";
            }
            default: return "";
        }
    }

    void DoGenerate()
    {
        char col = allSteps[stepIdx].cell[0];
        if (col == 'A')
        {
            lastRaw = Random.Range(data.s1Min, data.s1Max);
            measRawValueText.text = $"{lastRaw:F3} мм";
        }
        else if (col == 'B')
        {
            lastRaw = Random.Range(data.s2Min, data.s2Max);
            measRawValueText.text = $"{lastRaw:F3} мм";
        }
        else
        {
            lastRaw = Random.Range(data.t2Min, data.t2Max);
            measRawValueText.text = $"{lastRaw:F6} с";
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
            {
                float rounded = RoundHalf(lastRaw);
                if (Mathf.Abs(val - rounded) > 0.26f)
                    return $"Перевірте округлення до 0.5 мм. Правильно: {rounded:F1}";
                return null;
            }
            case 'B':
            {
                float rounded = RoundHalf(lastRaw);
                if (Mathf.Abs(val - rounded) > 0.26f)
                    return $"Перевірте округлення до 0.5 мм. Правильно: {rounded:F1}";
                return null;
            }
            case 'C':
            {
                float rounded = Round4dp(lastRaw);
                if (Mathf.Abs(val - rounded) > 0.00006f)
                    return $"Округліть до 4 знаків після коми. Правильно: {rounded:F4}";
                return null;
            }
            case 'D':
                if (Mathf.Abs(val - data.massM) > 0.15f)
                    return $"Очікується m = {data.massM:F1} г";
                return null;
            case 'E':
            {
                float correct = CalcTable1()[0];
                if (correct > 0 && Mathf.Abs(val - correct) / correct > 0.05f)
                    return "Перевірте обчислення (відхилення > 5%)";
                return null;
            }
            case 'F':
                if (Mathf.Abs(val - data.massM1) > 0.15f)
                    return $"Очікується m₁ = {data.massM1:F1} г";
                return null;
            case 'G':
                if (Mathf.Abs(val - data.gravG) > 0.015f)
                    return $"Очікується g = {data.gravG:F2} м/с²";
                return null;
            case 'H':
            {
                float correct = CalcTable1()[1];
                float denom = Mathf.Abs(correct) + 0.0001f;
                if (Mathf.Abs(val - correct) / denom > 0.05f)
                    return "Перевірте обчислення Δā (відхилення > 5%)";
                return null;
            }
            case 'I':
            {
                float correct = CalcTable1()[2];
                float denom = Mathf.Abs(correct) + 0.001f;
                if (Mathf.Abs(val - correct) / denom > 0.05f)
                    return "Перевірте обчислення ε (відхилення > 5%)";
                return null;
            }
            case 'J':
            {
                float correct = CalcTable2()[0];
                if (correct > 0 && Mathf.Abs(val - correct) / correct > 0.05f)
                    return "Перевірте обчислення ā (відхилення > 5%)";
                return null;
            }
            case 'K':
            {
                float correct = CalcTable2()[1];
                float denom = Mathf.Abs(correct) + 0.0001f;
                if (Mathf.Abs(val - correct) / denom > 0.05f)
                    return "Перевірте обчислення Δā (відхилення > 5%)";
                return null;
            }
            case 'L':
            {
                float correct = CalcTable2()[2];
                float denom = Mathf.Abs(correct) + 0.001f;
                if (Mathf.Abs(val - correct) / denom > 0.05f)
                    return "Перевірте обчислення ε (відхилення > 5%)";
                return null;
            }
        }
        return null;
    }

    void StoreValue(string cell, float val)
    {
        switch (cell)
        {
            case "A1": valA[0] = val; break;
            case "A2": valA[1] = val; break;
            case "A3": valA[2] = val; break;
            case "B1": valB    = val; break;
            case "C1": valC[0] = val; break;
            case "C2": valC[1] = val; break;
            case "C3": valC[2] = val; break;
        }
    }

    static string FormatCell(string cell, float val)
    {
        switch (cell[0])
        {
            case 'A': case 'B':             return $"{val:F1}";
            case 'C':                        return $"{val:F4}";
            case 'D': case 'F':             return $"{val:F1}";
            case 'G':                        return $"{val:F2}";
            case 'E': case 'J':             return $"{val:F4}";
            case 'H': case 'K':             return $"{val:F5}";
            case 'I': case 'L':             return $"{val:F2}";
            default:                         return val.ToString("G4");
        }
    }

    void RefreshInsertBtn()
    {
        bool genOk  = !allSteps[stepIdx].needsGenerate || hasGenerated;
        bool valOk  = !string.IsNullOrWhiteSpace(measValueInput.text);
        bool cellOk = !string.IsNullOrWhiteSpace(measCellInput.text);
        measInsertBtn.interactable = genOk && valOk && cellOk;
    }

    // ── Conclusion ────────────────────────────────────────────────────────────
    void ShowConclusion()
    {
        ToPhase(Phase.Conclusion);
        phaseTitleText.text = "Висновок";
        hintBarText.text    = "Лабораторна робота виконана";

        float a1  = CalcTable1()[0];
        float a2  = CalcTable2()[0];
        float avg = (a1 + a2) * 0.5f;
        float diff = avg > 0f ? Mathf.Abs(a1 - a2) / avg * 100f : 0f;

        conclusionText.text =
            "Лабораторна робота №2 виконана!\n\n" +
            $"Дослідне прискорення (Табл. 1):     ā₁ = {a1:F4} м/с²\n" +
            $"Теоретичне прискорення (Табл. 2):   ā₂ = {a2:F4} м/с²\n\n" +
            $"Відносне розходження:  {diff:F1}%\n\n" +
            (diff < 10f
                ? "Другий закон Ньютона підтверджено в межах похибки вимірювань."
                : "Є помітна розбіжність — перевірте точність введених значень.");
    }

    // ── Calculations ──────────────────────────────────────────────────────────
    // Returns [ā, Δā, ε%]
    float[] CalcTable1()
    {
        float S1 = S1Avg() / 1000f;
        float S2 = valB    / 1000f;
        float t2 = T2Avg();
        if (S1 <= 0f || S2 <= 0f || t2 <= 0f) return new float[3];
        float a   = S2 * S2 / (2f * S1 * t2 * t2);
        float eps = (2f * (data.s2SysError / 1000f) / S2 +
                         (data.s1SysError / 1000f) / S1 +
                     2f *  data.t2SysError          / t2) * 100f;
        return new[] { a, a * eps / 100f, eps };
    }

    // Returns [ā, Δā, ε%]
    float[] CalcTable2()
    {
        float m  = data.massM  / 1000f;
        float m1 = data.massM1 / 1000f;
        float g  = data.gravG;
        float a  = m1 * g / (2f * m + m1);
        const float dm = 0.0001f, dm1 = 0.0001f, dg = 0.01f;
        float eps = (dm1 * 2f * m / (m1 * (2f * m + m1)) + dg / g + 2f * dm / (2f * m + m1)) * 100f;
        return new[] { a, a * eps / 100f, eps };
    }

    float S1Avg() => (valA[0] + valA[1] + valA[2]) / 3f;
    float T2Avg() => (valC[0] + valC[1] + valC[2]) / 3f;

    static float RoundHalf(float v) => Mathf.Round(v * 2f) / 2f;
    static float Round4dp(float v)  => Mathf.Round(v * 10000f) / 10000f;

    // ── Cell management ───────────────────────────────────────────────────────
    void ClearAllCells()
    {
        for (int i = 0; i < 3; i++) { SetCell(t1BgA[i], t1TxtA[i], CELL_NORMAL, ""); }
        SetCell(t1BgB, t1TxtB, CELL_NORMAL, "");
        for (int i = 0; i < 3; i++) { SetCell(t1BgC[i], t1TxtC[i], CELL_NORMAL, ""); }
        SetCell(t1BgE, t1TxtE, CELL_NORMAL, "");
        SetCell(t1BgH, t1TxtH, CELL_NORMAL, "");
        SetCell(t1BgI, t1TxtI, CELL_NORMAL, "");
        SetCell(t2BgD, t2TxtD, CELL_NORMAL, "");
        SetCell(t2BgF, t2TxtF, CELL_NORMAL, "");
        SetCell(t2BgG, t2TxtG, CELL_NORMAL, "");
        SetCell(t2BgJ, t2TxtJ, CELL_NORMAL, "");
        SetCell(t2BgK, t2TxtK, CELL_NORMAL, "");
        SetCell(t2BgL, t2TxtL, CELL_NORMAL, "");
    }

    void HighlightCell(string cell, Color color)
    {
        switch (cell)
        {
            case "A1": t1BgA[0].color = color; break;
            case "A2": t1BgA[1].color = color; break;
            case "A3": t1BgA[2].color = color; break;
            case "B1": t1BgB.color    = color; break;
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
            case "B1": t1TxtB.text    = text; break;
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
