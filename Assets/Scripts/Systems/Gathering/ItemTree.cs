using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTree : MonoBehaviour
{
    [SerializeField] private List<ItemFruit> fruits;
    [SerializeField] private RectTransform fruitField;

    void Start()
    {
        GatheringManager manager = GatheringManager.Instance;
        manager.RegisterItemTree(this);
    }

    public void ResetItemTree()
    {
        List<Rect> fruitRects = new List<Rect>();

        foreach (var fruit in fruits)
        {
            Vector2 newPos = GetNewPos(fruit, fruitRects);
            fruit.ResetItemFruit(newPos);

            fruitRects.Add(fruit.GetFruitPanelRectAtPosition(newPos));
        }
    }

    private Vector2 GetNewPos(ItemFruit newFruit, List<Rect> fruitRects)
    {
        const int MAX_TRY = 30;

        Vector2 fieldSize = fruitField.rect.size;
        Vector2 fruitSize = newFruit.GetFruitPanelRect().size;

        float minX = -fieldSize.x / 2 + fruitSize.x / 2;
        float maxX = fieldSize.x / 2 - fruitSize.x / 2;
        float minY = -fieldSize.y / 2 + fruitSize.y / 2;
        float maxY = fieldSize.y / 2 - fruitSize.y / 2;

        for (int i = 0; i < MAX_TRY; i++)
        {
            Vector2 randomNewPos = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            Rect newRect = newFruit.GetFruitPanelRectAtPosition(randomNewPos);

            bool overlap = false;
            foreach (var existRect in fruitRects)
            {
                if (existRect.Overlaps(newRect))
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
            {
                Debug.Log("UI 겹치지 않음");
                return randomNewPos;
            }
        }

        Debug.Log("겹치지 않는 좌표 찾지 못함");
        return Vector2.zero;
    }


}
