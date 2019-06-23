// ========================================================
// 作者：E Star
// 创建时间：2019-02-27 01:18:23
// 当前版本：1.0
// 作用描述：可序列化故事类
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
    [CreateAssetMenu(menuName = "E Story/故事", order = 0)]
    public class ScriptableStory : ScriptableObjectDictionary<ScriptableStory>
    {
        [Tooltip("故事描述"), TextArea(1, 10)] public string Describe;
        [Tooltip("通过任意一个结局节点即可")] public bool IsPassed = false;
        public List<Node> Nodes = new List<Node>();

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public Node CreateNode(RectInt rect)
        {
            if (Nodes == null)
            {
                Nodes = new List<Node>();
            }

            NodeID id = new NodeID(1,1,1,1,1);
            while (ContainsID(id))
            {
                id.Branch++;
            }
            Node node = new Node(id, rect);
            Nodes.Add(node);
            return node;
        }
        /// <summary>
        /// 检查节点是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsID(NodeID id)
        {
            foreach (Node item in Nodes)
            {
                if (item.ID.Equals(id))
                {
                    return true;
                }
            }
            return false;
        }

        //获取
        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Node GetNode(NodeID id)
        {
            foreach (Node item in Nodes)
            {
                if (item.ID.Equals(id))
                {
                    return item;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取节点
        /// </summary>
        public Node GetStartNode()
        {
            foreach (Node item in Nodes)
            {
                if (item.Type == NodeType.起始节点)
                {
                    return item;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取结局节点
        /// </summary>
        public List<Node> GetEndingNodes()
        {
            List<Node> nodes = new List<Node>();
            foreach (Node item in Nodes)
            {
                if (item.Type == NodeType.结局节点)
                {
                    nodes.Add(item);
                }
            }
            return nodes;
        }
        /// <summary>
        /// 获取通过的节点
        /// </summary>
        public List<Node> GetPassedNodes()
        {
            List<Node> nodes = new List<Node>();
            foreach (Node item in Nodes)
            {
                if (item.IsPassed)
                {
                    nodes.Add(item);
                }
            }
            return nodes;
        }
        /// <summary>
        /// 获取通过的结局节点
        /// </summary>
        public List<Node> GetPassedEndingNodes()
        {
            List<Node> nodes = GetEndingNodes();
            List<Node> nodesP = new List<Node>();
            if (nodes.Count > 0)
            {
                foreach (Node item in nodes)
                {
                    if (item.IsPassed)
                    {
                        nodesP.Add(item);
                    }
                }
            }
            return nodesP;
        }
        /// <summary>
        /// 获取全节点通过百分比（分数格式）
        /// </summary>
        public string GetAllNodesPassFraction()
        {
            return GetPassedNodes().Count + "/" + Nodes.Count;
        }
        /// <summary>
        /// 获取全结局解锁百分比（分数格式）
        /// </summary>
        public string GetAllEndingNodesPassFraction()
        {
            return GetPassedEndingNodes().Count + "/" + GetEndingNodes().Count;
        }
        /// <summary>
        /// 获取全节点通过百分比（小数格式）
        /// </summary>
        public float GetAllNodesPassPercentage()
        {
            if (GetPassedNodes().Count == 0)
            {
                return 0;
            }
            return (float)GetPassedNodes().Count / Nodes.Count;
        }
        /// <summary>
        /// 获取全结局解锁百分比（小数格式）
        /// </summary>
        public float GetAllEndingNodesPassPercentage()
        {
            if (GetEndingNodes().Count == 0)
            {
                return 0;
            }
            return (float)GetPassedEndingNodes().Count / GetEndingNodes().Count;
        }

        //设置
        /// <summary>
        /// 设置节点编号
        /// </summary>
        /// <param name="id"></param>
        public void SetNodeID(Node node, NodeID id)
        {
            if (!ContainsID(id))
            {
                //同步上行节点连接
                foreach (Node item in Nodes)
                {
                    if (item.NextNodes != null)
                    {
                        for (int i = 0; i < item.NextNodes.Count; i++)
                        {
                            if (item.NextNodes[i].ID.Equals(node.ID))
                            {
                                item.NextNodes[i].ID = id;
                            }
                        }
                    }
                }
                node.ID = id;
            }
            else
            {
                Debug.LogError("此编号的节点已存在 {" + id.Round + "-" + id.Chapter + "-" + id.Scene + "-" + id.Part + "-" + id.Branch + "}");
            }
        }
        /// <summary>
        /// 设置节点类型
        /// </summary>
        public void SetNodeType(Node node, NodeType nodeType)
        {
            if (ContainsID(node.ID))
            {
                switch (nodeType)
                {
                    case NodeType.中间节点:
                        break;
                    case NodeType.起始节点:
                        foreach (Node item in Nodes)
                        {
                            if (item.Type == NodeType.起始节点)
                            {
                                item.Type = NodeType.中间节点;
                            }
                        }
                        ClearNodeUpChoices(node);
                        break;
                    case NodeType.结局节点:
                        ClearNodeDownChoices(node);
                        break;
                    default:
                        break;
                }
                node.Type = nodeType;
            }
        }

        //删除
        /// <summary>
        /// 删除节点
        /// </summary>
        public void DeleteNode(Node node)
        {
            if (Nodes.Contains(node))
            {
                string str = "确认要删除此节点吗？";
                if (EditorUtility.DisplayDialog("警告", str, "确认", "取消"))
                {
                    Nodes.Remove(node);
                }
            }
        }
        /// <summary>
        /// 删除节点内容
        /// </summary>
        public void DeleteNodeContent(Node node)
        {
            if (Nodes.Contains(node))
            {
                node.Content = null;
            }
        }

        //清除
        /// <summary>
        /// 清除节点下行连接
        /// </summary>
        /// <param name="node"></param>
        public void ClearNodeDownChoices(Node node)
        {
            if (node.NextNodes != null)
            {
                node.NextNodes.Clear();
            }
        }
        /// <summary>
        /// 清除节点上行连接
        /// </summary>
        /// <param name="node"></param>
        public void ClearNodeUpChoices(Node node)
        {
            foreach (Node item in Nodes)
            {
                if (item.NextNodes != null)
                {
                    for (int i = 0; i < item.NextNodes.Count; i++)
                    {
                        if (item.NextNodes[i].ID.Equals(node.ID))
                        {
                            item.NextNodes.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 清空节点
        /// </summary>
        public void ClearNodes()
        {
            string str = "确认要清空所有节点吗？";
            if (EditorUtility.DisplayDialog("警告", str, "确认", "取消"))
            {
                Nodes.Clear();
            }
        }
    }

    [Serializable]
    public class Node
    {
        [ReadOnly] public NodeID ID;
        [ReadOnly] public NodeType Type;
        public bool IsPassed;
        public bool IsMainNode;
        [Tooltip("节点内容")] public StoryContent Content;
        [Tooltip("节点分支选项")] public List<NextNode> NextNodes;

        [Tooltip("节点布局")] public RectInt Rect;
        [Tooltip("节点展开")] public bool IsFold;

        public Node(NodeID id, RectInt rect)
        {
            ID = id;
            Type = NodeType.中间节点;
            IsPassed = false;
            IsMainNode = true;
            Content = null;
            NextNodes = new List<NextNode>();
            Rect = rect;
            IsFold = false;
        }
        public bool ContainsNextNode(NodeID id)
        {
            foreach (NextNode item in NextNodes)
            {
                if (item.ID.Equals(id))
                {
                    return true;
                }
            }
            return false;
        }
    }

    [Serializable]
    public class NextNode
    {
        [Tooltip("选项描述")] public string Describe;
        [Tooltip("下一个节点")] public NodeID ID;

        public NextNode(string text, NodeID id)
        {
            Describe = text;
            ID = id;
        }
    }
    [Serializable]
    public struct NodeID
    {
        [Tooltip("周目号")] public int Round;
        [Tooltip("章节号")] public int Chapter;
        [Tooltip("场景号")] public int Scene;
        [Tooltip("片段号")] public int Part;
        [Tooltip("分支号")] public int Branch;

        public NodeID(int r, int c, int s, int p, int b)
        {
            Round = r;
            Chapter = c;
            Scene = s;
            Part = p;
            Branch = b;
        }

        public override bool Equals(object obj)
        {
            NodeID other = (NodeID)obj;
            if (Round == other.Round && Chapter == other.Chapter && Scene == other.Scene && Part == other.Part && Branch == other.Branch)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum NodeType
    {
        中间节点,
        起始节点,
        结局节点
    }
    public enum ContentType
    {
        剧情对话,
        过场动画
    }
}