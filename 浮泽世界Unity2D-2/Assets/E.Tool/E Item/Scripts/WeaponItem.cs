using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "E Item/武器", order = 3)]
public class WeaponItem : EquipmentItem
{
    [Header("【武器相关信息】")]
    [Tooltip("消耗弹药")] public AmmoItem requiredAmmo; // null if no ammo is required

    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        if (requiredAmmo != null)
        {
            tip.Replace("{消耗弹药}", requiredAmmo.name);
        }
        return tip.ToString();
    }
}