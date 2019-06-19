// ========================================================
// 作者：E Star
// 创建时间：2019-02-27 01:18:23
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
    [CreateAssetMenu(menuName = "E Story/故事树", order = 0)]
    public class StoryTree : ScriptableObject
    {
        [Header("故事树描述")]
        public string Describe;

        [Header("故事树状态")]
        [Tooltip("是否已通关此故事树")] public bool IsPassed = false;

        [Header("故事树节点集合")]
        [ReadOnly] public List<StoryNode> Nodes;

        [Header("故事树起始节点")]
        [ReadOnly] public StoryNode StartNode;

        [Header("故事树结束节点")]
        [ReadOnly, Tooltip("自动检测，由指定的起始节点出发获取所有结束节点")] public List<StoryNode> EndNodes = new List<StoryNode>();

        [Header("故事树完成进度")]
        [ReadOnly, Tooltip("自动计算，完成节点数/总节点数")] public float CompletionPercent = 0;
    }
}