using UnityEngine;

public static class YThuUsageTracker
{
    private static bool isTrackingTreatment;
    private static int openCountInCurrentTreatment;

    public static int OpenCountInCurrentTreatment
    {
        get { return openCountInCurrentTreatment; }
    }

    public static bool IsTrackingTreatment
    {
        get { return isTrackingTreatment; }
    }

    /// <summary>
    /// Bắt đầu một ca khám mới.
    /// </summary>
    public static void BeginTreatmentTracking()
    {
        BeginTreatmentTracking(0);
    }

    /// <summary>
    /// Bắt đầu hoặc khôi phục theo dõi với số lần mở Y thư đã có.
    /// </summary>
    public static void BeginTreatmentTracking(int restoredOpenCount)
    {
        isTrackingTreatment = true;
        openCountInCurrentTreatment = Mathf.Max(0, restoredOpenCount);

        Debug.Log(
            "Bắt đầu theo dõi Y thư. Số lần mở được khôi phục: "
            + openCountInCurrentTreatment
        );
    }

    public static void RecordYThuOpened()
    {
        if (!isTrackingTreatment)
            return;

        openCountInCurrentTreatment++;

        Debug.Log(
            "Số lần mở Y thư trong ca này: "
            + openCountInCurrentTreatment
        );
    }

    public static int FinishTreatmentTrackingAndGetOpenCount()
    {
        if (!isTrackingTreatment)
            return openCountInCurrentTreatment;

        isTrackingTreatment = false;

        Debug.Log(
            "Kết thúc theo dõi Y thư. Số lần mở: "
            + openCountInCurrentTreatment
        );

        return openCountInCurrentTreatment;
    }

    public static void CancelTreatmentTracking()
    {
        isTrackingTreatment = false;
        openCountInCurrentTreatment = 0;

        Debug.Log("Đã hủy theo dõi Y thư cho ca chữa bệnh hiện tại.");
    }
}