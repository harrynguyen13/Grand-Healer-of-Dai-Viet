
using UnityEngine;

public static class ClinicYThuUsageRewardService
{
    private const int NoUseBonusMoney = 5;
    private const int PenaltyPerExtraOpen = 5;

    /// <summary>
    /// Bắt đầu theo dõi một ca khám mới.
    /// Số lần mở Y thư sẽ bắt đầu từ 0.
    /// </summary>
    public static void BeginTracking()
    {
        YThuUsageTracker.BeginTreatmentTracking();
    }

    /// <summary>
    /// Khôi phục theo dõi một ca khám đang làm dở.
    /// Số lần mở Y thư trước khi đổi scene sẽ được giữ lại.
    /// </summary>
    public static void BeginTracking(
        int restoredOpenCount
    )
    {
        YThuUsageTracker.BeginTreatmentTracking(
            restoredOpenCount
        );
    }

    /// <summary>
    /// Lấy số lần mở Y thư hiện tại để lưu trước khi đổi scene.
    /// </summary>
    public static int GetCurrentOpenCount()
    {
        return
            YThuUsageTracker
                .OpenCountInCurrentTreatment;
    }

    /// <summary>
    /// Kiểm tra hiện tại có đang theo dõi ca khám hay không.
    /// </summary>
    public static bool IsTracking()
    {
        return
            YThuUsageTracker
                .IsTrackingTreatment;
    }

    /// <summary>
    /// Hủy theo dõi và xóa số lần mở hiện tại.
    /// Chỉ gọi sau khi đã lưu số lần mở vào phiên khám dở
    /// hoặc khi ca khám bị hủy hoàn toàn.
    /// </summary>
    public static void CancelTracking()
    {
        YThuUsageTracker.CancelTreatmentTracking();
    }

    public static void ApplyRewardOrPenalty(
        PendingPatientMailData mailData
    )
    {
        if (mailData == null)
        {
            Debug.LogWarning(
                "Không thể tính thưởng/phạt Y thư "
                + "vì mailData null."
            );

            return;
        }

        int openCount =
            YThuUsageTracker
                .FinishTreatmentTrackingAndGetOpenCount();

        int moneyDelta =
            CalculateMoneyDelta(
                openCount
            );

        string yThuNote =
            BuildYThuUsageNote(
                openCount,
                moneyDelta
            );

        // Cộng hoặc trừ tiền do sử dụng Y thư.
        mailData.moneyDelta += moneyDelta;

        // Không ghi đè nhắc nhở bệnh và dược liệu.
        // Nếu đã có nhắc nhở thì nối ghi chú Y thư xuống dưới.
        AppendYThuUsageNote(
            mailData,
            yThuNote
        );

        Debug.Log(
            "Số lần mở Y thư trong ca này: "
            + openCount
        );

        Debug.Log(
            "Điều chỉnh tiền do dùng Y thư: "
            + moneyDelta
        );

        Debug.Log(
            "Toàn bộ ghi chú trong thư:\n"
            + mailData.yThuUsageNote
        );

        Debug.Log(
            "Tổng tiền trong thư sau điều chỉnh Y thư: "
            + mailData.moneyDelta
        );
    }

    private static void AppendYThuUsageNote(
        PendingPatientMailData mailData,
        string note
    )
    {
        if (mailData == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(note))
        {
            return;
        }

        // Chưa có nhắc nhở bệnh hoặc dược liệu.
        if (string.IsNullOrWhiteSpace(
                mailData.yThuUsageNote
            ))
        {
            mailData.yThuUsageNote = note;
            return;
        }

        // Đã có nhắc nhở hệ thống:
        // nối ghi chú Y thư xuống dưới thay vì ghi đè.
        mailData.yThuUsageNote +=
            "\n\n"
            + note;
    }

    private static int CalculateMoneyDelta(
        int openCount
    )
    {
        if (openCount <= 0)
        {
            return NoUseBonusMoney;
        }

        if (openCount == 1)
        {
            return 0;
        }

        return
            -(openCount - 1)
            * PenaltyPerExtraOpen;
    }

    private static string BuildYThuUsageNote(
        int openCount,
        int moneyDelta
    )
    {
        if (openCount <= 0)
        {
            return
                "(Chú ý: bạn không mở Y thư trong lúc "
                + "chữa bệnh, được thưởng thêm "
                + Mathf.Abs(moneyDelta)
                + " xu.)";
        }

        if (openCount == 1)
        {
            return
                "(Chú ý: bạn đã mở Y thư 1 lần, "
                + "không bị khấu trừ xu.)";
        }

        return
            "(Chú ý: bạn đã mở Y thư "
            + openCount
            + " lần, bị khấu trừ "
            + Mathf.Abs(moneyDelta)
            + " xu.)";
    }
}
