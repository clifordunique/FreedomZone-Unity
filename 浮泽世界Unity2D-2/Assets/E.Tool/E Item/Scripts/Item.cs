using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public partial struct Item
{
    // 动态信息
    // 召唤相关
    public GameObject summoned; // summonable that's currently summoned
    public float summonedHealth; // stored in item while summonable unsummoned
    public int summonedLevel; // stored in item while summonable unsummoned
    public long summonedExperience; // stored in item while summonable unsummoned

    // 静态信息 只读
    public int Hash;
    public ScriptableItem Data
    {
        get
        {
            if (!ScriptableItem.Dictionary.ContainsKey(Hash))
                throw new KeyNotFoundException("哈希值{" + Hash + "}找不到对应ScriptableItem，确保所有ScriptableItem都在Resources文件夹中，以便正确加载它们。");
            return ScriptableItem.Dictionary[Hash];
        }
    }
    public string Name => Data.name;
    public int MaxStack => Data.maxStack;
    public long BuyPrice => Data.buyPrice;
    public long SellPrice => Data.sellPrice;
    public long ItemMallPrice => Data.itemMallPrice;
    public bool Sellable => Data.sellable;
    public bool Tradable => Data.tradable;
    public bool Destroyable => Data.destroyable;
    public Sprite Image => Data.image;

    // 构造器
    public Item(ScriptableItem data)
    {
        Hash = data.name.GetStableHashCode();
        summoned = null;
        summonedHealth = data is SummonableItem ? ((SummonableItem)data).summonPrefab.HealthMax : 0;
        summonedLevel = data is SummonableItem ? 1 : 0;
        summonedExperience = 0;
    }

    // tooltip
    public string ToolTip()
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(Data.ToolTip());
        tip.Replace("{召唤物生机}", summonedHealth.ToString());
        tip.Replace("{召唤物等级}", summonedLevel.ToString());
        tip.Replace("{召唤物经验}", summonedExperience.ToString());

        return tip.ToString();
    }
}
