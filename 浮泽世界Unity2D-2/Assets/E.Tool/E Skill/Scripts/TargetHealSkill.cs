using System.Text;
using UnityEngine;
using Mirror;
using E.Utility;

[CreateAssetMenu(menuName="E Skill/目标治疗", order = 2)]
public class TargetHealSkill : HealSkill
{
    public bool canHealSelf = true;
    public bool canHealOthers = false;

    // helper function to determine the target that the skill will be cast on
    // (e.g. cast on self if targeting a monster that isn't healable)
    Entity CorrectedTarget(Entity caster)
    {
        // targeting nothing? then try to cast on self
        if (caster.Target == null)
            return canHealSelf ? caster : null;

        // targeting self?
        if (caster.Target == caster)
            return canHealSelf ? caster : null;

        // targeting someone of same type? buff them or self
        if (caster.Target.GetType() == caster.GetType())
        {
            if (canHealOthers)
                return caster.Target;
            else if (canHealSelf)
                return caster;
            else
                return null;
        }

        // no valid target? try to cast on self or don't cast at all
        return canHealSelf ? caster : null;
    }

    public override bool CheckTarget(Entity caster)
    {
        // correct the target
        caster.Target = CorrectedTarget(caster);

        // can only buff the target if it's not dead
        return caster.Target != null && caster.Target.Health > 0;
    }

    // (has corrected target already)
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
    {
        // target still around?
        if (caster.Target != null)
        {
            destination = caster.Target.collider.ClosestPointOnBounds(caster.transform.position);
            return Utility.ClosestDistance(caster.collider, caster.Target.collider) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

    // (has corrected target already)
    public override void Apply(Entity caster, int skillLevel)
    {
        // can't heal dead people
        if (caster.Target != null && caster.Target.Health > 0)
        {
            caster.Target.Health += healsHealth.Get(skillLevel);
            caster.Target.Mind += healsMana.Get(skillLevel);

            // show effect on target
            SpawnEffect(caster, caster.Target);
        }
    }
}
