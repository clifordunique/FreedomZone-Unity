// ========================================================
// 作者：E Star
// 创建时间：2019-03-09 23:13:08
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using E.Utility;

namespace E.Tool
{
    [Serializable]
    public class StoryEditorConfig : ScriptableObjectDictionary<StoryEditorConfig>
    {
        public const string WindowTitle = "E Story";

        [Header("【资源文件夹】")]
        [Tooltip("在故事编辑窗口内点击右键创建的资源将会在此目录下创建对应文件")]
        public string StoryResourcesFolder = "Assets/E.Tool/E Story/Resources/Example Storys";

        [Header("【个性化界面】")]
        public int ViewWidth = 3000;
        public int ViewHeight = 3000;
        public Vector2Int DefaultNodeSize = new Vector2Int(250, 90);
        public Color NormalNode = new Color(0.9f, 0.9f, 0.9f);
        public Color SelectNode = new Color(0.9f, 0.85f, 0.5f);
        public Color MainLine = new Color(0, 0.7f, 0);
        public Color BranchLine = new Color(0.7f, 0, 0);
        public Color BGLine = new Color(0, 0, 0, 0.1f);

        public void Reset()
        {
            StoryResourcesFolder = "Assets/E.Tool/E Story/Resources/Example Storys";

            ViewWidth = 3000;
            ViewHeight = 3000;
            DefaultNodeSize = new Vector2Int(250, 90);
            NormalNode = new Color(0.9f, 0.9f, 0.9f);
            SelectNode = new Color(0.9f, 0.85f, 0.5f);
            MainLine = new Color(0, 0.7f, 0);
            BranchLine = new Color(0.7f, 0, 0);
            BGLine = new Color(0, 0, 0, 0.1f);
        }
    }
}
