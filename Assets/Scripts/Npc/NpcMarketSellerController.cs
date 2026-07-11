using System.Collections;
using UnityEngine;

public enum MarketGoodsType
{
    Rau,
    Thit,
    Gao,
    Thuoc,
    Vai
}

public class NpcMarketSellerController : MonoBehaviour
{
    [Header("Bật / tắt hội thoại bán hàng")]
    [SerializeField] private bool enableSellerTalk = true;

    [Header("Loại hàng bán")]
    [SerializeField] private MarketGoodsType goodsType = MarketGoodsType.Rau;

    [Header("Bong bóng thoại")]
    [SerializeField] private NpcSpeechBubble speechBubble;

    [Header("Chào hàng tự động")]
    [SerializeField] private bool autoGreeting = true;
    [SerializeField] private float greetingShowTime = 2f;
    [SerializeField] private float greetingDelay = 4f;

    [Header("Hội thoại với khách")]
    [SerializeField] private float talkDurationPerLine = 1.4f;
    [SerializeField] private float talkCooldown = 5f;
    [SerializeField] private float buyerMoveAwayTime = 0.8f;

    private bool isTalking;
    private float nextTalkTime;
    private Coroutine talkCoroutine;
    private Coroutine greetingCoroutine;

    private void Awake()
    {
        if (speechBubble == null)
            speechBubble = GetComponent<NpcSpeechBubble>();

        HideBubble();
    }

    private void OnEnable()
    {
        if (autoGreeting)
            greetingCoroutine = StartCoroutine(GreetingLoop());
    }

    private void OnDisable()
    {
        if (greetingCoroutine != null)
            StopCoroutine(greetingCoroutine);

        if (talkCoroutine != null)
            StopCoroutine(talkCoroutine);
    }

    private IEnumerator GreetingLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (enableSellerTalk && autoGreeting && !isTalking)
            {
                ShowBubble(GetGreetingLine());

                yield return new WaitForSeconds(greetingShowTime);

                if (!isTalking)
                    HideBubble();
            }

            yield return new WaitForSeconds(greetingDelay);
        }
    }

    public void TryTalkWithBuyer(NpcMarketBuyerController buyer)
    {
        if (!enableSellerTalk)
            return;

        if (buyer == null)
            return;

        if (!buyer.CanTalk)
            return;

        if (isTalking)
            return;

        if (Time.time < nextTalkTime)
            return;

        if (talkCoroutine != null)
            StopCoroutine(talkCoroutine);

        talkCoroutine = StartCoroutine(TalkRoutine(buyer));
    }

    private IEnumerator TalkRoutine(NpcMarketBuyerController buyer)
    {
        isTalking = true;
        nextTalkTime = Time.time + talkCooldown;

        buyer.BeginTalk(talkCooldown);
        buyer.FaceTarget(transform.position);

        HideBubble();
        buyer.HideBubble();

        // Khách hỏi
        buyer.ShowBubble(GetBuyerQuestionLine());
        yield return new WaitForSeconds(talkDurationPerLine);

        buyer.HideBubble();

        // Người bán trả lời
        ShowBubble(GetSellerAnswerLine());
        yield return new WaitForSeconds(talkDurationPerLine);

        HideBubble();

        // Người bán cảm ơn
        ShowBubble("Cảm ơn khách nhé.");
        yield return new WaitForSeconds(talkDurationPerLine);

        HideBubble();
        buyer.HideBubble();

        buyer.EndTalk();
        buyer.MoveAwayFrom(transform.position, buyerMoveAwayTime);

        isTalking = false;
    }

    private string GetGreetingLine()
    {
        switch (goodsType)
        {
            case MarketGoodsType.Rau:
                return "Rau tươi đây, khách xem đi!";

            case MarketGoodsType.Thit:
                return "Thịt mới đây, khách xem đi!";

            case MarketGoodsType.Gao:
                return "Gạo ngon đây, khách xem đi!";

            case MarketGoodsType.Thuoc:
                return "Quý khách vào xem thuốc đi!";

            case MarketGoodsType.Vai:
                return "Vải đẹp đây, khách xem đi!";

            default:
                return "Khách vào xem hàng đi!";
        }
    }

    private string GetBuyerQuestionLine()
    {
        switch (goodsType)
        {
            case MarketGoodsType.Rau:
                return "Rau này có tươi không?";

            case MarketGoodsType.Thit:
                return "Thịt hôm nay giá thế nào?";

            case MarketGoodsType.Gao:
                return "Gạo này bán thế nào?";

            case MarketGoodsType.Thuoc:
                return "Có vị thuốc nào trị ho không?";

            case MarketGoodsType.Vai:
                return "Vải này bán thế nào?";

            default:
                return "Hàng này bán thế nào?";
        }
    }

    private string GetSellerAnswerLine()
    {
        switch (goodsType)
        {
            case MarketGoodsType.Rau:
                return "Tươi lắm, mới hái sáng nay.";

            case MarketGoodsType.Thit:
                return "Giá vừa phải, thịt còn tươi.";

            case MarketGoodsType.Gao:
                return "Gạo thơm, hạt chắc lắm.";

            case MarketGoodsType.Thuoc:
                return "Có, cam thảo trị ho rất tốt.";

            case MarketGoodsType.Vai:
                return "Vải bền, may áo rất hợp.";

            default:
                return "Hàng còn tốt lắm.";
        }
    }

    private void ShowBubble(string line)
    {
        if (speechBubble != null)
            speechBubble.Show(line);
    }

    private void HideBubble()
    {
        if (speechBubble != null)
            speechBubble.Hide();
    }
}