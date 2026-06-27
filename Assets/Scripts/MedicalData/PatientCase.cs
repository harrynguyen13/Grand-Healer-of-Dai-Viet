using System.Collections.Generic;

[System.Serializable]
public class PatientCase
{
    public DiseaseData realDisease;
    public DiseaseData selectedDisease;

    public List<HerbData> selectedHerbs = new List<HerbData>();

    public bool hasAsked;
    public bool hasPulseChecked;
    public bool hasGivenMedicine;

    public PatientCase(DiseaseData disease)
    {
        realDisease = disease;
    }
}