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
using System.Collections.Generic;
using E.Utility;

namespace E.Tool
{
    public class StoryEditorWindow : EditorWindow
    {
        //运行数据
        public static StoryEditorWindow instance;
        private static StoryEditorWindowSetting config;
        private static List<ScriptableStory> storys;
        /// <summary>
        /// 窗口实例
        /// </summary>
        public static StoryEditorWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<StoryEditorWindow>();
                }
                return instance;
            }
        }
        /// <summary>
        /// 配置信息
        /// </summary>
        public static StoryEditorWindowSetting Config
        {
            get
            {
                if (config == null)
                {
                    config = StoryEditorWindowSetting.GetDictionaryValues()[0];
                }
                return config;
            }
        }
        /// <summary>
        /// 故事集和
        /// </summary>
        public static List<ScriptableStory> Storys
        {
            get
            {
                if (storys == null)
                {
                    storys = ScriptableStory.GetDictionaryValues();
                }
                return storys;
            }
        }
        /// <summary>
        /// 当前故事
        /// </summary>
        private static ScriptableStory CurrentStory;
        /// <summary>
        /// 当前节点
        /// </summary>
        private static Node CurrentNode;
        /// <summary>
        /// 上节点
        /// </summary>
        private static Node UpNode;
        /// <summary>
        /// 下节点
        /// </summary>
        private static Node DownNode;

        //窗口样式
        /// <summary>
        /// 窗口尺寸
        /// </summary>
        private static Rect View;
        /// <summary>
        /// 滚动条位置
        /// </summary>
        private static Vector2Int ScrollPos;
        /// <summary>
        /// 光标位置
        /// </summary>
        private static Vector2Int MousePos;
        /// <summary>
        /// X拖拽偏移
        /// </summary>
        private static int Xoffset;
        /// <summary>
        /// Y拖拽偏移
        /// </summary>
        private static int Yoffset;


        //打开
        /// <summary>
        /// 打开窗口
        /// </summary>
        [MenuItem("E Tool/E Story/打开编辑器窗口 %#w", false, 0)]
        public static void Open()
        {
            Initialize();
            Refresh();
            Debug.Log("已打开故事编辑器");
        }
        /// <summary>
        /// 打开故事
        /// </summary>
        /// <param name="story"></param>
        private static void OpenStory(ScriptableStory story)
        {
            if (story != null)
            {
                CurrentStory = story;
                Selection.activeObject = CurrentStory;
                Instance.ShowNotification(new GUIContent("已打开故事：" + CurrentStory.name));
            }
            else
            {
                Debug.LogError("未打开故事：对象为空");
            }
        }

        //关闭
        /// <summary>
        /// 关闭故事
        /// </summary>
        private static void CloseCurrentStory()
        {
            CurrentStory = null;
        }

        //初始化
        /// <summary>
        /// 重置窗口
        /// </summary>
        private static void Initialize()
        {
            Instance.titleContent = new GUIContent("E Story");

            View = new Rect(0, 0, Config.ViewWidth, Config.ViewHeight);
            ScrollPos = Vector2Int.zero;
            MousePos = Vector2Int.zero;
            Xoffset = 0;
            Yoffset = 0;
        }

        //刷新
        /// <summary>
        /// 刷新窗口
        /// </summary>
        private static void Refresh()
        {
            ClearTempConnect();
            RefreshStorys();

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
        /// 刷新故事结合
        /// </summary>
        private static void RefreshStorys()
        {
            storys = ScriptableStory.ReGetDictionaryValues();
        }
        /// <summary>
        /// 刷新鼠标坐标
        /// </summary>
        private static void RefreshMousePosition()
        {
            MousePos = new Vector2Int((int)((Event.current.mousePosition.x) + ScrollPos.x), (int)((Event.current.mousePosition.y) + ScrollPos.y));
        }
        /// <summary>
        /// 刷新节点高度
        /// </summary>
        /// <param name="node"></param>
        private static int RefreshNodeHeight(Node node)
        {
            int baseHeight = Config.DefaultNodeSize.y;
            if (node.Content != null)
            {
                switch (node.Content.Type)
                {
                    case ContentType.剧情对话:
                        if (node.Content != null)
                        {
                            baseHeight = 175;
                        }
                        break;
                    case ContentType.过场动画:
                        if (node.Content != null)
                        {
                            baseHeight = 175;
                        }
                        break;
                    default:
                        break;
                }
            }
            int addHeight = 0;
            if (node.NextNodes != null)
            {
                addHeight = node.NextNodes.Count * 20;
            }
            node.Rect.height = baseHeight + addHeight;
            return baseHeight;
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
                OpenStory(story);
                AssetDatabase.Refresh();
            }
        }
        [MenuItem("E Tool/E Story/创建故事节点", false, 2)]
        /// <summary>
        /// 创建故事节点
        /// </summary>
        private static void CreateNode()
        {
            if (CurrentStory != null)
            {
                CurrentStory.CreateNode(new RectInt(MousePos.x, MousePos.y, Config.DefaultNodeSize.x, Config.DefaultNodeSize.y));
            }
            else
            {
                Instance.ShowNotification(new GUIContent("未指定故事"));
            }
        }
        [MenuItem("E Tool/E Story/创建节点内容", false, 3)]
        /// <summary>
        /// 创建节点资源
        /// </summary>
        /// <param name="type"></param>
        private static void CreateContent()
        {
            if (CurrentStory.Nodes.Contains(CurrentNode))
            {
                StoryContent storyContent = AssetCreator<StoryContent>.CreateAsset(Config.StoryResourcesFolder, "StoryContent");
                if (storyContent != null)
                {
                    CurrentNode.Content = storyContent;
                    Selection.activeObject = storyContent;
                    RefreshNodeHeight(CurrentNode);
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
        private static void DeleteCurrentStory()
        {
            if (CurrentStory != null)
            {
                string str = "确认要删除故事 {" + CurrentStory.name + "} 以及其所有节点吗？本地文件也将一并删除，这将无法恢复。";
                if (EditorUtility.DisplayDialog("警告", str, "确认", "取消"))
                {
                    if (CurrentStory.Nodes != null)
                    {
                        CurrentStory.Nodes.Clear();
                        Debug.Log("已删除所有 {" + CurrentStory.name + "} 的故事节点");
                    }
                    string path = GetCurrentStoryPath();
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.Refresh();
                    Debug.Log("已删除故事 {" + path + "}");
                }
            }
            else
            {
                Instance.ShowNotification(new GUIContent("未指定故事"));
            }
        }

        //获取
        /// <summary>
        /// 获取当前故事的路径
        /// </summary>
        /// <returns></returns>
        private static string GetCurrentStoryPath()
        {
            if (CurrentStory != null)
            {
                UnityEngine.Object ob = Selection.activeObject;
                Selection.activeObject = CurrentStory;
                string[] strs = Selection.assetGUIDs;
                Selection.activeObject = ob;
                return AssetDatabase.GUIDToAssetPath(strs[0]);
            }
            else return null;
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
                    CreateNode();
                    break;
                case 3:
                    CreateContent();
                    break;
                case 4:
                    DeleteCurrentStory();
                    break;
                case 5:
                    CurrentStory.ClearNodeUpChoices(CurrentNode);
                    CurrentStory.ClearNodeDownChoices(CurrentNode);
                    break;
                case 6:
                    CurrentStory.DeleteNodeContent(CurrentNode);
                    break;
                case 7:
                    CurrentStory.DeleteNode(CurrentNode);
                    break;
                case 8:
                    CloseCurrentStory();
                    break;
                case 9:
                    CurrentStory.SetNodeType(CurrentNode, NodeType.起始节点);
                    break;
                case 10:
                    CurrentStory.SetNodeType(CurrentNode, NodeType.中间节点);
                    break;
                case 11:
                    CurrentStory.SetNodeType(CurrentNode, NodeType.结局节点);
                    break;
                case 12:
                    CurrentStory.ClearNodes();
                    break;

                default:
                    Debug.LogError("此右键菜单无实现方法");
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
                    foreach (Node item in CurrentStory.Nodes)
                    {
                        if (item.Rect.Contains(MousePos))
                        {
                            Xoffset = MousePos.x - item.Rect.x;
                            Yoffset = MousePos.y - item.Rect.y;
                            isInNode = true;
                            //选中此节点
                            CurrentNode = item;
                            //只展开此节点
                            foreach (Node it in CurrentStory.Nodes)
                            {
                                it.IsFold = false;
                            }
                            item.IsFold = true;
                            //选中节点内容资源
                            if (CurrentNode.Content != null)
                            {
                                Selection.activeObject = CurrentNode.Content;
                            }
                            else
                            {
                                Selection.activeObject = CurrentStory;
                            }
                            break;
                        }
                    }

                    if (isInNode)
                    {
                        //将选中节点置顶显示
                        CurrentStory.Nodes.Remove(CurrentNode);
                        CurrentStory.Nodes.Insert(CurrentStory.Nodes.Count, CurrentNode);
                    }
                    else
                    {
                        CurrentNode = null;
                        Selection.activeObject = CurrentStory;
                    }
                }
            }
            if (Event.current.type == EventType.MouseDrag)
            {
                RefreshMousePosition();
                if (CurrentStory != null)
                {
                    if (CurrentStory.Nodes.Contains(CurrentNode))
                    {
                        if (CurrentNode.Rect.Contains(MousePos))
                        {
                            CurrentNode.Rect = new RectInt(MousePos.x - Xoffset, MousePos.y - Yoffset, CurrentNode.Rect.width, CurrentNode.Rect.height);
                        }
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
                if (UpNode == null && DownNode == null)
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
            if (UpNode != null && DownNode != null)
            {
                if (UpNode != DownNode)
                {
                    if (UpNode.NextNodes == null)
                    {
                        UpNode.NextNodes = new List<NextNode>();
                    }
                    bool isHave = false;
                    foreach (NextNode item in UpNode.NextNodes)
                    {
                        if (item.ID.Equals(DownNode.ID))
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (!isHave)
                    {
                        NextNode sc = new NextNode("", DownNode.ID);
                        UpNode.NextNodes.Add(sc);
                    }
                    else
                    {
                        Instance.ShowNotification(new GUIContent("已存在连接"));
                    }
                }
                else
                {
                    Instance.ShowNotification(new GUIContent("不可连接自己"));
                }
                ClearTempConnect();
            }
        }

        //清除
        /// <summary>
        /// 清除节点临时连接
        /// </summary>
        private static void ClearTempConnect()
        {
            UpNode = null;
            DownNode = null;
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
                Instance.ShowNotification(new GUIContent("配置已重置"));
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
                        OpenStory(Storys[i]);
                    }
                }
            }
            EditorGUI.LabelField(new Rect(box.x + 5, box.y + 5, box.width - 10, 16), "故事列表");

            /*************当前故事*************/
            EditorGUI.DrawRect(new Rect(0, Instance.position.height - 20, Instance.position.width, 20), Config.NormalNode);
            string mousePos = "X: " + MousePos.x + "  Y: " + MousePos.y;
            EditorGUI.LabelField(new Rect(5, Instance.position.height - 20, 110, 20), mousePos);
            string currenStoryPath = GetCurrentStoryPath();
            if (currenStoryPath == null)
            {
                currenStoryPath = "无";
            }
            EditorGUI.LabelField(new Rect(115, Instance.position.height - 20, Instance.position.width, 20), "当前打开的故事：" + currenStoryPath);
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

                if (CurrentNode != null)
                {
                    menu.AddItem(new GUIContent("删除当前节点"), false, DoMethod, 7);
                    menu.AddItem(new GUIContent("清除所有节点"), false, DoMethod, 12);
                    menu.AddItem(new GUIContent("设为起始节点"), false, DoMethod, 9);
                    menu.AddItem(new GUIContent("设为中间节点"), false, DoMethod, 10);
                    menu.AddItem(new GUIContent("设为结局节点"), false, DoMethod, 11);
                    menu.AddItem(new GUIContent("清除当前节点连接"), false, DoMethod, 5);
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("创建当前节点内容"), false, DoMethod, 3);
                    if (CurrentNode.Content != null)
                    {
                        menu.AddItem(new GUIContent("清除当前节点内容"), false, DoMethod, 6);
                    }
                }
                else
                {
                    if (CurrentStory.Nodes != null)
                    {
                        menu.AddItem(new GUIContent("清除所有节点"), false, DoMethod, 12);
                    }
                }
            }
            menu.ShowAsContext();
        }
        /// <summary>
        /// 绘制故事
        /// </summary>
        private static void DrawStory()
        {
            if (CurrentStory != null)
            {
                if (CurrentStory.Nodes != null)
                {
                    foreach (Node item in CurrentStory.Nodes)
                    {
                        DrawNode(item);
                    }
                }
            }
            else
            {
                Instance.ShowNotification(new GUIContent("创建或打开一个故事"));
            }
        }
        /// <summary>
        /// 绘制节点
        /// </summary>
        private static void DrawNode(Node node)
        {
            if (node != null)
            {
                Rect rect = new Rect(node.Rect.x, node.Rect.y, node.Rect.width, node.Rect.height);

                //节点背景
                if (CurrentNode != null)
                {
                    if (CurrentNode == node)
                    {
                        EditorGUI.DrawRect(rect, Config.SelectNode);
                    }
                    else
                    {
                        EditorGUI.DrawRect(rect, Config.NormalNode);
                    }
                }
                else
                {
                    EditorGUI.DrawRect(rect, Config.NormalNode);
                }
                //节点类型
                switch (node.Type)
                {
                    case NodeType.中间节点:
                        DrawUpButton(node);
                        DrawDownButton(node);
                        break;
                    case NodeType.起始节点:
                        DrawDownButton(node);
                        break;
                    case NodeType.结局节点:
                        DrawUpButton(node);
                        break;
                    default:
                        break;
                }

                //编号
                EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 5, 30, 16), "周目");
                int round = EditorGUI.IntField(new Rect(rect.x + 5, rect.y + 25, 30, 16), node.ID.Round);
                EditorGUI.LabelField(new Rect(rect.x + 40, rect.y + 5, 30, 16), "章节");
                int chapter = EditorGUI.IntField(new Rect(rect.x + 40, rect.y + 25, 30, 16), node.ID.Chapter);
                EditorGUI.LabelField(new Rect(rect.x + 75, rect.y + 5, 30, 16), "场景");
                int scene = EditorGUI.IntField(new Rect(rect.x + 75, rect.y + 25, 30, 16), node.ID.Scene);
                EditorGUI.LabelField(new Rect(rect.x + 110, rect.y + 5, 30, 16), "片段");
                int part = EditorGUI.IntField(new Rect(rect.x + 110, rect.y + 25, 30, 16), node.ID.Part);
                EditorGUI.LabelField(new Rect(rect.x + 145, rect.y + 5, 30, 16), "分支");
                int branch = EditorGUI.IntField(new Rect(rect.x + 145, rect.y + 25, 30, 16), node.ID.Branch);
                NodeID id = new NodeID(round, chapter, scene, part, branch);
                if (!id.Equals(node.ID))
                {
                    CurrentStory.SetNodeID(node, id);
                }
                //通过
                bool ispass = EditorGUI.ToggleLeft(new Rect(rect.x + rect.width - 45, rect.y + 5, 100, 16), "通过", node.IsPassed);
                if (ispass != node.IsPassed)
                {
                    node.IsPassed = ispass;
                }
                //主线
                bool ismain = EditorGUI.ToggleLeft(new Rect(rect.x + rect.width - 45, rect.y + 25, 100, 16), "主线", node.IsMainNode);
                if (ismain != node.IsMainNode)
                {
                    node.IsMainNode = ismain;
                }

                //内容
                EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 45, 40, 16), "内容");
                node.Content = (StoryContent)EditorGUI.ObjectField(new Rect(rect.x + 40, rect.y + 45, 153, 16), node.Content, typeof(StoryContent));
                StoryContent sc = node.Content;
                if (sc != null)
                {
                    //形式
                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 65, 40, 16), "形式");
                    sc.Type = (ContentType)EditorGUI.EnumPopup(new Rect(rect.x + 40, rect.y + 65, 135, 20), GUIContent.none, sc.Type);
                    //阅读
                    sc.IsReaded = EditorGUI.ToggleLeft(new Rect(rect.x + rect.width - 45, rect.y + 65, 45, 16), "阅读", sc.IsReaded);
                    //时间
                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 85, 40, 16), "时间");
                    int y = EditorGUI.IntField(new Rect(rect.x + 40, rect.y + 85, 35, 16), sc.Time.Year);
                    int mo = EditorGUI.IntField(new Rect(rect.x + 78f, rect.y + 85, 30, 16), sc.Time.Month);
                    int d = EditorGUI.IntField(new Rect(rect.x + 111, rect.y + 85, 30, 16), sc.Time.Day);
                    int h = EditorGUI.IntField(new Rect(rect.x + 145, rect.y + 85, 30, 16), sc.Time.Hour);
                    int mi = EditorGUI.IntField(new Rect(rect.x + 180, rect.y + 85, 30, 16), sc.Time.Minute);
                    int s = EditorGUI.IntField(new Rect(rect.x + 215, rect.y + 85, 30, 16), sc.Time.Second);
                    sc.Time = new DateTime(y, mo, d, h, mi, s);
                    //地点
                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 105, 40, 16), "地点");
                    sc.Position = EditorGUI.TextField(new Rect(rect.x + 40, rect.y + 105, rect.width - 45, 16), sc.Position);
                    //概述
                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 125, 40, 16), "概述");
                    sc.Summary = EditorGUI.TextArea(new Rect(rect.x + 40, rect.y + 125, rect.width - 45, 42), sc.Summary);
                }
                else
                {
                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 65, rect.width - 83, 16), "请创建或引用一个节点内容");
                }

                int baseHeight = RefreshNodeHeight(node);

                //后续节点
                if (node.NextNodes != null)
                {
                    for (int i = 0; i < node.NextNodes.Count; i++)
                    {
                        NodeID nextNodeID = node.NextNodes[i].ID;
                        if (CurrentStory.ContainsID(nextNodeID))
                        {
                            string label = nextNodeID.Round + "-" + nextNodeID.Chapter + "-" + nextNodeID.Scene + "-" + nextNodeID.Part + "-" + nextNodeID.Branch;
                            int addWidth = (nextNodeID.Round/10 + nextNodeID.Chapter/ 10 + nextNodeID.Scene / 10 + nextNodeID.Part / 10 + nextNodeID.Branch / 10) * 7;
                            EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 20 * i + baseHeight, 60 + addWidth, 16), label);
                            node.NextNodes[i].Describe = EditorGUI.TextField(new Rect(rect.x + 65 + addWidth, rect.y + 20 * i + baseHeight, rect.width - 95 - addWidth, 16), node.NextNodes[i].Describe);
                            if (GUI.Button(new Rect(rect.x + rect.width - 25, rect.y + 20 * i + baseHeight, 20, 16), "X"))
                            {
                                node.NextNodes.RemoveAt(i);
                                i--;
                            }
                        }
                        else
                        {
                            node.NextNodes.RemoveAt(i);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 绘制节点上按钮
        /// </summary>
        /// <param name="node"></param>
        private static void DrawUpButton(Node node)
        {
            if (GUI.Button(new Rect(node.Rect.x + node.Rect.width / 2 - 10, node.Rect.y - 20, 20, 20), "↑"))
            {
                DownNode = node;
                CheckNodeConnect();
            }
        }
        /// <summary>
        /// 绘制节点下按钮
        /// </summary>
        /// <param name="node"></param>
        private static void DrawDownButton(Node node)
        {
            if (GUI.Button(new Rect(node.Rect.x + node.Rect.width / 2 - 10, node.Rect.y + node.Rect.height, 20, 20), "↓"))
            {
                UpNode = node;
                CheckNodeConnect();
            }
        }
        /// <summary>
        /// 绘制节点连接线
        /// </summary>
        private static void DrawNodeCurve()
        {
            if (CurrentStory != null)
            {
                if (CurrentStory.Nodes != null)
                {
                    foreach (Node item in CurrentStory.Nodes)
                    {
                        if (item.NextNodes != null)
                        {
                            foreach (NextNode choice in item.NextNodes)
                            {
                                if (CurrentStory.ContainsID(choice.ID))
                                {
                                    Node node = CurrentStory.GetNode(choice.ID);
                                    if (item.IsMainNode && node.IsMainNode)
                                    {
                                        DrawCurve(new Rect(item.Rect.x, item.Rect.y, item.Rect.width, item.Rect.height), new Rect(node.Rect.x, node.Rect.y, node.Rect.width, node.Rect.height), true);
                                    }
                                    else
                                    {
                                        DrawCurve(new Rect(item.Rect.x, item.Rect.y, item.Rect.width, item.Rect.height), new Rect(node.Rect.x, node.Rect.y, node.Rect.width, node.Rect.height), false);
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }
        /// <summary>
        /// 绘制节点预览连接线
        /// </summary>
        private static void DrawTempCurve()
        {
            if (UpNode != null)
            {
                DrawCurve(new Rect(UpNode.Rect.x, UpNode.Rect.y, UpNode.Rect.width, UpNode.Rect.height), new Rect(MousePos.x, MousePos.y + 20, 0, 0), true);
            }
            if (DownNode != null)
            {
                DrawCurve(new Rect(MousePos.x, MousePos.y - 20, 0, 0), new Rect(DownNode.Rect.x, DownNode.Rect.y, DownNode.Rect.width, DownNode.Rect.height), true);
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
            Instance.wantsMouseMove = true;
            Repaint();
        }
        private void OnGUI()
        {
            CheckMouseEvent();

            Vector2 v = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height - 20), ScrollPos, View);
            ScrollPos = new Vector2Int((int)v.x ,(int)v.y);
            DrawBG();
            DrawNodeCurve();
            DrawTempCurve();
            DrawStory();
            GUI.EndScrollView();

            DrawFixedPanel();
        }
        private void OnProjectChange()
        {
            Refresh();
        }
    }
}