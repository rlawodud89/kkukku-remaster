using UnityEngine;

[CreateAssetMenu(fileName = "Letter", menuName = "QuestSO/LetterSO")]
public class LetterSO : ScriptableObject
{
    public string letterName;
    public string description;
    public Sprite sleepImage;
}
