// ========================================================
// 作者：E Star
// 创建时间：2019-02-19 00:01:58
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Game
{
    public class GameManagerChecker : MonoBehaviour
    {
        [SerializeField] private GameObject m_DefaultCamera;

        private void Awake()
        {
            m_DefaultCamera.SetActive(false);

            Debug.Log("GameManager是否已存在：" + GameManager.IsBuild);
            Debug.Log("尝试创建GameManager:" + GameManager.Instance.name);
            Debug.Log("GameManager是否存在：" + GameManager.IsBuild);

            Debug.Log(Application.dataPath);
        }
    }
}