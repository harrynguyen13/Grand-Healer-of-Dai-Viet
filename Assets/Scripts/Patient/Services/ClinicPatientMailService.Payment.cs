using UnityEngine;

public static partial class ClinicPatientMailService
{
    private static void PayMedicineMoneyNow(
        int medicinePayment
    )
    {
        if (medicinePayment <= 0)
        {
            return;
        }

        if (PlayerEconomy.Instance == null)
        {
            Debug.LogWarning(
                "Không tìm thấy PlayerEconomy. "
                + "Không thể cộng tiền thuốc ngay."
            );

            return;
        }

        PlayerEconomy.Instance.AddMoney(
            medicinePayment
        );

        Debug.Log(
            "Bệnh nhân đã trả tiền thuốc ngay: "
            + medicinePayment
        );
    }
}