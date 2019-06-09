// ========================================================
// 作者：E Star
// 创建时间：2019-02-16 17:57:00
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Game
{
    [DisallowMultipleComponent]
    public class CharacterData : MonoBehaviour
    {
        [Header("角色体质信息")]
        [SerializeField, Tooltip("生命值"), Range(0, 1000)] protected int m_Health;
        [SerializeField, Tooltip("生命值上限"), Range(0, 1000)] protected int m_HealthMax;
        [SerializeField, Tooltip("体力值"), Range(0, 1000)] protected int m_Stamina;
        [SerializeField, Tooltip("体力值上限"), Range(0, 1000)] protected int m_StaminaMax;
        [SerializeField, Tooltip("脑力值"), Range(0, 1000)] protected int m_Mentality;
        [SerializeField, Tooltip("脑力值上限"), Range(0, 1000)] protected int m_MentalityMax;
        [SerializeField, Tooltip("饱食度"), Range(0, 1)] protected float m_Satiety;
        [SerializeField, Tooltip("洁净度"), Range(0, 1)] protected float m_Cleanness;
        [SerializeField, Tooltip("力量，单位kg"), Range(0, 1000)] protected int m_Strength;
        [SerializeField, Tooltip("速度，单位m/s"), Range(0, 100)] protected int m_RunSpeed; protected int m_WalkSpeed = 2; protected int m_SlowSpeed = 1;
        [Header("角色其他信息")]
        [SerializeField, Tooltip("携带的物品")] protected List<GameObject> m_CarryingProps;
        [SerializeField, Tooltip("掌握的技能")] protected List<Skill> m_OwnedSkills;

        //角色体质信息
        public int Health
        {
            get { return m_Health; }
            set { m_Health = value; }
        }
        public int HealthMax
        {
            get { return m_HealthMax; }
            set { m_HealthMax = value; }
        }
        public int Stamina
        {
            get { return m_Stamina; }
            set { m_Stamina = value; }
        }
        public int StaminaMax
        {
            get { return m_StaminaMax; }
            set { m_StaminaMax = value; }
        }
        public int Mentality
        {
            get { return m_Mentality; }
            set { m_Mentality = value; }
        }
        public int MentalityMax
        {
            get { return m_MentalityMax; }
            set { m_MentalityMax = value; }
        }
        public float Satiety
        {
            get { return m_Satiety; }
            set { m_Satiety = value; }
        }
        public float Cleanness
        {
            get { return m_Cleanness; }
            set { m_Cleanness = value; }
        }
        public int Strength
        {
            get { return m_Strength; }
            set { m_Strength = value; }
        }
        public int RunSpeed
        {
            get { return m_RunSpeed; }
            set
            {
                if (value >= 0)
                { m_RunSpeed = value; }
            }
        }
        public int WalkSpeed
        {
            get { return m_WalkSpeed; }
            set { if (value >= 0) { m_WalkSpeed = value; } }
        }
        public int SlowSpeed
        {
            get { return m_SlowSpeed; }
            set { if (value >= 0) { m_SlowSpeed = value; } }
        }
        //角色其他信息
        public List<GameObject> CarryingProps
        {
            get { return m_CarryingProps; }
            set { m_CarryingProps = value; }
        }
        public List<Skill> OwnedSkills
        {
            get { return m_OwnedSkills; }
            set { m_OwnedSkills = value; }
        }
    }
}