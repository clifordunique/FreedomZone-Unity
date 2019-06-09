// ========================================================
// 作者：E Star
// 创建时间：2019-01-20 21:20:48
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using E.Utility;

namespace E.Game
{
    public class PhoneManager : Manager<PhoneManager>
    {
        [Header("动画控制器")]
        [SerializeField] private Animator m_PhoneAnimator;

        [Header("UI组件")]
        [SerializeField] private Text m_MessageContent;
        [SerializeField] private Text m_Time;
        [SerializeField] private Image m_PowerImg;

        [Header("数据")]
        [SerializeField, ReadOnly, Tooltip("当前应用")] private GameObject m_CurrentApp;
        [SerializeField, Tooltip("手机主页")] private GameObject m_Home;
        [SerializeField, Tooltip("剩余电量")] private float m_Power;
        [SerializeField, Tooltip("电量消耗（每帧）")] private float m_PowerCost;

        private void Start()
        {
        }
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.P) & !UIManager.Singleton.IsShowChat)
            {
                if (m_PhoneAnimator.GetBool("IsShowPhone"))
                {
                    SetPhoneUseState(false);
                }
                else
                {
                    SetPhoneUseState(true);
                }
            }

            //滚轮旋转手机
            if (m_PhoneAnimator.GetBool("IsShowPhone"))
            {
                float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
                transform.Rotate(new Vector3(0, 0, mouseScrollWheel * 100));
            }

            ShowTime();
            PowerUsing();
        }

        /// <summary>
        /// 拿出/放下手机
        /// </summary>
        public void SetPhoneUseState(bool IsShow)
        {
            m_PhoneAnimator.SetBool("IsShowPhone", IsShow);
            UIManager.SetCursor(IsShow);
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="str">消息内容</param>
        public void ShowMessage(string str)
        {
            if (str != string.Empty)
            {
                m_MessageContent.text = str;
                m_PhoneAnimator.SetTrigger("MessageTrigger");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PowerUsing()
        {
            m_Power -= m_PowerCost;
            if (m_Power <= 0)
            {
                m_Power = 0;
            }

            if (m_Power <= 0.2 & m_Power > 0)
            {
                m_PowerImg.color = Color.red;
            }
            else if (m_Power <= 0.5 & m_Power > 0.2)
            {
                m_PowerImg.color = Color.yellow;
            }
            else
            {
                m_PowerImg.color = Color.green;
            }

            m_PowerImg.fillAmount = m_Power;
        }

        private void ShowTime()
        {
            string week;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    week = "日";
                    break;
                case DayOfWeek.Monday:
                    week = "月";
                    break;
                case DayOfWeek.Tuesday:
                    week = "火";
                    break;
                case DayOfWeek.Wednesday:
                    week = "水";
                    break;
                case DayOfWeek.Thursday:
                    week = "木";
                    break;
                case DayOfWeek.Friday:
                    week = "金";
                    break;
                case DayOfWeek.Saturday:
                    week = "土";
                    break;
                default:
                    week = "日";
                    break;
            }
            string time = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            m_Time.text = time + " " + week;
        }

        /// <summary>
        /// 点击按钮
        /// </summary>
        public void ClickButton()
        {
            ShowMessage("此功能正在开发中，暂时无法使用");
        }
        /// <summary>
        /// 打开应用
        /// </summary>
        /// <param name="app"></param>
        public void OpenApp(GameObject app)
        {
            app.transform.SetAsLastSibling();
            app.SetActive(true);
            m_CurrentApp = app;
        }

        /// <summary>
        /// 关闭应用
        /// </summary>
        /// <param name="app"></param>
        public void CloseApp(GameObject app)
        {
            app.SetActive(false);
            m_CurrentApp = null;
        }
        /// <summary>
        /// 关闭当前应用
        /// </summary>
        public void CloseCurrentApp()
        {
            ShowMessage("此功能正在开发中，暂时无法使用");
        }
        /// <summary>
        /// 回到主页
        /// </summary>
        public void BackHome()
        {
            m_Home.transform.SetAsLastSibling();
        }

        /// <summary>
        /// 显示手机
        /// </summary>
        public void ShowPhone()
        {
            gameObject.SetActive(true);
            UIManager.SetCursor(true);
        }
        /// <summary>
        /// 隐藏手机
        /// </summary>
        public void HidePhone()
        {
            gameObject.SetActive(false);
            UIManager.SetCursor(false);
        }

    }
}