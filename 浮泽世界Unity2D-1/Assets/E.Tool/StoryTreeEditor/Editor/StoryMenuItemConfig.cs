// ========================================================
// 作者：E Star
// 创建时间：2019-02-23 01:21:51
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using E.Utility;

namespace E.Tool
{
    public class StoryMenuItemConfig : MonoBehaviour
    {
        [MenuItem("Tools/故事树编辑器/创建故事树", false, 99)]
        public static void CreateStoryTreeAsset()
        {
            AssetCreator<StoryTree>.CreateAsset();
        }
        [MenuItem("Tools/故事树编辑器/创建故事节点", false, 100)]
        public static void CreateStoryNodeAsset()
        {
            AssetCreator<StoryNode>.CreateAsset();
        }
        [MenuItem("Tools/故事树编辑器/创建故事节点内容", false, 101)]
        public static void CreateStoryContentAsset()
        {
            AssetCreator<StoryContent>.CreateAsset();
        }
    }
}