// ========================================================
// 作者：E Star
// 创建时间：2019-01-27 19:47:38
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Game
{
    public class AppManager : MonoBehaviour
    {
        [Header("手机管理员")]
        [SerializeField] protected UI_PhoneController m_PhoneManager;
        [Header("应用主页")]
        [SerializeField] protected GameObject m_MainPage;

        protected Stack m_PageHistory;

        private void Awake()
        {
            m_PageHistory = new Stack();
        }
        private void OnEnable()
        {
            OpenNewPage(m_MainPage);
        }
        private void Start()
        {
        }

        /// <summary>
        /// 打开新的一页
        /// </summary>
        /// <param name="page"></param>
        public virtual void OpenNewPage(GameObject page)
        {
            if (page == null)
            {
                Debug.Log("此页不存在");
                return;
            }
            page.SetActive(true);
            page.transform.SetAsLastSibling();
            m_PageHistory.Push(page);
        }
        /// <summary>
        /// 回到上一页
        /// </summary>
        public virtual void BackLastPage()
        {
            if (m_PageHistory.Count > 1)
            {
                GameObject currentPage = m_PageHistory.Peek() as GameObject;
                currentPage.SetActive(true);
                m_PageHistory.Pop();
                GameObject lastPage = m_PageHistory.Peek() as GameObject;
                lastPage.transform.SetAsLastSibling();
            }
            else
            {
                m_PageHistory.Clear();
                m_PhoneManager.CloseApp(gameObject);
            }
        }
    }
}