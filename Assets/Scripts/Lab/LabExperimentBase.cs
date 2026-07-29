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

    public virtual float GenerateMeasurement() =>
        data.baseValue + Random.Range(-data.spread, data.spread);

    public void AddMeasurementPair(float shown, float entered)
    {
        generated.Add(shown);
        measurements.Add(entered);
    }

    public bool IsComplete   => measurements.Count >= data.measurementCount;
    public int CurrentCount  => measurements.Count;
    public int MeasuredCount => measurements.Count;
    public int TotalCount    => data.measurementCount;
    public float GetGenerated(int i)   => generated[i];
    public float GetMeasurement(int i) => measurements[i];

    public virtual string GetCurrentLabel() => "";
    public virtual string GetLabel(int i)   => $"{i + 1}";

    // Будує рядок таблиці результатів. null = використовується стандартна логіка контролера.
    public virtual string BuildResultsTable(LabResults r) => null;

    public abstract LabResults Calculate();
}
