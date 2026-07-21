using System;
using System.Collections.Generic;

public partial class QuestRuntimeManager
{
    private List<QuestDefinition> BuildQuestPool(int stage, int reputation)
    {
        List<QuestDefinition> quests = new List<QuestDefinition>();

        if (stage <= 1)
        {
            AddStage1Quests(quests, reputation, true);
            return quests;
        }

        if (stage == 2)
        {
            AddStage1Quests(quests, reputation, false);
            AddStage2Quests(quests, reputation, true);
            return quests;
        }

        if (stage == 3)
        {
            AddStage1Quests(quests, reputation, false);
            AddStage2Quests(quests, reputation, false);
            AddStage3Quests(quests, reputation, true);
            return quests;
        }

        if (stage == 4)
        {
            AddStage1Quests(quests, reputation, false);
            AddStage2Quests(quests, reputation, false);
            AddStage3Quests(quests, reputation, false);
            AddStage4Quests(quests, reputation, true);
            return quests;
        }

        if (stage == 5)
        {
            AddStage1Quests(quests, reputation, false);
            AddStage2Quests(quests, reputation, false);
            AddStage3Quests(quests, reputation, false);
            AddStage4Quests(quests, reputation, false);
            AddStage5Quests(quests);
            return quests;
        }

        AddStage1Quests(quests, reputation, false);
        AddStage2Quests(quests, reputation, false);
        AddStage3Quests(quests, reputation, false);
        AddStage4Quests(quests, reputation, false);
        AddStage5Quests(quests);
        AddEndGameQuests(quests);

        return quests;
    }

    private void AddStage1Quests(List<QuestDefinition> quests, int reputation, bool includeRankQuest)
    {
        AddQuest(quests, "S1_Cure_1", "Chữa khỏi 1 ca bệnh cho dân làng", GetCorrectTreatmentCount, 1);
        AddQuest(quests, "S1_Cure_3", "Chữa khỏi 3 ca bệnh cho dân làng", GetCorrectTreatmentCount, 3);

        AddQuest(quests, "S1_Level1_1", "Chữa khỏi 1 ca bệnh nhẹ cấp 1", () => GetLevelCuredValue(1), 1);
        AddQuest(quests, "S1_Level1_2", "Chữa khỏi 2 ca bệnh nhẹ cấp 1", () => GetLevelCuredValue(1), 2);

        AddQuest(quests, "S1_AchNghich", "Chữa khỏi bệnh Ách nghịch 1 lần", () => GetDiseaseCuredValue("AchNghichAnNac"), 1);

        AddQuest(quests, "S1_Buy_1", "Mua 1 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 1);
        AddQuest(quests, "S1_Buy_3", "Mua 3 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 3);

        if (includeRankQuest)
        {
            AddQuest(
                quests,
                "S1_Rank_100",
                "Đạt 100 tín nhiệm để lên cấp Lương Y và mở khóa vườn dược liệu",
                () => reputation,
                luongYTarget
            );
        }
    }

    private void AddStage2Quests(List<QuestDefinition> quests, int reputation, bool includeRankQuest)
    {
        AddQuest(quests, "S2_Cure_5", "Chữa khỏi 5 ca bệnh cho dân làng", GetCorrectTreatmentCount, 5);
        AddQuest(quests, "S2_Cure_10", "Chữa khỏi 10 ca bệnh cho dân làng", GetCorrectTreatmentCount, 10);

        AddQuest(quests, "S2_Level2_1", "Chữa khỏi 1 ca bệnh cấp 2", () => GetLevelCuredValue(2), 1);
        AddQuest(quests, "S2_Level2_2", "Chữa khỏi 2 ca bệnh cấp 2", () => GetLevelCuredValue(2), 2);

        AddQuest(quests, "S2_KhaiThau", "Chữa khỏi bệnh Khái thấu phong nhiệt 1 lần", () => GetDiseaseCuredValue("KhaiThauPhongNhiet"), 1);

        AddQuest(quests, "S2_Gather_3", "Thu hoạch 3 dược liệu trong vườn nhà", GetGatheredTotalValue, 3);
        AddQuest(quests, "S2_Gather_6", "Thu hoạch 6 dược liệu cơ bản trong vườn", GetGatheredTotalValue, 6);

        AddRandomGardenHerbHarvestQuest(quests, "S2_RandomHerb_2_A", 2);
        AddRandomGardenHerbHarvestQuest(quests, "S2_RandomHerb_3_B", 3);

        AddQuest(quests, "S2_Buy_5", "Mua 5 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 5);
        AddQuest(quests, "S2_Buy_8", "Mua 8 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 8);

        if (includeRankQuest)
        {
            AddQuest(quests, "S2_Rank_200", "Đạt 200 tín nhiệm để lên cấp Đại Phu", () => reputation, luongYTarget);
        }
    }

    private void AddStage3Quests(List<QuestDefinition> quests, int reputation, bool includeRankQuest)
    {
        AddQuest(quests, "S3_Cure_10", "Chữa khỏi 10 ca bệnh cho dân làng", GetCorrectTreatmentCount, 10);
        AddQuest(quests, "S3_Cure_15", "Chữa khỏi 15 ca bệnh cho dân làng", GetCorrectTreatmentCount, 15);

        AddQuest(quests, "S3_Level3_2", "Chữa khỏi 2 ca bệnh cấp 3", () => GetLevelCuredValue(3), 2);
        AddQuest(quests, "S3_Level3_3", "Chữa khỏi 3 ca bệnh cấp 3", () => GetLevelCuredValue(3), 3);

        AddQuest(quests, "S3_TamHoa", "Chữa khỏi bệnh Tâm hỏa vượng 1 lần", () => GetDiseaseCuredValue("TamHoaVuong"), 1);

        AddQuest(quests, "S3_Gather_15", "Thu hoạch 15 dược liệu trong vườn", GetGatheredTotalValue, 15);
        AddQuest(quests, "S3_Gather_20", "Thu hoạch 20 dược liệu trong vườn", GetGatheredTotalValue, 20);

        AddRandomGardenHerbHarvestQuest(quests, "S3_RandomHerb_4_A", 4);
        AddRandomGardenHerbHarvestQuest(quests, "S3_RandomHerb_5_B", 5);
        AddRandomGardenHerbHarvestQuest(quests, "S3_RandomHerb_6_C", 6);

        AddQuest(quests, "S3_Buy_10", "Mua 10 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 10);
        AddQuest(quests, "S3_Buy_12", "Mua 12 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 12);

        if (includeRankQuest)
        {
            AddQuest(quests, "S3_Rank_300", "Đạt 300 tín nhiệm để lên cấp Danh Y", () => reputation, daiPhuTarget);
        }
    }

    private void AddStage4Quests(List<QuestDefinition> quests, int reputation, bool includeRankQuest)
    {
        AddQuest(quests, "S4_Cure_15", "Chữa khỏi 15 ca bệnh cho dân làng", GetCorrectTreatmentCount, 15);
        AddQuest(quests, "S4_Cure_20", "Chữa khỏi 20 ca bệnh cho dân làng", GetCorrectTreatmentCount, 20);

        AddQuest(quests, "S4_Level4_2", "Chữa khỏi 2 ca bệnh nặng cấp 4", () => GetLevelCuredValue(4), 2);
        AddQuest(quests, "S4_Level4_3", "Chữa khỏi 3 ca bệnh nặng cấp 4", () => GetLevelCuredValue(4), 3);

        AddQuest(quests, "S4_ThanDuongHu", "Chữa khỏi bệnh Thận dương hư 1 lần", () => GetDiseaseCuredValue("ThanDuongHu"), 1);

        AddQuest(quests, "S4_Gather_25", "Thu hoạch 25 dược liệu trong vườn", GetGatheredTotalValue, 25);
        AddQuest(quests, "S4_Gather_35", "Thu hoạch 35 dược liệu trong vườn", GetGatheredTotalValue, 35);

        AddRandomGardenHerbHarvestQuest(quests, "S4_RandomHerb_8_A", 8);
        AddRandomGardenHerbHarvestQuest(quests, "S4_RandomHerb_10_B", 10);
        AddRandomGardenHerbHarvestQuest(quests, "S4_RandomHerb_12_C", 12);

        AddQuest(quests, "S4_Buy_12", "Mua 12 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 12);
        AddQuest(quests, "S4_Buy_15", "Mua 15 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 15);

        AddQuest(quests, "S4_TamThat_2", "Mua 2 vị Tam thất từ thương nhân", () => GetBoughtHerbValue("Tam thất"), 2);
        AddQuest(quests, "S4_NhucQue_1", "Mua 1 vị Nhục quế từ thương nhân", () => GetBoughtHerbValue("Nhục quế"), 1);

        if (includeRankQuest)
        {
            AddQuest(quests, "S4_Rank_500", "Đạt 500 tín nhiệm để thành Lương Y Đại Việt", () => reputation, danhYTarget);
        }
    }

    private void AddStage5Quests(List<QuestDefinition> quests)
    {
        AddQuest(quests, "S5_Official", "Đến khám bệnh cho quan huyện", () => IsOfficialQuestCompleted() ? 1 : 0, 1);

        AddQuest(quests, "S5_ThatDiet", "Chữa khỏi bệnh cho quan huyện", () => GetDiseaseCuredValue("ThatDietTrungDocDich"), 1);

        AddQuest(quests, "S5_Gather_40", "Thu hoạch 40 dược liệu để dự trữ kho thuốc", GetGatheredTotalValue, 40);
        AddQuest(quests, "S5_Gather_50", "Thu hoạch 50 dược liệu trong vườn dược", GetGatheredTotalValue, 50);

        AddRandomGardenHerbHarvestQuest(quests, "S5_RandomHerb_12_A", 12);
        AddRandomGardenHerbHarvestQuest(quests, "S5_RandomHerb_15_B", 15);

        AddQuest(quests, "S5_AllBasic_6", "Thu hoạch đủ 6 loại dược liệu cơ bản trong vườn", GetUnlockedBasicGardenHerbGroupCount, 6);

        AddQuest(quests, "S5_Buy_15", "Mua 15 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 15);
        AddQuest(quests, "S5_Buy_20", "Mua 20 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 20);

        AddQuest(quests, "S5_HungHoang_1", "Mua 1 vị Hùng hoàng từ thương nhân", () => GetBoughtHerbValue("Hùng hoàng"), 1);
        AddQuest(quests, "S5_HoangLien_1", "Mua 1 vị Hoàng liên từ thương nhân", () => GetBoughtHerbValue("Hoàng liên"), 1);
    }

    private void AddEndGameQuests(List<QuestDefinition> quests)
    {
        AddQuest(quests, "E_Cure_30", "Tiếp tục chữa khỏi 30 ca bệnh cho người dân", GetCorrectTreatmentCount, 30);
        AddQuest(quests, "E_Level4_5", "Chữa thêm 5 ca bệnh nặng cấp 4", () => GetLevelCuredValue(4), 5);
        AddQuest(quests, "E_Gather_80", "Tích lũy 80 dược liệu trong vườn dược", GetGatheredTotalValue, 80);
        AddQuest(quests, "E_Buy_30", "Mua thêm 30 dược liệu từ thương nhân", GetBoughtTotalValue, 30);
    }

    private void AddRandomGardenHerbHarvestQuest(
        List<QuestDefinition> quests,
        string questId,
        int target
    )
    {
        GardenHerbQuestTarget herbTarget = GetSavedGardenHerbQuestTarget(questId);

        string title =
            "Thu hoạch "
            + target
            + " lần vị "
            + herbTarget.displayName
            + " trong vườn";

        AddQuest(
            quests,
            questId,
            title,
            () => GetGatheredHerbValueAny(herbTarget.aliases),
            target
        );
    }

    private GardenHerbQuestTarget GetSavedGardenHerbQuestTarget(string questId)
    {
        GardenHerbQuestTarget[] targets = GetBasicGardenHerbTargets();

        if (targets == null || targets.Length == 0)
            return new GardenHerbQuestTarget("Gừng", new string[] { "Gừng", "Sinh khương" });

        string key = "Quest_GardenHerbTarget_" + questId;
        int savedIndex = UnityEngine.PlayerPrefs.GetInt(key, -1);

        if (savedIndex >= 0 && savedIndex < targets.Length)
            return targets[savedIndex];

        int newIndex = UnityEngine.Random.Range(0, targets.Length);

        UnityEngine.PlayerPrefs.SetInt(key, newIndex);
        UnityEngine.PlayerPrefs.Save();

        return targets[newIndex];
    }

    private GardenHerbQuestTarget[] GetBasicGardenHerbTargets()
    {
        return new GardenHerbQuestTarget[]
        {
            new GardenHerbQuestTarget(
                "Gừng",
                new string[] { "Gừng", "Sinh khương" }
            ),

            new GardenHerbQuestTarget(
                "Tía tô",
                new string[] { "Tía tô", "Tía Tô" }
            ),

            new GardenHerbQuestTarget(
                "Kinh giới",
                new string[] { "Kinh giới", "Kinh Giới" }
            ),

            new GardenHerbQuestTarget(
                "Bạc hà",
                new string[] { "Bạc hà", "Bạc Hà" }
            ),

            new GardenHerbQuestTarget(
                "Diếp cá",
                new string[] { "Diếp cá", "Diếp Cá" }
            ),

            new GardenHerbQuestTarget(
                "Bồ công anh",
                new string[] { "Bồ công anh", "Bồ Công Anh" }
            )
        };
    }

    private int GetGatheredHerbValueAny(params string[] herbNames)
    {
        int highestValue = 0;

        if (herbNames == null)
            return highestValue;

        for (int i = 0; i < herbNames.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(herbNames[i]))
                continue;

            int value = GetGatheredHerbValue(herbNames[i]);

            if (value > highestValue)
                highestValue = value;
        }

        return highestValue;
    }

    private int GetUnlockedBasicGardenHerbGroupCount()
    {
        int count = 0;

        if (GetGatheredHerbValueAny("Gừng", "Sinh khương") > 0)
            count++;

        if (GetGatheredHerbValueAny("Tía tô", "Tía Tô") > 0)
            count++;

        if (GetGatheredHerbValueAny("Kinh giới", "Kinh Giới") > 0)
            count++;

        if (GetGatheredHerbValueAny("Bạc hà", "Bạc Hà") > 0)
            count++;

        if (GetGatheredHerbValueAny("Diếp cá", "Diếp Cá") > 0)
            count++;

        if (GetGatheredHerbValueAny("Bồ công anh", "Bồ Công Anh") > 0)
            count++;

        return count;
    }

    private struct GardenHerbQuestTarget
    {
        public string displayName;
        public string[] aliases;

        public GardenHerbQuestTarget(string displayName, string[] aliases)
        {
            this.displayName = displayName;
            this.aliases = aliases;
        }
    }

    private void AddQuest(
        List<QuestDefinition> quests,
        string id,
        string title,
        Func<int> getCurrentValue,
        int target
    )
    {
        quests.Add(new QuestDefinition(id, title, getCurrentValue, target));
    }

    private QuestDefinition FindQuestById(List<QuestDefinition> quests, string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i] != null && quests[i].Id == id)
                return quests[i];
        }

        return null;
    }
}