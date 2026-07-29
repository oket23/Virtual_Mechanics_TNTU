using UnityEngine;

[CreateAssetMenu(fileName = "AtwoodLabData", menuName = "Lab/Машина Атвуда")]
public class AtwoodLabData : ScriptableObject
{
    [Header("S₁ — шлях рівноприскореного руху (мм)")]
    public float s1Min      = 118f;
    public float s1Max      = 123f;
    public float s1SysError = 0.5f;

    [Header("S₂ — шлях рівномірного руху (мм)")]
    public float s2Min      = 294f;
    public float s2Max      = 302f;
    public float s2SysError = 0.5f;

    [Header("t₂ — час проходження S₂ (с)")]
    public float t2Min      = 1.0544f;
    public float t2Max      = 1.2577f;
    public float t2SysError = 0.0005f;

    [Header("Фіксовані значення")]
    public float massM  = 61.8f;    // кожний тягарець, г
    public float massM1 = 10.0f;    // перевантаження, г
    public float gravG  = 9.81f;    // м/с²

    [Header("Посилання")]
    public string externalUrl = "";

    [Header("Зображення схеми")]
    public Texture2D schemeImage;
}
