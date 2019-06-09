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
        //Entrance
        [Header("Logo面板")]
        [SerializeField] private bool m_IsShowEntrance = false;
        public UI_EntranceController m_UIEntranceController;
        //Lobby
        [Header("菜单面板")]
        [SerializeField] private bool m_IsShowMenu = false;
        public UI_MenuController m_UIMenuController;
        [Header("登入面板")]
        [SerializeField] private bool m_IsShowSignIn = false;
        public UI_AccountController m_UIAccountController;
        //Game
        [Header("聊天面板")]
        [SerializeField] private bool m_IsShowChat = false;
        public UI_ChatController m_UIChatController;
        [Header("角色信息面板")]
        [SerializeField] private bool m_IsShowCharacterInfo = false;
        public UI_CharacterInfoController m_UICharacterInfoController;
        [Header("对话面板")]
        [SerializeField] private bool m_IsShowDialog = false;
        public UI_DialogController m_UIDialogController;
        [Header("电脑信息面板")]
        [SerializeField] private bool m_IsShowPCInfo = true;
        public UI_PCInfoController m_UIPCInfoController;
        [Header("提示面板")]
        [SerializeField] private bool m_IsShowMessage = true;
        public UI_MessageController m_UIMessageController;
        [Header("按键帮助面板")]
        [SerializeField] private bool m_IsShowBtnHelp = true;
        public UI_BtnHelpController m_UIBtnHelpController;
        [Header("手机面板")]
        [SerializeField] private bool m_IsShowPhone = false;
        public UI_PhoneController m_UIPhoneController;

        [Header("物品信息面板")]
        [SerializeField] private bool m_IsShowInteractorInfo = false;
        public UI_InteractorInfoController m_UIInteractorInfoController;
        [Header("背包面板")]
        [SerializeField] private bool m_IsShowBag = false;
        public UI_BagController m_UIBagController;


        public bool IsShowEntrance { get => m_IsShowEntrance; set => m_IsShowEntrance = value; }
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
                    m_IsShowCharacterInfo = false;
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
                    m_IsShowCharacterInfo = false;
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
                    m_IsShowCharacterInfo = false;
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
        public bool IsShowPlayerInfo
        {
            get
            {
                return m_IsShowCharacterInfo;
            }
            set
            {
                m_IsShowCharacterInfo = value;
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
                    m_IsShowCharacterInfo = false;
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
        public bool IsShowPhone { get => m_IsShowPhone; set => m_IsShowPhone = value; }
        public bool IsShowInteractorInfo { get => m_IsShowInteractorInfo; set => m_IsShowInteractorInfo = value; }
        public bool IsShowBag { get => m_IsShowBag; set => m_IsShowBag = value; }

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
            if (Input.GetKey(KeyCode.LeftControl) & Input.GetKeyUp(KeyCode.Tab))
            {
                IsShowPCInfo = !IsShowPCInfo;
            }
            if (Input.GetKeyUp(KeyCode.I) & !IsShowChat)
            {
                IsShowPlayerInfo = !IsShowPlayerInfo;
            }
            if (Input.GetKeyUp(KeyCode.B))
            {
                IsShowBag = !IsShowBag;
            }

            SetUIPanel();
        }


        /// <summary>
        /// 检测UI激活状态
        /// </summary>
        private void SetUIPanel()
        {
            if (m_UIEntranceController.gameObject.activeSelf != m_IsShowEntrance)
            { m_UIEntranceController.gameObject.SetActive(m_IsShowEntrance); }

            if (m_UIMenuController.gameObject.activeSelf != IsShowMenu)
            { m_UIMenuController.gameObject.SetActive(IsShowMenu); }
            if (m_UIAccountController.gameObject.activeSelf != IsShowSignIn)
            { m_UIAccountController.gameObject.SetActive(IsShowSignIn); }

            if (m_UIChatController.gameObject.activeSelf != IsShowChat)
            { m_UIChatController.gameObject.SetActive(IsShowChat); }
            if (m_UICharacterInfoController.gameObject.activeSelf != IsShowPlayerInfo)
            { m_UICharacterInfoController.gameObject.SetActive(IsShowPlayerInfo); }
            if (m_UIDialogController.gameObject.activeSelf != IsShowDialog)
            { m_UIDialogController.gameObject.SetActive(IsShowDialog); }
            if (m_UIPCInfoController.gameObject.activeSelf != IsShowPCInfo)
            { m_UIPCInfoController.gameObject.SetActive(IsShowPCInfo); }
            if (m_UIMessageController.gameObject.activeSelf != IsShowMessage)
            { m_UIMessageController.gameObject.SetActive(IsShowMessage); }
            if (m_UIBtnHelpController.gameObject.activeSelf != IsShowBtnHelp)
            { m_UIBtnHelpController.gameObject.SetActive(IsShowBtnHelp); }

            if (m_UIPhoneController.gameObject.activeSelf != m_IsShowPhone)
            { m_UIPhoneController.gameObject.SetActive(m_IsShowPhone); }

            if (m_UIInteractorInfoController.gameObject.activeSelf != IsShowInteractorInfo)
            { m_UIInteractorInfoController.gameObject.SetActive(IsShowInteractorInfo); }
            if (m_UIBagController.gameObject.activeSelf != IsShowBag)
            { m_UIBagController.gameObject.SetActive(IsShowBag); }
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
        /// <summary>
        /// 隐藏所有UI
        /// </summary>
        public void HideAllUI()
        {
            m_IsShowMenu = false;
            m_IsShowSignIn = false;
            m_IsShowChat = false;
            m_IsShowCharacterInfo = false;
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
            m_IsShowCharacterInfo = false;
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
    }
}