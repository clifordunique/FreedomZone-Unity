using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class ScriptableQuest : ScriptableObject
{
    [Header("通用")]
    [SerializeField, TextArea(1, 30)] protected string toolTip; // not public, use ToolTip()

    [Header("要求")]
    public int requiredLevel; // player.level
    public ScriptableQuest predecessor; // this quest has to be completed first

    [Header("奖励")]
    public long rewardGold;
    public long rewardExperience;
    public ScriptableItem rewardItem;

    // events to hook into /////////////////////////////////////////////////////
    public virtual void OnKilled(Player player, int questIndex, Entity victim) {}
    public virtual void OnLocation(Player player, int questIndex, Collider2D location) {}

    // fulfillment /////////////////////////////////////////////////////////////
    // we pass the Quest instead of an index for ease of use and because we are
    // read-only here anyway
    public abstract bool IsFulfilled(Player player, Quest quest);

    // OnComplete is called when the quest is completed at the npc.
    // -> can be used to remove quest items from the inventory, etc.
    public virtual void OnCompleted(Player player, Quest quest) {}

    public virtual string ToolTip(Player player, Quest quest)
    {
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{名称}", name);
        tip.Replace("{奖励金币}", rewardGold.ToString());
        tip.Replace("{奖励经验}", rewardExperience.ToString());
        tip.Replace("{奖励物品}", rewardItem != null ? rewardItem.name : "");
        return tip.ToString();
    }

    static Dictionary<int, ScriptableQuest> cache;
    public static Dictionary<int, ScriptableQuest> Dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableQuests in resources
                ScriptableQuest[] quests = Resources.LoadAll<ScriptableQuest>("");

                // check for duplicates, then add to cache
                List<string> duplicates = quests.ToList().FindDuplicates(quest => quest.name);
                if (duplicates.Count == 0)
                {
                    cache = quests.ToDictionary(quest => quest.name.GetStableHashCode(), quest => quest);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources文件夹包含多个同名的ScriptableQuests{" + duplicate + "}，如果您使用的是'Warrior / BeginnerQuest'和'Archer / BeginnerQuest'等子文件夹，请将它们重命名为'Warrior /（Warrior）BeginnerQuest'和'Archer /（Archer）BeginnerQuest'。");
                }
            }
            return cache;
        }
    }
}
