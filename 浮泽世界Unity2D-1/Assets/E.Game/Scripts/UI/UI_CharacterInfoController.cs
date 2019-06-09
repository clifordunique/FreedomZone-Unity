// ========================================================
// 作者：E Star
// 创建时间：2019-01-19 17:55:13
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace E.Game
{
    public class UI_CharacterInfoController : UI_Controller
    {
        [Header("显示玩家账号信息的控件")]
        [SerializeField, Tooltip("玩家昵称")] private Text m_PlayerName;
        [SerializeField, Tooltip("玩家编号")] private Text m_PlayerID;
        [SerializeField, Tooltip("初次登入时间")] private Text m_FirstLogin;
        [SerializeField, Tooltip("上次登入时间")] private Text m_LastLogin;
        [SerializeField, Tooltip("上次登出时间")] private Text m_LastLogout;
        [SerializeField, Tooltip("累计在线时间")] private Text m_TotalOnline;

        [Header("显示角色体质信息的控件")]
        [SerializeField, Tooltip("名称")] private Text m_CharacterName;
        [SerializeField, Tooltip("当前生命值/最大生命值")] private Text m_Health;
        [SerializeField, Tooltip("当前体力值/最大体力值")] private Text m_Stamina;
        [SerializeField, Tooltip("当前脑力值/最大脑力值")] private Text m_Mentality;
        [SerializeField, Tooltip("饱食度")] private Text m_Satiety;
        [SerializeField, Tooltip("洁净度")] private Text m_Cleanness;
        [SerializeField, Tooltip("最大力量")] private Text m_Strength;
        [SerializeField, Tooltip("最大速度")] private Text m_RunSpeed;

        [Header("显示角色其它信息的控件")]
        [SerializeField, Tooltip("携带的物品")] private Text m_CarryingProps;
        [SerializeField, Tooltip("掌握的技能")] private Text m_OwnedSkills;


        [Header("运行时变量")]
        [SerializeField] private CharacterData m_CurrentCharacterData;
        public CharacterData CurrentCharacterData { get => m_CurrentCharacterData; set => m_CurrentCharacterData = value; }

        private void Start()
        {
        }
        private void Update()
        {
            if (GameManager.Instance.GameState == GameState.InGame)
            {
                if (m_CurrentCharacterData != null)
                {
                    GetPlayerInfo();
                }
            }
        }

        private void GetPlayerInfo()
        {
            m_PlayerName.text = CurrentCharacterData.PlayerName;
            m_PlayerID.text = CurrentCharacterData.PlayerID;
            m_FirstLogin.text = CurrentCharacterData.FirstLogin.ToString();
            m_LastLogin.text = CurrentCharacterData.LastLogin.ToString();
            m_LastLogout.text = CurrentCharacterData.LastLogout.ToString();
            m_TotalOnline.text = CurrentCharacterData.TotalOnline.ToString();

            m_CharacterName.text = CurrentCharacterData.CharacterName;
            m_Health.text = CurrentCharacterData.Health.ToString();
            m_Stamina.text = CurrentCharacterData.Stamina.ToString();
            m_Mentality.text = CurrentCharacterData.Mentality.ToString();
            m_Satiety.text = CurrentCharacterData.Satiety.ToString();
            m_Cleanness.text = CurrentCharacterData.Cleanness.ToString();
            m_Strength.text = CurrentCharacterData.Strength.ToString();
            m_RunSpeed.text = CurrentCharacterData.RunSpeed.ToString();
            //m_CarryingProps.text = m_PlayerModel.CarryingProps;
            //m_OwnedSkills.text = m_PlayerModel.OwnedSkills;
        }
    }
}