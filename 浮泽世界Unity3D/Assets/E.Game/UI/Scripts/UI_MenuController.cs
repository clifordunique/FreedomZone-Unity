// ========================================================
// 作者：E Star
// 创建时间：2019-01-19 17:54:43
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace E.Game
{
    public class UI_MenuController : UI_Controller
    {
        /// <summary>
        /// 返回游戏
        /// </summary>
        public void Back()
        {
            gameObject.SetActive(false);
            Cursor.visible = false;
        }
        /// <summary>
        /// 退出游戏
        /// </summary>
        public void Exit()
        {
            Application.Quit();
            //SceneManager.LoadScene("Menu");
        }
    }
}