// only usable items need minLevel and usage functions
using System.Text;
using UnityEngine;

public abstract class UsableItem : ScriptableItem
{
    [Header("【使用限制】")]
    [Tooltip("使用者等级限制")] public int minLevel; // level required to use the item
    [Tooltip("冷却时间限制")] public TargetBuffSkill cooldownBuff;

    // [Server] and [Client] CanUse check for UI, Commands, etc.
    public virtual bool CanUse(Player player, int inventoryIndex)
    {
        // check level etc. and make sure that cooldown buff elapsed (if any)
        return player.level >= minLevel && (cooldownBuff == null || player.GetBuffIndexByName(cooldownBuff.name) == -1);
    }

    // [Server] Use logic: make sure to call base.Use() in overrides too.
    public virtual void Use(Player player, int inventoryIndex)
    {
        // start cooldown buff (if any)
        if (cooldownBuff != null)
        {
            // set target to player before applying buff
            Entity oldTarget = player.Target;
            player.Target = player;

            // apply the buff with skill level 1
            cooldownBuff.Apply(player, 1);

            // restore target
            player.Target = oldTarget;
        }
    }

    // [Client] OnUse Rpc callback for effects, sounds, etc.
    // -> can't pass slotIndex because .Use might clear it before getting here already
    public virtual void OnUsed(Player player) {}

    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{使用者等级限制}", minLevel.ToString());
        return tip.ToString();
    }
}
