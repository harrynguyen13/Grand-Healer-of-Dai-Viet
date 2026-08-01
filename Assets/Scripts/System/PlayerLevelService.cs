using UnityEngine;

public static class PlayerLevelService
{
    public const int YSinhTarget = 100;
    public const int LuongYTarget = 200;
    public const int DaiPhuTarget = 300;
    public const int DanhYTarget = 500;

    private const string OfficialQuestCompletedKey = "OfficialQuestCompleted";
    private const string OfficialQuestFailedKey = "OfficialQuestFailed";
    private const string SpecialCaseIsCuredKey = "SpecialCase_IsCured";
    private const string SpecialCaseIsFailedKey = "SpecialCase_IsFailed";

    public static int GetReputation()
    {
        if (PlayerEconomy.Instance == null)
            return 0;

        return PlayerEconomy.Instance.Reputation;
    }

    public static int GetCurrentStage()
    {
        return GetStageByReputation(GetReputation());
    }

    public static int GetStageByReputation(int reputation)
    {
        if (reputation < YSinhTarget)
            return 1;

        if (reputation < LuongYTarget)
            return 2;

        if (reputation < DaiPhuTarget)
            return 3;

        if (reputation < DanhYTarget)
            return 4;

        if (!CanBecomeYDaoSuccessor())
            return 5;

        return 6;
    }

    public static int GetCurrentUnlockLevel()
    {
        int stage = GetCurrentStage();

        if (stage < 1)
            return 1;

        if (stage > 5)
            return 5;

        return stage;
    }

    public static string GetCurrentRankName()
    {
        return GetRankNameByStage(GetCurrentStage());
    }

    public static string GetRankNameByStage(int stage)
    {
        if (stage <= 1)
            return "Y Sinh";

        if (stage == 2)
            return "Lương Y";

        if (stage == 3)
            return "Đại Phu";

        if (stage == 4)
            return "Danh Y";

        if (stage == 5)
            return "Lương Y Đại Việt";

        return "Truyền Nhân Y Đạo";
    }

    public static string GetChapterTitle(int stage)
    {
        if (stage == 1)
            return "Chương 1 - Y Sinh";

        if (stage == 2)
            return "Chương 2 - Lương Y";

        if (stage == 3)
            return "Chương 3 - Đại Phu";

        if (stage == 4)
            return "Chương 4 - Danh Y";

        if (stage == 5)
            return "Chương 5 - Phủ Huyện";

        return "Hậu truyện - Truyền Nhân Y Đạo";
    }

    public static int GetNextTargetReputation()
    {
        return GetNextTargetReputation(GetReputation());
    }

    public static int GetNextTargetReputation(int reputation)
    {
        if (reputation < YSinhTarget)
            return YSinhTarget;

        if (reputation < LuongYTarget)
            return LuongYTarget;

        if (reputation < DaiPhuTarget)
            return DaiPhuTarget;

        if (reputation < DanhYTarget)
            return DanhYTarget;

        return DanhYTarget;
    }

    public static bool HasUnlockedLevel(int level)
    {
        return GetCurrentUnlockLevel() >= Mathf.Max(1, level);
    }

    public static bool CanBecomeYDaoSuccessor()
    {
        if (GetReputation() < DanhYTarget)
            return false;

        if (IsOfficialQuestFailed())
            return false;

        if (!IsQuanHuyenCured())
            return false;

        if (!IsOfficialQuestCompleted())
            return false;

        return true;
    }

    public static bool IsQuanHuyenCured()
    {
        return PlayerPrefs.GetInt(SpecialCaseIsCuredKey, 0) == 1;
    }

    public static bool IsQuanHuyenFailed()
    {
        return PlayerPrefs.GetInt(SpecialCaseIsFailedKey, 0) == 1;
    }

    public static bool IsOfficialQuestFailed()
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.IsOfficialQuestFailed();

        return PlayerPrefs.GetInt(OfficialQuestFailedKey, 0) == 1;
    }

    private static bool IsOfficialQuestCompleted()
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.IsOfficialQuestCompleted();

        return PlayerPrefs.GetInt(OfficialQuestCompletedKey, 0) == 1;
    }
}