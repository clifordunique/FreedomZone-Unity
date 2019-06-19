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
    [CustomEditor(typeof(StoryContent))]
    public class StoryContentEditor : Editor
    {
        private StoryContent Content;

        private void OnEnable()
        {
            Content = (StoryContent)target;
        }
        public override void OnInspectorGUI()
        {
            Content.IsReaded = EditorGUILayout.ToggleLeft("是否已阅读过此内容", Content.IsReaded);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("发生时间（年-月-日-时-分-秒）");
            EditorGUILayout.BeginHorizontal();
            int y = EditorGUILayout.IntField(Content.Time.Year);
            int mo = EditorGUILayout.IntField(Content.Time.Month);
            int d = EditorGUILayout.IntField(Content.Time.Day);
            int h = EditorGUILayout.IntField(Content.Time.Hour);
            int mi = EditorGUILayout.IntField(Content.Time.Minute);
            int s = EditorGUILayout.IntField(Content.Time.Second);
            Content.Time = new DateTime(y, mo, d, h, mi, s);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("发生地点");
            Content.Position = EditorGUILayout.TextField(Content.Position);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("内容概述");
            Content.Summary = EditorGUILayout.TextArea(Content.Summary, GUILayout.Height(50));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("内容类型");
            Content.ContentType = (StoryContentType)EditorGUILayout.EnumPopup(Content.ContentType);
            EditorGUILayout.Space();

            SerializedObject serializedObject = new SerializedObject(Content);
            SerializedProperty Sentences = serializedObject.FindProperty("Sentences");
            SerializedProperty Animations = serializedObject.FindProperty("Animations");
            //设置内容折叠
            switch (Content.ContentType)
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