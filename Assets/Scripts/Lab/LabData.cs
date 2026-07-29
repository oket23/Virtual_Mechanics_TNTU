using UnityEngine;

[CreateAssetMenu(fileName = "NewLabData", menuName = "Lab/Дані приладу")]
public class LabData : ScriptableObject
{
    public enum ExperimentType { Default, PhotoEffect }

    [Header("Загальна інформація")]
    public string instrumentName = "Прилад";
    [TextArea(3, 10)]
    public string description = "Опис приладу, теорія та методика вимірювань.";

    [Header("Тип експерименту")]
    public ExperimentType experimentType = ExperimentType.Default;

    [Header("Параметри вимірювань")]
    public float baseValue = 10f;
    public float spread    = 0.5f;
    public int   measurementCount = 5;
    public string unit = "м";

    [Header("Підказки для студентів")]
    public string formula = "";
    [TextArea(2, 5)]
    public string hints = "";
    public string externalUrl = "";
}
