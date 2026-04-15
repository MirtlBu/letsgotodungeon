using UnityEngine;

public enum QuestObjectiveType { KillEnemies, CollectCoins }
public enum QuestRewardType    { None, Sword, Coins }

[CreateAssetMenu(fileName = "Quest", menuName = "Game/Quest")]
public class QuestSO : ScriptableObject
{
    public string questId;
    public string questName;
    [TextArea] public string description;

    [Header("Objective")]
    public QuestObjectiveType objectiveType;
    public int targetCount;

    [Header("Reward")]
    public QuestRewardType rewardType;
    public int rewardCoins;
}
