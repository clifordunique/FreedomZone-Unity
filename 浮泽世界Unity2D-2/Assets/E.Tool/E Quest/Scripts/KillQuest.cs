// a simple kill quest example.
// inherit from KillQuest and overwrite OnKilled for more advanced goals like
// 'kill a player 10 levels above you' or 'kill a pet in a guild war' etc.
using UnityEngine;
using System.Text;

[CreateAssetMenu(menuName = "E Quest/击杀", order = 2)]
public class KillQuest : ScriptableQuest
{
    [Header("执行")]
    public Monster killTarget;
    public int killAmount;

    // events //////////////////////////////////////////////////////////////////
    public override void OnKilled(Player player, int questIndex, Entity victim)
    {
        // not done yet, and same name as prefab? (hence same monster?)
        Quest quest = player.quests[questIndex];
        if (quest.progress < killAmount && victim.name == killTarget.name)
        {
            // increase int field in quest (up to 'amount')
            ++quest.progress;
            player.quests[questIndex] = quest;
        }
    }

    // fulfillment /////////////////////////////////////////////////////////////
    public override bool IsFulfilled(Player player, Quest quest)
    {
        return quest.progress >= killAmount;
    }

    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip(Player player, Quest quest)
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(base.ToolTip(player, quest));
        tip.Replace("{击杀目标}", killTarget != null ? killTarget.name : "");
        tip.Replace("{击杀数量}", killAmount.ToString());
        tip.Replace("{击杀进度}", quest.progress.ToString());
        return tip.ToString();
    }
}
