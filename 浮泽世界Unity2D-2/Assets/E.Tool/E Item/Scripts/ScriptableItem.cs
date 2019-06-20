﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using E.Utility;

[CreateAssetMenu(menuName= "E Item/基础物品", order = 0)]
public partial class ScriptableItem : StaticScriptableObject<ScriptableItem>
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