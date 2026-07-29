using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public static class LabUISetup
{
    [MenuItem("Tools/Налаштувати Lab Popup")]
    static void MenuItem() => RunSetup();

    // Повертає true якщо щось змінилось, false якщо вже було налаштовано
    public static bool RunSetup()
    {
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("[LabUISetup] UIManager не знайдено на сцені!");
            return false;
        }

        GameObject menuPanel = uiManager.menuPanel;
        if (menuPanel == null)
        {
            Debug.LogError("[LabUISetup] UIManager.menuPanel не призначено!");
            return false;
        }

        var existingLabPopup = menuPanel.transform.Find("LabPopup");
        if (existingLabPopup != null)
        {
            // LabPopup є, але посилання могло не зберегтись — відновлюємо
            var existingCtrl = existingLabPopup.GetComponent<LabPopupController>();
            if (existingCtrl != null && uiManager.labPopupController == null)
            {
                var soFix = new SerializedObject(uiManager);
                soFix.FindProperty("labPopupController").objectReferenceValue = existingCtrl;
                soFix.ApplyModifiedProperties();
                EditorUtility.SetDirty(uiManager);
                Debug.Log("[LabUISetup] Відновлено посилання labPopupController → UIManager.");
                return true;
            }
            return false;
        }

        // ── LabPopup root ─────────────────────────────────────────────────
        var labPopupGO = new GameObject("LabPopup");
        Undo.RegisterCreatedObjectUndo(labPopupGO, "Create LabPopup");
        labPopupGO.transform.SetParent(menuPanel.transform, false);
        SetFullStretch(labPopupGO);

        var ctrl       = labPopupGO.AddComponent<LabPopupController>();
        ctrl.uiManager = uiManager;

        // ── Панелі ────────────────────────────────────────────────────────
        var mainPanel = CreatePanel(labPopupGO, "MainPanel",       new Color(0.10f, 0.10f, 0.15f, 0.97f));
        var infoPanel = CreatePanel(labPopupGO, "InfoPanel",       new Color(0.08f, 0.12f, 0.20f, 0.97f));
        var expPanel  = CreatePanel(labPopupGO, "ExperimentPanel", new Color(0.08f, 0.14f, 0.12f, 0.97f));
        var resPanel  = CreatePanel(labPopupGO, "ResultsPanel",    new Color(0.10f, 0.10f, 0.10f, 0.97f));

        infoPanel.SetActive(false);
        expPanel.SetActive(false);
        resPanel.SetActive(false);

        // ── Main Panel ────────────────────────────────────────────────────
        var nameText = CreateLabel(mainPanel, "NameText", "Назва приладу", 30,
            TextAlignmentOptions.Center, new Vector2(600, 60), new Vector2(0, 100));
        var infoBtn  = CreateButton(mainPanel, "BtnInfo",  "Інформація",       new Vector2(280, 56), new Vector2(-160, 0));
        var startBtn = CreateButton(mainPanel, "BtnStart", "Почати виконання", new Vector2(280, 56), new Vector2(160, 0));
        var closeBtn = CreateButton(mainPanel, "BtnClose", "✕ Закрити",        new Vector2(150, 44), new Vector2(0, -110));
        StyleButton(closeBtn, new Color(0.55f, 0.12f, 0.12f));

        ctrl.mainPanel = mainPanel;
        ctrl.nameText  = nameText;
        ctrl.infoBtn   = infoBtn.GetComponent<Button>();
        ctrl.startBtn  = startBtn.GetComponent<Button>();
        ctrl.closeBtn  = closeBtn.GetComponent<Button>();

        // ── Info Panel ────────────────────────────────────────────────────
        var descText   = CreateScrollText(infoPanel, "DescText");
        var infoBackBtn = CreateButton(infoPanel, "BtnInfoBack", "← Назад", new Vector2(160, 44), new Vector2(0, -130));

        ctrl.infoPanel   = infoPanel;
        ctrl.descText    = descText;
        ctrl.infoBackBtn = infoBackBtn.GetComponent<Button>();

        // ── Experiment Panel ──────────────────────────────────────────────
        var progressText      = CreateLabel(expPanel, "ProgressText",      "Вимір 1 з 5",  22, TextAlignmentOptions.Center, new Vector2(400, 38), new Vector2(0, 125));
        var measuredValueText = CreateLabel(expPanel, "MeasuredValueText", "—",             30, TextAlignmentOptions.Center, new Vector2(400, 50), new Vector2(0,  60));
        var inputField        = CreateInputField(expPanel, "InputField",   "Введіть значення...", new Vector2(300, 44), new Vector2(0, 0));
        var measureBtn        = CreateButton(expPanel, "BtnMeasure",  "Зробити вимір", new Vector2(220, 50), new Vector2(-120, -65));
        var saveBtn           = CreateButton(expPanel, "BtnSave",     "Зберегти",      new Vector2(160, 50), new Vector2( 110, -65));
        var checkBtn          = CreateButton(expPanel, "BtnCheck",    "Перевірити →",  new Vector2(220, 50), new Vector2(   0, -65));
        var expBackBtn        = CreateButton(expPanel, "BtnExpBack",  "← Назад",       new Vector2(140, 40), new Vector2(   0, -130));

        StyleButton(saveBtn,  new Color(0.15f, 0.50f, 0.20f));
        StyleButton(checkBtn, new Color(0.20f, 0.40f, 0.70f));
        checkBtn.SetActive(false);

        ctrl.experimentPanel   = expPanel;
        ctrl.progressText      = progressText;
        ctrl.measuredValueText = measuredValueText;
        ctrl.inputField        = inputField;
        ctrl.measureBtn        = measureBtn.GetComponent<Button>();
        ctrl.saveBtn           = saveBtn.GetComponent<Button>();
        ctrl.checkBtn          = checkBtn.GetComponent<Button>();
        ctrl.experimentBackBtn = expBackBtn.GetComponent<Button>();

        // ── Results Panel ─────────────────────────────────────────────────
        var tableText    = CreateLabel(resPanel, "TableText",   "",                        15, TextAlignmentOptions.TopLeft, new Vector2(620, 220), new Vector2(0, 40));
        var summaryText  = CreateLabel(resPanel, "SummaryText", "Середнє: —   Похибка: —", 20, TextAlignmentOptions.Center, new Vector2(620,  40), new Vector2(0, -90));
        var resBackBtn   = CreateButton(resPanel, "BtnResBack", "← На початок",            new Vector2(200, 44), new Vector2(0, -135));

        ctrl.resultsPanel   = resPanel;
        ctrl.tableText      = tableText;
        ctrl.summaryText    = summaryText;
        ctrl.resultsBackBtn = resBackBtn.GetComponent<Button>();

        // ── Призначити в UIManager ────────────────────────────────────────
        var so = new SerializedObject(uiManager);
        so.FindProperty("labPopupController").objectReferenceValue = ctrl;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(menuPanel);
        Debug.Log("[LabUISetup] Lab Popup створено і прив'язано.");
        return true;
    }

    // ── Хелпери ──────────────────────────────────────────────────────────────

    static GameObject CreatePanel(GameObject parent, string name, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        SetFullStretch(go);
        go.AddComponent<Image>().color = bg;
        return go;
    }

    static TMP_Text CreateLabel(GameObject parent, string name, string text, float size,
        TextAlignmentOptions align, Vector2 sizeDelta, Vector2 pos)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta       = sizeDelta;
        rt.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.alignment = align;
        tmp.color     = Color.white;
        return tmp;
    }

    static TMP_Text CreateScrollText(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.15f);
        rt.anchorMax = new Vector2(0.95f, 0.90f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = "Опис приладу...";
        tmp.fontSize  = 18;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        return tmp;
    }

    static GameObject CreateButton(GameObject parent, string name, string label,
        Vector2 sizeDelta, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = sizeDelta;
        rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = new Color(0.25f, 0.35f, 0.50f);
        go.AddComponent<Button>();

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var lrt = labelGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return go;
    }

    static TMP_InputField CreateInputField(GameObject parent, string name,
        string placeholder, Vector2 sizeDelta, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = sizeDelta;
        rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = new Color(0.90f, 0.90f, 0.90f);
        var field = go.AddComponent<TMP_InputField>();

        var phGO = new GameObject("Placeholder");
        phGO.transform.SetParent(go.transform, false);
        SetFullStretch(phGO, new Vector2(8, 4));
        var phTmp      = phGO.AddComponent<TextMeshProUGUI>();
        phTmp.text      = placeholder;
        phTmp.fontSize  = 16;
        phTmp.color     = new Color(0.5f, 0.5f, 0.5f);
        phTmp.fontStyle = FontStyles.Italic;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        SetFullStretch(txtGO, new Vector2(8, 4));
        var txtTmp     = txtGO.AddComponent<TextMeshProUGUI>();
        txtTmp.fontSize = 16;
        txtTmp.color    = Color.black;

        field.textComponent  = txtTmp;
        field.placeholder    = phTmp;
        field.contentType    = TMP_InputField.ContentType.DecimalNumber;
        return field;
    }

    static void StyleButton(GameObject btn, Color color) =>
        btn.GetComponent<Image>().color = color;

    static void SetFullStretch(GameObject go, Vector2? inset = null)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        var off = inset ?? Vector2.zero;
        rt.offsetMin =  off;
        rt.offsetMax = -off;
    }
}
