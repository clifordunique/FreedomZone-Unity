// ========================================================
// 作者：E Star
// 创建时间：2019-04-27 00:41:38
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E.Utility;
using E.Tool;

namespace E.Game
{
    public class UI_SlotController : MonoBehaviour
    {
        [SerializeField] private Image m_ImgIcon;
        [SerializeField] private Image m_ImgHealth;
        [SerializeField] private Image m_ImgHand;

        public InteractorData InteractorDataInstance;

        private void Update()
        {
            if (InteractorDataInstance != null)
            {
                m_ImgIcon.sprite = InteractorDataInstance.Icon;
                m_ImgHealth.fillAmount = (float)InteractorDataInstance.HealthNow / InteractorDataInstance.HealthMax;
            }
            else
            {
                m_ImgIcon.sprite = null;
                m_ImgHealth.fillAmount = 0;
            }
        }
    }
}