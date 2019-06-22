﻿// Buffs are like Skills, but for the Buffs list.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;

[Serializable]
public partial struct Buff
{
    // hashcode used to reference the real ScriptableSkill (can't link to data
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int Hash;

    // dynamic stats (cooldowns etc.)
    public int level;
    public double buffTimeEnd; // server time. double for long term precision.

    // constructors
    public Buff(BuffSkill data, int level)
    {
        Hash = data.name.GetStableHashCode();
        this.level = level;
        buffTimeEnd = NetworkTime.time + data.buffTime.Get(level); // start buff immediately
    }

    // wrappers for easier access
    public BuffSkill data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableSkill.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableSkill.Dictionary.ContainsKey(Hash))
                throw new KeyNotFoundException("There is no ScriptableSkill with hash=" + Hash + ". Make sure that all ScriptableSkills are in the Resources folder so they are loaded properly.");
            return (BuffSkill)ScriptableSkill.Dictionary[Hash];
        }
    }
    public string name => data.name;
    public Sprite image => data.image;
    public float buffTime => data.buffTime.Get(level);

    public int bonusHealthMax => data.bonusHealthMax.Get(level);
    public int bonusMindMax => data.bonusMindMax.Get(level);
    public int bonusPowerMax => data.bonusPowerMax.Get(level);
    public float bonusHealthPercentPerSecond => data.bonusHealthPercentPerSecond.Get(level);
    public float bonusMindPercentPerSecond => data.bonusMindPercentPerSecond.Get(level);
    public float bonusPowerPercentPerSecond => data.bonusPowerPercentPerSecond.Get(level);

    public int bonusDamage => data.bonusStrength.Get(level);
    public int bonusDefense => data.bonusDefense.Get(level);
    public float bonusSpeed => data.bonusSpeed.Get(level);
    public int bonusIntelligence => data.bonusIntelligence.Get(level);

    public float bonusBlockChance => data.bonusBlockChance.Get(level);
    public float bonusCriticalChance => data.bonusCriticalChance.Get(level);

    public int maxLevel => data.maxLevel;

    // tooltip - runtime part
    public string ToolTip()
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(data.ToolTip(level));

        return tip.ToString();
    }

    public float BuffTimeRemaining()
    {
        // how much time remaining until the buff ends? (using server time)
        return NetworkTime.time >= buffTimeEnd ? 0 : (float)(buffTimeEnd - NetworkTime.time);
    }
}

public class SyncListBuff : SyncList<Buff> { }
