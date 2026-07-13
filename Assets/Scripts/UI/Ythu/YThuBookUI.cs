using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YThuBookUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Database")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("Book Settings")]
    [Tooltip("Nếu bật, bệnh Level 5 cũng hiện trong Y thư. Nếu tắt, Level 5 được xem là bệnh đặc biệt và không hiện.")]
    [SerializeField] private bool includeSpecialLevelDiseases = false;

    [Header("Book Image")]
    [SerializeField] private Image bookImage;
    [SerializeField] private Sprite idleOpenSprite;

    [Header("Content Text")]
    [SerializeField] private TMP_Text diseaseInfoText;
    [SerializeField] private TMP_Text prescriptionText;
    [SerializeField] private TMP_Text pageNumberText;

    [Header("Search - có thể bỏ trống")]
    [SerializeField] private TMP_InputField searchInput;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Animation Frames")]
    [SerializeField] private List<Sprite> turnRightFrames = new List<Sprite>();
    [SerializeField] private List<Sprite> turnLeftFrames = new List<Sprite>();
    [SerializeField] private float frameDuration = 0.05f;

    private readonly List<DiseaseData> unlockedDiseases = new List<DiseaseData>();
    private readonly List<DiseaseData> filteredDiseases = new List<DiseaseData>();
    private readonly List<BookPageData> bookPages = new List<BookPageData>();

    private int currentPageIndex = 0;

    private bool isAnimating = false;
    private bool isOpen = false;
    private bool hasLockedDiseasePage = false;

    private class BookPageData
    {
        public DiseaseData disease;
        public int prescriptionPageIndex;
        public bool isLockedPage;
    }

    private void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (prevButton != null)
            prevButton.onClick.AddListener(PreviousPage);

        if (searchInput != null)
            searchInput.onValueChanged.AddListener(OnSearchChanged);
    }

    private void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        isOpen = false;

        if (bookImage != null && idleOpenSprite != null)
            bookImage.sprite = idleOpenSprite;
    }

    public void ToggleBook()
    {
        if (isAnimating)
            return;

        if (isOpen)
            CloseBook();
        else
            OpenBook();
    }

    public void OpenBook()
    {
        if (isAnimating)
            return;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        isOpen = true;

        YThuUsageTracker.RecordYThuOpened();

        RefreshBookData();

        if (bookImage != null && idleOpenSprite != null)
            bookImage.sprite = idleOpenSprite;

        ShowCurrentPage();
        UpdateButtons();
    }

    public void CloseBook()
    {
        if (isAnimating)
            return;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        isOpen = false;
    }

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
            + ". Còn bệnh khóa: "
            + hasLockedDiseasePage
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

            List<string> prescriptionPages =
                YThuBookTextFormatter.BuildPrescriptionPages(disease);

            int pageCount = 1;

            if (prescriptionPages != null && prescriptionPages.Count > 0)
                pageCount = prescriptionPages.Count;

            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                BookPageData page = new BookPageData();
                page.disease = disease;
                page.prescriptionPageIndex = pageIndex;
                page.isLockedPage = false;

                bookPages.Add(page);
            }
        }

        if (ShouldShowLockedPage())
        {
            BookPageData lockedPage = new BookPageData();
            lockedPage.disease = null;
            lockedPage.prescriptionPageIndex = 0;
            lockedPage.isLockedPage = true;

            bookPages.Add(lockedPage);
        }
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
                diseaseInfoText.text = YThuBookTextFormatter.BuildDiseaseInfoText(disease);
            }
            else
            {
                diseaseInfoText.text = "";
            }
        }
        if (prescriptionText != null)
        {
            List<string> prescriptionPages =
                YThuBookTextFormatter.BuildPrescriptionPages(disease);

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