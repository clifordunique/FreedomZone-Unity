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
    public class StoryTreeWindow : EditorWindow
    {
        //单例
        public static StoryTreeWindow Instance;
        public ConfigInfo ConfigInfo;

        //窗口状态
        //private float m_Scale;
        private Rect m_View;
        private Vector3 m_ScrollPos;
        private Vector2 m_MousePos;
        private float xOffset;
        private float yOffset;
        //private int n = 0;

        //内容信息
        private static List<StoryTree> m_StoryTrees = new List<StoryTree>();
        private StoryTree m_CurrentStoryTree;
        private StoryNode m_CurrentStoryNode;
        private StoryNode m_UpStoryNode;
        private StoryNode m_DownStoryNode;


        private void Update()
        {
            ////控制每秒重绘20次（默认为100次）
            //if (n % 20 == 0)
            //{
            Repaint();
            //}
            //else
            //{
            //    n++;
            //}
            //if (n>10000)
            //{
            //    n = 0;
            //}
        }
        private void OnInspectorUpdate()
        {
            //Repaint();
        }
        private void OnGUI()
        {
            CheckMouseEvent();

            m_ScrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height - 20), m_ScrollPos, m_View);
            DrawBG();
            DrawNodeCurve();
            DrawTempCurve();
            DrawStoryTree(m_CurrentStoryTree);
            GUI.EndScrollView();

            DrawFixedPanel();
        }
        //private void OnFocus()
        //{
        //}
        //private void OnLostFocus()
        //{

        //}
        //private void OnHierarchyChange()
        //{

        //}
        //private void OnInspectorUpdate()
        //{
        //}
        //private void OnSelectionChange()
        //{
        //}
        private void OnProjectChange()
        {
            Refresh();
        }
        private void OnClickRightMouse(object obj)
        {
            switch (obj)
            {
                case 1:
                    CreateStoryTree();
                    break;
                case 2:
                    CreateStoryNode(m_CurrentStoryTree);
                    break;
                case 3:
                    CreateStoryNodeContent(m_CurrentStoryNode);
                    break;
                case 4:
                    DeleteStoryTree(m_CurrentStoryTree);
                    break;
                case 5:
                    ClearNodeUpChoices(m_CurrentStoryNode);
                    ClearNodeDownChoices(m_CurrentStoryNode);
                    break;
                case 6:
                    //SelectStoryNode(m_CurrentStoryNode);
                    break;
                case 7:
                    DeleteStoryNode(m_CurrentStoryNode, true);
                    break;
                case 8:
                    CloseStoryTree();
                    break;
                case 9:
                    SetNodeType(m_CurrentStoryNode, StoryNodeType.起始节点);
                    break;
                case 10:
                    SetNodeType(m_CurrentStoryNode, StoryNodeType.中间节点);
                    break;
                case 11:
                    SetNodeType(m_CurrentStoryNode, StoryNodeType.结局节点);
                    break;

                default:
                    Debug.LogError("此右键菜单无实现");
                    break;
            }
        }

        /// <summary>
        /// 打开窗口
        /// </summary>
        [MenuItem("Tools/故事树编辑器/打开编辑器窗口 %#w", false, 0)]
        public static void ShowWindow()
        {
            //创建窗口
            Instance = GetWindow<StoryTreeWindow>();
            Instance.titleContent = new GUIContent(ConfigInfo.WindowTitle);
            Instance.ResetWindowState();
            Instance.Refresh();
            //Instance.Show();
            Debug.Log("打开故事树编辑器");
        }
        /// <summary>
        /// 重置窗口样式
        /// </summary>
        private void ResetWindowState()
        {
            //m_Scale = 1;
            m_View = new Rect(0, 0, ConfigInfo.ViewWidth, ConfigInfo.ViewHeight);
            m_ScrollPos = Vector2.zero;
            m_MousePos = Vector2.zero;
            xOffset = 0;
            yOffset = 0;
        }
        /// <summary>
        /// 刷新窗口
        /// </summary>
        private void Refresh()
        {
            wantsMouseMove = true;
            ClearTempConnect();
            if (!Directory.Exists(ConfigInfo.StoryTreeFolder))
            {
                Directory.CreateDirectory(ConfigInfo.StoryTreeFolder);
                Debug.LogWarning("文件夹不存在，已重新创建：" + ConfigInfo.StoryTreeFolder);
            }

            //遍历故事树文件夹，获取所有该文件夹内的故事树文件
            List<StoryTree> tempStoryTrees = new List<StoryTree>();
            DirectoryInfo TheFolder = new DirectoryInfo(ConfigInfo.StoryTreeFolder);
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                string[] name = NextFile.Name.Split('.');
                if (name[1] == "asset" & name.Length == 2)
                {
                    //StoryTree storyTree = Resources.Load<StoryTree>("StoryTrees/" + name[0]);
                    StoryTree storyTree = AssetCreator<StoryTree>.GetAsset(ConfigInfo.StoryTreeFolder + "/" + name[0] + ".asset");
                    if (storyTree != null)
                    {
                        tempStoryTrees.Add(storyTree);
                    }
                }
            }

            if (m_StoryTrees == null)
            {
                m_StoryTrees = new List<StoryTree>();
            }

            //检测窗体引用的故事树是否还存在
            for (int i = 0; i < m_StoryTrees.Count; i++)
            {
                bool isHave = false;
                foreach (StoryTree tempStoryTree in tempStoryTrees)
                {
                    if (m_StoryTrees[i].Equals(tempStoryTree))
                    {
                        isHave = true;
                        break;
                    }
                }
                if (!isHave)
                {
                    m_StoryTrees.Remove(m_StoryTrees[i]);
                }
            }
            //检测文件夹内是否有新的故事树
            foreach (StoryTree tempStoryTree in tempStoryTrees)
            {
                bool isSame = false;
                if (m_StoryTrees.Count > 0)
                {
                    for (int i = 0; i < m_StoryTrees.Count; i++)
                    {
                        if (tempStoryTree.Equals(m_StoryTrees[i]))
                        {
                            isSame = true;
                            break;
                        }
                    }
                }
                if (!isSame)
                {
                    m_StoryTrees.Add(tempStoryTree);
                }
            }

            if (m_StoryTrees.Count == 0)
            {
                m_CurrentStoryTree = null;
            }
        }
        /// <summary>
        /// 获取鼠标坐标
        /// </summary>
        private void GetMousePos()
        {
            m_MousePos = new Vector2((int)(Event.current.mousePosition.x) + (int)m_ScrollPos.x,
                                     (int)(Event.current.mousePosition.y) + (int)m_ScrollPos.y);
        }
        /// <summary>
        /// 检测鼠标事件
        /// </summary>
        private void CheckMouseEvent()
        {
            if (Event.current.type == EventType.MouseMove)
            {
                GetMousePos();
            }
            if (Event.current.type == EventType.MouseDown)
            {
                GetMousePos();
                if (m_CurrentStoryTree != null && m_CurrentStoryTree.Nodes != null)
                {
                    //获取当前选中节点
                    bool isInNode = false;
                    for (int i = 0; i < m_CurrentStoryTree.Nodes.Count; i++)
                    {
                        StoryNode node = m_CurrentStoryTree.Nodes[i];
                        if (node != null)
                        {
                            if (node.Rect.Contains(m_MousePos))
                            {
                                xOffset = m_MousePos.x - node.Rect.x;
                                yOffset = m_MousePos.y - node.Rect.y;
                                isInNode = true;
                                //选中此节点
                                m_CurrentStoryNode = node;
                                Selection.activeObject = node;
                                break;
                            }
                        }
                    }

                    if (!isInNode)
                    {
                        m_CurrentStoryNode = null;
                        Selection.activeObject = m_CurrentStoryTree;
                    }
                    else
                    {
                        //将选中节点置顶显示
                        m_CurrentStoryTree.Nodes.Remove(m_CurrentStoryNode);
                        m_CurrentStoryTree.Nodes.Insert(0, m_CurrentStoryNode);
                    }
                }
            }
            if (Event.current.type == EventType.MouseDrag)
            {
                GetMousePos();
                if (m_CurrentStoryNode != null)
                {
                    if (m_CurrentStoryNode.Rect.Contains(m_MousePos))
                    {
                        m_CurrentStoryNode.Rect = new Rect(m_MousePos.x - xOffset, m_MousePos.y - yOffset, m_CurrentStoryNode.Rect.width, m_CurrentStoryNode.Rect.height);
                    }
                }
            }
            if (Event.current.type == EventType.MouseUp)
            {
                GetMousePos();
            }
            if (Event.current.type == EventType.ContextClick)
            {
                GetMousePos();
                if (m_UpStoryNode == null && m_DownStoryNode == null)
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
        /// 绘制背景网格
        /// </summary>
        private void DrawBG()
        {
            ////画垂线
            //for (int i = 0; i <= ConfigInfo.ViewWidth; i += 100)
            //{
            //    Vector3 start = new Vector3(i, 0, 0);
            //    Vector3 end = new Vector3(i, ConfigInfo.ViewHeight, 0);
            //    DrawLine(start, end);
            //}
            ////画平线
            //for (int i = 0; i <= ConfigInfo.ViewHeight; i += 100)
            //{
            //    Vector3 start = new Vector3(0, i, 0);
            //    Vector3 end = new Vector3(ConfigInfo.ViewWidth, i, 0);
            //    DrawLine(start, end);
            //}

            int xMin = (int)(m_ScrollPos.x);
            int xMax = (int)(m_ScrollPos.x + position.width);
            int yMin = (int)(m_ScrollPos.y);
            int yMax = (int)(m_ScrollPos.y + position.height);
            int xStart = xMin - xMin % 100;
            int yStart = yMin - yMin % 100;
            //Debug.Log(m_ScrollPos.x +"  *  " +m_ScrollPos.y);
            //Debug.Log(xMin + "." + xMax + "." + xStart);
            //Debug.Log(yMin + "." + yMax +"." +yStart);
            //画垂线
            for (int i = xStart; i <= xMax; i += 100)
            {
                Vector3 start = new Vector3(i, m_ScrollPos.y, 0);
                Vector3 end = new Vector3(i, m_ScrollPos.y + position.height, 0);
                DrawLine(start, end);
            }
            //画平线
            for (int i = yStart; i <= yMax; i += 100)
            {
                Vector3 start = new Vector3(m_ScrollPos.x, i, 0);
                Vector3 end = new Vector3(m_ScrollPos.x + position.width, i, 0);
                DrawLine(start, end);
            }
        }
        /// <summary>
        /// 绘制固定面板
        /// </summary>
        private void DrawFixedPanel()
        {
            /*************按钮*************/
            if (GUI.Button(new Rect(position.width - 95, 5, 70, 20), "编辑配置"))
            {
                Selection.activeObject = ConfigInfo;
            }
            if (GUI.Button(new Rect(position.width - 95, 30, 70, 20), "重置配置"))
            {
                ConfigInfo.Reset();
                Refresh();
            }
            m_View = new Rect(0, 0, ConfigInfo.ViewWidth, ConfigInfo.ViewHeight);
            /*************故事树列表*************/
            Rect box;
            if (m_StoryTrees.Count == 0)
            {
                box = new Rect(0, 0, 100, 25 * m_StoryTrees.Count + 50);
                EditorGUI.DrawRect(box, ConfigInfo.NormalNode);
                EditorGUI.LabelField(new Rect(box.x + 5, box.y + 25, box.width - 10, 16), "空");
                if (GUI.Button(new Rect(box.x + 35, box.y + 25, box.width - 45, 16), "刷新"))
                {
                    Refresh();
                }
            }
            else
            {
                box = new Rect(0, 0, 100, 25 * m_StoryTrees.Count + 25);
                EditorGUI.DrawRect(box, ConfigInfo.NormalNode);
                for (int i = 0; i < m_StoryTrees.Count; i++)
                {
                    if (GUI.Button(new Rect(box.x + 5, box.y + 25 * i + 25, box.width - 10, 20), m_StoryTrees[i].name))
                    {
                        OpenStoryTree(m_StoryTrees[i]);
                    }
                }
            }
            EditorGUI.LabelField(new Rect(box.x + 5, box.y + 5, box.width - 10, 16), "故事树列表");

            /*************当前故事树*************/
            EditorGUI.DrawRect(new Rect(0, position.height - 20, position.width, 20), ConfigInfo.NormalNode);
            string mousePos = "X: " + m_MousePos.x + "  Y: " + m_MousePos.y;
            EditorGUI.LabelField(new Rect(5, position.height - 20, 110, 20), mousePos);
            string currenStoryTreePath = "未指定（故事树文件夹：" + ConfigInfo.StoryTreeFolder + "）";
            if (m_CurrentStoryTree != null)
            {
                currenStoryTreePath = Application.dataPath + ConfigInfo.StoryTreeFolder + "/" + m_CurrentStoryTree.name + ".asset";
            }
            EditorGUI.LabelField(new Rect(115, position.height - 20, position.width, 20), "当前故事树：" + currenStoryTreePath);

            /*************缩放按钮*************/
            //if (GUI.Button(new Rect(position.width - 165, position.height - 20, 50, 20), "-20%"))
            //{
            //    m_Scale -= 0.2f;
            //}
            //if (GUI.Button(new Rect(position.width - 110, position.height - 20, 50, 20), "+20%"))
            //{
            //    m_Scale += 0.2f;
            //}
            //if (GUI.Button(new Rect(position.width - 55, position.height - 20, 50, 20), "=100%"))
            //{
            //    m_Scale = 1;
            //}
        }
        /// <summary>
        /// 绘制右键菜单
        /// </summary>
        private void DrawContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("创建故事树"), false, OnClickRightMouse, 1);
            if (m_CurrentStoryTree != null)
            {
                menu.AddItem(new GUIContent("关闭当前故事树"), false, OnClickRightMouse, 8);
                menu.AddItem(new GUIContent("删除当前故事树"), false, OnClickRightMouse, 4);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("创建节点"), false, OnClickRightMouse, 2);
            }
            if (m_CurrentStoryNode != null)
            {
                //menu.AddItem(new GUIContent("编辑节点"), false, OnClickRightMouse, 6);
                menu.AddItem(new GUIContent("删除节点"), false, OnClickRightMouse, 7);
                menu.AddItem(new GUIContent("设为起始节点"), false, OnClickRightMouse, 9);
                menu.AddItem(new GUIContent("设为中间节点"), false, OnClickRightMouse, 10);
                menu.AddItem(new GUIContent("设为结局节点"), false, OnClickRightMouse, 11);
                menu.AddItem(new GUIContent("清除节点连接"), false, OnClickRightMouse, 5);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("创建节点内容"), false, OnClickRightMouse, 3);
            }
            menu.ShowAsContext();
        }
        /// <summary>
        /// 绘制节点曲线
        /// </summary>
        private void DrawNodeCurve()
        {
            if (m_CurrentStoryTree != null)
            {
                if (m_CurrentStoryTree.Nodes != null)
                {
                    foreach (StoryNode node in m_CurrentStoryTree.Nodes)
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
                                if (choice.NextStoryNode != null & m_CurrentStoryTree.Nodes.Contains(choice.NextStoryNode))
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
        private void DrawTempCurve()
        {
            if (m_UpStoryNode != null)
            {
                DrawCurve(m_UpStoryNode.Rect,
                    new Rect(m_MousePos.x, m_MousePos.y + 20, 0, 0), true);
            }
            if (m_DownStoryNode != null)
            {
                DrawCurve(new Rect(m_MousePos.x, m_MousePos.y - 20, 0, 0),
                   m_DownStoryNode.Rect, true);
            }
        }
        /// <summary>
        /// 绘制曲线
        /// </summary>
        /// <param name="start">起始节点窗口</param>
        /// <param name="end">结束节点窗口</param>
        /// <param name="isMainLine">是否绘制成主线高亮颜色</param>
        private void DrawCurve(Rect start, Rect end, bool isMainLine)
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
                Handles.DrawBezier(startPos, endPos, startTan, endTan, ConfigInfo.MainLine, null, 4);
            }
            else
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, ConfigInfo.BranchLine, null, 3);
            }
        }
        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void DrawLine(Vector3 start, Vector3 end)
        {
            Handles.DrawBezier(start, end, start, end, ConfigInfo.BGLine, null, 2);
        }

        /// <summary>
        /// 创建故事树
        /// </summary>
        private void CreateStoryTree()
        {
            StoryTree storyTree = AssetCreator<StoryTree>.CreateAsset(ConfigInfo.StoryTreeFolder, "Story");
            if (storyTree != null)
            {
                OpenStoryTree(storyTree);
                AssetDatabase.Refresh();
            }
        }
        /// <summary>
        /// 打开故事树
        /// </summary>
        /// <param name="tree"></param>
        private void OpenStoryTree(StoryTree tree)
        {
            if (tree != null)
            {
                m_CurrentStoryTree = tree;
                ClearStoryTreeNullNodes(m_CurrentStoryTree);
                Selection.activeObject = m_CurrentStoryTree;
                ShowNotification(new GUIContent("故事树打开成功：" + m_CurrentStoryTree.name));
            }
            else
            {
                Debug.LogError("故事树打开失败：对象为空");
            }
        }
        /// <summary>
        /// 关闭故事树
        /// </summary>
        private void CloseStoryTree()
        {
            m_CurrentStoryTree = null;
        }
        /// <summary>
        /// 删除故事树
        /// </summary>
        private void DeleteStoryTree(StoryTree tree)
        {
            if (tree == null)
            {
                ShowNotification(new GUIContent("未指定要删除的故事树"));
            }
            else
            {
                string str = "确认要删除故事树 " + tree.name + " 以及其所有节点吗？这将无法恢复。";
                if (EditorUtility.DisplayDialog("警告", str, "确认", "取消"))
                {
                    if (tree.Nodes != null)
                    {
                        for (int i = 0; i < tree.Nodes.Count; i++)
                        {
                            if (tree.Nodes[i] != null)
                            {
                                DeleteStoryNode(tree.Nodes[i], false);
                                i--;
                            }
                        }
                        Debug.Log("已删除所有故事树节点资源");
                    }
                    Debug.Log("已删除故事树资源：" + tree.name + ".asset");
                    m_StoryTrees.Remove(tree);
                    File.Delete(Application.dataPath + ConfigInfo.StoryTreeFolder + "/" + tree.name + ".asset");
                    AssetDatabase.Refresh();
                }
            }
        }
        /// <summary>
        /// 绘制故事树
        /// </summary>
        /// <param name="storyTree"></param>
        private void DrawStoryTree(StoryTree tree)
        {
            if (tree != null)
            {
                if (tree.Nodes != null)
                {
                    for (int i = 0; i < tree.Nodes.Count; i++)
                    {
                        int j = tree.Nodes.Count - 1 - i;
                        if (tree.Nodes[j] != null)
                        {
                            DrawStoryNode(tree.Nodes[j]);
                        }
                    }
                }
            }
            else
            {
                ShowNotification(new GUIContent("创建或打开一个故事树"));
            }
        }

        /// <summary>
        /// 选择故事节点
        /// </summary>
        /// <param name="node"></param>
        private void SelectStoryNode(StoryNode node)
        {
            if (node != null)
            {
                Selection.activeObject = node;
            }
        }
        /// <summary>
        /// 创建故事节点
        /// </summary>
        private void CreateStoryNode(StoryTree tree)
        {
            StoryNode storyNode = AssetCreator<StoryNode>.CreateAsset(ConfigInfo.StoryNodeFolder, tree.name);
            if (storyNode != null)
            {
                storyNode.Rect = new Rect(m_MousePos.x, m_MousePos.y, ConfigInfo.DefaultNodeSize.x, ConfigInfo.DefaultNodeSize.y);
                if (tree.Nodes == null)
                {
                    tree.Nodes = new List<StoryNode>();
                }
                tree.Nodes.Add(storyNode);
                Selection.activeObject = storyNode;
            }
        }
        /// <summary>
        /// 删除故事节点
        /// </summary>
        private void DeleteStoryNode(StoryNode node, bool isShow)
        {
            if (node != null)
            {
                if (isShow)
                {
                    string str = "确认要删除节点 " + node.name + " 吗？";
                    if (EditorUtility.DisplayDialog("警告", str, "确认", "取消"))
                    {
                        Debug.Log("已删除节点资源：" + node.name + ".asset");
                        ClearNodeUpChoices(node);
                        File.Delete(ConfigInfo.StoryNodeFolder + "/" + node.name + ".asset");
                        AssetDatabase.Refresh();
                    }
                }
                else
                {
                    Debug.Log("已删除节点资源：" + node.name + ".asset");
                    ClearNodeUpChoices(node);
                    File.Delete( ConfigInfo.StoryNodeFolder + "/" + node.name + ".asset");
                    AssetDatabase.Refresh();
                }
            }
        }
        /// <summary>
        /// 绘制故事节点
        /// </summary>
        /// <param name="storyNodes"></param>
        /// <returns></returns>
        private void DrawStoryNode(StoryNode node)
        {
            if (node != null)
            {
                //节点背景
                if (m_CurrentStoryNode != null)
                {
                    if (m_CurrentStoryNode == node)
                    {
                        EditorGUI.DrawRect(node.Rect, ConfigInfo.SelectNode);
                    }
                    else
                    {
                        EditorGUI.DrawRect(node.Rect, ConfigInfo.NormalNode);
                    }
                }
                else
                {
                    EditorGUI.DrawRect(node.Rect, ConfigInfo.NormalNode);
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
        private void DrawUpButton(StoryNode node)
        {
            if (GUI.Button(new Rect(node.Rect.x + node.Rect.width / 2 - 10, node.Rect.y - 20, 20, 20), "↑"))
            {
                m_DownStoryNode = node;
                CheckNodeConnect();
            }
        }
        /// <summary>
        /// 绘制节点下按钮
        /// </summary>
        /// <param name="node"></param>
        private void DrawDownButton(StoryNode node)
        {
            if (GUI.Button(new Rect(node.Rect.x + node.Rect.width / 2 - 10, node.Rect.y + node.Rect.height, 20, 20), "↓"))
            {
                m_UpStoryNode = node;
                CheckNodeConnect();
            }
        }

        /// <summary>
        /// 创建节点资源
        /// </summary>
        /// <param name="type"></param>
        private void CreateStoryNodeContent(StoryNode node)
        {
            StoryContent Content = AssetCreator<StoryContent>.CreateAsset(ConfigInfo.StoryContentFolder, node.name);
            if (Content != null)
            {
                node.Content = Content;
                Selection.activeObject = Content;
            }
            RefreshStoryNodeHeight(node);
        }
        /// <summary>
        /// 刷新节点高度
        /// </summary>
        /// <param name="node"></param>
        private int RefreshStoryNodeHeight(StoryNode node)
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
        /// <summary>
        /// 设置节点类型
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeType"></param>
        private void SetNodeType(StoryNode node, StoryNodeType nodeType)
        {
            if (node != null)
            {
                switch (nodeType)
                {
                    case StoryNodeType.中间节点:
                        if (m_CurrentStoryTree.StartNode == node)
                        {
                            m_CurrentStoryTree.StartNode = null;
                        }
                        if (m_CurrentStoryTree.EndNodes.Contains(node))
                        {
                            m_CurrentStoryTree.EndNodes.Remove(node);
                        }
                        break;
                    case StoryNodeType.起始节点:
                        if (m_CurrentStoryTree.EndNodes.Contains(node))
                        {
                            m_CurrentStoryTree.EndNodes.Remove(node);
                        }
                        if (m_CurrentStoryTree.StartNode == null)
                        {
                            m_CurrentStoryTree.StartNode = node;
                            ClearNodeUpChoices(node);
                        }
                        else if (m_CurrentStoryTree.StartNode != node)
                        {
                            SetNodeType(m_CurrentStoryTree.StartNode, StoryNodeType.中间节点);
                            m_CurrentStoryTree.StartNode = node;
                            ClearNodeUpChoices(node);
                        }
                        break;
                    case StoryNodeType.结局节点:
                        if (m_CurrentStoryTree.StartNode == node)
                        {
                            m_CurrentStoryTree.StartNode = null;
                        }
                        m_CurrentStoryTree.EndNodes.Add(node);
                        ClearNodeDownChoices(node);
                        break;
                    default:
                        break;
                }
                node.NodeType = nodeType;
            }
        }
        /// <summary>
        /// 检查节点连接
        /// </summary>
        private void CheckNodeConnect()
        {
            if (m_UpStoryNode != null && m_DownStoryNode != null)
            {
                if (m_UpStoryNode != m_DownStoryNode)
                {
                    StoryChoice sc = new StoryChoice("", m_DownStoryNode);
                    if (m_UpStoryNode.Choices == null)
                    {
                        m_UpStoryNode.Choices = new List<StoryChoice>();
                    }
                    bool isHave = false;
                    foreach (StoryChoice item in m_UpStoryNode.Choices)
                    {
                        if (item.NextStoryNode == m_DownStoryNode)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (!isHave)
                    {
                        m_UpStoryNode.Choices.Add(sc);
                    }
                    else
                    {
                        ShowNotification(new GUIContent("已存在连接"));
                    }
                }
                else
                {
                    ShowNotification(new GUIContent("不可连接自身节点"));
                }
                ClearTempConnect();
            }
        }
        /// <summary>
        /// 清除节点临时连接
        /// </summary>
        private void ClearTempConnect()
        {
            m_UpStoryNode = null;
            m_DownStoryNode = null;
        }
        /// <summary>
        /// 清除节点下行连接
        /// </summary>
        /// <param name="node"></param>
        private void ClearNodeDownChoices(StoryNode node)
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
        private void ClearNodeUpChoices(StoryNode node)
        {
            if (node.Choices != null)
            {
                foreach (StoryNode item in m_CurrentStoryTree.Nodes)
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
        /// <summary>
        /// 清除故事树节点列表的空节点
        /// </summary>
        private void ClearStoryTreeNullNodes(StoryTree tree)
        {
            if (tree.Nodes != null)
            {
                //移除空位
                for (int i = 0; i < tree.Nodes.Count; i++)
                {
                    if (tree.Nodes[i] == null)
                    {
                        tree.Nodes.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (tree.EndNodes != null)
            {
                //移除空位
                for (int i = 0; i < tree.EndNodes.Count; i++)
                {
                    if (tree.EndNodes[i] == null)
                    {
                        tree.EndNodes.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        /// <summary>
        /// 清除节点的空分支
        /// </summary>
        /// <param name="choices"></param>
        private void ClearStoryNodeNullChoices(List<StoryChoice> choices)
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
    }
}