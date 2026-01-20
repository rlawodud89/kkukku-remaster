using UnityEngine;

[CreateAssetMenu(fileName = "Customer", menuName = "CustomerSO/CustomerSO")]
public class CustomerSO : ScriptableObject
{
    public string customerName;
    public GameObject prefab;
    public string smallTalk;
}
