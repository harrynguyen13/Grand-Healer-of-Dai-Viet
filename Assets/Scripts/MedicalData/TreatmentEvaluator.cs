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
        if (patientCase == null)
            return TreatmentResultType.Failed;

        if (patientCase.realDisease == null)
            return TreatmentResultType.Failed;

        bool isDiseaseCorrect = patientCase.selectedDisease == patientCase.realDisease;

        bool isMedicineCorrect = IsMedicineCorrect(
            patientCase.realDisease.requiredHerbs,
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

    private static bool IsMedicineCorrect(
        List<RequiredHerbAmount> requiredHerbs,
        List<HerbData> selectedHerbs,
        float requiredRate
    )
    {
        if (requiredHerbs == null || requiredHerbs.Count == 0)
            return false;

        if (selectedHerbs == null || selectedHerbs.Count == 0)
            return false;

        int correctRequiredCount = 0;

        foreach (RequiredHerbAmount required in requiredHerbs)
        {
            if (required == null || required.herb == null)
                continue;

            int selectedAmount = CountSelectedHerb(selectedHerbs, required.herb);

            if (selectedAmount >= required.amount)
            {
                correctRequiredCount++;
            }
        }

        float currentRate = (float)correctRequiredCount / requiredHerbs.Count;

        return currentRate >= requiredRate;
    }

    private static int CountSelectedHerb(List<HerbData> selectedHerbs, HerbData targetHerb)
    {
        int count = 0;

        foreach (HerbData selectedHerb in selectedHerbs)
        {
            if (selectedHerb == targetHerb)
            {
                count++;
            }
        }

        return count;
    }
}