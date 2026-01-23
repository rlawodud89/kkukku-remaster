using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab; // NPC 프리팹
    public static NPCSpawner Instance;
    public Transform entranceTransform; // 가게 입구

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // 가게 씬이 시작될 때 매니저에 저장된 데이터를 기반으로 NPC들을 다시 소환함
        foreach (var data in ShopManager.Instance.activeCustomers)
        {
            SpawnNPC(data);
        }
    }

    Vector3 GetEntrancePosition()
    {
        if (entranceTransform != null)
        {
            return entranceTransform.position; // 설정한 문 위치 반환
        }
        return Vector3.zero; // 설정 안 됐으면 0,0,0 반환
    }


    public void SpawnNPC(CustomerData data)
    {
        GameObject npc = Instantiate(npcPrefab, GetEntrancePosition(), Quaternion.identity);
        npc.GetComponent<NPCAI>().myData = data; // 데이터 연결
    }
}
