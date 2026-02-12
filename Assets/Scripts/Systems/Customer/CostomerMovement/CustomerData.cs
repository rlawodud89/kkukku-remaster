using UnityEngine;

[System.Serializable]
public class CustomerData
{
    public enum State { MovingToWardrobe, Deciding, MovingToCashier, Paying, Leaving }

    public int id;
    public State currentState;
    public Vector3Int currentTilePos;
    public int selectedItemID = -1; // -1이면 아직 안 고름
    public float timer; // 씬 밖에서 행동 완료까지 남은 시간 계산용
    public int prefabIndex = -1; // 💡 추가: 내 외형이 프리팹 배열의 몇 번째인지 기억하는 번호

    public CustomerData(int id)
    {
        this.id = id;
        this.currentState = State.MovingToWardrobe;
    }
}