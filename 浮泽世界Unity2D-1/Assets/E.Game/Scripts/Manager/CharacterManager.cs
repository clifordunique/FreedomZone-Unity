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
using E.Tool;
using E.Utility;

namespace E.Game
{
    public class CharacterManager : Manager<CharacterManager>
    {
        [Header("角色")]
        [SerializeField, ReadOnly] private GameObject m_Player;
        [SerializeField] private GameObject[] m_NPCs;
        [SerializeField] private GameObject zombie;

        [Header("计时器")]
        [SerializeField] private float m_ZombieCreatetime = 5;
        private float Timer;

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
            Timer = m_ZombieCreatetime;
        }
        private void Update()
        {
            if (m_ZombieCreatetime > 0)
            {
                m_ZombieCreatetime -= Time.deltaTime;
            }
            else
            {
                m_ZombieCreatetime = Timer;
                Instantiate(zombie, new Vector3(Random.Range(-30, 20), Random.Range(-20, 20), 0), Quaternion.identity);

            }
        }

        private void CheckPlayer()
        {
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