// ========================================================
// 作者：E Star
// 创建时间：2019-04-09 23:43:18
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace E.Game
{
    [CustomEditor(typeof(InteractorData))]
    public class InteractorDataInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedObject so = new SerializedObject(target);
            //模式
            SerializedProperty m_IsSurveyable = so.FindProperty("m_IsSurveyable");
            SerializedProperty m_IsPickable = so.FindProperty("m_IsPickable");
            SerializedProperty m_IsDestructible = so.FindProperty("m_IsDestructible");
            SerializedProperty m_IsContainer = so.FindProperty("m_IsContainer");
            SerializedProperty m_IsIndividual = so.FindProperty("m_IsIndividual");
            //静态信息
            SerializedProperty m_Prefab = so.FindProperty("m_Prefab");
            SerializedProperty m_Icon = so.FindProperty("m_Icon");
            SerializedProperty m_InteractorName = so.FindProperty("m_InteractorName");
            SerializedProperty m_Describe = so.FindProperty("m_Describe");
            SerializedProperty m_Price = so.FindProperty("m_Price");
            SerializedProperty m_HealthMax = so.FindProperty("m_HealthMax");
            SerializedProperty m_CapacityMax = so.FindProperty("m_CapacityMax");
            SerializedProperty m_LoadMax = so.FindProperty("m_LoadMax");
            SerializedProperty m_Component = so.FindProperty("m_Component");
            //动态信息
            SerializedProperty m_HealthNow = so.FindProperty("m_HealthNow");
            SerializedProperty m_Size = so.FindProperty("m_Size");
            SerializedProperty m_Mass = so.FindProperty("m_Mass");
            SerializedProperty m_CarryingItems = so.FindProperty("m_CarryingItems");
            SerializedProperty m_CapacityUsedNow = so.FindProperty("m_CapacityUsedNow");
            SerializedProperty m_LoadUsedNow = so.FindProperty("m_LoadUsedNow");

            EditorGUILayout.LabelField("--------【交互属性】----------------------------------------");
            EditorGUILayout.PropertyField(m_IsSurveyable, new GUIContent("可调查"));
            EditorGUILayout.PropertyField(m_IsPickable, new GUIContent("可拾取"));
            EditorGUILayout.PropertyField(m_IsDestructible, new GUIContent("可破坏"));
            EditorGUILayout.PropertyField(m_IsContainer, new GUIContent("是容器"));
            EditorGUILayout.PropertyField(m_IsIndividual, new GUIContent("是独立个体"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("--------【静态信息】----------------------------------------");
            EditorGUILayout.PropertyField(m_Prefab, new GUIContent("预制体"));
            EditorGUILayout.PropertyField(m_Icon, new GUIContent("图片"));
            EditorGUILayout.PropertyField(m_InteractorName, new GUIContent("名称"));
            EditorGUILayout.PropertyField(m_Describe, new GUIContent("描述"));
            EditorGUILayout.PropertyField(m_Price, new GUIContent("价格"));
            if (m_IsDestructible.boolValue)
            {
            EditorGUILayout.PropertyField(m_HealthMax, new GUIContent("耐久上限"));
            }
            if (m_IsContainer.boolValue)
            {
                EditorGUILayout.PropertyField(m_CapacityMax, new GUIContent("容量上限"));
                EditorGUILayout.PropertyField(m_LoadMax, new GUIContent("载重上限"));
            }
            EditorGUILayout.PropertyField(m_Component, new GUIContent("组成"), true);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("--------【动态信息】----------------------------------------");
            if (m_IsDestructible.boolValue)
            {
                EditorGUILayout.PropertyField(m_HealthNow, new GUIContent("当前耐久"));
            }
            EditorGUILayout.PropertyField(m_Size, new GUIContent("体积"));
            EditorGUILayout.PropertyField(m_Mass, new GUIContent("质量"));
            if (m_IsContainer.boolValue)
            {
                EditorGUILayout.PropertyField(m_CarryingItems, new GUIContent("容纳的物品"), true);
                EditorGUILayout.PropertyField(m_CapacityUsedNow, new GUIContent("当前已占用容量"));
                EditorGUILayout.PropertyField(m_LoadUsedNow, new GUIContent("当前已占用载重"));
            }

            so.ApplyModifiedProperties();
        }
    }
}
