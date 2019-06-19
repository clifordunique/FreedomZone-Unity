using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName= "E Item/基础物品", order = 0)]
public partial class ScriptableItem : ScriptableObject
{
    [Header("【基本信息】")]
    [Tooltip("物品图标")] public Sprite image;
    [Tooltip("最大堆叠数量")] public int maxStack;
    [Tooltip("购买价格")] public long buyPrice;
    [Tooltip("出售价格")] public long sellPrice;
    [Tooltip("在线商城价格")] public long itemMallPrice;
    [Tooltip("是否可出售")] public bool sellable;
    [Tooltip("是否可交易")] public bool tradable;
    [Tooltip("是否可破坏")] public bool destroyable;
    [Tooltip("工具提示"), SerializeField, TextArea(1, 30)] protected string toolTip;

    public virtual string ToolTip()
    {
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{名称}", name);
        tip.Replace("{可破坏}", (destroyable ? "是" : "否"));
        tip.Replace("{可出售}", (sellable ? "是" : "否"));
        tip.Replace("{可交易}", (tradable ? "是" : "否"));
        tip.Replace("{购买价}", buyPrice.ToString());
        tip.Replace("{出售价}", sellPrice.ToString());
        return tip.ToString();
    }

    static Dictionary<int, ScriptableItem> cache;
    public static Dictionary<int, ScriptableItem> Dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableItem[] items = Resources.LoadAll<ScriptableItem>("");

                // check for duplicates, then add to cache
                List<string> duplicates = items.ToList().FindDuplicates(item => item.name);
                if (duplicates.Count == 0)
                {
                    cache = items.ToDictionary(item => item.name.GetStableHashCode(), item => item);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources文件夹包含多个同名的ScriptableItem{" + duplicate + "}，如果您使用的是'Warrior / Ring'和'Archer / Ring'等子文件夹，请将它们重命名为'Warrior /（Warrior）Ring'和'Archer /（Archer）Ring'。");
                }
            }
            return cache;
        }
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {
        // make sure that the sell price <= buy price to avoid exploitation
        // (people should never buy an item for 1 gold and sell it for 2 gold)
        sellPrice = Math.Min(sellPrice, buyPrice);
    }
}

// ScriptableItem + Amount is useful for default items (e.g. spawn with 10 potions)
[Serializable]
public struct ScriptableItemAndAmount
{
    public ScriptableItem Item;
    public int Amount;
}