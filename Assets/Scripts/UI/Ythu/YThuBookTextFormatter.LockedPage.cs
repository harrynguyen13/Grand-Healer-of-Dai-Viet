public static partial class YThuBookTextFormatter
{
    public static string BuildEmptyDiseaseInfoText()
    {
        return
            "<align=\"center\"><b>Y THƯ</b></align>\n\n" +
            "Chưa có bệnh nào được mở khóa hoặc không tìm thấy kết quả phù hợp.";
    }

    public static string BuildLockedDiseaseInfoText(int nextLevel)
    {
        string text = "";

        text += "<align=\"center\"><b>TRANG CHƯA MỞ KHÓA</b></align>\n\n";
        text += "Kiến thức trong Y thư hiện chỉ ghi lại những bệnh phù hợp với cấp bậc hiện tại.\n\n";

        if (nextLevel > 0)
        {
            text += "Hãy lên <b>Cấp "
                + nextLevel
                + "</b> để mở khóa thêm kiến thức chữa bệnh.";
        }
        else
        {
            text += "Hãy tiếp tục nâng cao tay nghề để mở rộng Y thư.";
        }

        return text;
    }

    public static string BuildLockedPrescriptionText(int nextLevel, int nextLevelDiseaseCount)
    {
        string text = "";

        text += "<align=\"center\"><b>GỢI Ý</b></align>\n\n";

        if (nextLevel > 0)
        {
            text += "Cấp tiếp theo sẽ mở thêm:\n\n";
            text += "- " + nextLevelDiseaseCount + " bệnh mới\n";
            text += "- Nhiều chứng bệnh khó hơn\n";
            text += "- Các phương thuốc phức tạp hơn\n\n";
        }

        text += "Hãy chữa bệnh chính xác để tăng tín nhiệm và thăng cấp.";

        return text;
    }
}