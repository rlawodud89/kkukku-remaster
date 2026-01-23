using UnityEngine;
using System.Collections.Generic;

public class CashierManager : MonoBehaviour
{
    public static CashierManager Instance;
    public Transform cashierPos; // 계산대 입구 위치
    public float queueSpacing = 1.0f; // 줄 서는 간격

    // 현재 줄 서 있는 NPC 리스트
    private List<NPCAI> waitingQueue = new List<NPCAI>();

    void Awake() { Instance = this; }

    // 줄 서기 요청
    public void JoinQueue(NPCAI npc)
    {
        waitingQueue.Add(npc);
        UpdateQueuePositions(); // 줄 위치 갱신
    }

    // 줄 위치 계산 (0번은 계산대 바로 앞, 1번은 그 뒤...)
    public Vector3 GetQueuePosition(NPCAI npc)
    {
        int index = waitingQueue.IndexOf(npc);
        // 계산대 위치에서 아래쪽(또는 뒤쪽)으로 간격만큼 띄워서 좌표 계산
        return cashierPos.position + (Vector3.down * index * queueSpacing);
    }

    // 계산 완료 후 한 칸씩 당기기
    public void LeaveQueue(NPCAI npc)
    {
        waitingQueue.Remove(npc);
        UpdateQueuePositions();
    }

    private void UpdateQueuePositions()
    {
        // 줄 서 있는 모든 NPC에게 새 목표 지점으로 이동하라고 명령
        foreach (var npc in waitingQueue)
        {
            npc.MoveToQueuePoint();
        }
    }

    public bool IsItMyTurn(NPCAI npc)
    {
        // 리스트가 비어있지 않고, 리스트의 첫 번째(0번)가 나인 경우 true 반환
        if (waitingQueue.Count > 0 && waitingQueue[0] == npc)
        {
            return true;
        }
        return false;
    }
}