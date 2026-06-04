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
        dlgV.childForceExpandHeight = false;

        // 1. Header
        ctrl.headerCloseBtn = BuildHeader(dialog);

        // 2. Content row
        var content = Go(dialog, "ContentRow");
        content.AddComponent<LayoutElement>().flexibleHeight = 1;
        var rowH = content.AddComponent<HorizontalLayoutGroup>();
        rowH.spacing              = 1;
        rowH.childControlWidth    = true;
        rowH.childControlHeight   = true;
        rowH.childForceExpandWidth  = true;
        rowH.childForceExpandHeight = true;

        BuildLeft(content, ctrl);
        BuildCenter(content, ctrl);
        BuildRight(content, ctrl);

        // 3. Footer
        BuildFooter(dialog);

        // 4. Hint bar
        var hintGO = Go(dialog, "HintBar");
        hintGO.AddComponent<LayoutElement>().preferredHeight = 36;
        hintGO.AddComponent<Image>().color = BG_HEADER;
        var hintTxtGO = Go(hintGO, "T");
        Stretch(hintTxtGO);
        var hintTxt = hintTxtGO.AddComponent<TextMeshProUGUI>();
        hintTxt.text      = "";
        hintTxt.fontSize  = 13;
        hintTxt.alignment = TextAlignmentOptions.Center;
        hintTxt.color     = TEXT_DIM;
        ctrl.hintBarText  = hintTxt;

        uim.atwoodLabController = ctrl;
    }

    // ── Header ────────────────────────────────────────────────────────────────
    static Button BuildHeader(GameObject parent)
    {
        var hdr = Go(parent, "Header");
        hdr.AddComponent<LayoutElement>().preferredHeight = 48;
        hdr.AddComponent<Image>().color = BG_HEADER;
        var hlg = hdr.AddComponent<HorizontalLayoutGroup>();
        hlg.padding              = new RectOffset(16, 6, 0, 0);
        hlg.spacing              = 4;
        hlg.childControlWidth    = true;
        hlg.childControlHeight   = true;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;

        var titleGO = Go(hdr, "Title");
        titleGO.AddComponent<LayoutElement>().flexibleWidth = 1;
        var t = titleGO.AddComponent<TextMeshProUGUI>();
        t.text      = "ЛАБОРАТОРНА РОБОТА №2  —  МАШИНА АТВУДА";
        t.fontSize  = 17;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.color     = TEXT_ACCENT;

        var closeGO = Go(hdr, "CloseBtn");
        closeGO.AddComponent<LayoutElement>().preferredWidth = 110;
        closeGO.AddComponent<Image>().color = BTN_RED;
        var closeBtn = closeGO.AddComponent<Button>();
        SetBtnColors(closeBtn, BTN_RED);
        BtnLabel(closeGO, "✕  Закрити", 13f);
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

    // Table 1: columns №, A:S₁, B:S₂, C:t₂, E:ā, H:Δā, I:ε
    static void BuildTable1(GameObject parent, AtwoodLabController ctrl)
    {
        Lbl(parent, "T1Title", "ТАБЛИЦЯ 1 — ДОСЛІДНІ ДАНІ", 11, true, TEXT_ACCENT, 18);
        Space(parent, 4);

        float[] w = { 0.4f, 1.8f, 1.8f, 2.4f, 2.0f, 2.0f, 1.4f };

        // Header
        var hdrRow = TableRow(parent, 22);
        SetHdrCell(hdrRow, w[0], "№");
        SetHdrCell(hdrRow, w[1], "A  S₁\n(мм)");
        SetHdrCell(hdrRow, w[2], "B  S₂\n(мм)");
        SetHdrCell(hdrRow, w[3], "C  t₂\n(с)");
        SetHdrCell(hdrRow, w[4], "E  ā\n(м/с²)");
        SetHdrCell(hdrRow, w[5], "H  Δā\n(м/с²)");
        SetHdrCell(hdrRow, w[6], "I  ε\n(%)");

        Space(parent, 1);

        // Row 1 — A1, B1, C1, E1, H1, I1 are active data cells
        var r1 = TableRow(parent, 26);
        DataCell(r1, w[0], TEXT_DIM).txt.text = "1";
        var cellA1 = DataCell(r1, w[1], TEXT_WHITE);
        var cellB1 = DataCell(r1, w[2], TEXT_WHITE);
        var cellC1 = DataCell(r1, w[3], TEXT_WHITE);
        var cellE1 = DataCell(r1, w[4], TEXT_WHITE);
        var cellH1 = DataCell(r1, w[5], TEXT_WHITE);
        var cellI1 = DataCell(r1, w[6], TEXT_WHITE);
        ctrl.t1BgA[0] = cellA1.bg; ctrl.t1TxtA[0] = cellA1.txt;
        ctrl.t1BgB    = cellB1.bg; ctrl.t1TxtB    = cellB1.txt;
        ctrl.t1BgC[0] = cellC1.bg; ctrl.t1TxtC[0] = cellC1.txt;
        ctrl.t1BgE    = cellE1.bg; ctrl.t1TxtE    = cellE1.txt;
        ctrl.t1BgH    = cellH1.bg; ctrl.t1TxtH    = cellH1.txt;
        ctrl.t1BgI    = cellI1.bg; ctrl.t1TxtI    = cellI1.txt;

        Space(parent, 1);

        // Row 2 — A2, C2 only
        var r2 = TableRow(parent, 26);
        DataCell(r2, w[0], TEXT_DIM).txt.text = "2";
        var cellA2 = DataCell(r2, w[1], TEXT_WHITE);
        DataCell(r2, w[2], TEXT_DIM);   // B — empty
        var cellC2 = DataCell(r2, w[3], TEXT_WHITE);
        DataCell(r2, w[4], TEXT_DIM);   // E — spans row 1 only
        DataCell(r2, w[5], TEXT_DIM);   // H — spans row 1 only
        DataCell(r2, w[6], TEXT_DIM);   // I — spans row 1 only
        ctrl.t1BgA[1] = cellA2.bg; ctrl.t1TxtA[1] = cellA2.txt;
        ctrl.t1BgC[1] = cellC2.bg; ctrl.t1TxtC[1] = cellC2.txt;

        Space(parent, 1);

        // Row 3 — A3, C3 only
        var r3 = TableRow(parent, 26);
        DataCell(r3, w[0], TEXT_DIM).txt.text = "3";
        var cellA3 = DataCell(r3, w[1], TEXT_WHITE);
        DataCell(r3, w[2], TEXT_DIM);
        var cellC3 = DataCell(r3, w[3], TEXT_WHITE);
        DataCell(r3, w[4], TEXT_DIM);
        DataCell(r3, w[5], TEXT_DIM);
        DataCell(r3, w[6], TEXT_DIM);
        ctrl.t1BgA[2] = cellA3.bg; ctrl.t1TxtA[2] = cellA3.txt;
        ctrl.t1BgC[2] = cellC3.bg; ctrl.t1TxtC[2] = cellC3.txt;

        Space(parent, 4);
        Lbl(parent, "T1Note", "ΔS = 0.5 мм   |   Δt = 0.0005 с",
            10, false, TEXT_DIM, 14);
    }

    // Table 2: columns D:m, F:m₁, G:g, J:ā, K:Δā, L:ε
    static void BuildTable2(GameObject parent, AtwoodLabController ctrl)
    {
        Lbl(parent, "T2Title", "ТАБЛИЦЯ 2 — ТЕОРЕТИЧНІ ДАНІ", 11, true, TEXT_ACCENT, 18);
        Space(parent, 4);

        float[] w = { 1.6f, 1.6f, 1.6f, 2.0f, 2.0f, 1.4f };

        // Header
        var hdrRow = TableRow(parent, 22);
        SetHdrCell(hdrRow, w[0], "D  m\n(г)");
        SetHdrCell(hdrRow, w[1], "F  m₁\n(г)");
        SetHdrCell(hdrRow, w[2], "G  g\n(м/с²)");
        SetHdrCell(hdrRow, w[3], "J  ā\n(м/с²)");
        SetHdrCell(hdrRow, w[4], "K  Δā\n(м/с²)");
        SetHdrCell(hdrRow, w[5], "L  ε\n(%)");

        Space(parent, 1);

        // Single data row
        var r1 = TableRow(parent, 26);
        var cellD1 = DataCell(r1, w[0], TEXT_WHITE);
        var cellF1 = DataCell(r1, w[1], TEXT_WHITE);
        var cellG1 = DataCell(r1, w[2], TEXT_WHITE);
        var cellJ1 = DataCell(r1, w[3], TEXT_WHITE);
        var cellK1 = DataCell(r1, w[4], TEXT_WHITE);
        var cellL1 = DataCell(r1, w[5], TEXT_WHITE);
        ctrl.t2BgD = cellD1.bg; ctrl.t2TxtD = cellD1.txt;
        ctrl.t2BgF = cellF1.bg; ctrl.t2TxtF = cellF1.txt;
        ctrl.t2BgG = cellG1.bg; ctrl.t2TxtG = cellG1.txt;
        ctrl.t2BgJ = cellJ1.bg; ctrl.t2TxtJ = cellJ1.txt;
        ctrl.t2BgK = cellK1.bg; ctrl.t2TxtK = cellK1.txt;
        ctrl.t2BgL = cellL1.bg; ctrl.t2TxtL = cellL1.txt;

        Space(parent, 4);
        Lbl(parent, "T2Note", "Δm = Δm₁ = 0.1 г   |   Δg = 0.01 м/с²",
            10, false, TEXT_DIM, 14);
    }

    // ── Footer ────────────────────────────────────────────────────────────────
    static void BuildFooter(GameObject parent)
    {
        var footer = Go(parent, "Footer");
        footer.AddComponent<LayoutElement>().preferredHeight = 26;
        footer.AddComponent<Image>().color = BG_FOOTER;
        var hlg = footer.AddComponent<HorizontalLayoutGroup>();
        hlg.padding   = new RectOffset(14, 14, 0, 0);
        hlg.spacing   = 12;
        hlg.childControlWidth    = true;
        hlg.childControlHeight   = true;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;

        FooterText(footer, "Lab", "Лабораторія кіберфізичних систем ТНТУ", 10, TEXT_DIM);
        FooterText(footer, "Book", "Механіка та молекулярна фізика. Лабораторний практикум", 10, TEXT_DIM);

        // Spacer
        Go(footer, "Spacer").AddComponent<LayoutElement>().flexibleWidth = 1;

        FooterText(footer, "Date",
            $"Дата: {System.DateTime.Now:dd.MM.yyyy}", 10, TEXT_DIM);
        FooterText(footer, "ExpID",
            $"Дослід №2", 10, TEXT_DIM);
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
        t.text = text; t.fontSize = 9;
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
        t.text = ""; t.fontSize = 10;
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
