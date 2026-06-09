using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class AtwoodLabBootstrap
{
    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color BG_DIALOG  = new Color(0.10f, 0.12f, 0.18f, 1f);
    static readonly Color BG_HEADER  = new Color(0.14f, 0.18f, 0.30f, 1f);
    static readonly Color BG_PANEL   = new Color(0.11f, 0.13f, 0.20f, 1f);
    static readonly Color BG_INPUT   = new Color(0.18f, 0.20f, 0.28f, 1f);
    static readonly Color BG_OVERLAY = new Color(0.00f, 0.00f, 0.00f, 0.82f);
    static readonly Color BG_DARK    = new Color(0.07f, 0.09f, 0.14f, 1f);
    static readonly Color BG_FOOTER  = new Color(0.09f, 0.11f, 0.17f, 1f);

    static readonly Color BTN_BLUE   = new Color(0.20f, 0.38f, 0.72f, 1f);
    static readonly Color BTN_GREEN  = new Color(0.14f, 0.52f, 0.24f, 1f);
    static readonly Color BTN_RED    = new Color(0.60f, 0.14f, 0.14f, 1f);
    static readonly Color BTN_TEAL   = new Color(0.14f, 0.44f, 0.54f, 1f);
    static readonly Color BTN_QUIZ   = new Color(0.20f, 0.23f, 0.35f, 1f);
    static readonly Color BTN_ORANGE = new Color(0.70f, 0.38f, 0.05f, 1f);

    static readonly Color TEXT_WHITE  = new Color(1.00f, 1.00f, 1.00f, 1f);
    static readonly Color TEXT_DIM    = new Color(0.70f, 0.75f, 0.85f, 1f);
    static readonly Color TEXT_ACCENT = new Color(0.55f, 0.80f, 1.00f, 1f);
    static readonly Color TEXT_GREEN  = new Color(0.30f, 0.90f, 0.55f, 1f);
    static readonly Color TEXT_ORANGE = new Color(1.00f, 0.75f, 0.30f, 1f);
    static readonly Color DIVIDER     = new Color(0.25f, 0.30f, 0.45f, 1f);

    static readonly Color CELL_NORMAL  = new Color(0.11f, 0.13f, 0.20f, 1f);
    static readonly Color CELL_HEADER  = new Color(0.14f, 0.18f, 0.28f, 1f);
    static readonly Color CELL_BORDER  = new Color(0.20f, 0.24f, 0.36f, 1f);

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        UIManager uim = Object.FindFirstObjectByType<UIManager>();
        if (uim == null) return;
        if (uim.atwoodLabController != null) return;

        var existing = GameObject.Find("AtwoodLabOverlay");
        if (existing != null)
        {
            var c = existing.GetComponent<AtwoodLabController>();
            if (c != null) { uim.atwoodLabController = c; return; }
            Object.Destroy(existing);
        }

        var canvasGO = new GameObject("AtwoodLabCanvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 101;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        Build(uim, canvasGO);
    }

    // ── Build ─────────────────────────────────────────────────────────────────
    static void Build(UIManager uim, GameObject canvasGO)
    {
        // Full-screen overlay
        var overlay = Go(canvasGO, "AtwoodLabOverlay");
        Stretch(overlay);
        overlay.AddComponent<Image>().color = BG_OVERLAY;
        overlay.SetActive(false);

        var ctrl = overlay.AddComponent<AtwoodLabController>();
        ctrl.uiManager = uim;

        // Dialog box (2%–98% of screen)
        var dialog = Go(overlay, "Dialog");
        var drt = dialog.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.02f, 0.02f);
        drt.anchorMax = new Vector2(0.98f, 0.98f);
        drt.offsetMin = drt.offsetMax = Vector2.zero;
        dialog.AddComponent<Image>().color = BG_DIALOG;
        var dlgV = dialog.AddComponent<VerticalLayoutGroup>();
        dlgV.padding              = new RectOffset(0, 0, 0, 0);
        dlgV.spacing              = 1;
        dlgV.childControlWidth    = true;
        dlgV.childControlHeight   = true;
        dlgV.childForceExpandWidth  = true;
        dlgV.childForceExpandHeight = true;  // proportional layout

        // Layout: 10% header | 70% content | 10% hint | 10% info
        // 1. Header (10%)
        ctrl.headerCloseBtn = BuildHeader(dialog);

        // 2. Content row (70%)
        var content = Go(dialog, "ContentRow");
        content.AddComponent<LayoutElement>().flexibleHeight = 7;
        var rowH = content.AddComponent<HorizontalLayoutGroup>();
        rowH.spacing              = 1;
        rowH.childControlWidth    = true;
        rowH.childControlHeight   = true;
        rowH.childForceExpandWidth  = true;
        rowH.childForceExpandHeight = true;

        BuildLeft(content, ctrl);
        BuildCenter(content, ctrl);
        BuildRight(content, ctrl);

        // 3. Hint row (10%)
        ctrl.hintBarText = BuildHintRow(dialog);

        // 4. Info row (10%)
        BuildInfoRow(dialog);

        uim.atwoodLabController = ctrl;
    }

    // ── Header (10% of dialog height) ────────────────────────────────────────
    static Button BuildHeader(GameObject parent)
    {
        var hdr = Go(parent, "Header");
        hdr.AddComponent<LayoutElement>().flexibleHeight = 1;  // 10% share
        hdr.AddComponent<Image>().color = BG_HEADER;
        var hlg = hdr.AddComponent<HorizontalLayoutGroup>();
        hlg.padding              = new RectOffset(12, 4, 0, 0);
        hlg.spacing              = 4;
        hlg.childControlWidth    = true;
        hlg.childControlHeight   = true;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;

        var titleGO = Go(hdr, "Title");
        titleGO.AddComponent<LayoutElement>().flexibleWidth = 1;
        var t = titleGO.AddComponent<TextMeshProUGUI>();
        t.text      = "ЛАБОРАТОРНА РОБОТА №2  —  МАШИНА АТВУДА";
        t.fontSize  = 13;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.color     = TEXT_ACCENT;

        var closeGO = Go(hdr, "CloseBtn");
        closeGO.AddComponent<LayoutElement>().preferredWidth = 80;
        closeGO.AddComponent<Image>().color = BTN_RED;
        var closeBtn = closeGO.AddComponent<Button>();
        SetBtnColors(closeBtn, BTN_RED);
        BtnLabel(closeGO, "✕  Закрити", 11f);
        return closeBtn;
    }

    // ── Left panel ────────────────────────────────────────────────────────────
    static void BuildLeft(GameObject parent, AtwoodLabController ctrl)
    {
        var left = VPanel(parent, "LeftPanel", 18f, BG_PANEL);

        Lbl(left, "Title",  "МАШИНА АТВУДА",        14, true,  TEXT_ACCENT, 22);
        Divider(left);
        Space(left, 6);

        var imgGO = Go(left, "SchemeImage");
        imgGO.AddComponent<LayoutElement>().flexibleHeight = 1f;
        var rawImg = imgGO.AddComponent<RawImage>();
        rawImg.color = Color.white;
        ctrl.schemeRawImage = rawImg;

        Divider(left);
        Space(left, 6);
        Lbl(left, "FmLbl", "ФОРМУЛИ", 12, true, TEXT_ACCENT, 18);
        Space(left, 4);
        Lbl(left, "Fm1", "Дослідне:\na = S2^2/(2*S1*t2^2)", 12, false, TEXT_WHITE, 38);
        Space(left, 6);
        Lbl(left, "Fm2", "Теоретичне:\na = m1*g/(2m+m1)",    12, false, TEXT_WHITE, 38);
        Divider(left);
        Space(left, 6);
        Lbl(left, "ErrLbl", "СИСТЕМ. ПОХИБКИ",     11, true,  TEXT_ACCENT, 16);
        Space(left, 4);
        Lbl(left, "ErrS",  "DS = 0.5 мм",          12, false, TEXT_DIM,    18);
        Lbl(left, "ErrT",  "Dt = 0.0005 с",         12, false, TEXT_DIM,    18);
        Lbl(left, "ErrM",  "Dm = 0.1 г",            12, false, TEXT_DIM,    18);
    }

    // ── Center panel ──────────────────────────────────────────────────────────
    static void BuildCenter(GameObject parent, AtwoodLabController ctrl)
    {
        var center = VPanel(parent, "CenterPanel", 45f, BG_PANEL);

        // Phase title
        var titleGO = Go(center, "PhaseTitle");
        titleGO.AddComponent<LayoutElement>().preferredHeight = 36;
        var t = titleGO.AddComponent<TextMeshProUGUI>();
        t.text = ""; t.fontSize = 17; t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center; t.color = TEXT_ACCENT;
        ctrl.phaseTitleText = t;
        Divider(center);
        Space(center, 4);

        ctrl.quizPanel       = BuildQuizPanel(center, ctrl);
        ctrl.measurePanel    = BuildMeasurePanel(center, ctrl);
        ctrl.conclusionPanel = BuildConclusionPanel(center, ctrl);

        ctrl.quizPanel.SetActive(true);
        ctrl.measurePanel.SetActive(false);
        ctrl.conclusionPanel.SetActive(false);
    }

    // ── Quiz panel ────────────────────────────────────────────────────────────
    static GameObject BuildQuizPanel(GameObject parent, AtwoodLabController ctrl)
    {
        var panel = SubPanel(parent, "QuizPanel");

        ctrl.quizQText = Lbl(panel, "Q", "", 16, false, TEXT_WHITE, 0, expand: true);
        Space(panel, 8);

        var optBtns   = new Button[4];
        var optLabels = new TMP_Text[4];
        for (int i = 0; i < 4; i++)
        {
            optBtns[i]   = BtnH(panel, $"Opt{i}", $"Варіант {i + 1}", BTN_QUIZ, 50, 13);
            optLabels[i] = optBtns[i].GetComponentInChildren<TMP_Text>();
            if (i < 3) Space(panel, 4);
        }
        ctrl.quizOptBtns   = optBtns;
        ctrl.quizOptLabels = optLabels;

        Space(panel, 8);
        ctrl.quizFeedText = Lbl(panel, "Feed", "", 14, false, TEXT_WHITE, 50);
        ctrl.quizFeedText.gameObject.SetActive(false);
        Space(panel, 6);
        ctrl.quizNextBtn = BtnH(panel, "BtnNext", "Наступне  >>", BTN_BLUE, 42, 14);
        ctrl.quizNextBtn.gameObject.SetActive(false);
        ctrl.quizNextLabel = ctrl.quizNextBtn.GetComponentInChildren<TMP_Text>();
        return panel;
    }

    // ── Measure / enter panel ─────────────────────────────────────────────────
    static GameObject BuildMeasurePanel(GameObject parent, AtwoodLabController ctrl)
    {
        var panel = SubPanel(parent, "MeasurePanel");

        // Context text area (scrollable)
        var ctxBox = Go(panel, "CtxBox");
        ctxBox.AddComponent<LayoutElement>().flexibleHeight = 1f;
        ctxBox.AddComponent<Image>().color = BG_DARK;
        var ctxV = ctxBox.AddComponent<VerticalLayoutGroup>();
        ctxV.padding = new RectOffset(12, 12, 8, 8);
        ctxV.childControlWidth = ctxV.childControlHeight = true;
        ctxV.childForceExpandWidth = true;
        ctxV.childForceExpandHeight = false;
        ctrl.measContextText = Lbl(ctxBox, "CtxTxt", "", 13, false, TEXT_WHITE, 0, expand: true);

        Space(panel, 8);
        Divider(panel);
        Space(panel, 6);

        // Generate group (generate btn + raw value display + rounding instruction)
        var genGroup = Go(panel, "GenerateGroup");
        genGroup.AddComponent<LayoutElement>().preferredHeight = 140;
        var genV = genGroup.AddComponent<VerticalLayoutGroup>();
        genV.spacing = 0;
        genV.childControlWidth = genV.childControlHeight = true;
        genV.childForceExpandWidth = true;
        genV.childForceExpandHeight = false;
        ctrl.measGenerateGroup = genGroup;

        ctrl.measGenerateBtn = BtnH(genGroup, "BtnGenerate",
            "Генерувати результат вимірювання", BTN_ORANGE, 42, 14);
        Space(genGroup, 6);

        // Raw value display box
        var rawBox = Go(genGroup, "RawBox");
        rawBox.AddComponent<LayoutElement>().preferredHeight = 52;
        rawBox.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.14f, 1f);
        var rawV = rawBox.AddComponent<VerticalLayoutGroup>();
        rawV.childControlWidth = rawV.childControlHeight = true;
        rawV.childForceExpandWidth = rawV.childForceExpandHeight = true;
        var rawTxtGO = Go(rawBox, "RawVal");
        var rawTmp = rawTxtGO.AddComponent<TextMeshProUGUI>();
        rawTmp.text = "—"; rawTmp.fontSize = 26;
        rawTmp.alignment = TextAlignmentOptions.Center; rawTmp.color = TEXT_ORANGE;
        ctrl.measRawValueText = rawTmp;

        Space(genGroup, 6);
        ctrl.measRoundInstrText = Lbl(genGroup, "RoundInstr",
            "Округліть це значення, враховуючи систематичну похибку приладу, " +
            "і запишіть у відповідних одиницях для внесення в таблицю",
            12, false, TEXT_DIM, 28);

        Space(panel, 6);

        // Value input
        Lbl(panel, "ValLbl", "Введіть значення:", 13, false, TEXT_DIM, 18);
        Space(panel, 4);
        ctrl.measValueInput = MakeInput(panel, "ValInput", "0.0000", 44,
            TMP_InputField.ContentType.Standard);

        Space(panel, 10);
        Divider(panel);
        Space(panel, 6);

        // Cell question
        ctrl.measCellQuestionText = Lbl(panel, "CellQ",
            "В яку клітинку таблиці слід внести це значення?",
            13, false, TEXT_DIM, 18);
        Space(panel, 4);
        ctrl.measCellInput = MakeInput(panel, "CellInput", "A1", 40,
            TMP_InputField.ContentType.Alphanumeric);

        Space(panel, 6);
        ctrl.measErrorText = Lbl(panel, "ErrTxt", "", 12, false,
            new Color(1f, 0.4f, 0.4f, 1f), 20);

        Space(panel, 4);
        ctrl.measInsertBtn = BtnH(panel, "BtnInsert",
            "Внести дані в таблицю", BTN_GREEN, 44, 14);
        ctrl.measInsertBtn.interactable = false;

        return panel;
    }

    // ── Conclusion panel ──────────────────────────────────────────────────────
    static GameObject BuildConclusionPanel(GameObject parent, AtwoodLabController ctrl)
    {
        var panel = SubPanel(parent, "ConclusionPanel");
        var box = Go(panel, "Box");
        box.AddComponent<LayoutElement>().flexibleHeight = 1;
        box.AddComponent<Image>().color = BG_DARK;
        var bv = box.AddComponent<VerticalLayoutGroup>();
        bv.padding = new RectOffset(16, 16, 14, 14);
        bv.childControlWidth = bv.childControlHeight = true;
        bv.childForceExpandWidth = true;
        bv.childForceExpandHeight = false;
        ctrl.conclusionText = Lbl(box, "Txt", "", 15, false, TEXT_WHITE, 0, expand: true);
        Space(panel, 10);
        ctrl.conclusionCloseBtn = BtnH(panel, "BtnClose",
            "Завершити лабораторну  ✓", BTN_GREEN, 46, 15);
        return panel;
    }

    // ── Right panel (tables) ──────────────────────────────────────────────────
    static void BuildRight(GameObject parent, AtwoodLabController ctrl)
    {
        var right = VPanel(parent, "RightPanel", 37f, BG_PANEL);

        BuildTable1(right, ctrl);
        Space(right, 10);
        Divider(right);
        Space(right, 6);
        BuildTable2(right, ctrl);
    }

// Table 1: №, S₁, ΔS₁, S₂, ΔS₂, t₂, Δt₂, a₁, Δa₁, ε  (+Сер.Д avg row)
    static void BuildTable1(GameObject parent, AtwoodLabController ctrl)
    {
        Lbl(parent, "T1Title", "ТАБЛИЦЯ 1 — ДОСЛІДНІ ДАНІ", 11, true, TEXT_ACCENT, 18);
        Space(parent, 4);

        // column flex weights
        float n=0.30f, s1=1.2f, ds1=1.0f, s2=1.2f, ds2=1.0f, t2=1.3f, dt2=1.0f,
              a=1.3f, da=1.3f, ep=1.0f;

        // Header row (Змінено порядок, одиниці та назву a1)
        var hdr = TableRow(parent, 26);
        SetHdrCell(hdr, n,   "№");
        SetHdrCell(hdr, s1,  "S₁\n(10⁻³ м)");
        SetHdrCell(hdr, ds1, "ΔS₁\n(10⁻³ м)");
        SetHdrCell(hdr, s2,  "S₂\n(10⁻³ м)");
        SetHdrCell(hdr, ds2, "ΔS₂\n(10⁻³ м)");
        SetHdrCell(hdr, t2,  "t₂\n(с)");
        SetHdrCell(hdr, dt2, "Δt₂\n(с)");
        SetHdrCell(hdr, a,   "a₁\n(м/с²)");
        SetHdrCell(hdr, da,  "Δa₁\n(м/с²)");
        SetHdrCell(hdr, ep,  "ε\n(%)");

        Space(parent, 1);

        // Row 1
        var r1 = TableRow(parent, 30);
        DataCell(r1, n,   TEXT_DIM).txt.text = "1";
        var cA1  = DataCell(r1, s1,  TEXT_WHITE);
        var cDS1_0 = DataCell(r1, ds1, TEXT_DIM);
        DataCell(r1, s2,  TEXT_DIM); // Порожня декоративна S2
        DataCell(r1, ds2, TEXT_DIM); // Порожня декоративна DS2
        var cC1  = DataCell(r1, t2,  TEXT_WHITE);
        var cDT2_0 = DataCell(r1, dt2, TEXT_DIM);
        DataCell(r1, a,   TEXT_DIM);
        DataCell(r1, da,  TEXT_DIM);
        DataCell(r1, ep,  TEXT_DIM);
        
        ctrl.t1BgA[0] = cA1.bg; ctrl.t1TxtA[0] = cA1.txt;
        ctrl.t1TxtDS1[0] = cDS1_0.txt;
        ctrl.t1BgC[0] = cC1.bg; ctrl.t1TxtC[0] = cC1.txt;
        ctrl.t1TxtDT2[0] = cDT2_0.txt;

        Space(parent, 1);

        // Row 2
        var r2 = TableRow(parent, 30);
        DataCell(r2, n,   TEXT_DIM).txt.text = "2";
        var cA2  = DataCell(r2, s1,  TEXT_WHITE);
        var cDS1_1 = DataCell(r2, ds1, TEXT_DIM);
        DataCell(r2, s2,  TEXT_DIM); // Порожня декоративна S2
        DataCell(r2, ds2, TEXT_DIM); // Порожня декоративна DS2
        var cC2  = DataCell(r2, t2,  TEXT_WHITE);
        var cDT2_1 = DataCell(r2, dt2, TEXT_DIM);
        DataCell(r2, a,   TEXT_DIM);
        DataCell(r2, da,  TEXT_DIM);
        DataCell(r2, ep,  TEXT_DIM);
        
        ctrl.t1BgA[1] = cA2.bg; ctrl.t1TxtA[1] = cA2.txt;
        ctrl.t1TxtDS1[1] = cDS1_1.txt;
        ctrl.t1BgC[1] = cC2.bg; ctrl.t1TxtC[1] = cC2.txt;
        ctrl.t1TxtDT2[1] = cDT2_1.txt;

        Space(parent, 1);

        // Row 3
        var r3 = TableRow(parent, 30);
        DataCell(r3, n,   TEXT_DIM).txt.text = "3";
        var cA3  = DataCell(r3, s1,  TEXT_WHITE);
        var cDS1_2 = DataCell(r3, ds1, TEXT_DIM);
        DataCell(r3, s2,  TEXT_DIM); // Порожня декоративна S2
        DataCell(r3, ds2, TEXT_DIM); // Порожня декоративна DS2
        var cC3  = DataCell(r3, t2,  TEXT_WHITE);
        var cDT2_2 = DataCell(r3, dt2, TEXT_DIM);
        DataCell(r3, a,   TEXT_DIM);
        DataCell(r3, da,  TEXT_DIM);
        DataCell(r3, ep,  TEXT_DIM);
        
        ctrl.t1BgA[2] = cA3.bg; ctrl.t1TxtA[2] = cA3.txt;
        ctrl.t1TxtDS1[2] = cDS1_2.txt;
        ctrl.t1BgC[2] = cC3.bg; ctrl.t1TxtC[2] = cC3.txt;
        ctrl.t1TxtDT2[2] = cDT2_2.txt;

        Space(parent, 1);

        // Сер.Д row
        var rAvg = TableRow(parent, 30);
        DataCell(rAvg, n,   TEXT_DIM).txt.text = "Сер.";
        var cAvgA  = DataCell(rAvg, s1,  TEXT_DIM);
        var cDS1_3 = DataCell(rAvg, ds1, TEXT_DIM);
        var cB1    = DataCell(rAvg, s2,  TEXT_WHITE);
        var cDS2   = DataCell(rAvg, ds2, TEXT_DIM);
        var cAvgC  = DataCell(rAvg, t2,  TEXT_DIM);
        var cDT2_3 = DataCell(rAvg, dt2, TEXT_DIM);
        var cE1    = DataCell(rAvg, a,   TEXT_WHITE);
        var cH1    = DataCell(rAvg, da,  TEXT_WHITE);
        var cI1    = DataCell(rAvg, ep,  TEXT_WHITE);
        
        ctrl.t1TxtAvgA   = cAvgA.txt;
        ctrl.t1TxtDS1[3] = cDS1_3.txt;
        ctrl.t1BgB       = cB1.bg;  ctrl.t1TxtB = cB1.txt;
        ctrl.t1TxtDS2    = cDS2.txt;
        ctrl.t1TxtAvgC   = cAvgC.txt;
        ctrl.t1TxtDT2[3] = cDT2_3.txt;
        ctrl.t1BgE       = cE1.bg;  ctrl.t1TxtE = cE1.txt;
        ctrl.t1BgH       = cH1.bg;  ctrl.t1TxtH = cH1.txt;
        ctrl.t1BgI       = cI1.bg;  ctrl.t1TxtI = cI1.txt;
    }

    // Table 2: m, Δm, m₁, Δm₁, g, Δg, a, Δa, ε
    static void BuildTable2(GameObject parent, AtwoodLabController ctrl)
    {
        Lbl(parent, "T2Title", "ТАБЛИЦЯ 2 — ТЕОРЕТИЧНІ ДАНІ", 11, true, TEXT_ACCENT, 18);
        Space(parent, 4);

        float m=1.2f, dm=1.0f, m1=1.2f, dm1=1.0f, g=1.3f, dg=1.0f,
              a=1.3f, da=1.3f, ep=1.0f;

        // Header (Змінено одиниці та назву a)
        var hdr = TableRow(parent, 26);
        SetHdrCell(hdr, m,   "m\n(10⁻³ кг)");
        SetHdrCell(hdr, dm,  "Δm\n(10⁻³ кг)");
        SetHdrCell(hdr, m1,  "m₁\n(10⁻³ кг)");
        SetHdrCell(hdr, dm1, "Δm₁\n(10⁻³ кг)");
        SetHdrCell(hdr, g,   "g\n(м/с²)");
        SetHdrCell(hdr, dg,  "Δg\n(м/с²)");
        SetHdrCell(hdr, a,   "a\n(м/с²)");
        SetHdrCell(hdr, da,  "Δa\n(м/с²)");
        SetHdrCell(hdr, ep,  "ε\n(%)");

        Space(parent, 1);

        // Single data row
        var r1 = TableRow(parent, 30);
        var cD1  = DataCell(r1, m,   TEXT_WHITE);
        var cDM  = DataCell(r1, dm,  TEXT_DIM);
        var cF1  = DataCell(r1, m1,  TEXT_WHITE);
        var cDM1 = DataCell(r1, dm1, TEXT_DIM);
        var cG1  = DataCell(r1, g,   TEXT_WHITE);
        var cDG  = DataCell(r1, dg,  TEXT_DIM);
        var cJ1  = DataCell(r1, a,   TEXT_WHITE);
        var cK1  = DataCell(r1, da,  TEXT_WHITE);
        var cL1  = DataCell(r1, ep,  TEXT_WHITE);
        
        ctrl.t2BgD  = cD1.bg;  ctrl.t2TxtD  = cD1.txt;
        ctrl.t2TxtDM  = cDM.txt;
        ctrl.t2BgF  = cF1.bg;  ctrl.t2TxtF  = cF1.txt;
        ctrl.t2TxtDM1 = cDM1.txt;
        ctrl.t2BgG  = cG1.bg;  ctrl.t2TxtG  = cG1.txt;
        ctrl.t2TxtDG  = cDG.txt;
        ctrl.t2BgJ  = cJ1.bg;  ctrl.t2TxtJ  = cJ1.txt;
        ctrl.t2BgK  = cK1.bg;  ctrl.t2TxtK  = cK1.txt;
        ctrl.t2BgL  = cL1.bg;  ctrl.t2TxtL  = cL1.txt;
    }

    // ── Hint row — 10% of dialog height ──────────────────────────────────────
    static TMP_Text BuildHintRow(GameObject parent)
    {
        var row = Go(parent, "HintRow");
        var le = row.AddComponent<LayoutElement>();
        le.flexibleHeight = 1f;   // 1 share out of 10 total → 10%
        row.AddComponent<Image>().color = BG_HEADER;
        var tGO = Go(row, "T");
        Stretch(tGO);
        var txt = tGO.AddComponent<TextMeshProUGUI>();
        txt.text      = "";
        txt.fontSize  = 12;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color     = TEXT_DIM;
        return txt;
    }

    // ── Info row — 10% of dialog height ──────────────────────────────────────
    static void BuildInfoRow(GameObject parent)
    {
        var row = Go(parent, "InfoRow");
        var le = row.AddComponent<LayoutElement>();
        le.flexibleHeight = 1f;   // 1 share out of 10 total → 10%
        row.AddComponent<Image>().color = BG_FOOTER;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.padding              = new RectOffset(14, 14, 0, 0);
        hlg.spacing              = 12;
        hlg.childControlWidth    = true;
        hlg.childControlHeight   = true;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        FooterText(row, "Lab",  "Лабораторія кіберфізичних систем ТНТУ", 11, TEXT_WHITE);
        FooterText(row, "Book", "Механіка та молекулярна фізика. Лабораторний практикум", 11, TEXT_WHITE);
        Go(row, "Spacer").AddComponent<LayoutElement>().flexibleWidth = 1;
        FooterText(row, "Date",  $"Дата: {System.DateTime.Now:dd.MM.yyyy}", 11, TEXT_WHITE);
        FooterText(row, "ExpID", "Дослід №2", 11, TEXT_WHITE);
    }

    // ── Table helpers ─────────────────────────────────────────────────────────

    struct CellRef { public Image bg; public TMP_Text txt; }

    static GameObject TableRow(GameObject parent, int h)
    {
        var row = Go(parent, "Row");
        row.AddComponent<LayoutElement>().preferredHeight = h;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing              = 1;
        hlg.childControlWidth    = true;
        hlg.childControlHeight   = true;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        return row;
    }

    static void SetHdrCell(GameObject row, float flex, string text)
    {
        var go = Go(row, "H");
        go.AddComponent<LayoutElement>().flexibleWidth = flex;
        go.AddComponent<Image>().color = CELL_HEADER;
        var tgo = Go(go, "T");
        Stretch(tgo);
        var t = tgo.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = 10;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = TEXT_ACCENT;
    }

    static CellRef DataCell(GameObject row, float flex, Color textColor)
    {
        var go = Go(row, "D");
        go.AddComponent<LayoutElement>().flexibleWidth = flex;
        var img = go.AddComponent<Image>();
        img.color = CELL_NORMAL;
        var tgo = Go(go, "T");
        Stretch(tgo);
        var t = tgo.AddComponent<TextMeshProUGUI>();
        t.text = ""; t.fontSize = 11;
        t.alignment = TextAlignmentOptions.Center;
        t.color = textColor;
        return new CellRef { bg = img, txt = t };
    }

    // ── Sub-panel helpers ─────────────────────────────────────────────────────

    static GameObject SubPanel(GameObject parent, string name)
    {
        var go = Go(parent, name);
        go.AddComponent<LayoutElement>().flexibleHeight = 1;
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.padding              = new RectOffset(8, 8, 6, 6);
        v.spacing              = 0;
        v.childAlignment       = TextAnchor.UpperLeft;
        v.childControlWidth    = true;
        v.childControlHeight   = true;
        v.childForceExpandWidth  = true;
        v.childForceExpandHeight = false;
        return go;
    }

    static GameObject VPanel(GameObject parent, string name, float flexW, Color? bg = null)
    {
        var go = Go(parent, name);
        go.AddComponent<LayoutElement>().flexibleWidth = flexW;
        if (bg.HasValue) go.AddComponent<Image>().color = bg.Value;
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.padding              = new RectOffset(10, 10, 10, 8);
        v.spacing              = 0;
        v.childAlignment       = TextAnchor.UpperLeft;
        v.childControlWidth    = true;
        v.childControlHeight   = true;
        v.childForceExpandWidth  = true;
        v.childForceExpandHeight = false;
        return go;
    }

    // ── Generic UI helpers ────────────────────────────────────────────────────

    static TMP_Text Lbl(GameObject parent, string name, string text,
        float size, bool bold, Color color, int h, bool expand = false)
    {
        var go = Go(parent, name);
        var le = go.AddComponent<LayoutElement>();
        if (expand) le.flexibleHeight = 1;
        else        le.preferredHeight = h;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = size;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.color     = color;
        return t;
    }

    static void FooterText(GameObject parent, string name, string text, float size, Color color)
    {
        var go = Go(parent, name);
        go.AddComponent<LayoutElement>().flexibleWidth = 0;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.color = color;
    }

    static void Divider(GameObject parent)
    {
        var go = Go(parent, "Div");
        go.AddComponent<LayoutElement>().preferredHeight = 2;
        go.AddComponent<Image>().color = DIVIDER;
    }

    static void Space(GameObject parent, int h)
        => Go(parent, "Sp").AddComponent<LayoutElement>().preferredHeight = h;

    static Button BtnH(GameObject parent, string name, string label, Color color, int h, float fs)
    {
        var go = Go(parent, name);
        go.AddComponent<LayoutElement>().preferredHeight = h;
        go.AddComponent<Image>().color = color;
        var b = go.AddComponent<Button>();
        SetBtnColors(b, color);
        BtnLabel(go, label, fs);
        return b;
    }

    static void BtnLabel(GameObject btn, string text, float fs)
    {
        var lgo = Go(btn, "Label");
        Stretch(lgo);
        var t = lgo.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fs;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = TEXT_WHITE;
    }

    static void SetBtnColors(Button btn, Color normal)
    {
        var cb = btn.colors;
        cb.normalColor      = normal;
        cb.highlightedColor = Color.Lerp(normal, Color.white, 0.25f);
        cb.pressedColor     = Color.Lerp(normal, Color.black, 0.25f);
        cb.selectedColor    = normal;
        btn.colors          = cb;
    }

    static TMP_InputField MakeInput(GameObject parent, string name,
        string placeholder, int h, TMP_InputField.ContentType contentType)
    {
        var go = Go(parent, name);
        go.AddComponent<LayoutElement>().preferredHeight = h;
        go.AddComponent<Image>().color = BG_INPUT;
        var f = go.AddComponent<TMP_InputField>();

        var vp = Go(go, "TextArea");
        Stretch(vp, new Vector2(8, 4));
        vp.AddComponent<RectMask2D>();
        f.textViewport = vp.GetComponent<RectTransform>();

        var pgo = Go(vp, "Placeholder");
        Stretch(pgo);
        var pt = pgo.AddComponent<TextMeshProUGUI>();
        pt.text = placeholder; pt.fontSize = 18;
        pt.color = TEXT_DIM; pt.fontStyle = FontStyles.Italic;
        pt.alignment = TextAlignmentOptions.Center;

        var tgo = Go(vp, "Text");
        Stretch(tgo);
        var tt = tgo.AddComponent<TextMeshProUGUI>();
        tt.fontSize = 22; tt.color = TEXT_ACCENT;
        tt.alignment = TextAlignmentOptions.Center;

        f.textComponent = tt;
        f.placeholder   = pt;
        f.contentType   = contentType;
        return f;
    }

    static GameObject Go(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void Stretch(GameObject go, Vector2? inset = null)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        var v = inset ?? Vector2.zero;
        rt.offsetMin =  v;
        rt.offsetMax = -v;
    }
}
