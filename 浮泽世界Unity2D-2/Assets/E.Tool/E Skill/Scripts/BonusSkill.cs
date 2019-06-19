// Base type for bonus skill templates.
// => can be used for passive skills, buffs, etc.
using System.Text;
using UnityEngine;
using Mirror;

public abstract class BonusSkill : ScriptableSkill
{
    [Header("【奖励相关信息】")]
    public LinearInt bonusHealthMax;
    public LinearInt bonusMindMax;
    public LinearInt bonusPowerMax;
    public LinearFloat bonusHealthPercentPerSecond; // 0.1=10%; can be negative too
    public LinearFloat bonusMindPercentPerSecond; // 0.1=10%; can be negative too
    public LinearFloat bonusPowerPercentPerSecond; // 0.1=10%; can be negative too

    public LinearInt bonusStrength;
    public LinearInt bonusDefense;
    public LinearFloat bonusSpeed; // can be negative too
    public LinearInt bonusIntelligence;

    public LinearFloat bonusBlockChance; // range [0,1]
    public LinearFloat bonusCriticalChance; // range [0,1]


    // tooltip
    public override string ToolTip(int skillLevel, bool showRequirements = false)
    {
        StringBuilder tip = new StringBuilder(base.ToolTip(skillLevel, showRequirements));
        tip.Replace("{BONUSHEALTHMAX}", bonusHealthMax.Get(skillLevel).ToString());
        tip.Replace("{BONUSMANAMAX}", bonusMindMax.Get(skillLevel).ToString());
        tip.Replace("{增加体力上限}", bonusPowerMax.Get(skillLevel).ToString());
        tip.Replace("{BONUSHEALTHPERCENTPERSECOND}", Mathf.RoundToInt(bonusHealthPercentPerSecond.Get(skillLevel) * 100).ToString());
        tip.Replace("{BONUSMANAPERCENTPERSECOND}", Mathf.RoundToInt(bonusMindPercentPerSecond.Get(skillLevel) * 100).ToString());
        tip.Replace("{增加体力恢复速度}", Mathf.RoundToInt(bonusPowerPercentPerSecond.Get(skillLevel) * 100).ToString());

        tip.Replace("{BONUSDAMAGE}", bonusStrength.Get(skillLevel).ToString());
        tip.Replace("{BONUSDEFENSE}", bonusDefense.Get(skillLevel).ToString());
        tip.Replace("{BONUSSPEED}", bonusSpeed.Get(skillLevel).ToString("F2"));
        tip.Replace("{增加智力}", bonusIntelligence.Get(skillLevel).ToString());

        tip.Replace("{BONUSBLOCKCHANCE}", Mathf.RoundToInt(bonusBlockChance.Get(skillLevel) * 100).ToString());
        tip.Replace("{BONUSCRITICALCHANCE}", Mathf.RoundToInt(bonusCriticalChance.Get(skillLevel) * 100).ToString());
        return tip.ToString();
    }
}
