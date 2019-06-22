// ========================================================
// 作者：E Star
// 创建时间：2019-02-22 01:31:07
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using E.Utility;

namespace E.Tool
{
    public class StoryEditorWindow : EditorWindow
    {
        /// <summary>
        /// 窗口实例
        /// </summary>
        public static StoryEditorWindow Instance;
        /// <summary>
        /// 配置信息
        /// </summary>
        public static StoryEditorConfig Config;

        /// <summary>
        /// 故事集和
        /// </summary>
        private static List<ScriptableStory> Storys = new List<ScriptableStory>();
        /// <summary>
        /// 当前故事
        /// </summary>
        private static ScriptableStory CurrentStory;
        /// <summary>
        /// 当前节点
        /// </summary>
        private static StoryNode CurrentStoryNode;
        /// <summary>
        /// 上节点
        /// </summary>
        private static StoryNode UpStoryNode;
        /// <summary>
        /// 下节点
        /// </summary>
        private static StoryNode DownStoryNode;

        /// <summary>
        /// 窗口尺寸
        /// </summary>
        private static Rect View;
        /// <summary>
        /// 滚动条位置
        /// </summary>
        private static Vector3 ScrollPos;
        /// <summary>
        /// 光标位置
        /// </summary>
        private static Vector2 MousePos;
        /// <summary>
        /// X拖拽偏移
        /// </summary>
        private static float Xoffset;
        /// <summary>
        /// Y拖拽偏移
        /// </summary>
        private static float Yoffset;

        //打开
        /// <summary>
        /// 打开窗口
        /// </summary>
        [MenuItem("E Tool/E Story/打开编辑器窗口 %#w", false, 0)]
        public static void Open()
        {
            //获取配置信息
            Config = StoryEditorConfig.GetDictionaryValues()[0];
            //创建窗口
            Instance = GetWindow<StoryEditorWindow>();
            Instance.titleContent = new GUIContent(StoryEditorConfig.WindowTitle);
            Reset();
            Refresh();
            //Show();
            Debug.Log("打开故事编辑器");
        }
        /// <summary>
        /// 打开故事
        /// </summary>
        /// <param name="story"></param>
        private static void OpenStoryTree(ScriptableStory story)
        {
            if (story != null)
            {
                CurrentStory = story;
                ClearStoryTreeNullNodes(CurrentStory);
                Selection.activeObject = CurrentStory;
                Instance.ShowNotification(new GUIContent("故事打开成功：" + CurrentStory.name));
            }
            else
            {
                Debug.LogError("故事打开失败：对象为空");
            }
        }

        //关闭
        /// <summary>
        /// 关闭故事
        /// </summary>
        private static void CloseStory()
        {
            CurrentStory = null;
        }

        //创建
        [MenuItem("E Tool/E Story/创建故事", false, 1)]
        /// <summary>
        /// 创建故事
        /// </summary>
        private static void CreateStory()
        {
            ScriptableStory story = AssetCreator<ScriptableStory>.CreateAsset(Config.StoryResourcesFolder, "Story");
            if (story != null)
            {
                OpenStoryTree(story);
                AssetDatabase.Refresh();
            }
        }
        [MenuItem("E Tool/E Story/创建故事节点", false, 2)]
        /// <summary>
        /// 创建故事节点
        /// </summary>
        private static void CreateStoryNode()
        {
            if (CurrentStory != null)
            {
                StoryNode storyNode = AssetCreator<StoryNode>.CreateAsset(Config.StoryResourcesFolder, CurrentStory.name);
                if (storyNode != null)
                {
                    storyNode.Rect = new Rect(MousePos.x, MousePos.y, Config.DefaultNodeSize.x, Config.DefaultNodeSize.y);
                    if (CurrentStory.Nodes == null)
                    {
                        CurrentStory.Nodes = new List<StoryNode>();
                    }
                    CurrentStory.Nodes.Add(storyNode);
                    Selection.activeObject = storyNode;
                }
            }
            else
            {
                Debug.LogWarning("你需要先打开一个故事才能为其创建故事节点");
            }
        }
        [MenuItem("E Tool/E Story/创建节点内容", false, 3)]
        /// <summary>
        /// 创建节点资源
        /// </summary>
        /// <param name="type"></param>
        private static void CreateStoryNodeContent()
        {
            if (CurrentStoryNode != null)
            {
                StoryContent storyContent = AssetCreator<StoryContent>.CreateAsset(Config.StoryResourcesFolder, CurrentStoryNode.name);
                if (storyContent != null)
                {
                    CurrentStoryNode.Content = storyContent;
                    Selection.activeObject = storyContent;
                    RefreshStoryNodeHeight(CurrentStoryNode);
                }
            }
            else
            {
                Debug.LogWarning("你需要先选择一个节点才能为其创建资源");
            }
        }

        //删除
        /// <summary>
        /// 删除故事
        /// </summary>
        private static void DeleteStory(ScriptableStory story)
        {
            if (story == null)
            {
                Instance.ShowNotification(new GUIContent("未指定要删除的故事"));
            }
            else
            {
                string str = "确认要删除故事 {" + story.name + "} 以及其所有节点吗？本地文件也将一并删除，这将无法恢复。";
                if (EditorUtility.DisplayDialog("警告", str, "确认", "取消"))
                {
                    if (story.Nodes != null)
                    {
                        for (int i = 0; i < story.Nodes.Count; i++)
                        {
                            if (story.Nodes[i] != null)
                            {
                                DeleteStoryNode(story.Nodes[i], false);
                                i--;
                            }
                        }
                        Debug.Log("已删除所有 {" + story.name + "} 的故事节点");
                    }
                    Storys.Remove(story);

                    Selection.activeObject = story;
                    string[] strs = Selection.assetGUIDs;
                    string path = AssetDatabase.GUIDToAssetPath(strs[0]);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.Refresh();
                    Debug.Log("已删除故事 {" + path + "}");
                }
            }
        }
        /// <summary>
        /// 删除故事节点
        /// </summary>
        private static void DeleteStoryNode(StoryNode node, bool isShow)
        {
            if (node != null)
            {
                Selection.activeObject = node;
                string[] strs = Selection.assetGUIDs;
                string path = AssetDatabase.GUIDToAssetPath(strs[0]);
                if (isShow)
                {
                    string str = "确认要删除节点 {" + node.name + "} 吗？本地文件也将一并删除，这将无法恢复。";
                    if (EditorUtility.DisplayDialog("警告", str, "确认", "取消"))
                    {
                        ClearNodeUpChoices(node);
                        AssetDatabase.DeleteAsset(path);
                        AssetDatabase.Refresh();
                        Debug.Log("已删除故事节点 {" + path + "}");
                    }
                }
                else
                {
                    ClearNodeUpChoices(node);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.Refresh();
                    Debug.Log("已删除故事节点 {" + path + "}");
                }
            }
        }

        //刷新
        /// <summary>
        /// 刷新窗口
        /// </summary>
        private static void Refresh()
        {
            Instance.wantsMouseMove = true;
            ClearTempConnect();

            Storys.Clear();
            Storys.AddRange(ScriptableStory.ReGetDictionary().Values);
            string names = "已载入故事 {";
            for (int i = 0; i < Storys.Count; i++)
            {
                if (i < Storys.Count-1)
                {
                    names += Storys[i].name + ", ";
                }
                else
                {
                    names += Storys[i].name;
                }
            }
            names += "}";
            if (Storys.Count > 0)
            {
                Debug.Log(names);
            }
            else
            {
                Debug.Log("未找到任何故事，请确保其位于Resources文件夹内。");
                CurrentStory = null;
            }
        }
        /// <summary>
        /// 刷新鼠标坐标
        /// </summary>
        private static void RefreshMousePosition()
        {
            MousePos = new Vector2((int)(Event.current.mousePosition.x) + (int)ScrollPos.x,
                                     (int)(Event.current.mousePosition.y) + (int)ScrollPos.y);
        }
        /// <summary>
        /// 刷新节点高度
        /// </summary>
        /// <param name="node"></param>
        private static int RefreshStoryNodeHeight(StoryNode node)
        {
            int baseHeight = 50;
            if (node.Content != null)
            {
                switch (node.Content.ContentType)
                {
                    case StoryContentType.剧情对话:
                        if (node.Content != null)
                        {
                            baseHeight = 175;
                        }
                        else
                        {
                            baseHeight = 50;
                        }
                        break;
                    case StoryContentType.过场动画:
                        if (node.Content != null)
                        {
                            baseHeight = 175;
                        }
                        else
                        {
                            baseHeight = 50;
                        }
                        break;
                    default:
                        break;
                }
            }
            int addHeight = 0;
            if (node.Choices != null)
            {
                addHeight = node.Choices.Count * 20;
            }
            node.Rect.height = baseHeight + addHeight;
            return baseHeight;
        }

        //设置
        /// <summary>
        /// 重置窗口
        /// </summary>
        private static void Reset()
        {
            View = new Rect(0, 0, Config.ViewWidth, Config.ViewHeight);
            ScrollPos = Vector2.zero;
            MousePos = Vector2.zero;
            Xoffset = 0;
            Yoffset = 0;
        }
        /// <summary>
        /// 设置节点类型
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeType"></param>
        private static void SetNodeType(StoryNode node, StoryNodeType nodeType)
        {
            if (node != null)
            {
                switch (nodeType)
                {
                    case StoryNodeType.中间节点:
                        if (CurrentStory.StartNode == node)
                        {
                            CurrentStory.StartNode = null;
                        }
                        if (CurrentStory.EndNodes.Contains(node))
                        {
                            CurrentStory.EndNodes.Remove(node);
                        }
                        break;
                    case StoryNodeType.起始节点:
                        if (CurrentStory.EndNodes.Contains(node))
                        {
                            CurrentStory.EndNodes.Remove(node);
                        }
                        if (CurrentStory.StartNode == null)
                        {
                            CurrentStory.StartNode = node;
                            ClearNodeUpChoices(node);
                        }
                        else if (CurrentStory.StartNode != node)
                        {
                            SetNodeType(CurrentStory.StartNode, StoryNodeType.中间节点);
                            CurrentStory.StartNode = node;
                            ClearNodeUpChoices(node);
                        }
                        break;
                    case StoryNodeType.结局节点:
                        if (CurrentStory.StartNode == node)
                        {
                            CurrentStory.StartNode = null;
                        }
                        CurrentStory.EndNodes.Add(node);
                        ClearNodeDownChoices(node);
                        break;
                    default:
                        break;
                }
                node.NodeType = nodeType;
            }
        }

        //选择
        /// <summary>
        /// 选择故事节点
        /// </summary>
        /// <param name="node"></param>
        private static void SelectStoryNode(StoryNode node)
        {
            if (node != null)
            {
                Selection.activeObject = node;
            }
        }

        //执行
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="obj"></param>
        private static void DoMethod(object obj)
        {
            switch (obj)
            {
                case 1:
                    CreateStory();
                    break;
                case 2:
                    CreateStoryNode();
                    break;
                case 3:
                    CreateStoryNodeContent();
                    break;
                case 4:
                    DeleteStory(CurrentStory);
                    break;
                case 5:
                    ClearNodeUpChoices(CurrentStoryNode);
                    ClearNodeDownChoices(CurrentStoryNode);
                    break;
                case 6:
                    //SelectStoryNode(m_CurrentStoryNode);
                    break;
                case 7:
                    DeleteStoryNode(CurrentStoryNode, true);
                    break;
                case 8:
                    CloseStory();
                    break;
                case 9:
                    SetNodeType(CurrentStoryNode, StoryNodeType.起始节点);
                    break;
                case 10:
                    SetNodeType(CurrentStoryNode, StoryNodeType.中间节点);
                    break;
                case 11:
                    SetNodeType(CurrentStoryNode, StoryNodeType.结局节点);
                    break;

                default:
                    Debug.LogError("此右键菜单无实现");
                    break;
            }
        }

        //检查
        /// <summary>
        /// 检测鼠标事件
        /// </summary>
        private static void CheckMouseEvent()
        {
            if (Event.current.type == EventType.MouseMove)
            {
                RefreshMousePosition();
            }
            if (Event.current.type == EventType.MouseDown)
            {
                RefreshMousePosition();
                if (CurrentStory != null && CurrentStory.Nodes != null)
                {
                    //获取当前选中节点
                    bool isInNode = false;
                    for (int i = 0; i < CurrentStory.Nodes.Count; i++)
                    {
                        StoryNode node = CurrentStory.Nodes[i];
                        if (node != null)
                        {
                            if (node.Rect.Contains(MousePos))
                            {
                                Xoffset = MousePos.x - node.Rect.x;
                                Yoffset = MousePos.y - node.Rect.y;
                                isInNode = true;
                                //选中此节点
                                CurrentStoryNode = node;
                                Selection.activeObject = node;
                                break;
                            }
                        }
                    }

                    if (!isInNode)
                    {
                        CurrentStoryNode = null;
                        //Selection.activeObject = m_CurrentStoryTree;
                    }
                    else
                    {
                        //将选中节点置顶显示
                        CurrentStory.Nodes.Remove(CurrentStoryNode);
                        CurrentStory.Nodes.Insert(0, CurrentStoryNode);
                    }
                }
            }
            if (Event.current.type == EventType.MouseDrag)
            {
                RefreshMousePosition();
                if (CurrentStoryNode != null)
                {
                    if (CurrentStoryNode.Rect.Contains(MousePos))
                    {
                        CurrentStoryNode.Rect = new Rect(MousePos.x - Xoffset, MousePos.y - Yoffset, CurrentStoryNode.Rect.width, CurrentStoryNode.Rect.height);
                    }
                }
            }
            if (Event.current.type == EventType.MouseUp)
            {
                RefreshMousePosition();
            }
            if (Event.current.type == EventType.ContextClick)
            {
                RefreshMousePosition();
                if (UpStoryNode == null && DownStoryNode == null)
                {
                    DrawContextMenu();
                    //设置该事件被使用
                    Event.current.Use();
                }
                else
                {
                    ClearTempConnect();
                }
            }
            if (Event.current.isScrollWheel)
            {
            }
        }
        /// <summary>
        /// 检查节点连接
        /// </summary>
        private static void CheckNodeConnect()
        {
            if (UpStoryNode != null && DownStoryNode != null)
            {
                if (UpStoryNode != DownStoryNode)
                {
                    StoryChoice sc = new StoryChoice("", DownStoryNode);
                    if (UpStoryNode.Choices == null)
                    {
                        UpStoryNode.Choices = new List<StoryChoice>();
                    }
                    bool isHave = false;
                    foreach (StoryChoice item in UpStoryNode.Choices)
                    {
                        if (item.NextStoryNode == DownStoryNode)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (!isHave)
                    {
                        UpStoryNode.Choices.Add(sc);
                    }
                    else
                    {
                        Instance.ShowNotification(new GUIContent("已存在连接"));
                    }
                }
                else
                {
                    Instance.ShowNotification(new GUIContent("不可连接自身节点"));
                }
                ClearTempConnect();
            }
        }

        //清除
        /// <summary>
        /// 清除故事节点列表的空节点
        /// </summary>
        private static void ClearStoryTreeNullNodes(ScriptableStory story)
        {
            if (story.Nodes != null)
            {
                //移除空位
                for (int i = 0; i < story.Nodes.Count; i++)
                {
                    if (story.Nodes[i] == null)
                    {
                        story.Nodes.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (story.EndNodes != null)
            {
                //移除空位
                for (int i = 0; i < story.EndNodes.Count; i++)
                {
                    if (story.EndNodes[i] == null)
                    {
                        story.EndNodes.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        /// <summary>
        /// 清除节点的空分支
        /// </summary>
        /// <param name="choices"></param>
        private static void ClearStoryNodeNullChoices(List<StoryChoice> choices)
        {
            if (choices != null)
            {
                //移除空位
                for (int i = 0; i < choices.Count; i++)
                {
                    if (choices[i] == null)
                    {
                        choices.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        /// <summary>
        /// 清除节点临时连接
        /// </summary>
        private static void ClearTempConnect()
        {
            UpStoryNode = null;
            DownStoryNode = null;
        }
        /// <summary>
        /// 清除节点下行连接
        /// </summary>
        /// <param name="node"></param>
        private static void ClearNodeDownChoices(StoryNode node)
        {
            if (node.Choices != null)
            {
                node.Choices.Clear();
            }
        }
        /// <summary>
        /// 清除节点上行连接
        /// </summary>
        /// <param name="node"></param>
        private static void ClearNodeUpChoices(StoryNode node)
        {
            if (node.Choices != null)
            {
                foreach (StoryNode item in CurrentStory.Nodes)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    if (item.Choices != null)
                    {
                        for (int i = 0; i < item.Choices.Count; i++)
                        {
                            if (item.Choices[i].NextStoryNode == node)
                            {
                                item.Choices.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }
        }

        //绘制
        /// <summary>
        /// 绘制背景网格
        /// </summary>
        private static void DrawBG()
        {
            int xMin = (int)(ScrollPos.x);
            int xMax = (int)(ScrollPos.x + Instance.position.width);
            int yMin = (int)(ScrollPos.y);
            int yMax = (int)(ScrollPos.y + Instance.position.height);
            int xStart = xMin - xMin % 100;
            int yStart = yMin - yMin % 100;
            //画垂线
            for (int i = xStart; i <= xMax; i += 100)
            {
                Vector3 start = new Vector3(i, ScrollPos.y, 0);
                Vector3 end = new Vector3(i, ScrollPos.y + Instance.position.height, 0);
                DrawLine(start, end);
            }
            //画平线
            for (int i = yStart; i <= yMax; i += 100)
            {
                Vector3 start = new Vector3(ScrollPos.x, i, 0);
                Vector3 end = new Vector3(ScrollPos.x + Instance.position.width, i, 0);
                DrawLine(start, end);
            }
        }
        /// <summary>
        /// 绘制固定面板
        /// </summary>
        private static void DrawFixedPanel()
        {
            /*************按钮*************/
            if (GUI.Button(new Rect(5, Instance.position.height - 85, 70, 20), "编辑配置"))
            {
                Selection.activeObject = Config;
            }
            if (GUI.Button(new Rect(5, Instance.position.height - 60, 70, 20), "重置配置"))
            {
                Config.Reset();
                Refresh();
                Selection.activeObject = Config;
            }
            View = new Rect(0, 0, Config.ViewWidth, Config.ViewHeight);
            /*************故事列表*************/
            Rect box;
            if (Storys.Count == 0)
            {
                box = new Rect(0, 0, 100, 25 * Storys.Count + 50);
                EditorGUI.DrawRect(box, Config.NormalNode);
                EditorGUI.LabelField(new Rect(box.x + 5, box.y + 25, box.width - 10, 16), "空");
                if (GUI.Button(new Rect(box.x + 35, box.y + 25, box.width - 45, 16), "刷新"))
                {
                    Refresh();
                }
            }
            else
            {
                box = new Rect(0, 0, 100, 25 * Storys.Count + 25);
                EditorGUI.DrawRect(box, Config.NormalNode);
                for (int i = 0; i < Storys.Count; i++)
                {
                    if (GUI.Button(new Rect(box.x + 5, box.y + 25 * i + 25, box.width - 10, 20), Storys[i].name))
                    {
                        OpenStoryTree(Storys[i]);
                    }
                }
            }
            EditorGUI.LabelField(new Rect(box.x + 5, box.y + 5, box.width - 10, 16), "故事列表");

            /*************当前故事*************/
            EditorGUI.DrawRect(new Rect(0, Instance.position.height - 20, Instance.position.width, 20), Config.NormalNode);
            string mousePos = "X: " + MousePos.x + "  Y: " + MousePos.y;
            EditorGUI.LabelField(new Rect(5, Instance.position.height - 20, 110, 20), mousePos);
            string currenStoryTreePath = "无";
            if (CurrentStory != null)
            {
                currenStoryTreePath = Application.dataPath + /*Resources. +*/ "/" + CurrentStory.name + ".asset";
            }
            EditorGUI.LabelField(new Rect(115, Instance.position.height - 20, Instance.position.width, 20), "当前打开的故事：" + currenStoryTreePath);
        }
        /// <summary>
        /// 绘制右键菜单
        /// </summary>
        private static void DrawContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("创建故事"), false, DoMethod, 1);
            if (CurrentStory != null)
            {
                menu.AddItem(new GUIContent("关闭当前故事"), false, DoMethod, 8);
                menu.AddItem(new GUIContent("删除当前故事"), false, DoMethod, 4);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("创建节点"), false, DoMethod, 2);
            }
            if (CurrentStoryNode != null)
            {
                //menu.AddItem(new GUIContent("编辑节点"), false, OnClickRightMouse, 6);
                menu.AddItem(new GUIContent("删除节点"), false, DoMethod, 7);
                menu.AddItem(new GUIContent("设为起始节点"), false, DoMethod, 9);
                menu.AddItem(new GUIContent("设为中间节点"), false, DoMethod, 10);
                menu.AddItem(new GUIContent("设为结局节点"), false, DoMethod, 11);
                menu.AddItem(new GUIContent("清除节点连接"), false, DoMethod, 5);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("创建节点内容"), false, DoMethod, 3);
            }
            menu.ShowAsContext();
        }
        /// <summary>
        /// 绘制故事
        /// </summary>
        /// <param name="storyTree"></param>
        private static void DrawStory(ScriptableStory story)
        {
            if (story != null)
            {
                if (story.Nodes != null)
                {
                    for (int i = 0; i < story.Nodes.Count; i++)
                    {
                        int j = story.Nodes.Count - 1 - i;
                        if (story.Nodes[j] != null)
                        {
                            DrawStoryNode(story.Nodes[j]);
                        }
                    }
                }
            }
            else
            {
                Instance.ShowNotification(new GUIContent("创建或打开一个故事"));
            }
        }
        /// <summary>
        /// 绘制故事节点
        /// </summary>
        /// <param name="storyNodes"></param>
        /// <returns></returns>
        private static void DrawStoryNode(StoryNode node)
        {
            if (node != null)
            {
                //节点背景
                if (CurrentStoryNode != null)
                {
                    if (CurrentStoryNode == node)
                    {
                        EditorGUI.DrawRect(node.Rect, Config.SelectNode);
                    }
                    else
                    {
                        EditorGUI.DrawRect(node.Rect, Config.NormalNode);
                    }
                }
                else
                {
                    EditorGUI.DrawRect(node.Rect, Config.NormalNode);
                }
                //节点类型
                switch (node.NodeType)
                {
                    case StoryNodeType.中间节点:
                        DrawUpButton(node);
                        DrawDownButton(node);
                        break;
                    case StoryNodeType.起始节点:
                        DrawDownButton(node);
                        break;
                    case StoryNodeType.结局节点:
                        DrawUpButton(node);
                        break;
                    default:
                        break;
                }

                //节点属性
                node.IsPassed = EditorGUI.ToggleLeft(new Rect(node.Rect.x + node.Rect.width - 45, node.Rect.y + 5, 100, 16), "通过", node.IsPassed);
                node.IsMainNode = EditorGUI.ToggleLeft(new Rect(node.Rect.x + node.Rect.width - 45, node.Rect.y + 25, 100, 16), "主线", node.IsMainNode);

                //节点内容
                SerializedObject serializedObject = new SerializedObject(node);
                SerializedProperty Content = serializedObject.FindProperty("Content");
                EditorGUI.PropertyField(new Rect(node.Rect.x + 5, node.Rect.y + 5, node.Rect.width - 65, 16), Content, GUIContent.none);

                //节点内容类型
                if (node.Content != null)
                {
                    EditorGUI.LabelField(new Rect(node.Rect.x + 5, node.Rect.y + 25, 40, 16), "类型");
                    node.Content.ContentType = (StoryContentType)EditorGUI.EnumPopup(new Rect(node.Rect.x + 45, node.Rect.y + 25, node.Rect.width - 123, 20), GUIContent.none, node.Content.ContentType);
                    node.Content.IsReaded = EditorGUI.ToggleLeft(new Rect(node.Rect.x + node.Rect.width - 45, node.Rect.y + 65, 45, 16), "阅读", node.Content.IsReaded);

                    EditorGUI.LabelField(new Rect(node.Rect.x + 5, node.Rect.y + 45, 30, 16), "周目");
                    node.ID.Round = EditorGUI.IntField(new Rect(node.Rect.x + 5, node.Rect.y + 65, 30, 16), node.ID.Round);
                    EditorGUI.LabelField(new Rect(node.Rect.x + 40, node.Rect.y + 45, 30, 16), "章节");
                    node.ID.Chapter = EditorGUI.IntField(new Rect(node.Rect.x + 40, node.Rect.y + 65, 30, 16), node.ID.Chapter);
                    EditorGUI.LabelField(new Rect(node.Rect.x + 75, node.Rect.y + 45, 30, 16), "场景");
                    node.ID.Scene = EditorGUI.IntField(new Rect(node.Rect.x + 75, node.Rect.y + 65, 30, 16), node.ID.Scene);
                    EditorGUI.LabelField(new Rect(node.Rect.x + 110, node.Rect.y + 45, 30, 16), "片段");
                    node.ID.Part = EditorGUI.IntField(new Rect(node.Rect.x + 110, node.Rect.y + 65, 30, 16), node.ID.Part);
                    EditorGUI.LabelField(new Rect(node.Rect.x + 145, node.Rect.y + 45, 30, 16), "分支");
                    node.ID.Branch = EditorGUI.IntField(new Rect(node.Rect.x + 145, node.Rect.y + 65, 30, 16), node.ID.Branch);

                    EditorGUI.LabelField(new Rect(node.Rect.x + 5, node.Rect.y + 85, 40, 16), "时间");
                    int y = EditorGUI.IntField(new Rect(node.Rect.x + 45, node.Rect.y + 85, 40, 16), node.Content.Time.Year);
                    int mo = EditorGUI.IntField(new Rect(node.Rect.x + 88, node.Rect.y + 85, 28, 16), node.Content.Time.Month);
                    int d = EditorGUI.IntField(new Rect(node.Rect.x + 119, node.Rect.y + 85, 28, 16), node.Content.Time.Day);
                    int h = EditorGUI.IntField(new Rect(node.Rect.x + 155, node.Rect.y + 85, 28, 16), node.Content.Time.Hour);
                    int mi = EditorGUI.IntField(new Rect(node.Rect.x + 186, node.Rect.y + 85, 28, 16), node.Content.Time.Minute);
                    int s = EditorGUI.IntField(new Rect(node.Rect.x + 217, node.Rect.y + 85, 28, 16), node.Content.Time.Second);
                    node.Content.Time = new DateTime(y, mo, d, h, mi, s);
                    EditorGUI.LabelField(new Rect(node.Rect.x + 5, node.Rect.y + 105, 40, 16), "地点");
                    node.Content.Position = EditorGUI.TextField(new Rect(node.Rect.x + 45, node.Rect.y + 105, node.Rect.width - 50, 16), node.Content.Position);
                    EditorGUI.LabelField(new Rect(node.Rect.x + 5, node.Rect.y + 125, 40, 16), "概述");
                    node.Content.Summary = EditorGUI.TextArea(new Rect(node.Rect.x + 45, node.Rect.y + 125, node.Rect.width - 50, 42), node.Content.Summary);
                }
                else
                {
                    EditorGUI.LabelField(new Rect(node.Rect.x + 5, node.Rect.y + 25, node.Rect.width - 83, 16), "请创建或引用一个节点内容");
                }

                int baseHeight = RefreshStoryNodeHeight(node);
                //节点接点
                if (node.Choices != null)
                {
                    ClearStoryNodeNullChoices(node.Choices);
                    for (int i = 0; i < node.Choices.Count; i++)
                    {
                        StoryNode nextNode = node.Choices[i].NextStoryNode;
                        if (nextNode != null)
                        {
                            string label = "";
                            if (nextNode.Content != null)
                            {
                                label = "分支" + nextNode.ID.Branch;
                            }
                            else
                            {
                                label = "分支?";
                            }
                            EditorGUI.LabelField(new Rect(node.Rect.x + 5, node.Rect.y + 20 * i + baseHeight, 40, 16), label);
                            node.Choices[i].ChoiceText = EditorGUI.TextField(new Rect(node.Rect.x + 45, node.Rect.y + 20 * i + baseHeight, node.Rect.width - 75, 16), node.Choices[i].ChoiceText);
                            if (GUI.Button(new Rect(node.Rect.x + node.Rect.width - 25, node.Rect.y + 20 * i + baseHeight, 20, 16), "X"))
                            {
                                node.Choices.RemoveAt(i);
                                i--;
                            }
                        }
                        else
                        {
                            node.Choices.RemoveAt(i);
                        }
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
        /// <summary>
        /// 绘制节点上按钮
        /// </summary>
        /// <param name="node"></param>
        private static void DrawUpButton(StoryNode node)
        {
            if (GUI.Button(new Rect(node.Rect.x + node.Rect.width / 2 - 10, node.Rect.y - 20, 20, 20), "↑"))
            {
                DownStoryNode = node;
                CheckNodeConnect();
            }
        }
        /// <summary>
        /// 绘制节点下按钮
        /// </summary>
        /// <param name="node"></param>
        private static void DrawDownButton(StoryNode node)
        {
            if (GUI.Button(new Rect(node.Rect.x + node.Rect.width / 2 - 10, node.Rect.y + node.Rect.height, 20, 20), "↓"))
            {
                UpStoryNode = node;
                CheckNodeConnect();
            }
        }
        /// <summary>
        /// 绘制节点曲线
        /// </summary>
        private static void DrawNodeCurve()
        {
            if (CurrentStory != null)
            {
                if (CurrentStory.Nodes != null)
                {
                    foreach (StoryNode node in CurrentStory.Nodes)
                    {
                        if (node == null)
                        {
                            continue;
                        }
                        else
                        {
                        }

                        if (node.Choices != null)
                        {
                            foreach (StoryChoice choice in node.Choices)
                            {
                                if (choice.NextStoryNode != null & CurrentStory.Nodes.Contains(choice.NextStoryNode))
                                {
                                    if (node.IsMainNode & choice.NextStoryNode.IsMainNode)
                                    {
                                        DrawCurve(node.Rect, choice.NextStoryNode.Rect, true);
                                    }
                                    else
                                    {
                                        DrawCurve(node.Rect, choice.NextStoryNode.Rect, false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 绘制节点临时曲线
        /// </summary>
        private static void DrawTempCurve()
        {
            if (UpStoryNode != null)
            {
                DrawCurve(UpStoryNode.Rect, new Rect(MousePos.x, MousePos.y + 20, 0, 0), true);
            }
            if (DownStoryNode != null)
            {
                DrawCurve(new Rect(MousePos.x, MousePos.y - 20, 0, 0), DownStoryNode.Rect, true);
            }
        }
        /// <summary>
        /// 绘制曲线
        /// </summary>
        /// <param name="start">起始节点窗口</param>
        /// <param name="end">结束节点窗口</param>
        /// <param name="isMainLine">是否绘制成主线高亮颜色</param>
        private static void DrawCurve(Rect start, Rect end, bool isMainLine)
        {
            Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height + 20, 0);
            Vector3 endPos = new Vector3(end.x + end.width / 2, end.y - 20, 0);
            Vector3 startTan = startPos + Vector3.up * 50;
            Vector3 endTan = endPos + Vector3.down * 50;
            //绘制阴影
            for (int i = 0; i < 3; i++)
            {
                // Handles.DrawBezier(startPos, endPos, startTan, endTan, m_Shadow, null, (i + 1) * 5);
            }
            //绘制线条
            if (isMainLine)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, Config.MainLine, null, 4);
            }
            else
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, Config.BranchLine, null, 3);
            }
        }
        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private static void DrawLine(Vector3 start, Vector3 end)
        {
            Handles.DrawBezier(start, end, start, end, Config.BGLine, null, 2);
        }

        //mono
        private void Update()
        {
            Repaint();
        }
        private void OnGUI()
        {
            CheckMouseEvent();

            ScrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height - 20), ScrollPos, View);
            DrawBG();
            DrawNodeCurve();
            DrawTempCurve();
            DrawStory(CurrentStory);
            GUI.EndScrollView();

            DrawFixedPanel();
        }
        private void OnProjectChange()
        {
            Refresh();
        }
    }
}