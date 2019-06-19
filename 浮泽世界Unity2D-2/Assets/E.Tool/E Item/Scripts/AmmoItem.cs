using UnityEngine;

[CreateAssetMenu(menuName= "E Item/弹药", order = 4)]
public class AmmoItem : UsableItem
{
    [Header("【弹药相关信息】")]
    [Tooltip("伤害加成")] public int damageBonus;
    [Tooltip("效果加成")] public TargetBuffSkill effectBuff;
}