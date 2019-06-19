// ========================================================
// 作者：E Star
// 创建时间：2019-02-27 01:06:46
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
    [CreateAssetMenu(menuName = "E Story/故事节点", order = 1)]
    public class StoryNode : ScriptableObject
    {
        //节点信息
        public StoryNodeID ID;
        public StoryNodeType NodeType;
        public bool IsPassed;
        public bool IsMainNode;
        //节点内容
        public StoryContent Content;
        //节点显示
        [ReadOnly, Tooltip("节点在故事树编辑器内的坐标及尺寸")] public Rect Rect = new Rect(100, 100, 250, 50);
        [ReadOnly, Tooltip("节点分支选项（请在故事书编辑器内指定）")] public List<StoryChoice> Choices = new List<StoryChoice>();
    }

    [Serializable]
    public class StoryChoice
    {
        [Tooltip("选项描述")] public string ChoiceText;
        [Tooltip("下一个故事节点")] public StoryNode NextStoryNode;

        public StoryChoice(string text, StoryNode nextStoryNode)
        {
            ChoiceText = text;
            NextStoryNode = nextStoryNode;
        }
    }
    [Serializable]
    public struct StoryNodeID
    {
        [Tooltip("周目号")] public int Round;
        [Tooltip("章节号")] public int Chapter;
        [Tooltip("场景号")] public int Scene;
        [Tooltip("片段号")] public int Part;
        [Tooltip("分支号")] public int Branch;
    }

    public enum StoryNodeType
    {
        中间节点,
        起始节点,
        结局节点
    }
    public enum StoryContentType
    {
        剧情对话,
        过场动画
    }
}