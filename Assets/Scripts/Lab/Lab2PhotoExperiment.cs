using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Лаб. №69 — Зняття вольт-амперних характеристик фотоелемента.
// Таблиця 4.1: α | cos α | I покази (мкА) | I записано (мкА) | ΔI
// Закон: I = I₀ · cos α
public class Lab2PhotoExperiment : LabExperimentBase
{
    // Кути діафрагми відповідно до таблиці 4.1
    static readonly float[] Angles = { 0f, 10f, 20f, 30f, 40f, 50f, 60f };

    public Lab2PhotoExperiment(LabData data) : base(data) { }

    // ── Генерація показу приладу ──────────────────────────────────────────────

    public override float GenerateMeasurement()
    {
        int   idx   = CurrentCount;
        float angle = idx < Angles.Length ? Angles[idx] : 0f;
        float cosA  = Mathf.Cos(angle * Mathf.Deg2Rad);
        float noise = Random.Range(-data.spread * 0.04f, data.spread * 0.04f);
        float value = data.baseValue * cosA + noise;
        return Mathf.Round(Mathf.Max(0f, value) * 100f) / 100f;
    }

    // ── Мітки ────────────────────────────────────────────────────────────────

    public override string GetCurrentLabel()
    {
        float angle = CurrentCount < Angles.Length ? Angles[CurrentCount] : 0f;
        return $"α = {angle:F0}°";
    }

    public override string GetLabel(int i)
    {
        float angle = i < Angles.Length ? Angles[i] : 0f;
        return $"α = {angle:F0}°";
    }

    public float GetAngleAt(int i) => i < Angles.Length ? Angles[i] : 0f;
    public float GetCosAt(int i)   => Mathf.Cos(GetAngleAt(i) * Mathf.Deg2Rad);

    // ── Обчислення результатів ────────────────────────────────────────────────

    public override LabResults Calculate()
    {
        var deviations = measurements
            .Select((m, i) => Mathf.Abs(m - generated[i]))
            .ToList();

        float mean      = measurements.Average();
        float sqSum     = measurements.Sum(m => (m - mean) * (m - mean));
        float uncertainty = measurements.Count > 1
            ? Mathf.Sqrt(sqSum / (measurements.Count - 1))
            : 0f;

        var labels = Enumerable.Range(0, measurements.Count)
            .Select(i => GetLabel(i))
            .ToList();

        return new LabResults
        {
            Labels       = labels,
            Generated    = new List<float>(generated),
            Measurements = new List<float>(measurements),
            Deviations   = deviations,
            Mean         = mean,
            Uncertainty  = uncertainty,
            Unit         = data.unit
        };
    }

    // ── Таблиця 4.1 (повна, 5 стовпців) ──────────────────────────────────────

    public override string BuildResultsTable(LabResults r)
    {
        var sb = new StringBuilder();

        // Заголовок таблиці 4.1
        sb.AppendLine("Таблиця 4.1");
        sb.AppendLine($"{"α",-8} {"cos α",-7} {"I покази",-10} {"I запис",-10} {"ΔI",-6}");
        sb.AppendLine($"{"°",-8} {"",-7} {"мкА",-10} {"мкА",-10} {"мкА",-6}");
        sb.AppendLine(new string('─', 44));

        for (int i = 0; i < r.Measurements.Count; i++)
        {
            float cosA  = GetCosAt(i);
            float angle = GetAngleAt(i);
            sb.AppendLine(
                $"{angle,-8:F0} {cosA,-7:F3} {r.Generated[i],-10:F3} {r.Measurements[i],-10:F3} {r.Deviations[i],-6:F3}");
        }

        return sb.ToString();
    }
}
