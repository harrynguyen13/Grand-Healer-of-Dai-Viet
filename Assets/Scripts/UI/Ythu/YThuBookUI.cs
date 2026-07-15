using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class YThuBookUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Database")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("Book Settings")]
    [Tooltip("Nếu bật, bệnh Level 5 cũng hiện trong Y thư. Nếu tắt, Level 5 được xem là bệnh đặc biệt và không hiện.")]
    [SerializeField] private bool includeSpecialLevelDiseases = false;

    [Header("Bệnh đặc biệt Quan Huyện")]
    [SerializeField] private bool showNamedSpecialDiseasePage = true;

    [Tooltip("Kéo DiseaseData bệnh Quan Huyện vào đây, ví dụ ThatDietTrungDocDich.")]
    [SerializeField] private DiseaseData specialDiseaseForBook;

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
        public bool isSpecialDiseasePage;
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
}