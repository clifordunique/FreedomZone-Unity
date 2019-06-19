using System.Text;
using UnityEngine;
using Mirror;
using E.Utility;

[CreateAssetMenu(menuName="E Skill/目标伤害", order = 1)]
public class TargetDamageSkill : DamageSkill
{
    public override bool CheckTarget(Entity caster)
    {
        // target exists, alive, not self, oktype?
        return caster.Target != null && caster.CanAttack(caster.Target);
    }

    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
    {
        // target still around?
        if (caster.Target != null)
        {
            destination = caster.Target.collider.ClosestPointOnBounds(caster.transform.position);
            return Utils.ClosestDistance(caster.collider, caster.Target.collider) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

    public override void Apply(Entity caster, int skillLevel)
    {
        // deal damage directly with base damage + skill damage
        caster.DealDamageAt(caster.Target,
                            caster.Strength + damage.Get(skillLevel),
                            stunChance.Get(skillLevel),
                            stunTime.Get(skillLevel));
    }
}
