// ========================================================
// 作者：E Star 
// 创建时间：2018-12-21 13:33:17 
// 当前版本：1.0 
// 作用描述：实现玩家信息
// 挂载目标：Player
// ========================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace E.Game
{
    [DisallowMultipleComponent]
    public class PlayerData : CharacterData
    {
        [Header("玩家账号信息")]
        [SerializeField, Tooltip("玩家昵称")]
        private string m_PlayerName;
        [SerializeField, Tooltip("玩家编号")]
        private string m_PlayerID;
        [SerializeField, Tooltip("初次登入时间")]
        private DateTime m_FirstLogin;
        [SerializeField, Tooltip("上次登入时间")]
        private DateTime m_LastLogin;
        [SerializeField, Tooltip("上次登出时间")]
        private DateTime m_LastLogout;
        [SerializeField, Tooltip("累计在线时间")]
        private TimeSpan m_TotalOnline;

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