using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] npcPrefab; // NPC 프리팹
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
        // 1. 프리팹 배열이 비어있는지 확인 (에러 방지)
        if (npcPrefab == null || npcPrefab.Length == 0)
        {
            Debug.LogWarning("NPCSpawner: NPC 프리팹이 할당되지 않았습니다!");
            return;
        }

        int targetIndex;

        if (data.prefabIndex == -1)
        {
            // 랜덤으로 외형을 정하고, 데이터에 영구 저장합니다.
            targetIndex = Random.Range(0, npcPrefab.Length);
            data.prefabIndex = targetIndex;
        }
        else
        {
            // 2. 이미 외형이 정해진 손님(씬 재진입)이라면 저장된 번호를 사용합니다.
            targetIndex = data.prefabIndex;
        }




        // 결정된 번호의 프리팹을 소환
        GameObject npc = Instantiate(npcPrefab[targetIndex], GetEntrancePosition(), Quaternion.identity);

        // 4. 생성된 NPC에 데이터 연결
        NPCAI npcAI = npc.GetComponent<NPCAI>();
        if (npcAI != null)
        {
            npcAI.myData = data;
        }
    }
}
