using UnityEngine;

[CreateAssetMenu(fileName = "GardenPlantData", menuName = "Grand Healer/Garden Plant Data")]
public class GardenPlantData : ScriptableObject
{
    [Header("Tên hiển thị trong UI")]
    public string plantName;

    [Header("Icon hiện trong UI chọn cây")]
    public Sprite iconSprite;

    [Header("Sprite cây non khi mới trồng")]
    public Sprite seedlingSprite;

    [Header("Sprite cây lớn khi đã chín")]
    public Sprite matureSprite;

    [Header("Dược liệu nhận khi thu hoạch")]
    public HerbData rewardHerb;

    [Header("Số lượng nhận khi thu hoạch")]
    public int harvestAmount = 1;

    [Header("Thời gian phát triển")]
    public float growDurationSeconds = 60f;
}