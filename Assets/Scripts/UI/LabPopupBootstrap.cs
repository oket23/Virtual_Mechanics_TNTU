using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class LabPopupBootstrap
{
    // ── Кольорова схема ───────────────────────────────────────────────────────
    static readonly Color BG_DIALOG  = new Color(0.10f, 0.12f, 0.18f, 1.00f);
    static readonly Color BG_HEADER  = new Color(0.14f, 0.18f, 0.30f, 1.00f);
    static readonly Color BG_PANEL   = new Color(0.11f, 0.13f, 0.20f, 1.00f);
    static readonly Color BG_INPUT   = new Color(0.18f, 0.20f, 0.28f, 1.00f);
    static readonly Color BG_OVERLAY = new Color(0.00f, 0.00f, 0.00f, 0.82f);

    static readonly Color BTN_BLUE   = new Color(0.20f, 0.38f, 0.72f, 1f);
    static readonly Color BTN_GREEN  = new Color(0.14f, 0.52f, 0.24f, 1f);
    static readonly Color BTN_RED    = new Color(0.60f, 0.14f, 0.14f, 1f);
    static readonly Color BTN_GREY   = new Color(0.22f, 0.25f, 0.35f, 1f);
    static readonly Color BTN_TEAL   = new Color(0.14f, 0.44f, 0.54f, 1f);

    static readonly Color TEXT_WHITE  = new Color(1.00f, 1.00f, 1.00f, 1f);
    static readonly Color TEXT_DIM    = new Color(0.70f, 0.75f, 0.85f, 1f);
    static readonly Color TEXT_ACCENT = new Color(0.55f, 0.80f, 1.00f, 1f);
    static readonly Color DIVIDER     = new Color(0.25f, 0.30f, 0.45f, 1f);

    // ── Bootstrap ─────────────────────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null) { Debug.LogError("[Bootstrap] UIManager не знайдено!"); return; }
        if (uiManager.labPopupController != null) return;

        var existingOverlay = GameObject.Find("LabPopupOverlay");
        if (existingOverlay != null)
        {
            var c = existingOverlay.GetComponent<LabPopupController>();
            if (c != null) { uiManager.labPopupController = c; return; }
            Object.Destroy(existingOverlay);
        }

        // Власний Canvas з CanvasScaler — масштабується під будь-який екран
        var canvasGO = new GameObject("LabPopupCanvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;   // балансує між шириною і висотою

        canvasGO.AddComponent<GraphicRaycaster>();

        Build(uiManager, canvasGO);
        Debug.Log("[Bootstrap] LabPopup створено.");
    }

    // ── Побудова ──────────────────────────────────────────────────────────────

    static void Build(UIManager uiManager, GameObject canvasGO)
    {
        var overlay = Go(canvasGO, "LabPopupOverlay");
        Stretch(overlay);
        overlay.AddComponent<Image>().color = BG_OVERLAY;
        overlay.SetActive(false); // деактивуємо ДО AddComponent

        var ctrl       = overlay.AddComponent<LabPopupController>();
        ctrl.uiManager = uiManager;

        // Діалог: максимально використовуємо екран
        var dialog = Go(overlay, "Dialog");
        var drt    = dialog.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.03f, 0.02f);
        drt.anchorMax = new Vector2(0.97f, 0.98f);
        drt.offsetMin = drt.offsetMax = Vector2.zero;
        dialog.AddComponent<Image>().color = BG_DIALOG;

        var mainPanel = BuildMain(dialog, ctrl);
        var infoPanel = BuildInfo(dialog, ctrl);
        var expPanel  = BuildExp(dialog, ctrl);
        var resPanel  = BuildRes(dialog, ctrl);

        ctrl.mainPanel       = mainPanel;
        ctrl.infoPanel       = infoPanel;
        ctrl.experimentPanel = expPanel;
        ctrl.resultsPanel    = resPanel;

        infoPanel.SetActive(false);
        expPanel.SetActive(false);
        resPanel.SetActive(false);

        uiManager.labPopupController = ctrl;
    }

    // ── Головне меню ─────────────────────────────────────────────────────────

    static GameObject BuildMain(GameObject parent, LabPopupController ctrl)
    {
        var root = ContentRoot(parent, "MainPanel");

        Header(root, "ЛАБОРАТОРНА РОБОТА");
        Divider(root);
        Space(root, 16);

        ctrl.nameText = Txt(root, "NameText", "Назва приладу", 30,
                            TextAlignmentOptions.Center, 46, color: TEXT_ACCENT);
        Space(root, 4);
        Txt(root, "Subtitle", "Оберіть дію для продовження", 17,
            TextAlignmentOptions.Center, 26, color: TEXT_DIM);
        Space(root, 20);

        var row = HRow(root, "BtnRow", 58);
        ctrl.infoBtn  = Btn(row, "BtnInfo",  "Інформація",       BTN_BLUE,  19);
        ctrl.startBtn = Btn(row, "BtnStart", "Почати виконання", BTN_GREEN, 19);

        Space(root, 14);
        Divider(root);
        Space(root, 10);
        ctrl.closeBtn = BtnH(root, "BtnClose", "Закрити", BTN_RED, 46, 18);

        return root;
    }

    // ── Інформація ────────────────────────────────────────────────────────────

    static GameObject BuildInfo(GameObject parent, LabPopupController ctrl)
    {
        var root = ContentRoot(parent, "InfoPanel");

        Header(root, "ІНФОРМАЦІЯ ПРО ПРИЛАД");
        Divider(root);
        Space(root, 10);

        ctrl.descText = Txt(root, "DescText", "", 19,
                            TextAlignmentOptions.TopLeft, 0, expand: true, color: TEXT_WHITE);
        Space(root, 10);
        Divider(root);
        Space(root, 10);
        ctrl.infoBackBtn = BtnH(root, "BtnBack", "<< Назад", BTN_GREY, 46, 18);
        return root;
    }

    // ── Вимірювання ───────────────────────────────────────────────────────────

    static GameObject BuildExp(GameObject parent, LabPopupController ctrl)
    {
        var root = ContentRoot(parent, "ExperimentPanel");

        Header(root, "ВИКОНАННЯ ВИМІРЮВАНЬ");
        Divider(root);
        Space(root, 10);

        ctrl.progressText = Txt(root, "ProgressText", "Вимір 1 з 5", 19,
                                TextAlignmentOptions.Center, 28, color: TEXT_DIM);
        Space(root, 6);

        var valBox = Box(root, "ValBox", new Color(0.08f, 0.10f, 0.16f, 1f), 72);
        ctrl.measuredValueText = Txt(valBox, "MeasuredValue", "—", 38,
                                     TextAlignmentOptions.Center, 0, expand: true,
                                     color: TEXT_ACCENT);

        Space(root, 10);
        Txt(root, "InputLabel", "Введіть прочитане значення:", 17,
            TextAlignmentOptions.Center, 22, color: TEXT_DIM);
        Space(root, 4);
        ctrl.inputField = MakeInput(root, "InputField", "0.000", 50);
        Space(root, 12);

        var row = HRow(root, "BtnRow", 54);
        ctrl.measureBtn = Btn(row, "BtnMeasure", "Зробити вимір", BTN_BLUE,  18);
        ctrl.saveBtn    = Btn(row, "BtnSave",    "Записати",      BTN_GREEN, 18);

        Space(root, 8);
        ctrl.checkBtn = BtnH(root, "BtnCheck", "Переглянути результати >>", BTN_TEAL, 50, 18);
        ctrl.checkBtn.gameObject.SetActive(false);

        Space(root, 6);
        ctrl.experimentBackBtn = BtnH(root, "BtnBack", "<< Назад", BTN_GREY, 44, 17);
        return root;
    }

    // ── Результати ────────────────────────────────────────────────────────────

    static GameObject BuildRes(GameObject parent, LabPopupController ctrl)
    {
        var root = ContentRoot(parent, "ResultsPanel");

        Header(root, "РЕЗУЛЬТАТИ ВИМІРЮВАНЬ");
        Divider(root);
        Space(root, 14);

        // Таблиця
        var tableBox = Box(root, "TableBox", new Color(0.08f, 0.10f, 0.16f, 1f), 0,
                           expand: true);
        ctrl.tableText = Txt(tableBox, "TableText", "", 15,
                             TextAlignmentOptions.TopLeft, 0, expand: true,
                             color: TEXT_WHITE);

        Space(root, 10);
        Divider(root);
        Space(root, 8);

        // Підсумок
        var sumBox = Box(root, "SumBox", BG_HEADER, 50);
        ctrl.summaryText = Txt(sumBox, "SummaryText", "Середнє: —     Похибка: —",
                               19, TextAlignmentOptions.Center, 0,
                               expand: true, color: TEXT_ACCENT);

        Space(root, 10);
        ctrl.resultsBackBtn = BtnH(root, "BtnBack", "<< На початок", BTN_GREY, 46, 18);
        return root;
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    // Повноекранний кореневий контейнер панелі
    static GameObject ContentRoot(GameObject parent, string name)
    {
        var go = Go(parent, name);
        Stretch(go);
        go.AddComponent<Image>().color = BG_PANEL;
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.padding              = new RectOffset(28, 28, 0, 18);
        v.spacing              = 0;
        v.childAlignment       = TextAnchor.UpperCenter;
        v.childControlWidth    = true;
        v.childControlHeight   = true;
        v.childForceExpandWidth  = true;
        v.childForceExpandHeight = false;
        return go;
    }

    // Кольорова шапка панелі
    static TMP_Text Header(GameObject parent, string title)
    {
        var box = Go(parent, "Header");
        box.AddComponent<LayoutElement>().preferredHeight = 52;
        box.AddComponent<Image>().color = BG_HEADER;
        var t = Go(box, "Title");
        Stretch(t);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text      = title;
        tmp.fontSize  = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = TEXT_ACCENT;
        return tmp;
    }

    // Тонка розподільна лінія
    static void Divider(GameObject parent)
    {
        var go = Go(parent, "Divider");
        go.AddComponent<LayoutElement>().preferredHeight = 2;
        go.AddComponent<Image>().color = DIVIDER;
    }

    // Кольоровий блок-контейнер
    static GameObject Box(GameObject parent, string name, Color bg, int h,
        bool expand = false)
    {
        var go = Go(parent, name);
        var le = go.AddComponent<LayoutElement>();
        if (expand) le.flexibleHeight = 1;
        else        le.preferredHeight = h;
        go.AddComponent<Image>().color = bg;
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.padding              = new RectOffset(14, 14, 8, 8);
        v.childControlWidth    = true;
        v.childControlHeight   = true;
        v.childForceExpandWidth  = true;
        v.childForceExpandHeight = false;
        return go;
    }

    static GameObject HRow(GameObject parent, string name, int h)
    {
        var go = Go(parent, name);
        go.AddComponent<LayoutElement>().preferredHeight = h;
        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing              = 20;
        hlg.childControlWidth    = true;
        hlg.childControlHeight   = true;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        return go;
    }

    static TMP_Text Txt(GameObject parent, string name, string text,
        float size, TextAlignmentOptions align, int h,
        bool expand = false, Color? color = null)
    {
        var go = Go(parent, name);
        var le = go.AddComponent<LayoutElement>();
        if (expand) le.flexibleHeight = 1;
        else        le.preferredHeight = h;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = size;
        t.alignment = align;
        t.color     = color ?? TEXT_WHITE;
        return t;
    }

    static void Space(GameObject parent, int h)
    {
        Go(parent, "Space").AddComponent<LayoutElement>().preferredHeight = h;
    }

    // Кнопка що розтягується (HRow)
    static Button Btn(GameObject parent, string name, string label,
        Color color, float fontSize)
    {
        var go = Go(parent, name);
        go.AddComponent<Image>().color = color;
        var b = go.AddComponent<Button>();
        SetupButtonColors(b, color);
        BtnLabel(go, label, fontSize);
        return b;
    }

    // Кнопка з фіксованою висотою (VPanel)
    static Button BtnH(GameObject parent, string name, string label,
        Color color, int h, float fontSize)
    {
        var go = Go(parent, name);
        go.AddComponent<LayoutElement>().preferredHeight = h;
        go.AddComponent<Image>().color = color;
        var b = go.AddComponent<Button>();
        SetupButtonColors(b, color);
        BtnLabel(go, label, fontSize);
        return b;
    }

    static void BtnLabel(GameObject btn, string text, float fontSize)
    {
        var lgo = Go(btn, "Label");
        Stretch(lgo);
        var t = lgo.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color     = TEXT_WHITE;
    }

    static void SetupButtonColors(Button btn, Color normal)
    {
        var cb = btn.colors;
        cb.normalColor      = normal;
        cb.highlightedColor = Color.Lerp(normal, Color.white, 0.25f);
        cb.pressedColor     = Color.Lerp(normal, Color.black, 0.25f);
        cb.selectedColor    = normal;
        btn.colors          = cb;
    }

    static TMP_InputField MakeInput(GameObject parent, string name, string ph, int h)
    {
        var go = Go(parent, name);
        go.AddComponent<LayoutElement>().preferredHeight = h;
        go.AddComponent<Image>().color = BG_INPUT;
        var f = go.AddComponent<TMP_InputField>();

        // TextViewport — обов'язковий для TMP_InputField
        var viewport = Go(go, "Text Area");
        Stretch(viewport, new Vector2(10, 4));
        viewport.AddComponent<RectMask2D>();
        f.textViewport = viewport.GetComponent<RectTransform>();

        // Placeholder всередині viewport
        var pgo = Go(viewport, "Placeholder");
        Stretch(pgo);
        var pt = pgo.AddComponent<TextMeshProUGUI>();
        pt.text      = ph;
        pt.fontSize  = 22;
        pt.color     = TEXT_DIM;
        pt.fontStyle = FontStyles.Italic;
        pt.alignment = TextAlignmentOptions.Center;

        // Text всередині viewport
        var tgo = Go(viewport, "Text");
        Stretch(tgo);
        var tt = tgo.AddComponent<TextMeshProUGUI>();
        tt.fontSize  = 26;
        tt.color     = TEXT_ACCENT;
        tt.alignment = TextAlignmentOptions.Center;

        f.textComponent = tt;
        f.placeholder   = pt;
        f.contentType   = TMP_InputField.ContentType.DecimalNumber;
        return f;
    }

    // ── Base helpers ──────────────────────────────────────────────────────────

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
