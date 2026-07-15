using UnityEngine;

public partial class GovernmentSpecialExamManager
{
    private void HandleTreatmentSuccess(SpecialPrescriptionEvaluationResult result)
    {
        Debug.Log(
            "Quan Huyện đã được chữa khỏi bằng đơn thuốc đặc biệt."
            + " | Message = " + result.message
        );

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.CompleteOfficialQuest();
        }
        else
        {
            PlayerPrefs.SetInt(OfficialQuestCompletedKey, 1);
            PlayerPrefs.SetInt(OfficialQuestFailedKey, 0);
            PlayerPrefs.Save();
        }

        Debug.Log("Đã đánh dấu nhiệm vụ Quan Huyện hoàn thành. Điều kiện Truyền Nhân Y Đạo đã mở.");
    }

    private void HandleTreatmentWrongButCanRetry(SpecialPrescriptionEvaluationResult result)
    {
        int attempt = specialDiseaseCase.TreatmentAttemptCount;
        int remaining = specialDiseaseCase.RemainingAttempts;

        SendWrongTreatmentMail(attempt, remaining);

        Debug.LogWarning(
            "Đơn thuốc chưa phù hợp."
            + " | Lý do: " + result.message
            + " | Lần sai = " + attempt
            + " | Còn " + remaining + " lần thử."
        );
    }

    private void HandleTreatmentFailed(SpecialPrescriptionEvaluationResult result)
    {
        int attempt = specialDiseaseCase.TreatmentAttemptCount;

        SendFailedTreatmentMail(attempt);

        Debug.LogWarning(
            "Nhiệm vụ Quan Huyện thất bại."
            + " | Lý do đơn cuối: " + result.message
            + " | Player sẽ ở lại cấp Lương Y Đại Việt."
        );

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.FailOfficialQuest();
        }
        else
        {
            PlayerPrefs.SetInt(OfficialQuestFailedKey, 1);
            PlayerPrefs.SetInt(OfficialQuestCompletedKey, 0);
            PlayerPrefs.Save();
        }
    }

    private void SendWrongTreatmentMail(int attempt, int remaining)
    {
        if (MailboxManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy MailboxManager để gửi thư Quan Huyện.");
            return;
        }

        string content = "";

        if (attempt <= 1)
        {
            content =
                "Lương y,\n\n" +
                "Sau khi dùng đơn thuốc vừa rồi, bệnh tình của ta vẫn chưa có chuyển biến rõ rệt. " +
                "Hơi thở vẫn nặng, trong ngực còn đau tức, thân thể vẫn suy yếu.\n\n" +
                "Ta nghĩ ngươi nên nghiên cứu thêm về căn bệnh này trước khi kê đơn lần nữa.\n\n" +
                "Số lần còn lại: " + remaining;
        }
        else
        {
            content =
                "Lương y,\n\n" +
                "Bệnh của ta vẫn chưa thuyên giảm. Đơn thuốc lần này xem ra vẫn chưa chạm đúng căn nguyên của bệnh.\n\n" +
                "Ta sẽ cho ngươi thêm một cơ hội cuối cùng. Hãy suy xét thật cẩn trọng trước khi kê đơn.\n\n" +
                "Số lần còn lại: " + remaining;
        }

        MailboxManager.Instance.AddMail(
            MailType.PatientFail,
            "Quan Huyện",
            content,
            0,
            0
        );

        Debug.Log("Đã gửi thư Quan Huyện báo kê đơn chưa phù hợp. Lần sai: " + attempt);
    }

    private void SendFailedTreatmentMail(int attempt)
    {
        if (MailboxManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy MailboxManager để gửi thư thất bại Quan Huyện.");
            return;
        }

        string content =
            "Lương y,\n\n" +
            "Ta đã đặt niềm tin vào ngươi, nhưng qua nhiều lần dùng thuốc, bệnh tình vẫn không hề thuyên giảm. " +
            "Thân thể ta ngày một suy kiệt, lòng tin cũng chẳng còn như trước.\n\n" +
            "Ta quá thất vọng về ngươi. Có lẽ y thuật của ngươi vẫn chưa đủ để chữa căn bệnh này. " +
            "Hãy tiếp tục rèn luyện nhiều hơn trước khi gánh lấy những ca bệnh lớn như vậy.\n\n" +
            "Nhiệm vụ Quan Huyện đã thất bại.";

        MailboxManager.Instance.AddMail(
            MailType.PatientFail,
            "Quan Huyện",
            content,
            0,
            0
        );

        Debug.Log("Đã gửi thư thất bại nhiệm vụ Quan Huyện. Lần sai cuối: " + attempt);
    }
}