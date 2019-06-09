// ========================================================
// 作者：E Star
// 创建时间：2019-04-24 00:20:57
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
    public class UI_BagController : MonoBehaviour
    {
        [SerializeField] private GameObject m_PrfSlot;
        [SerializeField] private Transform m_PanSlotList;


        public void Refresh()
        {
            Clear();
            List<InteractorData> interactorDatas = CharacterManager.Singleton.Player.GetComponent<CharacterData>().CarryingProps;
            if (interactorDatas != null)
            {
                for (int i = 0; i < interactorDatas.Count; i++)
                {
                    GameObject item = Instantiate(m_PrfSlot, m_PanSlotList);
                    item.GetComponent<UI_SlotController>().InteractorDataInstance = interactorDatas[i];
                }
            }
        }
        private void Clear()
        {
            UI_SlotController[] interactorDatas = GetComponentsInChildren<UI_SlotController>();
            foreach (UI_SlotController item in interactorDatas)
            {
                Destroy(item.gameObject);
            }
        }
    }
}
