// ========================================================
// 作者：E Star
// 创建时间：2019-03-10 17:03:03
// 当前版本：1.0
// 作用描述：自定义Inspector面板显示ScriptableStory的样式
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
    [CustomEditor(typeof(ScriptableStory))]
    public class StoryEditor : Editor
    {
        private ScriptableStory Target;

        private void OnEnable()
        {
            Target = (ScriptableStory)target;
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("【故事描述】");
            Target.Describe = EditorGUILayout.TextArea(Target.Describe);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("【故事进度】");
            Target.IsPassed = EditorGUILayout.Toggle("是否已通关", Target.IsPassed);
            EditorGUILayout.TextField("全节点通过百分比", (Target.GetAllNodesPassPercentage() * 100).ToString("f2") + "% (" + Target.GetAllNodesPassFraction() + ")"); 
            EditorGUILayout.TextField("全结局解锁百分比", (Target.GetAllEndingNodesPassPercentage() * 100).ToString("f2") + "% (" + Target.GetAllEndingNodesPassFraction() + ")");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("【故事节点】");
            if (Target.Nodes.Count > 0)
            {
                if (Target.GetStartNode() == null)
                {
                    EditorGUILayout.HelpBox("请设置一个起始节点", MessageType.Warning);
                }
                if (Target.GetEndingNodes().Count == 0)
                {
                    EditorGUILayout.HelpBox("请至少设置一个结局节点", MessageType.Warning);
                }
                foreach (Node item in Target.Nodes)
                {
                    item.IsFold = EditorGUILayout.Foldout(item.IsFold, item.ID.Round + "-" + item.ID.Chapter + "-" + item.ID.Scene + "-" + item.ID.Part + "-" + item.ID.Branch, true);
                    if (item.IsFold)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                        //节点编号
                        EditorGUILayout.LabelField("节点编号 {周目-章节-场景-片段-分支}");
                        EditorGUILayout.BeginHorizontal();
                        int round = EditorGUILayout.IntField(item.ID.Round);
                        int chapter = EditorGUILayout.IntField(item.ID.Chapter);
                        int scene = EditorGUILayout.IntField(item.ID.Scene);
                        int part = EditorGUILayout.IntField(item.ID.Part);
                        int branch = EditorGUILayout.IntField(item.ID.Branch);
                        EditorGUILayout.EndHorizontal();
                        NodeID id = new NodeID(round, chapter, scene, part, branch);
                        if (!id.Equals(item.ID))
                        {
                            Target.SetNodeID(item, id);
                        }
                        //节点布局
                        EditorGUILayout.LabelField("节点布局 {X坐标-Y坐标-宽-高}");
                        EditorGUILayout.BeginHorizontal();
                        int x = EditorGUILayout.IntField(item.Rect.x);
                        int y = EditorGUILayout.IntField(item.Rect.y);
                        int w = EditorGUILayout.IntField(item.Rect.width);
                        int h = EditorGUILayout.IntField(item.Rect.height);
                        EditorGUILayout.EndHorizontal();
                        item.Rect = new RectInt(x, y, w, h);
                        //节点类型
                        NodeType type = (NodeType)EditorGUILayout.EnumPopup("节点类型", item.Type);
                        Target.SetNodeType(item, type);
                        //节点内容
                        item.Content = (ScriptableContent)EditorGUILayout.ObjectField("节点内容", item.Content, typeof(ScriptableContent));
                        //是否已通过
                        item.IsPassed = EditorGUILayout.Toggle("是否已通过", item.IsPassed);
                        //是否为主线
                        item.IsMainNode = EditorGUILayout.Toggle("是否为主线", item.IsMainNode);
                        //后续节点
                        int i = 0;
                        if (item.NextNodes != null)
                        {
                            i = item.NextNodes.Count;
                        }
                        EditorGUILayout.LabelField("后续节点 (" + i + ")");
                        EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                        if (i > 0)
                        {
                            for (int j = 0; j < item.NextNodes.Count; j++)
                            {
                                NextNode nn = item.NextNodes[j];
                                EditorGUILayout.LabelField("选项 " + (j + 1).ToString() + " (" + nn.ID.Round + "-" + nn.ID.Chapter + "-" + nn.ID.Scene + "-" + nn.ID.Part + "-" + nn.ID.Branch + ")");
                                nn.Describe = EditorGUILayout.TextArea(nn.Describe);
                            }
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("请至少创建二个节点", MessageType.Warning);
            }
        }
    }
}