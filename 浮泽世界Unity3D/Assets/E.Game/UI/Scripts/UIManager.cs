// ========================================================
// 作者：E Star 
// 创建时间：2018-12-21 13:33:17 
// 当前版本：1.0 
// 作用描述：UI管理员
// 挂载目标：UICanvas
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace E.Game
{
    [DisallowMultipleComponent]
    public class UIManager : Manager<UIManager>
    {
        [Header("菜单面板")]
        [SerializeField] private bool m_IsShowMenu = false;
        public bool IsShowMenu
        {
            get
            {
                return m_IsShowMenu;
            }
            set
            {
                m_IsShowMenu = value;
                if (value)
                {
                    SetCursor(true);
                    m_IsShowSignIn = false;
                    m_IsShowChat = false;
                    m_IsShowPlayerInfo = false;
                    m_IsShowDialog = false;
                    //m_IsShowPCInfo = false;
                    //m_IsShowMessage = false;
                    //m_IsShowBtnHelp = false;
                }
                else
                {
                    SetCursor(false);
                }
            }
        }
        public UI_MenuController m_UIMenuController;
        [Header("登入面板")]
        [SerializeField] private bool m_IsShowSignIn = false;
        public bool IsShowSignIn
        {
            get
            {
                return m_IsShowSignIn;
            }
            set
            {
                m_IsShowSignIn = value;
                if (value)
                {
                    SetCursor(true);
                    m_IsShowMenu = false;
                    m_IsShowChat = false;
                    m_IsShowPlayerInfo = false;
                    m_IsShowDialog = false;
                    //m_IsShowPCInfo = false;
                    //m_IsShowMessage = false;
                    //m_IsShowBtnHelp = false;
                }
                else
                {
                    SetCursor(false);
                }
            }
        }
        public UI_AccountController m_UIAccountController;
        [Header("聊天面板")]
        [SerializeField] private bool m_IsShowChat = false;
        public bool IsShowChat
        {
            get
            {
                return m_IsShowChat;
            }
            set
            {
                m_IsShowChat = value;
                if (value)
                {
                    SetCursor(true);
                    m_IsShowMenu = false;
                    m_IsShowSignIn = false;
                    m_IsShowPlayerInfo = false;
                    m_IsShowDialog = false;
                    //m_IsShowPCInfo = false;
                    //m_IsShowMessage = false;
                    //m_IsShowBtnHelp = false;
                }
                else
                {
                    SetCursor(false);
                }
            }
        }
        public UI_ChatController m_UIChatController;
        [Header("信息面板")]
        [SerializeField] private bool m_IsShowPlayerInfo = false;
        public bool IsShowPlayerInfo
        {
            get
            {
                return m_IsShowPlayerInfo;
            }
            set
            {
                m_IsShowPlayerInfo = value;
                if (value)
                {
                    SetCursor(true);
                    m_IsShowMenu = false;
                    m_IsShowSignIn = false;
                    m_IsShowChat = false;
                    m_IsShowDialog = false;
                    //m_IsShowPCInfo = false;
                    //m_IsShowMessage = false;
                    //m_IsShowBtnHelp = false;
                }
                else
                {
                    SetCursor(false);
                }
            }
        }
        public UI_PlayerInfoController m_UIPlayerInfoController;
        [Header("对话面板")]
        [SerializeField] private bool m_IsShowDialog = false;
        public bool IsShowDialog
        {
            get
            {
                return m_IsShowDialog;
            }
            set
            {
                m_IsShowDialog = value;
                if (value)
                {
                    SetCursor(true);
                    m_IsShowMenu = false;
                    m_IsShowSignIn = false;
                    m_IsShowChat = false;
                    m_IsShowPlayerInfo = false;
                    //m_IsShowPCInfo = false;
                    //m_IsShowMessage = false;
                    //m_IsShowBtnHelp = false;
                }
                else
                {
                    SetCursor(false);
                }
            }
        }
        public UI_DialogController m_UIDialogController;
        [Header("电脑信息面板")]
        [SerializeField] private bool m_IsShowPCInfo = true;
        public bool IsShowPCInfo
        {
            get
            {
                return m_IsShowPCInfo;
            }
            set
            {
                m_IsShowPCInfo = value;
                if (value)
                {
                    //SetCursor(true);
                    //m_IsShowMenu = false;
                    //m_IsShowSignIn = false;
                    //m_IsShowChat = false;
                    //m_IsShowPlayerInfo = false;
                    //m_IsShowDialog = false;
                    //m_IsShowMessage = false;
                    //m_IsShowBtnHelp = false;
                }
                else
                {
                    //SetCursor(false);
                }
            }
        }
        public UI_PCInfoController m_UIPCInfoController;
        [Header("提示面板")]
        [SerializeField] private bool m_IsShowMessage = true;
        public bool IsShowMessage
        {
            get
            {
                return m_IsShowMessage;
            }
            set
            {
                m_IsShowMessage = value;
                if (value)
                {
                    //SetCursor(true);
                    //m_IsShowMenu = false;
                    //m_IsShowSignIn = false;
                    //m_IsShowChat = false;
                    //m_IsShowPlayerInfo = false;
                    //m_IsShowDialog = false;
                    //m_IsShowPCInfo = false;
                    //m_IsShowBtnHelp = false;
                }
                else
                {
                    //SetCursor(false);
                }
            }
        }
        public UI_MessageController m_UIMessageController;
        [Header("按键帮助面板")]
        [SerializeField] private bool m_IsShowBtnHelp = true;
        public bool IsShowBtnHelp
        {
            get
            {
                return m_IsShowBtnHelp;
            }
            set
            {
                m_IsShowBtnHelp = value;
                if (value)
                {
                    //SetCursor(true);
                    //m_IsShowMenu = false;
                    //m_IsShowSignIn = false;
                    //m_IsShowChat = false;
                    //m_IsShowPlayerInfo = false;
                    //m_IsShowDialog = false;
                    //m_IsShowPCInfo = false;
                    //m_IsShowMessage = false;
                }
                else
                {
                    //SetCursor(false);
                }
            }
        }
        public UI_BtnHelpController m_UIBtnHelpController;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (IsShowSignIn | IsShowChat | IsShowPlayerInfo | IsShowDialog)
                {
                    HideAllDefaultUI();
                }
                else
                {
                    IsShowMenu = !IsShowMenu;
                }
            }
            if (Input.GetKey(KeyCode.LeftControl) & Input.GetKeyUp(KeyCode.T))
            {
                IsShowChat = !IsShowChat;
            }
            if (Input.GetKeyUp(KeyCode.I) & !IsShowChat)
            {
                IsShowPlayerInfo = !IsShowPlayerInfo;
            }
            if (Input.GetKey(KeyCode.LeftControl) & Input.GetKeyUp(KeyCode.Tab))
            {
                IsShowPCInfo = !IsShowPCInfo;
            }

            SetUIPanel();
        }


        /// <summary>
        /// 隐藏所有UI
        /// </summary>
        public void HideAllUI()
        {
            m_IsShowMenu = false;
            m_IsShowSignIn = false;
            m_IsShowChat = false;
            m_IsShowPlayerInfo = false;
            m_IsShowDialog = false;
            m_IsShowPCInfo = false;
            m_IsShowMessage = false;
            m_IsShowBtnHelp = false;
            SetCursor(false);
        }
        /// <summary>
        /// 隐藏所有普通UI
        /// </summary>
        public void HideAllDefaultUI()
        {
            m_IsShowMenu = false;
            m_IsShowSignIn = false;
            m_IsShowChat = false;
            m_IsShowPlayerInfo = false;
            m_IsShowDialog = false;
            SetCursor(false);
        }
        /// <summary>
        /// 设置所有特殊UI
        /// </summary>
        public void SetAllSpecialUI(bool isShow)
        {
            m_IsShowPCInfo = isShow;
            m_IsShowMessage = isShow;
            m_IsShowBtnHelp = isShow;
        }

        /// <summary>
        /// 检测UI激活状态
        /// </summary>
        private void SetUIPanel()
        {
            if (m_UIMenuController.gameObject.activeSelf != IsShowMenu)
            { m_UIMenuController.gameObject.SetActive(IsShowMenu); }
            if (m_UIAccountController.gameObject.activeSelf != IsShowSignIn)
            { m_UIAccountController.gameObject.SetActive(IsShowSignIn); }
            if (m_UIChatController.gameObject.activeSelf != IsShowChat)
            { m_UIChatController.gameObject.SetActive(IsShowChat); }
            if (m_UIPlayerInfoController.gameObject.activeSelf != IsShowPlayerInfo)
            { m_UIPlayerInfoController.gameObject.SetActive(IsShowPlayerInfo); }
            if (m_UIDialogController.gameObject.activeSelf != IsShowDialog)
            { m_UIDialogController.gameObject.SetActive(IsShowDialog); }

            if (m_UIPCInfoController.gameObject.activeSelf != IsShowPCInfo)
            { m_UIPCInfoController.gameObject.SetActive(IsShowPCInfo); }
            if (m_UIMessageController.gameObject.activeSelf != IsShowMessage)
            { m_UIMessageController.gameObject.SetActive(IsShowMessage); }
            if (m_UIBtnHelpController.gameObject.activeSelf != IsShowBtnHelp)
            { m_UIBtnHelpController.gameObject.SetActive(IsShowBtnHelp); }
        }
        /// <summary>
        /// 设置光标状态
        /// </summary>
        /// <param name="isShow">是否展示</param>
        public static void SetCursor(bool isShow)
        {
            Cursor.visible = isShow;
            if (isShow)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}