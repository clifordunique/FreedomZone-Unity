// ========================================================
// 作者：E Star
// 创建时间：2019-01-19 18:05:30
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
    public class UI_AccountController : UI_Controller
    {
        /// <summary>
        /// 登入
        /// </summary>
        public void SignIn()
        {
            string tmp_1 = transform.Find("Inp_Name").GetComponent<InputField>().text;
            if (tmp_1 != string.Empty)
            {
                gameObject.SetActive(false);
                Cursor.visible = false;
                //SceneManager.LoadScene("SJTU");
            }
            else
            {
                //m_MessageManager.ShowMessage("名称不能为空");
            }
        }
    }
}