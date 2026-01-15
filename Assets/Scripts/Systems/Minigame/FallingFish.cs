using UnityEngine;

public class FallingFish : MonoBehaviour
{
    private float fallSpeed;
    private FishingGameManager gameManager;

    public void Setup(float speed, FishingGameManager manager)
    {
        fallSpeed = speed;
        gameManager = manager;
    }

    void Update()
    {
        // 아래로 이동
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // 화면 밖으로 나가면 Miss 처리 (예: Y좌표 -5 이하)
        if (transform.position.y < -6f)
        {
            gameManager.OnFishMiss(this);
            Destroy(gameObject);
        }
    }
}