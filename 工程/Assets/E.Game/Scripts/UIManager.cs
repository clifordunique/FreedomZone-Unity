// ========================================================
// 作者：E Star 
// 创建时间：2018-12-21 13:33:17 
// 当前版本：1.0 
// 作用描述：UI管理员
// 挂载目标：UICanvas
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using E.Utility;

namespace E.Game
{
    public class UIManager : SingletonPattern<UIManager>
    {
        [Header("【组件】")]
        public CanvasGroup cvgStatic;
        public CanvasGroup cvgWindows;
        public CanvasGroup cvgPopups;
        public CanvasGroup cvgDebug;
        public CanvasGroup cvgReady;

        [Header("【运行时变量】")]
        public UIDisplayMode uIDisplayMode = UIDisplayMode.Default;
        public EntityInfoDisplayMode entityInfoDisplayMode = EntityInfoDisplayMode.HoverShowAndHitShow;


        private void Update()
        {
            switch (uIDisplayMode)
            {
                case UIDisplayMode.Default:
                    SetCursor(true);
                    cvgStatic.alpha = 1;
                    cvgWindows.alpha = 1;
                    cvgPopups.alpha = 1;
                    cvgDebug.alpha = 0;
                    cvgReady.alpha = 1;
                    break;
                case UIDisplayMode.Debug:
                    SetCursor(true);
                    cvgStatic.alpha = 1;
                    cvgWindows.alpha = 1;
                    cvgPopups.alpha = 1;
                    cvgDebug.alpha = 1;
                    cvgReady.alpha = 1;
                    break;
                case UIDisplayMode.Hide:
                    SetCursor(false);
                    cvgStatic.alpha = 0;
                    cvgWindows.alpha = 0;
                    cvgPopups.alpha = 0;
                    cvgDebug.alpha = 0;
                    cvgReady.alpha = 0;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置光标状态
        /// </summary>
        /// <param name="isShow">是否展示</param>
        private void SetCursor(bool isShow)
        {
            Cursor.visible = isShow;
            if (isShow)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public enum UIDisplayMode
        {
            Default = 0,
            Debug = 1,
            Hide = 2,
        }
        public enum EntityInfoDisplayMode
        {
            AlwaysShow = 0,
            HoverShowOnly = 1,
            HitShowOnly = 2,
            HoverShowAndHitShow = 3,
            AlwaysHide = 4
        }
    }
}