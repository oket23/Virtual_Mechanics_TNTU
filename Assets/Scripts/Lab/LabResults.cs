using System.Collections.Generic;

public class LabResults
{
    public List<string> Labels;       // мітка умови для кожного виміру (напр. "α = 30°")
    public List<float>  Generated;    // покази приладу (згенеровані)
    public List<float>  Measurements; // введені значення гравця
    public List<float>  Deviations;   // |введене - показ приладу|
    public float Mean;
    public float Uncertainty;
    public string Unit;
}
