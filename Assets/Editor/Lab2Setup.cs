using UnityEngine;
using UnityEditor;

public static class Lab2Setup
{
    const string ASSET_PATH = "Assets/Resources/LabData/Lab2_Фотоелемент.asset";

    [MenuItem("Tools/Lab 2 — Оновити LabData фотоелемента")]
    public static void RunSetup()
    {
        EnsureFolders();

        LabData data = AssetDatabase.LoadAssetAtPath<LabData>(ASSET_PATH);
        bool isNew = data == null;
        if (isNew)
            data = ScriptableObject.CreateInstance<LabData>();

        Fill(data);

        if (isNew)
            AssetDatabase.CreateAsset(data, ASSET_PATH);

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Lab2Setup] LabData збережено: {ASSET_PATH}");

        AssignToInteractables(data);
    }

    static void Fill(LabData d)
    {
        d.instrumentName   = "Вакуумний фотоелемент";
        d.baseValue        = 5.2f;
        d.spread           = 1.4f;
        d.measurementCount = 7;
        d.unit             = "мкА";

        d.description =
            "ЛАБОРАТОРНА РОБОТА №69\n" +
            "ЗНЯТТЯ ВОЛЬТ-АМПЕРНИХ ХАРАКТЕРИСТИК І ВИЗНАЧЕННЯ СТРУМУ НАСИЧЕННЯ ФОТОЕЛЕМЕНТА\n\n" +
            "Зовнішній фотоефект — явище вибивання електронів з поверхні речовини під дією " +
            "електромагнітного випромінювання. Відкрив О.Г. Столєтов у 1887–1889 рр., " +
            "теорію розробив А. Ейнштейн у 1905 р.\n\n" +
            "ЗАКОНИ ЗОВНІШНЬОГО ФОТОЕФЕКТУ:\n" +
            "1. Кількість електронів, вибитих за 1 с, пропорційна інтенсивності світла.\n" +
            "2. Максимальна кінетична енергія фотоелектронів залежить тільки від частоти " +
            "світла, але не від його інтенсивності.\n" +
            "3. Фотоефект не відбувається, якщо частота світла менша за червону межу (ν < ν₀).\n\n" +
            "Вакуумний фотоелемент — прилад, що перетворює потік фотонів у електричний струм " +
            "на основі зовнішнього фотоефекту. Складається з анода і катода, між якими " +
            "прикладається напруга. Катод освітлюється монохроматичним світлом, вибиті " +
            "електрони утворюють фотострум.\n\n" +
            "Вольт-амперна характеристика показує залежність фотоструму I від напруги U " +
            "між анодом і катодом. При певній негативній напрузі U_зат фотострум = 0 — це " +
            "затримуючий потенціал. За ним визначають максимальну кінетичну енергію електронів.";

        d.formula =
            "Рівняння Ейнштейна:\n" +
            "hν = A + mv²_max / 2\n\n" +
            "Через затримуючий потенціал:\n" +
            "eU_зат = hν - A\n\n" +
            "Фотострум насичення:\n" +
            "I = ρ · Φ\n\n" +
            "Кутова залежність:\n" +
            "I ~ cos α";

        d.hints =
            "ПОРЯДОК ВИКОНАННЯ:\n" +
            "1. Зберіть схему згідно рисунку (джерело живлення, фотоелемент Ф, " +
            "гальванометр Г, вольтметр V).\n" +
            "2. Встановіть діафрагму під кутом α = 0°.\n" +
            "3. Натисніть 'Зробити вимір' — прочитайте покази гальванометра (мкА).\n" +
            "4. Введіть значення в поле і натисніть 'Записати'.\n" +
            "5. Повторіть для 7 різних положень діафрагми.\n" +
            "6. За результатами побудуйте графік I = f(cos α).\n\n" +
            "УВАГА: Напруга живлення не повинна перевищувати 6,5 В!\n" +
            "Від'єднуйте діафрагму від джерела плавно (0 → -6,5 В).";

        d.externalUrl = "";
    }

    static void AssignToInteractables(LabData data)
    {
        var objects = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        bool assigned = false;

        foreach (var obj in objects)
        {
            string n = obj.gameObject.name.ToLower();
            // Призначаємо якщо назва містить lab2, photo, фото або якщо labData ще не призначено
            bool isLab2 = n.Contains("lab2") || n.Contains("lab_2") ||
                          n.Contains("photo") || n.Contains("фото") || n.Contains("фотоелемент");
            if (!isLab2) continue;

            obj.labData     = data;
            obj.tooltipText = "[E]  Вакуумний фотоелемент";
            EditorUtility.SetDirty(obj);
            assigned = true;
            Debug.Log($"[Lab2Setup] LabData → {obj.gameObject.name}");
        }

        if (!assigned)
            Debug.LogWarning("[Lab2Setup] Не знайдено об'єкта Lab2 у сцені. " +
                             "Призначте LabData вручну: Assets/Resources/LabData/Lab2_Фотоелемент.asset");
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/LabData"))
            AssetDatabase.CreateFolder("Assets/Resources", "LabData");
    }
}
