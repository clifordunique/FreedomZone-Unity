// ========================================================
// 作者：E Star
// 创建时间：2019-02-16 15:29:09
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Game
{
    public class SpawnManager : Manager<SpawnManager>
    {
        public GameObject m_PlayerPrefab;
        public Transform m_PlayerSpawnPoint;

        private void Update()
        {
        }

        public GameObject SpawnPlayer()
        {
            GameObject go = Instantiate(m_PlayerPrefab);
            go.transform.position = m_PlayerSpawnPoint.position;
            go.transform.rotation = m_PlayerSpawnPoint.rotation;
            return go;
        }
        public GameObject SpawnPlayer(Vector3 worldPosition, Vector3 worldRotation)
        {
            GameObject go = Instantiate(m_PlayerPrefab);
            go.transform.position = worldPosition;
            go.transform.eulerAngles = worldRotation;
            return go;
        }
    }
}