// ========================================================
// 作者：E Star
// 创建时间：2019-04-26 00:20:44
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E.Utility;
using E.Tool;

namespace E.Game
{
    public class PickUpTrigger : MonoBehaviour
    {
        [SerializeField] private CharacterController m_CharacterController;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 9)
            {

            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 9)
            {

            }
        }
    }
}
