using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class YThuBookUI
{
    private void NextPage()
    {
        if (!isOpen || isAnimating)
            return;

        if (currentPageIndex >= GetTotalPageCount() - 1)
            return;

        StartCoroutine(PlayFlipAnimation(true));
    }

    private void PreviousPage()
    {
        if (!isOpen || isAnimating)
            return;

        if (currentPageIndex <= 0)
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

        int targetIndex = forward ? currentPageIndex + 1 : currentPageIndex - 1;
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
}