using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AutoSetup
{
    static AutoSetup()
    {
        // Запускається після кожної компіляції скриптів
        EditorApplication.delayCall += () => TrySetup();
        EditorSceneManager.sceneOpened += (_, __) => TrySetup();
    }

    [MenuItem("Tools/⚡ Автоналаштування (запустити вручну)")]
    public static void RunManual() => TrySetup();

    static void TrySetup()
    {
        // Не запускаємо під час Play Mode
        if (EditorApplication.isPlaying) return;

        Debug.Log("[AutoSetup] Перевіряю сцену...");

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning("[AutoSetup] Сцена не завантажена. Відкрий SampleScene і запусти Tools → ⚡ Автоналаштування.");
            return;
        }

        Debug.Log($"[AutoSetup] Сцена: {scene.name}");

        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("[AutoSetup] UIManager не знайдено. Переконайся що SampleScene відкрита.");
            return;
        }

        Debug.Log("[AutoSetup] UIManager знайдено. Починаю налаштування...");

        if (uiManager.menuPanel == null)
        {
            Debug.LogError("[AutoSetup] UIManager.menuPanel = null. Призначте поле menuPanel в Inspector на об'єкті UIManager.");
            return;
        }

        bool changed = false;
        changed |= TooltipSetup.RunSetup();
        changed |= LabUISetup.RunSetup();
        changed |= EnsureLabDataAndAssign();

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[AutoSetup] ✓ Готово! Натисни Ctrl+S і потім Play.");
        }
        else
        {
            Debug.Log("[AutoSetup] Все вже налаштовано, змін не потрібно.");
        }
    }

    static bool EnsureLabDataAndAssign()
    {
        LabData data = EnsureDefaultLabData();
        return AssignLabData(data);
    }

    static LabData EnsureDefaultLabData()
    {
        const string path = "Assets/Resources/LabData/DefaultLabData.asset";

        var existing = AssetDatabase.LoadAssetAtPath<LabData>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/LabData"))
            AssetDatabase.CreateFolder("Assets/Resources", "LabData");

        var data = ScriptableObject.CreateInstance<LabData>();
        data.instrumentName   = "Прилад";
        data.description      = "Замініть цей текст описом приладу та теорією вимірювань.\n\n" +
                                 "Для кожного приладу створіть окремий ScriptableObject:\n" +
                                 "Assets → Create → Lab → Дані приладу";
        data.baseValue        = 9.81f;
        data.spread           = 0.30f;
        data.measurementCount = 5;
        data.unit             = "м/с²";

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AutoSetup] Створено LabData: {path}");
        return data;
    }

    static bool AssignLabData(LabData data)
    {
        var objects = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        bool changed = false;

        foreach (var obj in objects)
        {
            if (obj.labData != null) continue;
            obj.labData     = data;
            obj.tooltipText = $"[E]  {data.instrumentName}";
            EditorUtility.SetDirty(obj);
            changed = true;
            Debug.Log($"[AutoSetup] LabData → {obj.gameObject.name}");
        }

        return changed;
    }
}
