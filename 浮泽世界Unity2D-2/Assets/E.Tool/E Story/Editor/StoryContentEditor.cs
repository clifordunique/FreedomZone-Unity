// ========================================================
// 作者：E Star
// 创建时间：2019-03-10 17:03:03
// 当前版本：1.0
// 作用描述：自定义Inspector面板显示StoryContent的样式
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
    [CustomEditor(typeof(StoryContent))]
    public class StoryContentEditor : Editor
    {
        private StoryContent Target;

        private void OnEnable()
        {
            Target = (StoryContent)target;
        }
        public override void OnInspectorGUI()
        {
            //SerializedObject serializedObject = new SerializedObject(Target);
            //SerializedProperty Sentences = serializedObject.FindProperty("Sentences");
            //SerializedProperty Animations = serializedObject.FindProperty("Animations");

            EditorGUILayout.LabelField("【阅读情况】");
            Target.IsReaded = EditorGUILayout.ToggleLeft("已阅读过此内容", Target.IsReaded);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("【基本信息】");
            EditorGUILayout.LabelField("发生时间 {年-月-日-时-分-秒}");
            EditorGUILayout.BeginHorizontal();
            int y = EditorGUILayout.IntField(Target.Time.Year);
            int mo = EditorGUILayout.IntField(Target.Time.Month);
            int d = EditorGUILayout.IntField(Target.Time.Day);
            int h = EditorGUILayout.IntField(Target.Time.Hour);
            int mi = EditorGUILayout.IntField(Target.Time.Minute);
            int s = EditorGUILayout.IntField(Target.Time.Second);
            Target.Time = new DateTime(y, mo, d, h, mi, s);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("发生地点");
            Target.Position = EditorGUILayout.TextField(Target.Position);
            EditorGUILayout.LabelField("内容概述");
            Target.Summary = EditorGUILayout.TextArea(Target.Summary);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("【内容详情】");
            Target.Type = (ContentType)EditorGUILayout.EnumPopup(Target.Type);
            //设置内容折叠
            switch (Target.Type)
            {
                case ContentType.剧情对话:
                    //EditorGUILayout.PropertyField(Sentences, true);
                    //EditorGUILayout.LabelField("【对话内容】");
                    if (Target.Sentences.Count > 0)
                    {
                        for (int i = 0; i < Target.Sentences.Count; i++)
                        {
                            StoryContent.Sentence item = Target.Sentences[i];
                            EditorGUILayout.LabelField("对话 " + (i + 1).ToString());

                            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                            EditorGUILayout.BeginHorizontal();
                            item.Speaker = EditorGUILayout.TextField(item.Speaker);
                            item.Expression = (Sprite)EditorGUILayout.ObjectField(item.Expression, typeof(Sprite));
                            EditorGUILayout.EndHorizontal();

                            item.Words = EditorGUILayout.TextArea(item.Words);

                            EditorGUILayout.BeginHorizontal();
                            item.IsReaded = EditorGUILayout.ToggleLeft("已阅读过此句话", item.IsReaded);
                            if (GUILayout.Button("X"))
                            {
                                Target.Sentences.Remove(item);
                                i--;
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("请点击按钮添加对话", MessageType.Warning);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal(EditorStyles.inspectorDefaultMargins);
                    EditorGUILayout.LabelField("");
                    if (GUILayout.Button("+"))
                    {
                        Target.Sentences.Add(new StoryContent.Sentence());
                    }
                    EditorGUILayout.EndHorizontal();
                    break;
                case ContentType.过场动画:
                    //EditorGUILayout.PropertyField(Animations, true);
                    //EditorGUILayout.LabelField("【动画内容】");
                    break;
                default:
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}