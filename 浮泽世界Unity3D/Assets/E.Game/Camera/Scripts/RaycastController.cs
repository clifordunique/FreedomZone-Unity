// ========================================================
// ���ߣ�E Star 
// ����ʱ�䣺2018-12-21 13:33:17 
// ��ǰ�汾��1.0 
// �������������߿�����
// ����Ŀ�꣺Camera
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
    /// �㼶�ɰ棬-1������
    /// </summary>
    public LayerMask m_LayerMask = -1;
    /// <summary>
    /// ���߳��ȣ�-1������
    /// </summary>
    public float m_RayLength = 2f;
    /// <summary>
    /// ��긲���¼�
    /// </summary>
    public RaycastEvent OnHover;
    /// <summary>
    /// ������ײ��Ŀ��
    /// </summary>
    public GameObject m_HitTarget;
    
    /// <summary>
    /// ������
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
            // ��������
            RaycastHit m_HitInfo;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            // �����ײ
            if (Physics.Raycast(ray, out m_HitInfo, m_RayLength >= 0f ? m_RayLength : Mathf.Infinity, m_LayerMask.value))
            {
                m_HitTarget = m_HitInfo.collider.gameObject;
                OnHover.Invoke(m_HitInfo);
                //�������ߣ�ֻ����scene��ͼ�в��ܿ���
                Debug.DrawLine(ray.origin, m_HitInfo.point);
            }
            else
            {
                m_HitTarget = null;
                //�������ߣ�ֻ����scene��ͼ�в��ܿ���
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * m_RayLength);
            }
        }
    }
}
