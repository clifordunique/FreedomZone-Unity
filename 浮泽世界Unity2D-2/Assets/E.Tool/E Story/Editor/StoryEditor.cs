// ========================================================
// 作者：E Star
// 创建时间：2019-03-10 17:03:03
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using E.Utility;

namespace E.Tool
{
    [CustomEditor(typeof(ScriptableStory))]
    public class StoryEditor : Editor
    {
        private ScriptableStory Story;

        private void OnEnable()
        {
            Story = (ScriptableStory)target;
        }

        public override void OnInspectorGUI()
        {
            SerializedObject str = new SerializedObject(Story);
            SerializedProperty describe = str.FindProperty("Describe");
            SerializedProperty isPassed = str.FindProperty("IsPassed");
            //SerializedProperty nodes = str.FindProperty("Nodes");

            EditorGUILayout.PropertyField(describe, new GUIContent("【故事描述】"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("【故事进度】");
            EditorGUILayout.PropertyField(isPassed, new GUIContent("是否已通关"), true);
            //EditorGUILayout.PropertyField(nodes, new GUIContent("节点"), true);
            EditorGUILayout.TextField("全节点通过百分比", (Story.GetAllNodesPassPercentage() * 100).ToString("f2") + "% (" + Story.GetAllNodesPassFraction() + ")"); 
            EditorGUILayout.TextField("全结局解锁百分比", (Story.GetAllEndingNodesPassPercentage() * 100).ToString("f2") + "% (" + Story.GetAllEndingNodesPassFraction() + ")");
            str.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("【故事节点】");
            if (Story.Nodes.Count > 0)
            {
                if (Story.GetStartNode() == null)
                {
                    EditorGUILayout.HelpBox("请设置一个起始节点", MessageType.Warning);
                }
                if (Story.GetEndingNodes().Count == 0)
                {
                    EditorGUILayout.HelpBox("请至少设置一个结局节点", MessageType.Warning);
                }
                foreach (Node item in Story.Nodes)
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
                            Story.SetNodeID(item, id);
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
                        Story.SetNodeType(item, type);
                        //节点内容
                        item.Content = (StoryContent)EditorGUILayout.ObjectField("节点内容", item.Content, typeof(StoryContent));
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