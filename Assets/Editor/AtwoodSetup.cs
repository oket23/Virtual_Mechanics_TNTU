using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AtwoodSetup
{
    const string ASSET_PATH = "Assets/Resources/LabData/AtwoodLabData.asset";

    // ── Створити / оновити asset та спробувати автопризначення ────────────────
    [MenuItem("Tools/Atwood — Створити / оновити AtwoodLabData")]
    public static void RunSetup()
    {
        AtwoodLabData data = EnsureAsset();
        int count = AutoAssign(data);

        if (count == 0)
        {
            // Перелічити всі InteractableObject у сцені щоб користувач знав назви
            var all = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
            if (all.Length == 0)
            {
                Debug.LogWarning("[AtwoodSetup] У сцені немає жодного InteractableObject. " +
                    "Додай компонент InteractableObject до моделі машини Атвуда, " +
                    "потім запусти: Tools → Atwood — Призначити до виділеного об'єкта");
            }
            else
            {
                var names = new System.Text.StringBuilder();
                foreach (var o in all) names.Append($"\n  • {o.gameObject.name}");
                Debug.LogWarning(
                    "[AtwoodSetup] Авто-призначення не вдалось — жоден об'єкт не містить " +
                    "ключових слів (atwood / атвуд / pulley / блок / lr2 / лр2 / lab2 / ЛР).\n" +
                    "Знайдені InteractableObject у сцені:" + names +
                    "\n\nВибери потрібний в Hierarchy і запусти:\n" +
                    "Tools → Atwood — Призначити до виділеного об'єкта");
            }
        }
    }

    // ── Призначити до об'єкта, виділеного в Hierarchy ─────────────────────────
    [MenuItem("Tools/Atwood — Призначити до виділеного об'єкта")]
    public static void AssignToSelected()
    {
        AtwoodLabData data = EnsureAsset();

        var selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("[AtwoodSetup] Нічого не виділено. " +
                "Вибери об'єкт машини Атвуда в Hierarchy і повтори.");
            return;
        }

        int count = 0;
        foreach (var go in selected)
        {
            var io = go.GetComponent<InteractableObject>();
            if (io == null)
                io = go.GetComponentInChildren<InteractableObject>();

            if (io == null)
            {
                Debug.LogWarning($"[AtwoodSetup] '{go.name}' не має InteractableObject — пропускаємо.");
                continue;
            }

            Apply(io, data);
            count++;
        }

        if (count > 0)
            Debug.Log($"[AtwoodSetup] Призначено до {count} об'єкт(ів). Тепер натисни Play.");
    }

    [MenuItem("Tools/Atwood — Призначити до виділеного об'єкта", true)]
    static bool AssignToSelectedValidate() => Selection.gameObjects.Length > 0;

    // ── Helpers ───────────────────────────────────────────────────────────────

    static AtwoodLabData EnsureAsset()
    {
        EnsureFolders();
        AtwoodLabData data = AssetDatabase.LoadAssetAtPath<AtwoodLabData>(ASSET_PATH);
        bool isNew = data == null;
        if (isNew) data = ScriptableObject.CreateInstance<AtwoodLabData>();
        Fill(data);
        if (isNew) AssetDatabase.CreateAsset(data, ASSET_PATH);
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AtwoodSetup] AtwoodLabData збережено: {ASSET_PATH}");
        return data;
    }

    static int AutoAssign(AtwoodLabData data)
    {
        var objects = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var obj in objects)
        {
            if (!IsAtwoodName(obj.gameObject.name)) continue;
            Apply(obj, data);
            count++;
        }
        return count;
    }

    static bool IsAtwoodName(string name)
    {
        string n = name.ToLower();
        return n.Contains("atwood")  || n.Contains("атвуд")  ||
               n.Contains("pulley")  || n.Contains("блок")   ||
               n.Contains("machine") || n.Contains("машина") ||
               n.Contains("lr2")     || n.Contains("лр2")    ||
               n.Contains("lab2")    || n.Contains("lab_2")  ||
               n.Contains("лр_2")    || n.Contains("лабораторна_2");
    }

    static void Apply(InteractableObject io, AtwoodLabData data)
    {
        io.atwoodLabData = data;
        io.tooltipText   = "[E]  Машина Атвуда";
        // Якщо на цьому ж об'єкті стояв старий labData — прибираємо,
        // бо інакше Interact() відкриє стару лабораторію замість Atwood.
        if (io.labData != null)
        {
            Debug.LogWarning($"[AtwoodSetup] '{io.gameObject.name}': labData очищено " +
                $"(було: {io.labData.name}) — atwoodLabData має пріоритет.");
            io.labData = null;
        }
        EditorUtility.SetDirty(io);
        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(io.gameObject.scene);
        Debug.Log($"[AtwoodSetup] AtwoodLabData → {io.gameObject.name}");
    }

    static void Fill(AtwoodLabData d)
    {
        d.s1Min       = 118f;    d.s1Max    = 123f;    d.s1SysError = 0.5f;
        d.s2Min       = 294f;    d.s2Max    = 302f;    d.s2SysError = 0.5f;
        d.t2Min       = 1.0544f; d.t2Max    = 1.2577f; d.t2SysError = 0.0005f;
        d.massM       = 61.8f;
        d.massM1      = 10.0f;
        d.gravG       = 9.81f;
        d.externalUrl = "";

        const string imgPath = "Assets/Labs/Lab_2/setup_лаб2_2026.webp";
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(imgPath);
        if (tex != null) d.schemeImage = tex;
        else Debug.LogWarning($"[AtwoodSetup] Зображення не знайдено: {imgPath}");

        d.quizQuestions = new AtwoodLabData.QuizQuestion[]
        {
            new AtwoodLabData.QuizQuestion
            {
                question     = "Що досліджується в лабораторній роботі «Машина Атвуда»?",
                options      = new[]
                {
                    "Рівноприскорений рух та перевірка другого закону Ньютона",
                    "Закон збереження імпульсу при пружному ударі",
                    "Коливання математичного маятника",
                    "Визначення коефіцієнта тертя ковзання"
                },
                correctIndex = 0
            },
            new AtwoodLabData.QuizQuestion
            {
                question     = "Що таке машина Атвуда?",
                options      = new[]
                {
                    "Прилад для вимірювання сили тяжіння",
                    "Два тягарці однакової маси, з'єднані ниткою через нерухомий блок",
                    "Пристрій для визначення швидкості звуку",
                    "Пристрій для визначення коефіцієнта пружності"
                },
                correctIndex = 1
            },
            new AtwoodLabData.QuizQuestion
            {
                question     = "Яка теоретична формула прискорення в машині Атвуда?",
                options      = new[]
                {
                    "ā = m₁ · g / (2m + m₁)",
                    "ā = S₂² / (2 · S₁ · t₂²)",
                    "ā = (m₁ + m) · g / 2",
                    "ā = 2S · g / v²"
                },
                correctIndex = 0
            },
            new AtwoodLabData.QuizQuestion
            {
                question     = "Яка формула розрахунку прискорення за дослідними даними?",
                options      = new[]
                {
                    "ā = S₂ · t₂ / (2 · S₁)",
                    "ā = S₂² / (2 · S₁ · t₂²)",
                    "ā = 2 · S₁ / (S₂ · t₂²)",
                    "ā = (S₂ - S₁) / t₂²"
                },
                correctIndex = 1
            }
        };
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/LabData"))
            AssetDatabase.CreateFolder("Assets/Resources", "LabData");
    }
}
