// ========================================================
// 作者：E Star
// 创建时间：2019-03-01 01:33:49
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace E.Utility
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

    [CustomPropertyDrawer(typeof(RenameAttribute))] 
    public class RenameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //替换属性名称
            RenameAttribute rename = (RenameAttribute)attribute;
            label.text = rename.Name;
            //重绘GUI
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}