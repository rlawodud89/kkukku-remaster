using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] npcPrefab; // NPC 프리팹
    public static NPCSpawner Instance;
    public Transform entranceTransform; // 가게 입구

    private int lastSpawnedIndex = -1;

    void Awake()
    {
        if (Instance == null) Instance = this;
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
            if (npcPrefab.Length > 1) // 프리팹 종류가 2개 이상일 때만 다르게 뽑기 시도
            {
                // 이전 번호와 다를 때까지 계속 다시 뽑기
                do
                {
                    targetIndex = Random.Range(0, npcPrefab.Length);
                }
                while (targetIndex == lastSpawnedIndex);
            }
            else
            {
                targetIndex = Random.Range(0, npcPrefab.Length);
                data.prefabIndex = targetIndex;
            }

            data.prefabIndex = targetIndex;
            lastSpawnedIndex = targetIndex; // 지금 뽑힌 번호를 기억해둠
        }
        else
        {
            targetIndex = data.prefabIndex;
        }


        // 결정된 번호의 프리팹을 소환
        GameObject npc = Instantiate(npcPrefab[targetIndex], GetEntrancePosition(), Quaternion.identity);

        // 4. 생성된 NPC에 데이터 연결
        NPCAI npcAI = npc.GetComponent<NPCAI>();
        if (npcAI != null)
        {
            npcAI.myData = data;
            npcAI.SetupSurvivor(data);
        }
    }
}
