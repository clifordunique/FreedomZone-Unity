// ========================================================
// 作者：E Star
// 创建时间：2019-02-16 17:08:40
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Game
{
    public class CameraManager : Manager<CameraManager>
    {
        [Tooltip("当前相机")] public Camera m_CurrentCamera;
        [Tooltip("第一人称相机")] public GameObject m_FirstCamera;
        [Tooltip("第三人称相机")] public GameObject m_ThirdCamera;

        private void Update()
        {
            if (m_CurrentCamera == null)
            {
                m_CurrentCamera = Camera.main;
            }
        }

        /// <summary>
        /// 设置镜面相机
        /// </summary>
        public void SetMirrorsCamera()
        {
            GameObject[] Mirrors = GameObject.FindGameObjectsWithTag("MirrorCamera");
            foreach (GameObject item in Mirrors)
            {
                item.GetComponent<ChinarMirror>().mainCamera = m_CurrentCamera;
            }
        }
        /// <summary>
        /// 使用第一人称
        /// </summary>
        public void UseFirstPersonView()
        {
            m_FirstCamera.SetActive(true);
            m_ThirdCamera.SetActive(false);
            m_CurrentCamera = m_FirstCamera.GetComponent<Camera>();
            SetMirrorsCamera();

            UIManager.SetCursor(false);
        }
        /// <summary>
        /// 使用第三人称
        /// </summary>
        public void UseThirdPersonView()
        {
            m_FirstCamera.SetActive(false);
            m_ThirdCamera.SetActive(true);
            m_CurrentCamera = m_ThirdCamera.transform.Find("Pivot").Find("Camera").GetComponent<Camera>();
            SetMirrorsCamera();

            UIManager.SetCursor(false);
        }
        /// <summary>
        /// 切换相机
        /// </summary>
        public void SwitchCamera()
        {
            if (m_CurrentCamera == m_FirstCamera.GetComponent<Camera>())
            {
                UseThirdPersonView();
            }
            else
            {
                UseFirstPersonView();
            }
        }
    }
}