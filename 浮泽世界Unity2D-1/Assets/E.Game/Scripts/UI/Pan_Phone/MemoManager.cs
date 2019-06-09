// ========================================================
// 作者：E Star
// 创建时间：2019-01-24 14:00:25
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
    public class MemoManager : AppManager
    {
        [Header("备忘录主页")]
        [SerializeField] private Transform m_MemoList;
        [Header("备忘细节页")]
        [SerializeField] private GameObject m_MemoPage;
        [SerializeField] private InputField m_MemoPageTitle;
        [SerializeField] private InputField m_MemoPageContent;
        [Header("变量")]
        [SerializeField] private GameObject m_MemoItemTemp;
        [SerializeField] private GameObject m_CurrentMemo;

        /// <summary>
        /// 新建备忘录条目
        /// </summary>
        public void AddMemoItem()
        {
            GameObject memoItem = Instantiate(m_MemoItemTemp);
            memoItem.transform.SetParent(m_MemoList);
            memoItem.GetComponent<Button>().onClick.AddListener
            (delegate ()
            {
            //载入并打开
            m_CurrentMemo = memoItem;
                LoadMemo(memoItem);
                OpenNewPage(m_MemoPage);
            });

            //载入并打开
            LoadMemo(memoItem);
            OpenNewPage(m_MemoPage);
        }
        /// <summary>
        /// 载入备忘录条目
        /// </summary>
        public void LoadMemo(GameObject memoItem)
        {
            m_CurrentMemo = memoItem;
            m_MemoPageTitle.text = m_CurrentMemo.transform.Find("Txt_Title").GetComponent<Text>().text;
            m_MemoPageContent.text = m_CurrentMemo.transform.Find("Txt_Content").GetComponent<Text>().text;
        }
        /// <summary>
        /// 储存备忘录条目
        /// </summary>
        public void SaveMemo()
        {
            m_CurrentMemo.transform.Find("Txt_Title").GetComponent<Text>().text = m_MemoPageTitle.text;
            m_CurrentMemo.transform.Find("Txt_Content").GetComponent<Text>().text = m_MemoPageContent.text;
        }

        /// <summary>
        /// 移除备忘录条目
        /// </summary>
        public void RemoveCurrentMemo()
        {
            Destroy(m_CurrentMemo);
            m_PhoneManager.ShowMessage("已移除一条备忘录");

            BackLastPage();
        }
    }
}