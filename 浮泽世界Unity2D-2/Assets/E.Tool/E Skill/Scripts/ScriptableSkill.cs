using E.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract partial class ScriptableSkill : ScriptableObjectDictionary<ScriptableSkill>
{
    [Header("【基本信息】")]
    public Sprite image;
    public bool followupDefaultAttack;
    public bool learnDefault; // normal attack etc.
    public bool showCastBar;
    public bool cancelCastIfTargetDied; // direct hit may want to cancel if target died. buffs doesn't care. etc.
    [SerializeField, TextArea(1, 30)] protected string toolTip;

    [Header("【使用限制】")]
    public ScriptableSkill predecessor; // this skill has to be learned first
    public int predecessorLevel = 1; // level of predecessor skill that is required
    public string requiredWeaponCategory = ""; // "" = no weapon needed; "Weapon" = requires a weapon, "WeaponSword" = requires a sword weapon, etc.
    public LinearInt requiredLevel; // required player level
    public LinearLong requiredSkillExperience;

    [Header("【属性】")]
    public int maxLevel = 1;
    public LinearInt mindCosts;
    public LinearFloat castTime;
    public LinearFloat cooldown;
    public LinearFloat castRange;

    [Header("【声音】")]
    public AudioClip castSound;

    public virtual bool CheckSelf(Entity caster, int skillLevel)
    {
        // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
        return caster.Health > 0 &&
               caster.Mind >= mindCosts.Get(skillLevel) &&
               caster.GetEquippedWeaponCategory().StartsWith(requiredWeaponCategory);
    }

    // 2. target check: can we cast this skill 'here' or on this 'target'?
    // => e.g. sword hit checks if target can be attacked
    //         skill shot checks if the position under the mouse is valid etc.
    //         buff checks if it's a friendly player, etc.
    // ===> IMPORTANT: this function HAS TO correct the target if necessary,
    //      e.g. for a buff that is cast on 'self' even though we target a NPC
    //      while casting it
    public abstract bool CheckTarget(Entity caster);

    // 3. distance check: do we need to walk somewhere to cast it?
    //    e.g. on a monster that's far away
    //    => returns 'true' if distance is fine, 'false' if we need to move
    // (has corrected target already)
    public abstract bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination);

    // 4. apply skill: deal damage, heal, launch projectiles, etc.
    // (has corrected target already)
    public abstract void Apply(Entity caster, int skillLevel);

    // events for client sided effects /////////////////////////////////////////
    // [Client]
    public virtual void OnCastStarted(Entity caster)
    {
        if (caster.audioSource != null && castSound != null)
            caster.audioSource.PlayOneShot(castSound);
    }

    // [Client]
    public virtual void OnCastFinished(Entity caster) {}

    public virtual string ToolTip(int level, bool showRequirements = false)
    {
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{名称}", name);
        tip.Replace("{等级}", level.ToString());
        tip.Replace("{持续时间}", Utils.PrettySeconds(castTime.Get(level)));
        tip.Replace("{冷却时间}", Utils.PrettySeconds(cooldown.Get(level)));
        tip.Replace("{施法距离}", castRange.Get(level).ToString());
        tip.Replace("{脑力消耗}", mindCosts.Get(level).ToString());

        // only show requirements if necessary
        if (showRequirements)
        {
            tip.Append("\n<b><i>Required Level: " + requiredLevel.Get(1) + "</i></b>\n" +
                       "<b><i>Required Skill Exp.: " + requiredSkillExperience.Get(1) + "</i></b>\n");
            if (predecessor != null)
                tip.Append("<b><i>Required Skill: " + predecessor.name + " Lv. " + predecessorLevel + " </i></b>\n");
        }

        return tip.ToString();
    }
}
