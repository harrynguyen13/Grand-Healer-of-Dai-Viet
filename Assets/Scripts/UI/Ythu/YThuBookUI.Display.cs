using System.Collections.Generic;
using UnityEngine;

public partial class YThuBookUI
{
    private int GetTotalPageCount()
    {
        return bookPages.Count;
    }

    private bool ShouldShowLockedPage()
    {
        if (IsSearching())
            return false;

        return hasLockedDiseasePage;
    }

    private bool IsCurrentPageLockedPage()
    {
        if (bookPages.Count == 0)
            return false;

        if (currentPageIndex < 0 || currentPageIndex >= bookPages.Count)
            return false;

        return bookPages[currentPageIndex].isLockedPage;
    }

    private void ClampCurrentPageIndex()
    {
        int totalPageCount = GetTotalPageCount();

        if (totalPageCount <= 0)
        {
            currentPageIndex = 0;
            return;
        }

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, totalPageCount - 1);
    }

    private void ShowCurrentPage()
    {
        ClampCurrentPageIndex();

        if (GetTotalPageCount() <= 0)
        {
            ShowEmptyPage();
            return;
        }

        if (IsCurrentPageLockedPage())
        {
            ShowLockedDiseasePage();
            return;
        }

        BookPageData page = bookPages[currentPageIndex];

        if (page == null || page.disease == null)
        {
            ShowEmptyPage();
            return;
        }

        DiseaseData disease = page.disease;

        List<string> prescriptionPages =
            GetPrescriptionPagesForDisease(
                disease,
                page.isSpecialDiseasePage
            );

        if (prescriptionPages == null)
            prescriptionPages = new List<string>();

        if (page.prescriptionPageIndex <= 0)
        {
            // Spread đầu tiên:
            // Trái = thông tin bệnh
            // Phải = phương thuốc trang 1
            if (diseaseInfoText != null)
            {
                if (page.isSpecialDiseasePage)
                {
                    diseaseInfoText.text =
                        YThuBookTextFormatter.BuildDiseaseInfoText(
                            disease,
                            SpecialYThuDiseaseService.GetSelectedDiseaseName()
                        );
                }
                else
                {
                    diseaseInfoText.text =
                        YThuBookTextFormatter.BuildDiseaseInfoText(disease);
                }
            }

            if (prescriptionText != null)
            {
                if (prescriptionPages.Count > 0)
                    prescriptionText.text = prescriptionPages[0];
                else
                    prescriptionText.text = "";
            }
        }
        else
        {
            // Các spread sau:
            // Trái = phương thuốc trang hiện tại
            // Phải = phương thuốc trang kế tiếp nếu có
            int leftPrescriptionIndex = page.prescriptionPageIndex;
            int rightPrescriptionIndex = leftPrescriptionIndex + 1;

            if (diseaseInfoText != null)
            {
                if (leftPrescriptionIndex >= 0 && leftPrescriptionIndex < prescriptionPages.Count)
                    diseaseInfoText.text = prescriptionPages[leftPrescriptionIndex];
                else
                {
                    diseaseInfoText.text = "";
                }
            }

            if (prescriptionText != null)
            {
                if (rightPrescriptionIndex >= 0 && rightPrescriptionIndex < prescriptionPages.Count)
                    prescriptionText.text = prescriptionPages[rightPrescriptionIndex];
                else
                    prescriptionText.text = "";
            }
        }

        if (pageNumberText != null)
        {
            pageNumberText.text =
                (currentPageIndex + 1)
                + " / "
                + GetTotalPageCount();
        }
    }
    private void ShowEmptyPage()
    {
        if (diseaseInfoText != null)
            diseaseInfoText.text = YThuBookTextFormatter.BuildEmptyDiseaseInfoText();

        if (prescriptionText != null)
            prescriptionText.text = "";

        if (pageNumberText != null)
            pageNumberText.text = "0 / 0";
    }

    private void ShowLockedDiseasePage()
    {
        int nextLevel =
            YThuBookDataService.GetNextLockedLevel(
                medicalDatabase,
                includeSpecialLevelDiseases
            );

        int nextLevelDiseaseCount =
            YThuBookDataService.CountDiseasesAtLevel(
                medicalDatabase,
                nextLevel,
                includeSpecialLevelDiseases
            );

        if (diseaseInfoText != null)
        {
            diseaseInfoText.text =
                YThuBookTextFormatter.BuildLockedDiseaseInfoText(nextLevel);
        }

        if (prescriptionText != null)
        {
            prescriptionText.text =
                YThuBookTextFormatter.BuildLockedPrescriptionText(
                    nextLevel,
                    nextLevelDiseaseCount
                );
        }

        if (pageNumberText != null)
        {
            pageNumberText.text =
                (currentPageIndex + 1)
                + " / "
                + GetTotalPageCount();
        }
    }

    private void SetContentVisible(bool visible)
    {
        if (diseaseInfoText != null)
            diseaseInfoText.gameObject.SetActive(visible);

        if (prescriptionText != null)
            prescriptionText.gameObject.SetActive(visible);

        if (pageNumberText != null)
            pageNumberText.gameObject.SetActive(visible);
    }

    private void UpdateButtons()
    {
        if (prevButton != null)
            prevButton.gameObject.SetActive(isOpen && currentPageIndex > 0);

        if (nextButton != null)
            nextButton.gameObject.SetActive(isOpen && GetNextSpreadStartIndex() > currentPageIndex);
    }
    private void SetButtonsInteractable(bool canInteract)
    {
        if (nextButton != null)
            nextButton.interactable = canInteract;

        if (prevButton != null)
            prevButton.interactable = canInteract;
    }
}