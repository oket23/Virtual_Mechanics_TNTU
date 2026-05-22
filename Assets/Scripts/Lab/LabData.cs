using UnityEngine;

// Створюй через: Assets → Create → Lab → Дані приладу
// Один ScriptableObject = один прилад. Заповни поля в Inspector.
[CreateAssetMenu(fileName = "NewLabData", menuName = "Lab/Дані приладу")]
public class LabData : ScriptableObject
{
    [Header("Загальна інформація")]
    public string instrumentName = "Прилад";

    [TextArea(3, 10)]
    public string description = "Опис приладу, теорія та методика вимірювань.";

    [Header("Параметри вимірювань")]
    public float baseValue = 10f;
    public float spread = 0.5f;
    public int measurementCount = 5;
    public string unit = "м";

    [Header("Підказки для студентів")]
    public string formula = "";
    [TextArea(2, 5)]
    public string hints = "";
    public string externalUrl = "";
}
