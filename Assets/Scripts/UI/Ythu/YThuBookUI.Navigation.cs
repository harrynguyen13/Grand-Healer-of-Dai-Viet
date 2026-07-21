using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class YThuBookUI
{
    private void NextPage()
    {
        if (!isOpen || isAnimating)
            return;

        int targetIndex = GetNextSpreadStartIndex();

        if (targetIndex <= currentPageIndex)
            return;

        StartCoroutine(PlayFlipAnimation(true));
    }

    private void PreviousPage()
    {
        if (!isOpen || isAnimating)
            return;

        int targetIndex = GetPreviousSpreadStartIndex();

        if (targetIndex >= currentPageIndex)
            return;

        StartCoroutine(PlayFlipAnimation(false));
    }

    private IEnumerator PlayFlipAnimation(bool forward)
    {
        isAnimating = true;
        SetButtonsInteractable(false);
        SetContentVisible(false);

        List<Sprite> frames = forward ? turnRightFrames : turnLeftFrames;

        if (frames == null || frames.Count == 0)
        {
            Debug.LogWarning("Chưa gán frame animation lật trang.");

            isAnimating = false;
            SetContentVisible(true);
            SetButtonsInteractable(true);

            yield break;
        }

        int targetIndex = forward
            ? GetNextSpreadStartIndex()
            : GetPreviousSpreadStartIndex();

        targetIndex = Mathf.Clamp(targetIndex, 0, Mathf.Max(0, GetTotalPageCount() - 1));

        int midPoint = frames.Count / 2;
        bool changedContent = false;

        for (int i = 0; i < frames.Count; i++)
        {
            if (bookImage != null && frames[i] != null)
                bookImage.sprite = frames[i];

            if (!changedContent && i >= midPoint)
            {
                currentPageIndex = targetIndex;
                changedContent = true;
            }

            yield return new WaitForSecondsRealtime(frameDuration);
        }

        if (bookImage != null && idleOpenSprite != null)
            bookImage.sprite = idleOpenSprite;

        ShowCurrentPage();

        SetContentVisible(true);
        UpdateButtons();
        SetButtonsInteractable(true);

        isAnimating = false;
    }

    private int GetNextSpreadStartIndex()
    {
        int totalPageCount = GetTotalPageCount();

        if (totalPageCount <= 0)
            return 0;

        if (currentPageIndex < 0)
            return 0;

        if (currentPageIndex >= totalPageCount - 1)
            return currentPageIndex;

        BookPageData currentPage = bookPages[currentPageIndex];

        if (currentPage == null || currentPage.isLockedPage)
            return Mathf.Clamp(currentPageIndex + 1, 0, totalPageCount - 1);

        // Spread đầu tiên chỉ dùng:
        // Trái = thông tin bệnh
        // Phải = đơn thuốc trang 0
        // Nên next phải sang prescriptionPageIndex 1.
        if (currentPage.prescriptionPageIndex <= 0)
            return Mathf.Clamp(currentPageIndex + 1, 0, totalPageCount - 1);

        int nextIndex = currentPageIndex + 1;

        // Nếu trang kế tiếp cùng bệnh và là prescription page tiếp theo,
        // nó đã được hiển thị ở trang phải rồi, nên lần next phải nhảy qua nó.
        if (CanPairWithNextPrescriptionPage(currentPageIndex))
            nextIndex = currentPageIndex + 2;

        return Mathf.Clamp(nextIndex, 0, totalPageCount - 1);
    }

    private int GetPreviousSpreadStartIndex()
    {
        int totalPageCount = GetTotalPageCount();

        if (totalPageCount <= 0)
            return 0;

        if (currentPageIndex <= 0)
            return 0;

        int previousIndex = currentPageIndex - 1;

        // Nếu previousIndex là trang phải của spread trước,
        // thì phải lùi thêm 1 để về đúng trang trái của spread đó.
        if (previousIndex > 0 && WasPairedAsRightPrescriptionPage(previousIndex))
            previousIndex--;

        return Mathf.Clamp(previousIndex, 0, totalPageCount - 1);
    }

    private bool CanPairWithNextPrescriptionPage(int leftIndex)
    {
        int rightIndex = leftIndex + 1;

        if (rightIndex >= GetTotalPageCount())
            return false;

        BookPageData leftPage = bookPages[leftIndex];
        BookPageData rightPage = bookPages[rightIndex];

        if (leftPage == null || rightPage == null)
            return false;

        if (leftPage.disease != rightPage.disease)
            return false;

        if (leftPage.isSpecialDiseasePage != rightPage.isSpecialDiseasePage)
            return false;

        return rightPage.prescriptionPageIndex == leftPage.prescriptionPageIndex + 1;
    }

    private bool WasPairedAsRightPrescriptionPage(int pageIndex)
    {
        int leftIndex = pageIndex - 1;

        if (leftIndex < 0)
            return false;

        BookPageData leftPage = bookPages[leftIndex];
        BookPageData rightPage = bookPages[pageIndex];

        if (leftPage == null || rightPage == null)
            return false;

        if (leftPage.disease != rightPage.disease)
            return false;

        if (leftPage.isSpecialDiseasePage != rightPage.isSpecialDiseasePage)
            return false;

        if (leftPage.prescriptionPageIndex <= 0)
            return false;

        return rightPage.prescriptionPageIndex == leftPage.prescriptionPageIndex + 1;
    }
}