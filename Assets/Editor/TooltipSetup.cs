using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public static class TooltipSetup
{
    [MenuItem("Tools/Налаштувати підказку (Tooltip)")]
    static void MenuItem() => RunSetup();

    // Повертає true якщо щось змінилось, false якщо вже було налаштовано
    public static bool RunSetup()
    {
        if (GameObject.Find("TooltipPanel") != null) return false;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
        }

        GameObject panel = new GameObject("TooltipPanel");
        Undo.RegisterCreatedObjectUndo(panel, "Create TooltipPanel");
        panel.transform.SetParent(canvas.transform, false);

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f);

        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(0.5f, 0f);
        panelRect.anchorMax        = new Vector2(0.5f, 0f);
        panelRect.pivot            = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 40f);
        panelRect.sizeDelta        = new Vector2(400f, 50f);

        var labelGO = new GameObject("TooltipLabel");
        labelGO.transform.SetParent(panel.transform, false);
        var label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text      = "Підказка";
        label.fontSize  = 20;
        label.alignment = TextAlignmentOptions.Center;
        label.color     = Color.white;

        var labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 0f);
        labelRect.offsetMax = new Vector2(-8f, 0f);

        panel.SetActive(false);

        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            var so = new SerializedObject(uiManager);
            so.FindProperty("tooltipPanel").objectReferenceValue = panel;
            so.FindProperty("tooltipLabel").objectReferenceValue = label;
            so.ApplyModifiedProperties();
        }

        PlayerInteract playerInteract = Object.FindFirstObjectByType<PlayerInteract>();
        if (playerInteract != null && uiManager != null)
        {
            var so = new SerializedObject(playerInteract);
            so.FindProperty("uiManager").objectReferenceValue = uiManager;
            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("[TooltipSetup] TooltipPanel створено.");
        return true;
    }
}
