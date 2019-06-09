// ========================================================
// 作者：E Star
// 创建时间：2019-02-16 18:54:53
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Game
{
    public class Manager<T> : MonoBehaviour
    where T : MonoBehaviour
    {
        private static T m_Singleton;
        public static T Singleton
        {
            get
            {
                return m_Singleton;
            }
        }

        public virtual void Awake()
        {
            if (m_Singleton == null)
            {
                m_Singleton = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static bool IsExist
        {
            get
            {
                return m_Singleton != null;
            }
        }
    }
}