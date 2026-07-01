using UnityEngine;

[System.Serializable]
public class PatientVisitData
{
    public GameObject patientPrefab;
    public PatientCase patientCase;

    public PatientVisitData(GameObject patientPrefab, PatientCase patientCase)
    {
        this.patientPrefab = patientPrefab;
        this.patientCase = patientCase;
    }
}