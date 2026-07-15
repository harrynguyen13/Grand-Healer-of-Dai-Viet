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

        if (diseaseInfoText != null)
        {
            if (page.prescriptionPageIndex == 0)
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
            else
            {
                diseaseInfoText.text = "";
            }
        }

        if (prescriptionText != null)
        {
            List<string> prescriptionPages =
                GetPrescriptionPagesForDisease(
                    disease,
                    page.isSpecialDiseasePage
                );

            if (prescriptionPages == null || prescriptionPages.Count == 0)
            {
                prescriptionText.text = "";
            }
            else
            {
                int prescriptionPageIndex =
                    Mathf.Clamp(page.prescriptionPageIndex, 0, prescriptionPages.Count - 1);

                prescriptionText.text = prescriptionPages[prescriptionPageIndex];
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
        int totalPageCount = GetTotalPageCount();

        if (prevButton != null)
            prevButton.gameObject.SetActive(isOpen && currentPageIndex > 0);

        if (nextButton != null)
            nextButton.gameObject.SetActive(isOpen && currentPageIndex < totalPageCount - 1);
    }

    private void SetButtonsInteractable(bool canInteract)
    {
        if (nextButton != null)
            nextButton.interactable = canInteract;

        if (prevButton != null)
            prevButton.interactable = canInteract;
    }
}