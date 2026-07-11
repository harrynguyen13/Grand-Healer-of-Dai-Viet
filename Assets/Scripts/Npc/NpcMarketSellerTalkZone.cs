using UnityEngine;

public class NpcMarketSellerTalkZone : MonoBehaviour
{
    [SerializeField] private NpcMarketSellerController seller;

    private void Awake()
    {
        if (seller == null)
            seller = GetComponentInParent<NpcMarketSellerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTalk(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryTalk(other);
    }
    private void TryTalk(Collider2D other)
    {
        if (seller == null)
            return;

        if (other == null)
            return;

        NpcMarketBuyerController buyer =
            other.GetComponent<NpcMarketBuyerController>();

        if (buyer == null)
            buyer = other.GetComponentInParent<NpcMarketBuyerController>();

        if (buyer == null)
            return;

        seller.TryTalkWithBuyer(buyer);
    }
}