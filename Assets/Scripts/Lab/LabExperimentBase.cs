using System.Collections.Generic;
using UnityEngine;

// Базовий клас для будь-якої лабораторної роботи.
// Для нового приладу: успадкуй і перевизнач Calculate().
public abstract class LabExperimentBase
{
    protected LabData data;
    protected List<float> generated = new();    // покази приладу
    protected List<float> measurements = new(); // введені гравцем

    protected LabExperimentBase(LabData data) => this.data = data;

    public float GenerateMeasurement() =>
        data.baseValue + Random.Range(-data.spread, data.spread);

    public void AddMeasurementPair(float shown, float entered)
    {
        generated.Add(shown);
        measurements.Add(entered);
    }

    public bool IsComplete => measurements.Count >= data.measurementCount;
    public int CurrentCount  => measurements.Count;
    public int MeasuredCount => measurements.Count;
    public int TotalCount    => data.measurementCount;
    public float GetGenerated(int i)   => generated[i];
    public float GetMeasurement(int i) => measurements[i];

    // Точка розширення: вкажи свої формули в класі-нащадку
    public abstract LabResults Calculate();
}
