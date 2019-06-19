using System.Text;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName= "E Item/装备", order = 2)]
public class EquipmentItem : UsableItem
{
    [Header("【装备相关信息】")]
    [Tooltip("分类")] public string category;
    [Tooltip("生机加成")] public int healthBonus;
    [Tooltip("脑力加成")] public int mindBonus;
    [Tooltip("体力加成")] public int powerBonus;
    [Tooltip("伤害加成")] public int damageBonus;
    [Tooltip("防御加成")] public int defenseBonus;
    [Tooltip("智力加成")] public int intelligenceBonus;
    [Tooltip("眩晕率加成"), Range(0, 1)] public float blockChanceBonus;
    [Tooltip("暴击率加成"), Range(0, 1)] public float criticalChanceBonus;

    // animated equipment sprites. list instead of array so that we can pass it
    // around without copying the whole thing each time
    public List<Sprite> sprites = new List<Sprite>();

    // usage
    // -> can we equip this into any slot?
    public override bool CanUse(Player player, int inventoryIndex)
    {
        return FindEquipableSlotFor(player, inventoryIndex) != -1;
    }

    // can we equip this item into this specific equipment slot?
    public bool CanEquip(Player player, int inventoryIndex, int equipmentIndex)
    {
        string requiredCategory = player.equipmentInfo[equipmentIndex].requiredCategory;
        return base.CanUse(player, inventoryIndex) && requiredCategory != "" && category.StartsWith(requiredCategory);
    }

    int FindEquipableSlotFor(Player player, int inventoryIndex)
    {
        for (int i = 0; i < player.equipment.Count; ++i)
            if (CanEquip(player, inventoryIndex, i))
                return i;
        return -1;
    }

    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        // find a slot that accepts this category, then equip it
        int slot = FindEquipableSlotFor(player, inventoryIndex);
        if (slot != -1)
        {
            // reuse Player.SwapInventoryEquip function for validation etc.
            player.SwapInventoryEquip(inventoryIndex, slot);
        }
    }

    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{分类}", category);

        tip.Replace("{生机检查}", healthBonus.ToString());
        tip.Replace("{脑力加成}", mindBonus.ToString());
        tip.Replace("{体力加成}", powerBonus.ToString());

        tip.Replace("{伤害加成}", damageBonus.ToString());
        tip.Replace("{防御加成}", defenseBonus.ToString());
        tip.Replace("{智慧加成}", intelligenceBonus.ToString());

        tip.Replace("{眩晕率加成}", Mathf.RoundToInt(blockChanceBonus * 100).ToString());
        tip.Replace("{暴击率加成}", Mathf.RoundToInt(criticalChanceBonus * 100).ToString());
        return tip.ToString();
    }
}
