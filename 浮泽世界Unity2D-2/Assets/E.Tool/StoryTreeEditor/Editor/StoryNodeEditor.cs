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
    [CustomEditor(typeof(StoryNode))]
    public class StoryNodeEditor : Editor
    {
        private StoryNode Node;

        private void OnEnable()
        {
            Node = (StoryNode)target;
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("--------【节点信息】----------------------------------------");
            EditorGUILayout.LabelField("节点编号（周目号-章节号-场景号-片段号-分支号）");
            EditorGUILayout.BeginHorizontal();
            Node.ID.Round = EditorGUILayout.IntField(Node.ID.Round);
            Node.ID.Chapter = EditorGUILayout.IntField(Node.ID.Chapter);
            Node.ID.Scene = EditorGUILayout.IntField(Node.ID.Scene);
            Node.ID.Part = EditorGUILayout.IntField(Node.ID.Part);
            Node.ID.Branch = EditorGUILayout.IntField(Node.ID.Branch);
            EditorGUILayout.EndHorizontal();
            if (Node.ID.Round <= 0 || Node.ID.Chapter <= 0 || Node.ID.Scene <= 0 || Node.ID.Part <= 0 || Node.ID.Branch <= 0)
            {
                EditorGUILayout.HelpBox("请指定一组有效编号（每个数字都要是正整数）", MessageType.Warning);
            }
            EditorGUILayout.LabelField("节点类型");
            Node.NodeType = (StoryNodeType)EditorGUILayout.EnumPopup(Node.NodeType);
            Node.IsPassed = EditorGUILayout.ToggleLeft("是否已通过过此节点", Node.IsPassed);
            Node.IsMainNode = EditorGUILayout.ToggleLeft("是否是主线节点", Node.IsMainNode);


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("--------【节点内容】----------------------------------------");
            SerializedObject so = new SerializedObject(Node);
            SerializedProperty Content = so.FindProperty("Content");
            EditorGUILayout.LabelField("节点内容");
            EditorGUILayout.PropertyField(Content, GUIContent.none);
            so.ApplyModifiedProperties();
            EditorGUILayout.Space();
            if (Node.Content != null)
            {
                Node.Content.IsReaded = EditorGUILayout.ToggleLeft("是否已阅读过此内容", Node.Content.IsReaded);
                //EditorGUILayout.Space();

                EditorGUILayout.LabelField("发生时间（年-月-日-时-分-秒）");
                EditorGUILayout.BeginHorizontal();
                int y = EditorGUILayout.IntField(Node.Content.Time.Year);
                int mo = EditorGUILayout.IntField(Node.Content.Time.Month);
                int d = EditorGUILayout.IntField(Node.Content.Time.Day);
                int h = EditorGUILayout.IntField(Node.Content.Time.Hour);
                int mi = EditorGUILayout.IntField(Node.Content.Time.Minute);
                int s = EditorGUILayout.IntField(Node.Content.Time.Second);
                Node.Content.Time = new DateTime(y, mo, d, h, mi, s);
                EditorGUILayout.EndHorizontal();
                //EditorGUILayout.Space();

                EditorGUILayout.LabelField("发生地点");
                Node.Content.Position = EditorGUILayout.TextField(Node.Content.Position);
                //EditorGUILayout.Space();

                EditorGUILayout.LabelField("内容概述");
                Node.Content.Summary = EditorGUILayout.TextArea(Node.Content.Summary, GUILayout.Height(50));
                //EditorGUILayout.Space();

                EditorGUILayout.LabelField("内容类型");
                Node.Content.ContentType = (StoryContentType)EditorGUILayout.EnumPopup(Node.Content.ContentType);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("内容详情");
                SerializedObject serializedObject = new SerializedObject(Node.Content);
                SerializedProperty Sentences = serializedObject.FindProperty("Sentences");
                SerializedProperty Animations = serializedObject.FindProperty("Animations");
                //设置内容折叠
                switch (Node.Content.ContentType)
                {
                    case StoryContentType.剧情对话:
                        EditorGUILayout.PropertyField(Sentences, true);
                        break;
                    case StoryContentType.过场动画:
                        EditorGUILayout.PropertyField(Animations, true);
                        break;
                    default:
                        break;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}