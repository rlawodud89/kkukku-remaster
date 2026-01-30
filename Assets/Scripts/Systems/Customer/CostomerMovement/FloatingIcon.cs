using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    public float amplitude = 0.2f; // 위아래 이동 범위
    public float frequency = 1.0f; // 이동 속도

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    void Start()
    {
        posOffset = transform.localPosition;
    }

    void Update()
    {
        tempPos = posOffset;
        // 사인 함수를 이용해 부드러운 위아래 움직임 구현
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        transform.localPosition = tempPos;
    }
}