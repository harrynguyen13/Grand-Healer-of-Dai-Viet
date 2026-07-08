using UnityEngine;

public static class ClinicYThuUsageRewardService
{
    private const int NoUseBonusMoney = 5;
    private const int PenaltyPerExtraOpen = 5;

    public static void BeginTracking()
    {
        YThuUsageTracker.BeginTreatmentTracking();
    }

    public static void CancelTracking()
    {
        YThuUsageTracker.CancelTreatmentTracking();
    }

    public static void ApplyRewardOrPenalty(PendingPatientMailData mailData)
    {
        if (mailData == null)
            return;

        int openCount = YThuUsageTracker.FinishTreatmentTrackingAndGetOpenCount();
        int moneyDelta = CalculateMoneyDelta(openCount);

        mailData.moneyDelta += moneyDelta;
        mailData.yThuUsageNote = BuildYThuUsageNote(openCount, moneyDelta);

        Debug.Log("Số lần mở Y thư trong ca này: " + openCount);
        Debug.Log("Điều chỉnh tiền do dùng Y thư: " + moneyDelta);
        Debug.Log("Ghi chú Y thư: " + mailData.yThuUsageNote);
        Debug.Log("Tổng tiền trong thư sau điều chỉnh Y thư: " + mailData.moneyDelta);
    }

    private static int CalculateMoneyDelta(int openCount)
    {
        if (openCount <= 0)
            return NoUseBonusMoney;

        if (openCount == 1)
            return 0;

        return -(openCount - 1) * PenaltyPerExtraOpen;
    }

    private static string BuildYThuUsageNote(int openCount, int moneyDelta)
    {
        if (openCount <= 0)
        {
            return "(Chú ý: bạn không mở Y thư trong lúc chữa bệnh, được thưởng thêm "
                + Mathf.Abs(moneyDelta)
                + " xu.)";
        }

        if (openCount == 1)
        {
            return "(Chú ý: bạn đã mở Y thư 1 lần, không bị khấu trừ xu.)";
        }

        return "(Chú ý: bạn đã mở Y thư "
            + openCount
            + " lần, bị khấu trừ "
            + Mathf.Abs(moneyDelta)
            + " xu.)";
    }
}