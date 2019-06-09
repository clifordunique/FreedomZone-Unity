// ========================================================
// 作者：E Star
// 创建时间：2019-01-19 17:29:52
// 当前版本：1.0
// 作用描述：
// 挂载目标：UI
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace E.Game
{
    public class UI_DialogController : UI_Controller
    {
        [SerializeField] private Image m_Avatar;
        [SerializeField] private Text m_Speaker;
        [SerializeField] private Text m_Content;

        public void SetAvatar(Sprite image)
        {
            m_Avatar.sprite = image;
        }
        public void SetSpeaker(string speaker)
        {
            m_Speaker.text = speaker;
        }
        public void SetContent(string content)
        {
            m_Content.text = content;
        }
    }
}