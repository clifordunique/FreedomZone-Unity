// ========================================================
// 作者：E Star 
// 创建时间：2018-12-21 13:33:17 
// 当前版本：1.0 
// 作用描述：游戏管理员
// 挂载目标：？
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace E.Game
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager m_Instance;
        public static GameManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    GameObject go = (GameObject)Instantiate(Resources.Load("GameManagers"));
                    m_Instance = go.GetComponent<GameManager>();
                }
                return m_Instance;
            }
        }

        private void Awake()
        {
            if (m_Instance == null)
            {
                m_Instance = this;
            }
            else if (m_Instance != null & m_Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            Debug.Log("运行平台：" + Application.platform);
        }

        public static bool IsBuild
        {
            get
            {
                return m_Instance != null;
            }
        }
    }
}