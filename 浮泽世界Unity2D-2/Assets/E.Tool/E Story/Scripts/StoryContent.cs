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
    [CreateAssetMenu(menuName = "E Story/故事内容", order = 2)]
    public class StoryContent : ScriptableObjectDictionary<StoryContent>
    {
        [Tooltip("是否已阅读")] public bool IsReaded;
        [Tooltip("发生时间")] public DateTime Time;
        [Tooltip("发生地点")] public string Position;
        [Tooltip("摘要"), TextArea] public string Summary;
        [Tooltip("内容类型")] public ContentType Type;
        [Tooltip("剧情对话")] public Sentence[] Sentences;
        [Tooltip("过场动画")] public Animation[] Animations;

        public StoryContent()
        {
            IsReaded = false;
            Time = new DateTime();
            Position = "";
            Summary = "";
            Type = ContentType.剧情对话;
            Sentences = new Sentence[0];
            Animations = new Animation[0];
        }

        [Serializable]
        public struct Sentence
        {
            [Tooltip("发言者")] public string Speaker;
            [Tooltip("发言者表情")] public Sprite SpeakerExpression;
            [Tooltip("是否已阅读过此句话")] public bool IsReaded;
            [Tooltip("发言内容"), TextArea] public string Words;
        }
    }
}