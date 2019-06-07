﻿// Group heal that heals all entities of same type in cast range
// => player heals players in cast range
// => monster heals monsters in cast range
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Skill/Area Heal", order=999)]
public class AreaHealSkill : HealSkill
{
    public override bool CheckTarget(Entity caster)
    {
        // no target necessary, but still set to self so that LookAt(target)
        // doesn't cause the player to look at a target that doesn't even matter
        caster.Target = caster;
        return true;
    }

    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
    {
        // can cast anywhere
        destination = caster.transform.position;
        return true;
    }

    public override void Apply(Entity caster, int skillLevel)
    {
        // candidates hashset to be 100% sure that we don't apply an area skill
        // to a candidate twice. this could happen if the candidate has more
        // than one collider (which it often has).
        HashSet<Entity> candidates = new HashSet<Entity>();

        // find all entities of same type in castRange around the caster
        Collider2D[] colliders = Physics2D.OverlapCircleAll(caster.transform.position, castRange.Get(skillLevel));
        foreach (Collider2D co in colliders)
        {
            Entity candidate = co.GetComponentInParent<Entity>();
            if (candidate != null &&
                candidate.Health > 0 && // can't heal dead people
                candidate.GetType() == caster.GetType()) // only on same type
            {
                candidates.Add(candidate);
            }
        }

        // apply to all candidates
        foreach (Entity candidate in candidates)
        {
            candidate.Health += healsHealth.Get(skillLevel);
            candidate.Mind += healsMana.Get(skillLevel);

            // show effect on candidate
            SpawnEffect(caster, candidate);
        }
    }
}
