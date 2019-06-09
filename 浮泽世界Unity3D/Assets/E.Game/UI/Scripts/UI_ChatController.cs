// ========================================================
// 作者：E Star
// 创建时间：2019-01-19 17:53:58
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
    public class UI_ChatController : UI_Controller
    {
        [SerializeField] private InputField m_InputWords;
        [SerializeField] private Text m_ChatHistory;

        private void OnEnable()
        {
            ActivateInputField();
        }
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                SendWords();
                AddHistory();
            }
        }

        /// <summary>
        /// 发送聊天内容
        /// </summary>
        public void SendWords()
        {

        }
        /// <summary>
        /// 激活输入框
        /// </summary>
        public void ActivateInputField()
        {
            m_InputWords.ActivateInputField();
        }
        /// <summary>
        /// 添加聊天内容历史
        /// </summary>
        public void AddHistory()
        {
            if (m_InputWords.text != string.Empty)
            {
                m_ChatHistory.text += "\n" + m_InputWords.text;
                m_InputWords.text = "";
                m_InputWords.ActivateInputField();
            }
            else
            {
                Debug.Log("发送内容不能为空");
            }
        }
    }
}