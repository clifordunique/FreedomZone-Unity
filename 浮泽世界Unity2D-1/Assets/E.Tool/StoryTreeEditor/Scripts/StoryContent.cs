// ========================================================
// 作者：E Star
// 创建时间：2019-02-27 01:05:45
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using E.Utility;

namespace E.Tool
{
    [Serializable, CreateAssetMenu]
    public class StoryContent : ScriptableObject
    {
        //是否已阅读
        public bool IsReaded;

        //内容信息
        public DateTime Time;
        public string Position;
        [TextArea] public string Summary;
        public StoryContentType ContentType;
        [Tooltip("剧情对话")] public StorySentence[] Sentences;
        [Tooltip("过场动画")] public Animation[] Animations;
    }

    [Serializable]
    public struct StorySentence
    {
        [Tooltip("发言者")] public string Speaker;
        [Tooltip("发言者表情")] public Sprite SpeakerExpression;
        [Tooltip("是否已阅读过此句话")] public bool IsReaded;
        [Tooltip("发言内容"), TextArea] public string Words;
    }
}