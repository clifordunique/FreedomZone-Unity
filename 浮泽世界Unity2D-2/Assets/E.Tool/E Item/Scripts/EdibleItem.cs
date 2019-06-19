using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName= "E Item/食品", order = 1)]
public class EdibleItem : UsableItem
{
    [Header("可食用物品相关信息")]
    [Tooltip("生机变化")] public int usageHealth;
    [Tooltip("脑力变化")] public int usageMind;
    [Tooltip("经验变化")] public int usageExperience;
    [Tooltip("宠物生机变化")] public int usagePetHealth;

    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        player.Health += usageHealth;
        player.Mind += usageMind;
        player.Experience += usageExperience;
        if (player.ActivePet != null) player.ActivePet.Health += usagePetHealth;

        // decrease amount
        ItemSlot slot = player.inventory[inventoryIndex];
        slot.DecreaseAmount(1);
        player.inventory[inventoryIndex] = slot;
    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{生机变化}", usageHealth.ToString());
        tip.Replace("{脑力变化}", usageMind.ToString());
        tip.Replace("{经验变化}", usageExperience.ToString());
        tip.Replace("{宠物生机变化}", usagePetHealth.ToString());
        return tip.ToString();
    }
}
