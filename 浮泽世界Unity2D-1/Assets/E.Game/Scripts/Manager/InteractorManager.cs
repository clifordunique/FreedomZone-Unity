// ========================================================
// 作者：E Star
// 创建时间：2019-02-20 00:14:16
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E.Utility;
using E.Tool;
using System.IO;
using UnityEditor;

namespace E.Game
{
    public class InteractorManager : Manager<InteractorManager>
    {
        [SerializeField] private List<InteractorData> m_InteractorDatas = new List<InteractorData>();

        public List<InteractorData> InteractorDatas { get => m_InteractorDatas; }
    }
}