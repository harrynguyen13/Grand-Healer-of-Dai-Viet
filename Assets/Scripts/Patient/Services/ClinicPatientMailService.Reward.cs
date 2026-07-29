using UnityEngine;

public static partial class ClinicPatientMailService
{
    private static int CalculateReputationChange(
        int diseaseLevel,
        bool diagnosisCorrect,
        bool prescriptionCorrect
    )
    {
        if (
            diagnosisCorrect
            && prescriptionCorrect
        )
        {
            Debug.Log(
                "Kết quả: ĐÚNG BỆNH + ĐÚNG THUỐC."
            );

            return GetCorrectTreatmentReward(
                diseaseLevel
            );
        }

        if (
            diagnosisCorrect
            && !prescriptionCorrect
        )
        {
            Debug.Log(
                "Kết quả: ĐÚNG BỆNH nhưng SAI THUỐC."
            );

            return -GetWrongPrescriptionPenalty(
                diseaseLevel
            );
        }

        if (
            !diagnosisCorrect
            && prescriptionCorrect
        )
        {
            Debug.Log(
                "Kết quả: SAI BỆNH nhưng "
                + "THUỐC ĐÚNG BỆNH THẬT."
            );

            return -GetWrongDiagnosisPenalty(
                diseaseLevel
            );
        }

        Debug.Log(
            "Kết quả: SAI BỆNH + SAI THUỐC."
        );

        return -GetWrongTreatmentPenalty(
            diseaseLevel
        );
    }

    private static int GetCorrectTreatmentReward(
        int diseaseLevel
    )
    {
        if (diseaseLevel <= 1)
        {
            return 10;
        }

        if (diseaseLevel == 2)
        {
            return 15;
        }

        if (diseaseLevel == 3)
        {
            return 22;
        }

        if (diseaseLevel == 4)
        {
            return 30;
        }

        return 45;
    }

    private static int
        GetWrongPrescriptionPenalty(
            int diseaseLevel
        )
    {
        if (diseaseLevel <= 1)
        {
            return 3;
        }

        if (diseaseLevel == 2)
        {
            return 5;
        }

        if (diseaseLevel == 3)
        {
            return 8;
        }

        if (diseaseLevel == 4)
        {
            return 12;
        }

        return 18;
    }

    private static int
        GetWrongDiagnosisPenalty(
            int diseaseLevel
        )
    {
        if (diseaseLevel <= 1)
        {
            return 2;
        }

        if (diseaseLevel == 2)
        {
            return 4;
        }

        if (diseaseLevel == 3)
        {
            return 6;
        }

        if (diseaseLevel == 4)
        {
            return 9;
        }

        return 14;
    }

    private static int
        GetWrongTreatmentPenalty(
            int diseaseLevel
        )
    {
        if (diseaseLevel <= 1)
        {
            return 5;
        }

        if (diseaseLevel == 2)
        {
            return 8;
        }

        if (diseaseLevel == 3)
        {
            return 12;
        }

        if (diseaseLevel == 4)
        {
            return 18;
        }

        return 25;
    }
}