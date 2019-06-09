// ========================================================
// 作者：E Star
// 创建时间：2019-01-19 17:53:45
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
    public class UI_MessageController : UI_Controller
    {
        [SerializeField] private Text m_MessageContent;


        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="str">消息内容</param>
        public void ShowMessage(string str)
        {
            if (str != string.Empty)
            {
                m_MessageContent.text = str;
                m_Animator.SetTrigger("MessageTrigger");
            }
        }
    }
}