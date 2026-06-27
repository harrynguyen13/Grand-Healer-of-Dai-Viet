using System.Collections.Generic;

public enum TreatmentResultType
{
    Perfect,                    // Đúng bệnh đúng thuốc
    RightDiseaseWrongMedicine,  // Đúng bệnh sai thuốc
    WrongDiseaseRightMedicine,  // Sai bệnh đúng thuốc
    Failed                      // Sai bệnh sai thuốc
}

public static class TreatmentEvaluator
{
    public static TreatmentResultType Evaluate(PatientCase patientCase)
    {
        bool isDiseaseCorrect = patientCase.selectedDisease == patientCase.realDisease;

        bool isMedicineCorrect = IsMedicineCorrect(
            patientCase.realDisease.correctHerbs,
            patientCase.selectedHerbs,
            patientCase.realDisease.medicineCorrectRate
        );

        if (isDiseaseCorrect && isMedicineCorrect)
            return TreatmentResultType.Perfect;

        if (isDiseaseCorrect && !isMedicineCorrect)
            return TreatmentResultType.RightDiseaseWrongMedicine;

        if (!isDiseaseCorrect && isMedicineCorrect)
            return TreatmentResultType.WrongDiseaseRightMedicine;

        return TreatmentResultType.Failed;
    }

    private static bool IsMedicineCorrect(List<HerbData> correctHerbs, List<HerbData> selectedHerbs, float requiredRate)
    {
        if (correctHerbs == null || correctHerbs.Count == 0)
            return false;

        int correctCount = 0;

        foreach (HerbData herb in correctHerbs)
        {
            if (selectedHerbs.Contains(herb))
            {
                correctCount++;
            }
        }

        float currentRate = (float)correctCount / correctHerbs.Count;

        return currentRate >= requiredRate;
    }
}