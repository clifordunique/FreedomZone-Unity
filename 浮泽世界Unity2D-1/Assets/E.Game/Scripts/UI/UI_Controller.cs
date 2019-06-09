// ========================================================
// 作者：E Star
// 创建时间：2019-02-20 01:31:58
// 当前版本：1.0
// 作用描述：UI控制器基类脚本
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Game
{
    public class UI_Controller : MonoBehaviour
    {
        protected Animator m_Animator;

        protected void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }
        private void Start()
        {

        }
        private void Update()
        {

        }
    }
}