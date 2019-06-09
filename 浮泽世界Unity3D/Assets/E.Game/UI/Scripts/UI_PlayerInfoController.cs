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
    public class UI_PlayerInfoController : UI_Controller
    {
        private PlayerData m_PlayerModel;

        [Header("显示玩家账号信息的控件")]
        [SerializeField, Tooltip("玩家昵称")] private Text m_PlayerName;
        [SerializeField, Tooltip("玩家编号")] private Text m_PlayerID;
        [SerializeField, Tooltip("初次登入时间")] private Text m_FirstLogin;
        [SerializeField, Tooltip("上次登入时间")] private Text m_LastLogin;
        [SerializeField, Tooltip("上次登出时间")] private Text m_LastLogout;
        [SerializeField, Tooltip("累计在线时间")] private Text m_TotalOnline;

        [Header("显示角色体质信息的控件")]
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


        private void Start()
        {
        }
        private void Update()
        {
            if (m_PlayerModel == null)
            {
                m_PlayerModel = CharacterManager.Singleton.Player.GetComponent<PlayerData>();
            }
            RefreshPlayerInfo();
        }


        private void RefreshPlayerInfo()
        {
            m_PlayerName.text = m_PlayerModel.PlayerName;
            m_PlayerID.text = m_PlayerModel.PlayerID;
            m_FirstLogin.text = m_PlayerModel.FirstLogin.ToString();
            m_LastLogin.text = m_PlayerModel.LastLogin.ToString();
            m_LastLogout.text = m_PlayerModel.LastLogout.ToString();
            m_TotalOnline.text = m_PlayerModel.TotalOnline.ToString();

            m_Health.text = m_PlayerModel.Health.ToString();
            m_Stamina.text = m_PlayerModel.Stamina.ToString();
            m_Mentality.text = m_PlayerModel.Mentality.ToString();
            m_Satiety.text = m_PlayerModel.Satiety.ToString();
            m_Cleanness.text = m_PlayerModel.Cleanness.ToString();
            m_Strength.text = m_PlayerModel.Strength.ToString();
            m_RunSpeed.text = m_PlayerModel.RunSpeed.ToString();
            //m_CarryingProps.text = m_PlayerModel.CarryingProps;
            //m_OwnedSkills.text = m_PlayerModel.OwnedSkills;
        }
    }
}