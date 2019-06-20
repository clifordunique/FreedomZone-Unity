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

namespace E.Utility
{
    public class SingletonPattern<T> : MonoBehaviour
    where T : MonoBehaviour
    {
        private static T singleton;
        public static T Singleton
        {
            get
            {
                return singleton;
            }
        }

        protected virtual void Awake()
        {
            if (singleton == null)
            {
                singleton = this as T;
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
                return singleton != null;
            }
        }
    }
}