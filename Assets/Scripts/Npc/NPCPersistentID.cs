using UnityEngine;

public class NPCPersistentID : MonoBehaviour
{
    [SerializeField]
    private int npcID;

    public int NPCID => npcID;
}