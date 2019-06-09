// ========================================================
// 作者：E Star
// 创建时间：2019-04-24 00:20:57
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
    public class UI_InteractorInfoController : MonoBehaviour
    {
        [SerializeField] private Image m_Img_Icon;
        [SerializeField] private Text m_Txt_InteractorName;
        [SerializeField] private Text m_Txt_Describe;
        [SerializeField] private Text m_Txt_Type;
        [SerializeField] private Text m_Txt_Health;
        [SerializeField] private Text m_Txt_Price;
        [SerializeField] private Text m_Txt_Size;
        [SerializeField] private Text m_Txt_Mass; 

         [SerializeField, ReadOnly] private InteractorData m_CurrentInteractorData;


        private void Update()
        {
            if (m_CurrentInteractorData != null)
            {
                Refresh();
            }
        }
        private void Refresh()
        {
            m_Img_Icon.sprite = m_CurrentInteractorData.Icon;
            m_Txt_InteractorName.text = m_CurrentInteractorData.InteractorName;
            m_Txt_Describe.text = m_CurrentInteractorData.Describe;
            //m_Txt_Type.text = m_CurrentInteractorData.Describe;
            m_Txt_Health.text = m_CurrentInteractorData.HealthNow.ToString();
            m_Txt_Price.text = m_CurrentInteractorData.Price.ToString() + " $";
            m_Txt_Size.text = m_CurrentInteractorData.Size.ToString() + " L";
            m_Txt_Mass.text = m_CurrentInteractorData.Mass.ToString() + " kg";
            
        }
        public void ShowInteractorInfo(InteractorData data)
        {
            UIManager.Singleton.IsShowInteractorInfo = true;
            m_CurrentInteractorData = data;
        }
        public void Close()
        {
            UIManager.Singleton.IsShowInteractorInfo = false;
        }
    }
}
