using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringManager : MonoBehaviour
{
    public static GatheringManager Instance;

    [Header("전체 제한시간")]
    [SerializeField] private float timeLimit = 2f;

    private List<ItemTree> trees = new List<ItemTree>();

    private float timer;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartRound();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ResetAllButtons();
            StartRound();
        }
    }

    public void RegisterItemTree(ItemTree itemTree)
    {
        if (!trees.Contains(itemTree))
            trees.Add(itemTree);
    }

    void StartRound()
    {
        timer = timeLimit;
    }

    void ResetAllButtons()
    {
        foreach (var itemTree in trees)
        {
            itemTree.ResetItemTree();
        }
    }
}
