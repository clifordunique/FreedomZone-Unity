using System;
using UnityEngine;
using UnityEngine.UI;

namespace E.Game
{
    public class UI_PCInfoController : UI_Controller
    {
        [SerializeField, Tooltip("FPS计数器显示控件")] private Text m_FPSText;
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private int m_CurrentFps;
        const string display = "{0} FPS";


        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }

        private void Update()
        {
            FPSCounter();
        }
        /// <summary>
        /// FPS计数器
        /// </summary>
        private void FPSCounter()
        {
            // measure average frames per second
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
                m_FPSText.text = string.Format(display, m_CurrentFps);
            }
        }
    }
}
