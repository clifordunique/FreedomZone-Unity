// ========================================================
// 作者：E Star 
// 创建时间：2018-12-21 13:33:17 
// 当前版本：1.0 
// 作用描述：射线控制器
// 挂载目标：Camera
// ========================================================
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class RaycastEvent : UnityEvent<RaycastHit> { }

[RequireComponent(typeof(Camera))]
public class RaycastController : MonoBehaviour
{
    /// <summary>
    /// 层级蒙版，-1是所有
    /// </summary>
    public LayerMask m_LayerMask = -1;
    /// <summary>
    /// 射线长度，-1是无限
    /// </summary>
    public float m_RayLength = 2f;
    /// <summary>
    /// 鼠标覆盖事件
    /// </summary>
    public RaycastEvent OnHover;
    /// <summary>
    /// 射线碰撞的目标
    /// </summary>
    public GameObject m_HitTarget;
    
    /// <summary>
    /// 相机组件
    /// </summary>
    private Camera m_Camera;
    
    void Awake()
    {
        m_Camera = GetComponent<Camera>();
    }
    void Update()
    {
        if (!Cursor.visible)
        {
            if (m_Camera == null) { return; }
            if (OnHover == null) { return; }
            // 创建射线
            RaycastHit m_HitInfo;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            // 检测碰撞
            if (Physics.Raycast(ray, out m_HitInfo, m_RayLength >= 0f ? m_RayLength : Mathf.Infinity, m_LayerMask.value))
            {
                m_HitTarget = m_HitInfo.collider.gameObject;
                OnHover.Invoke(m_HitInfo);
                //划出射线，只有在scene视图中才能看到
                Debug.DrawLine(ray.origin, m_HitInfo.point);
            }
            else
            {
                m_HitTarget = null;
                //划出射线，只有在scene视图中才能看到
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * m_RayLength);
            }
        }
    }
}
