public partial class YThuBookUI
{
    private bool ShouldShowSpecialDiseasePage()
    {
        if (PlayerLevelService.GetCurrentStage() < 5)
            return false;

        if (!showNamedSpecialDiseasePage)
            return false;

        if (!SpecialYThuDiseaseService.HasSpecialDiseaseInYThu())
            return false;

        DiseaseData specialDisease = GetSpecialDiseaseForBook();

        if (specialDisease == null)
            return false;

        if (IsSearching())
            return DoesSpecialDiseaseMatchSearch(specialDisease);

        return true;
    }

    private DiseaseData GetSpecialDiseaseForBook()
    {
        if (specialDiseaseForBook != null)
            return specialDiseaseForBook;

        return SpecialYThuDiseaseService.GetSpecialDisease();
    }

    private bool DoesSpecialDiseaseMatchSearch(DiseaseData specialDisease)
    {
        if (specialDisease == null)
            return false;

        string keyword = GetSearchKeyword();

        if (string.IsNullOrWhiteSpace(keyword))
            return true;

        string selectedName = SpecialYThuDiseaseService.GetSelectedDiseaseName();

        if (ContainsSearchText(selectedName, keyword))
            return true;

        if (ContainsSearchText(specialDisease.diseaseName, keyword))
            return true;

        if (ContainsSearchText(specialDisease.description, keyword))
            return true;

        if (ContainsSearchText(GetDiseaseAssetName(specialDisease), keyword))
            return true;

        return false;
    }

    private bool ContainsSearchText(string source, string keyword)
    {
        if (string.IsNullOrWhiteSpace(source))
            return false;

        if (string.IsNullOrWhiteSpace(keyword))
            return true;

        string normalizedSource = YThuBookDataService.NormalizeSearchText(source);

        return normalizedSource.Contains(keyword);
    }

    private string GetDiseaseAssetName(DiseaseData disease)
    {
        if (disease == null)
            return "";

        return disease.name;
    }

    private bool IsNormalDiseaseAlreadyShowing(DiseaseData disease)
    {
        if (disease == null)
            return false;

        for (int i = 0; i < filteredDiseases.Count; i++)
        {
            if (filteredDiseases[i] == disease)
                return true;
        }

        return false;
    }
}