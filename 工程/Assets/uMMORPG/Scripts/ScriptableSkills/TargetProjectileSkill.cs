using System.Text;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="uMMORPG Skill/Target Projectile", order=999)]
public class TargetProjectileSkill : DamageSkill
{
    [Header("Projectile")]
    public ProjectileSkillEffect projectile; // Arrows, Bullets, Fireballs, ...

    bool HasRequiredWeaponAndAmmo(Entity caster)
    {
        int weaponIndex = caster.GetEquippedWeaponIndex();
        if (weaponIndex != -1)
        {
            // no ammo required, or has that ammo equipped?
            WeaponItem itemData = (WeaponItem)caster.equipment[weaponIndex].item.data;
            return itemData.requiredAmmo == null ||
                   caster.GetEquipmentIndexByName(itemData.requiredAmmo.name) != -1;
        }
        return false;
    }

    void ConsumeRequiredWeaponsAmmo(Entity caster)
    {
        int weaponIndex = caster.GetEquippedWeaponIndex();
        if (weaponIndex != -1)
        {
            // no ammo required, or has that ammo equipped?
            WeaponItem itemData = (WeaponItem)caster.equipment[weaponIndex].item.data;
            if (itemData.requiredAmmo != null)
            {
                int ammoIndex = caster.GetEquipmentIndexByName(itemData.requiredAmmo.name);
                if (ammoIndex != 0)
                {
                    // reduce it
                    ItemSlot slot = caster.equipment[ammoIndex];
                    --slot.amount;
                    caster.equipment[ammoIndex] = slot;
                }
            }
        }
    }

    public override bool CheckSelf(Entity caster, int skillLevel)
    {
        // check base and ammo
        return base.CheckSelf(caster, skillLevel) &&
               HasRequiredWeaponAndAmmo(caster);
    }

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
        //消耗弹药
        ConsumeRequiredWeaponsAmmo(caster);

        //显示技能效果
        if (projectile != null)
        {
            GameObject go = Instantiate(projectile.gameObject, caster.EffectMount.position, caster.EffectMount.rotation);
            ProjectileSkillEffect effect = go.GetComponent<ProjectileSkillEffect>();
            effect.target = caster.Target;
            effect.caster = caster;
            effect.damage = damage.Get(skillLevel);
            effect.stunChance = stunChance.Get(skillLevel);
            effect.stunTime = stunTime.Get(skillLevel);
            NetworkServer.Spawn(go);
        }
        else Debug.LogWarning(name + ": missing projectile");
    }
}
