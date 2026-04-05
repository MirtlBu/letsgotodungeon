using UnityEngine;

public class WellInteraction : InteractionZone
{
    [SerializeField] private int coinCost = 1;
    [SerializeField] private int coinReward = 5;

    protected override void OnInteract()
    {
        if (CoinCounter.Instance.GetCount() < coinCost)
        {
            ShowResult("No money!");
            return;
        }

        CoinCounter.Instance.Add(-coinCost);

        if (Random.value < 0.6666f)
        {
            ShowResult("Nothing...");
        }
        else
        {
            int reward = Random.Range(1, coinReward + 1);
            CoinCounter.Instance.Add(reward);
            ShowResult($"You received {reward} coins!");
        }
    }
}
