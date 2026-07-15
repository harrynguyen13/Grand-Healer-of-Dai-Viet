using System.Collections.Generic;
using UnityEngine;

public partial class YThuBookUI
{
    private void RefreshBookData()
    {
        unlockedDiseases.Clear();

        unlockedDiseases.AddRange(
            YThuBookDataService.GetUnlockedDiseasesForBook(
                medicalDatabase,
                includeSpecialLevelDiseases
            )
        );

        hasLockedDiseasePage =
            YThuBookDataService.HasLockedDiseasesAboveCurrentLevel(
                medicalDatabase,
                includeSpecialLevelDiseases
            );

        ApplySearchFilter();

        Debug.Log(
            "Y thư đã mở "
            + unlockedDiseases.Count
            + " bệnh theo cấp hiện tại: "
            + PlayerLevelService.GetCurrentUnlockLevel()
            + ". CurrentStage = "
            + PlayerLevelService.GetCurrentStage()
            + ". Còn bệnh khóa: "
            + hasLockedDiseasePage
            + ". Có bệnh đặc biệt Quan Huyện: "
            + ShouldShowSpecialDiseasePage()
        );
    }

    private void OnSearchChanged(string value)
    {
        if (!isOpen)
            return;

        currentPageIndex = 0;

        ApplySearchFilter();
        ShowCurrentPage();
        UpdateButtons();
    }

    private void ApplySearchFilter()
    {
        filteredDiseases.Clear();

        filteredDiseases.AddRange(
            YThuBookDataService.FilterDiseases(
                unlockedDiseases,
                GetSearchKeyword()
            )
        );

        BuildBookPages();
        ClampCurrentPageIndex();
    }

    private void BuildBookPages()
    {
        bookPages.Clear();

        for (int i = 0; i < filteredDiseases.Count; i++)
        {
            DiseaseData disease = filteredDiseases[i];

            if (disease == null)
                continue;

            AddDiseasePages(disease, false);
        }

        if (ShouldShowSpecialDiseasePage())
        {
            DiseaseData specialDisease = GetSpecialDiseaseForBook();

            if (specialDisease != null && !IsNormalDiseaseAlreadyShowing(specialDisease))
            {
                AddDiseasePages(specialDisease, true);
            }
        }

        if (ShouldShowLockedPage())
        {
            BookPageData lockedPage = new BookPageData();
            lockedPage.disease = null;
            lockedPage.prescriptionPageIndex = 0;
            lockedPage.isLockedPage = true;
            lockedPage.isSpecialDiseasePage = false;

            bookPages.Add(lockedPage);
        }
    }

    private void AddDiseasePages(DiseaseData disease, bool isSpecialDiseasePage)
    {
        if (disease == null)
            return;

        List<string> prescriptionPages =
            GetPrescriptionPagesForDisease(disease, isSpecialDiseasePage);

        int pageCount = 1;

        if (prescriptionPages != null && prescriptionPages.Count > 0)
            pageCount = prescriptionPages.Count;

        for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            BookPageData page = new BookPageData();
            page.disease = disease;
            page.prescriptionPageIndex = pageIndex;
            page.isLockedPage = false;
            page.isSpecialDiseasePage = isSpecialDiseasePage;

            bookPages.Add(page);
        }
    }

    private List<string> GetPrescriptionPagesForDisease(
        DiseaseData disease,
        bool isSpecialDiseasePage
    )
    {
        if (isSpecialDiseasePage)
        {
            return YThuBookTextFormatter.BuildSpecialPrescriptionPages(disease);
        }

        return YThuBookTextFormatter.BuildPrescriptionPages(disease);
    }

    private string GetSearchKeyword()
    {
        if (searchInput == null)
            return "";

        return YThuBookDataService.NormalizeSearchText(searchInput.text);
    }

    private bool IsSearching()
    {
        return !string.IsNullOrWhiteSpace(GetSearchKeyword());
    }
}