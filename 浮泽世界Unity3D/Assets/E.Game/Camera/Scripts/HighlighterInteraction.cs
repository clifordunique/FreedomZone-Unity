// ========================================================
// 作者：E Star 
// 创建时间：2018-12-21 13:33:17 
// 当前版本：1.0 
// 作用描述：添加高亮效果
// 挂载目标：Prop
// ========================================================
using System;
using UnityEngine;
using HighlightingSystem;

public class HighlighterInteraction : MonoBehaviour
{
    /// <summary>
    /// 光标覆盖颜色
    /// </summary>
    public Color m_HoverColor = Color.yellow;

    /// <summary>
    /// 光标覆盖事件
    /// </summary>
    /// <param name="hitInfo">覆盖目标</param>
    public void OnHover(RaycastHit hitInfo)
    {
        Transform tr = hitInfo.collider.transform;
        if (tr == null) { return; }

        var highlighter = tr.GetComponentInParent<Highlighter>();
        if (highlighter == null) { return; }

        // 光标覆盖
        highlighter.Hover(m_HoverColor);
    }
}
