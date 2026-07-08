using UnityEngine;

public static class YThuUsageTracker
{
    private static bool isTrackingTreatment = false;
    private static int openCountInCurrentTreatment = 0;

    public static int OpenCountInCurrentTreatment
    {
        get { return openCountInCurrentTreatment; }
    }

    public static bool IsTrackingTreatment
    {
        get { return isTrackingTreatment; }
    }

    public static void BeginTreatmentTracking()
    {
        isTrackingTreatment = true;
        openCountInCurrentTreatment = 0;

        Debug.Log("Bắt đầu theo dõi số lần mở Y thư trong ca chữa bệnh.");
    }

    public static void RecordYThuOpened()
    {
        if (!isTrackingTreatment)
            return;

        openCountInCurrentTreatment++;

        Debug.Log("Số lần mở Y thư trong ca này: " + openCountInCurrentTreatment);
    }

    public static int FinishTreatmentTrackingAndGetOpenCount()
    {
        if (!isTrackingTreatment)
            return 0;

        isTrackingTreatment = false;

        Debug.Log("Kết thúc theo dõi Y thư. Số lần mở: " + openCountInCurrentTreatment);

        return openCountInCurrentTreatment;
    }

    public static void CancelTreatmentTracking()
    {
        isTrackingTreatment = false;
        openCountInCurrentTreatment = 0;

        Debug.Log("Đã hủy theo dõi Y thư cho ca chữa bệnh hiện tại.");
    }
}