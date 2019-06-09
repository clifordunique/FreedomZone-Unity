// ========================================================
// 作者：E Star
// 创建时间：2019-02-20 00:14:41
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
    public class CharacterManager : Manager<CharacterManager>
    {
        [Header("玩家对象")]
        private GameObject m_Player;
        public GameObject Player
        {
            get
            {
                CheckPlayer();
                return m_Player;
            }
            set
            {
                m_Player = value;
            }
        }
        [Header("NPC对象")]
        private GameObject[] m_NPCs;
        public GameObject[] NPCs
        {
            get
            {
                return m_NPCs;
            }
            set
            {
                m_NPCs = value;
            }
        }

        private void Start()
        {
        }
        private void Update()
        {
        }

        private void CheckPlayer()
        {
            if (SceneManager.CurrentSceneName != "")
            {

            }
            if (SceneManager.CurrentSceneID != 0 && SceneManager.CurrentSceneID != 1)
            {
                if (m_Player == null)
                {
                    if (GameObject.FindWithTag("Player") == null)
                    {
                        m_Player = SpawnManager.Singleton.SpawnPlayer();
                    }
                    else
                    {
                        m_Player = GameObject.FindWithTag("Player");
                    }
                }
            }
        }
    }
}