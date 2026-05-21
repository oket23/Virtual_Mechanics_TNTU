using System.Collections.Generic;

public class LabResults
{
    public List<float> Generated;    // покази приладу (згенеровані)
    public List<float> Measurements; // введені значення гравця
    public List<float> Deviations;   // |введене - середнє|
    public float Mean;
    public float Uncertainty;
    public string Unit;
}
