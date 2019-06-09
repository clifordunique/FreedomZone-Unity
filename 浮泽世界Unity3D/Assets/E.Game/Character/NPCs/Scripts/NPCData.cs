// ========================================================
// 作者：E Star
// 创建时间：2019-01-19 17:09:36
// 当前版本：1.0
// 作用描述：配置与角色的对话内容
// 挂载目标：layer:character
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E.Tool;

namespace E.Game
{
    [DisallowMultipleComponent]
    public class NPCData : CharacterData
    {
        public StoryNode m_StoryNode;
    }
}