using System.Linq;
using UnityEngine;

// Реалізація за замовчуванням: середнє + середнє абсолютне відхилення.
//
// Щоб додати формули для нового приладу:
//   1. Створи файл MyPriladExperiment.cs
//   2. успадкуй від LabExperimentBase
//   3. Перевизнач Calculate() зі своїми формулами
//   4. У LabPopupController.Open() або в окремому полі LabData вкажи тип класу
public class DefaultLabExperiment : LabExperimentBase
{
    public DefaultLabExperiment(LabData data) : base(data) { }

    public override LabResults Calculate()
    {
        float mean = measurements.Average();
        var deviations = measurements.Select(m => Mathf.Abs(m - mean)).ToList();
        float uncertainty = deviations.Average();

        return new LabResults
        {
            Generated    = new(generated),
            Measurements = new(measurements),
            Deviations   = deviations,
            Mean         = mean,
            Uncertainty  = uncertainty,
            Unit         = data.unit
        };
    }
}
