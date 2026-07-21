using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NPCSaveManager : MonoBehaviour
{
    public static NPCSaveManager Instance { get; private set; }

    private const string SaveFileName = "npc_positions.json";

    private string SavePath =>
        GameSavePath.GetSavePath(SaveFileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Nếu trước đây từng lưu ở thư mục project thì chuyển sang thư mục save chuẩn
        GameSavePath.MigrateLegacyRootSave(SaveFileName);
    }

    public void SaveNPCs()
    {
        NPCPersistentID[] npcs =
            FindObjectsByType<NPCPersistentID>(FindObjectsInactive.Exclude);

        NPCSaveFile saveFile = new NPCSaveFile();

        foreach (NPCPersistentID npc in npcs)
        {
            if (npc == null)
                continue;

            NPCSaveData data = new NPCSaveData
            {
                npcID = npc.NPCID,
                posX = npc.transform.position.x,
                posY = npc.transform.position.y,
                posZ = npc.transform.position.z
            };

            saveFile.npcList.Add(data);
        }

        string json = JsonUtility.ToJson(saveFile, true);

        File.WriteAllText(SavePath, json);

        Debug.Log("NPC Save Folder : " + GameSavePath.GetSaveFolderPath());
        Debug.Log("NPC Save Path   : " + SavePath);
        Debug.Log("Đã lưu " + saveFile.npcList.Count + " NPC.");
    }

    public void LoadNPCs()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("===== LOAD NPC =====");
            Debug.Log("Không tìm thấy file: " + SavePath);
            return;
        }

        string json = File.ReadAllText(SavePath);

        NPCSaveFile saveFile = JsonUtility.FromJson<NPCSaveFile>(json);

        if (saveFile == null || saveFile.npcList == null)
        {
            Debug.LogWarning("File NPC rỗng.");
            return;
        }

        NPCPersistentID[] npcs =
            FindObjectsByType<NPCPersistentID>(FindObjectsInactive.Exclude);

        Dictionary<int, NPCPersistentID> npcDictionary =
            new Dictionary<int, NPCPersistentID>();

        foreach (NPCPersistentID npc in npcs)
        {
            if (npc == null)
                continue;

            npcDictionary[npc.NPCID] = npc;
        }

        int loadedCount = 0;

        foreach (NPCSaveData data in saveFile.npcList)
        {
            if (!npcDictionary.TryGetValue(data.npcID, out NPCPersistentID npc))
                continue;

            npc.transform.position = new Vector3(
                data.posX,
                data.posY,
                data.posZ
            );

            loadedCount++;
        }

        Debug.Log("Đã load " + loadedCount + " NPC.");
    }

    public void DeleteNPCSave()
    {
        GameSavePath.DeleteSaveAndLegacy(SaveFileName);
    }
}