using UnityEngine;
using UnityEngine.EventSystems;

public class HerbTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private HerbData herbData;

    public void SetHerb(HerbData herb)
    {
        herbData = herb;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HerbRoleTooltipUI.Instance == null)
            return;

        HerbRoleTooltipUI.Instance.Show(herbData, eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (HerbRoleTooltipUI.Instance == null)
            return;

        HerbRoleTooltipUI.Instance.Move(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (HerbRoleTooltipUI.Instance == null)
            return;

        HerbRoleTooltipUI.Instance.Hide();
    }

    private void OnDisable()
    {
        if (HerbRoleTooltipUI.Instance != null)
            HerbRoleTooltipUI.Instance.Hide();
    }
}