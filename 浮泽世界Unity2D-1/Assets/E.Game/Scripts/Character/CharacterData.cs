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
using System;
using E.Tool;

namespace E.Game
{
    [DisallowMultipleComponent]
    public class CharacterData : MonoBehaviour
    {
        [Header("角色基本信息")]
        [SerializeField, Tooltip("姓名")] private string characterName = "无名氏";
        [SerializeField, Tooltip("生日")] private int year = 2000;
        [SerializeField, Tooltip("性别")] private Gender gender = Gender.无性;

        [Header("角色体质信息")]
        [SerializeField, Tooltip("生命值"), Range(0, 1000)] private int m_Health = 100;
        [SerializeField, Tooltip("生命值上限"), Range(0, 1000)] private int m_HealthMax = 100;
        [SerializeField, Tooltip("体力值"), Range(0, 1000)] private int m_Stamina = 100;
        [SerializeField, Tooltip("体力值上限"), Range(0, 1000)] private int m_StaminaMax = 100;
        [SerializeField, Tooltip("脑力值"), Range(0, 1000)] private int m_Mentality = 100;
        [SerializeField, Tooltip("脑力值上限"), Range(0, 1000)] private int m_MentalityMax = 100;
        [SerializeField, Tooltip("饱食度"), Range(0, 1)] private float m_Satiety = 1;
        [SerializeField, Tooltip("洁净度"), Range(0, 1)] private float m_Cleanness = 1;
        [SerializeField, Tooltip("身高，单位cm"), Range(0, 1000)] private int m_Height = 160;
        [SerializeField, Tooltip("体重，单位kg"), Range(0, 1000)] private int m_Weight = 50;
        [SerializeField, Tooltip("力量，单位kg"), Range(0, 1000)] private int m_Strength = 50;
        [SerializeField, Tooltip("速度，单位m/s"), Range(0, 100)] private int m_RunSpeed = 5;
        private int m_WalkSpeed = 2;
        private int m_SlowSpeed = 1;

        [Header("角色其他信息")]
        [SerializeField, Tooltip("携带的物品")] private List<InteractorData> m_CarryingProps = new List<InteractorData>();
        [SerializeField, Tooltip("掌握的技能")] private List<Skill> m_OwnedSkills = new List<Skill>();

        [Header("剧情信息")]
        [SerializeField, Tooltip("剧情对话")] private StoryNode m_StoryNode;

        [Header("玩家账号信息")]
        [SerializeField, Tooltip("玩家昵称")]
        private string m_PlayerName = "未登录玩家";
        [SerializeField, Tooltip("玩家编号")]
        private string m_PlayerID = "000000";
        [SerializeField, Tooltip("初次登入时间")]
        private DateTime m_FirstLogin;
        [SerializeField, Tooltip("上次登入时间")]
        private DateTime m_LastLogin;
        [SerializeField, Tooltip("上次登出时间")]
        private DateTime m_LastLogout;
        [SerializeField, Tooltip("累计在线时间")]
        private TimeSpan m_TotalOnline;

        //角色基本信息
        public string CharacterName { get => characterName; set => characterName = value; }
        public int Year { get => year; set => year = value; }
        public Gender Gender { get => gender; set => gender = value; }
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
        public int Height { get => m_Height; set => m_Height = value; }
        public int Weight { get => m_Weight; set => m_Weight = value; }
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
        public List<InteractorData> CarryingProps
        {
            get { return m_CarryingProps; }
            set { m_CarryingProps = value; }
        }
        public List<Skill> OwnedSkills
        {
            get { return m_OwnedSkills; }
            set { m_OwnedSkills = value; }
        }
        //
        public StoryNode StoryNode { get => m_StoryNode; set => m_StoryNode = value; }
        //玩家账号信息
        public string PlayerName
        {
            get { return m_PlayerName; }
            set { m_PlayerName = value; }
        }
        public string PlayerID
        {
            get { return m_PlayerID; }
            set { m_PlayerID = value; }
        }
        public DateTime FirstLogin
        {
            get { return m_FirstLogin; }
            set { m_FirstLogin = value; }
        }
        public DateTime LastLogin
        {
            get { return m_LastLogin; }
            set { m_LastLogin = value; }
        }
        public DateTime LastLogout
        {
            get { return m_LastLogout; }
            set { m_LastLogout = value; }
        }
        public TimeSpan TotalOnline
        {
            get { return m_TotalOnline; }
            set { m_TotalOnline = value; }
        }

    }

    /// <summary>
    /// 性别
    /// </summary>
    public enum Gender
    {
        无性,
        女性,
        男性,
        双性
    }
    /// <summary>
    /// 技能
    /// </summary>
    [Serializable]
    public struct Skill
    {
        public string Name;
        public SkillLevel Level;
    }

    /// <summary>
    /// 交互模式
    /// </summary>
    public enum InteractiveMode
    {
        普通,
        战斗,
        投掷
    }

    /// <summary>
    /// 技能熟练度
    /// </summary>
    public enum SkillLevel
    {
        新手,
        掌握,
        熟练,
        精通,
        专家
    }
}