using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuideTextController : MonoBehaviour
{
    [Header("Text hướng dẫn")]
    [SerializeField] private TextMeshProUGUI guideText;

    [Header("Scroll View")]
    [SerializeField] private ScrollRect guideScrollRect;

    private void OnEnable()
    {
        ShowGuideText();
    }

    private void ShowGuideText()
    {
        if (guideText == null)
            return;

        guideText.text =
@"<b>HƯỚNG DẪN ĐIỀU KHIỂN</b>

- W / A / S / D: Di chuyển nhân vật.
- Shift: Nhấn giữ để chạy nhanh.
- E: Nhấn để mở cửa, đi vào nhà hoặc mua dược liệu.
- F: Nhấn để khám bệnh.
- X: Nhấn để đóng mở bảng nhiệm vụ.
- Q: Nhấn để mở giao diện trồng thuốc.
- Chuột trái: Bấm chọn / thu hoạch.

<b>CHÚ Ý:</b>

- Khi cây thuốc sẵn sàng thu hoạch, biểu tượng sẽ hiện phía trên cây.
- Đến gần cửa rồi bấm E để chuyển cảnh.
- Bạn nên chú ý đến lượt tham khảo Y thư của mình:
    + Không tham khảo lần nào: +5 xu.
    + Tham khảo 1 lần không có phần thưởng.
    + Tham khảo lần thứ 2 trở lên: -5 xu cho mỗi lần mở.

<b>MẸO BỐC THUỐC:</b>

- Chủ dược: bốc 6-8 đơn vị/dược liệu.  
- Phụ dược: 3-5 đơn vị/dược liệu.
- Điều hòa: 1,2 đơn vị/dược liệu.";

        Canvas.ForceUpdateCanvases();

        if (guideScrollRect != null)
            guideScrollRect.verticalNormalizedPosition = 1f;
    }
}