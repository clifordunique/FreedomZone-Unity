// ========================================================
// 作者：E Star
// 创建时间：2019-02-16 18:22:12
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Game
{
    [DisallowMultipleComponent]
    public class CharacterController : MonoBehaviour
    {
        protected Rigidbody2D m_Rigidbody2D;
        protected Animator m_Animator;
        protected CharacterData m_CharacterData;

        protected virtual void Awake()
        {
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            m_Animator = GetComponent<Animator>();
            m_CharacterData = GetComponent<CharacterData>();
        }

        public void GetInteractor(GameObject go)
        {
            if (m_CharacterData.CarryingProps == null)
            {
                m_CharacterData.CarryingProps = new List<InteractorData>();
            }
            m_CharacterData.CarryingProps.Add(go.GetComponent<InteractorController>().InteractorDataInstance);
            UIManager.Singleton.m_UIBagController.Refresh();
        }
    }
}