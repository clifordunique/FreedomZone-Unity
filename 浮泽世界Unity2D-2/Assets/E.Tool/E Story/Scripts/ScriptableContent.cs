// ========================================================
// 作者：E Star
// 创建时间：2019-02-27 01:05:45
// 当前版本：1.0
// 作用描述：可序列化故事内容类
// 挂载目标：无
// ========================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using E.Utility;

namespace E.Tool
{
    [CreateAssetMenu(menuName = "E Story/故事内容", order = 2)]
    public class ScriptableContent : ScriptableObjectDictionary<ScriptableContent>
    {
        [Tooltip("是否已阅读")] public bool IsReaded;
        [Tooltip("发生时间")] public DateTime Time;
        [Tooltip("发生地点")] public string Position;
        [Tooltip("摘要"), TextArea] public string Summary;
        [Tooltip("内容形式")] public ContentType Type;
        [Tooltip("剧情对话")] public List<Sentence> Sentences;
        [Tooltip("过场动画")] public List<Animation> Animations;

        public ScriptableContent()
        {
            IsReaded = false;
            Time = new DateTime();
            Position = "";
            Summary = "";
            Type = ContentType.剧情对话;
            Sentences = new List<Sentence>();
            Animations = new List<Animation>();
        }

        [Serializable]
        public class Sentence
        {
            [Tooltip("角色名称")] public string Speaker;
            [Tooltip("角色表情")] public Sprite Expression;
            [Tooltip("角色说话内容"), TextArea(1, 10)] public string Words;
            [Tooltip("是否已阅读过此句话")] public bool IsReaded;

            [Tooltip("节点展开")] public bool IsFold;

            public Sentence()
            {
                Speaker = "";
                Expression = null;
                Words = "";
                IsReaded = false;
                IsFold = true;
            }
        }
    }
}