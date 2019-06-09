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
    public class ConfigInfo : ScriptableObject
    {
        public const string WindowTitle = "故事树编辑器";

        [Header("资源文件夹")]
        public string StoryTreeFolder = "Assets/E.Tool/StoryTreeEditor/Storys/StoryTrees";
        public string StoryNodeFolder = "Assets/E.Tool/StoryTreeEditor/Storys/StoryNodes";
        public string StoryContentFolder = "Assets/E.Tool/StoryTreeEditor/Storys/StoryContents";

        [Header("个性化界面")]
        public int ViewWidth = 5000;
        public int ViewHeight = 5000;
        public Vector2 DefaultNodeSize = new Vector2(250, 50);
        public Color NormalNode = new Color(0.9f, 0.9f, 0.9f);
        public Color SelectNode = new Color(0.9f, 0.85f, 0.5f);
        public Color MainLine = new Color(0, 0.7f, 0);
        public Color BranchLine = new Color(0.7f, 0, 0);
        public Color BGLine = new Color(0, 0, 0, 0.1f);
        //public Color Shadow = new Color(0, 0, 0, 0.1f);


        public void Reset()
        {
            StoryTreeFolder = "Assets/E.Tool/StoryTreeEditor/Storys/StoryTrees";
            StoryNodeFolder = "Assets/E.Tool/StoryTreeEditor/Storys/StoryNodes";
            StoryContentFolder = "Assets/E.Tool/StoryTreeEditor/Storys/StoryContents";

            ViewWidth = 3000;
            ViewHeight = 3000;
            NormalNode = new Color(0.9f, 0.9f, 0.9f);
            SelectNode = new Color(0.9f, 0.85f, 0.5f);
            MainLine = new Color(0, 0.7f, 0);
            BranchLine = new Color(0.7f, 0, 0);
            BGLine = new Color(0, 0, 0, 0.1f);
            //Shadow = new Color(0, 0, 0, 0.1f);
            DefaultNodeSize = new Vector2(250, 50);
        }
    }
}
