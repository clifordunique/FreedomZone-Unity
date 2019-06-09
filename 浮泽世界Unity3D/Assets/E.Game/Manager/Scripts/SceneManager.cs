// ========================================================
// 作者：E Star
// 创建时间：2019-01-27 01:44:22
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace E.Game
{
    public class SceneManager : Manager<SceneManager>
    {
        private static int m_CurrentSceneID;
        public static int CurrentSceneID
        {
            get
            {
                m_CurrentSceneID = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                return m_CurrentSceneID;
            }
        }
        private static string m_CurrentSceneName;
        public static string CurrentSceneName
        {
            get
            {
                m_CurrentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                return m_CurrentSceneName;
            }
        }

        private void Start()
        {
        }
        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.F1))
            {
                LoadScene(0);
            }
            if (Input.GetKeyUp(KeyCode.F2))
            {
                LoadScene(1);
            }
            if (Input.GetKeyUp(KeyCode.F3))
            {
                LoadScene(2);
            }
            if (Input.GetKeyUp(KeyCode.F4))
            {
                LoadScene(3);
            }
#endif
        }

        public void LoadScene(int index)
        {
            Debug.Log("载入场景：" + index);
            UnityEngine.SceneManagement.SceneManager.LoadScene(index);
        }
        public void LoadScene(string name)
        {
            Debug.Log("载入场景：" + name);
            UnityEngine.SceneManagement.SceneManager.LoadScene(name);
        }
    }
}