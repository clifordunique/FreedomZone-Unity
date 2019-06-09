// ========================================================
// 作者：E Star 
// 创建时间：2018-12-21 13:33:17 
// 当前版本：1.0 
// 作用描述：玩家控制器
// 挂载目标：Player
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace E.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterData))]
    public class PlayerController : CharacterController
    {
        [Header("交互相关")]
        [SerializeField, Tooltip("交互模式")] private InteractiveMode m_InteractiveMode = InteractiveMode.普通;
        private AudioSource AS;
        int state;



        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            InitPlayerModel();
            UIManager.Singleton.m_UIMessageController.ShowMessage("欢迎光临浮泽世界！亲爱的" + m_CharacterData.PlayerName);

            GetComponent<Rigidbody2D>().mass = m_CharacterData.Weight;
            AS = GetComponent<AudioSource>();
            AS.loop = true;
        }
        private void Update()
        {
            //记录本次游戏时间
            m_CharacterData.TotalOnline += TimeSpan.FromSeconds(1f / 60);

            StateChange();
            PlayerMove();
            DinosaurVoiceControl();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Map")
            {
                //m_PlayerData.speedMax *= 0.8f;
                //m_PlayerData.speedWalk *= 0.8f;
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Map")
            {
                //m_PlayerData.speedMax /= 0.8f;
                //m_PlayerData.speedWalk /= 0.8f;
            }
        }


        /// <summary>
        /// 初始化玩家信息
        /// </summary>
        private void InitPlayerModel()
        {
            m_CharacterData.PlayerName = "E";
            m_CharacterData.PlayerID = "000001";
            m_CharacterData.FirstLogin = DateTime.Now;
            m_CharacterData.LastLogin = DateTime.Now;
            m_CharacterData.LastLogout = DateTime.Now;

            m_CharacterData.TotalOnline = TimeSpan.Zero;
            m_CharacterData.Health = 100;
            m_CharacterData.Stamina = 100;
            m_CharacterData.Mentality = 100;
            m_CharacterData.Satiety = 1;
            m_CharacterData.Cleanness = 1;
            m_CharacterData.Strength = 40;
            m_CharacterData.RunSpeed = 5;

            m_CharacterData.CarryingProps = null;
            m_CharacterData.OwnedSkills = null;
        }


        private void StateChange()
        {
            if (Input.GetKey(KeyCode.W) | Input.GetKey(KeyCode.UpArrow) | Input.GetKey(KeyCode.S) | Input.GetKey(KeyCode.DownArrow) |
                Input.GetKey(KeyCode.A) | Input.GetKey(KeyCode.LeftArrow) | Input.GetKey(KeyCode.D) | Input.GetKey(KeyCode.RightArrow))
            {
                state = 2;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    state = 3;
                }
                if (m_CharacterData.Stamina == 0)
                {
                    state = 2;
                }
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                state = 0;
            }
            else if (Input.GetKeyUp(KeyCode.W) | Input.GetKeyUp(KeyCode.UpArrow) | Input.GetKeyUp(KeyCode.S) | Input.GetKeyUp(KeyCode.DownArrow) |
                     Input.GetKeyUp(KeyCode.A) | Input.GetKeyUp(KeyCode.LeftArrow) | Input.GetKeyUp(KeyCode.D) | Input.GetKeyUp(KeyCode.RightArrow))
            {
                state = 1;
            }
        }
        private void PlayerMove()
        {
            float speedNow;
            if (state == 2)
            {
                speedNow = m_CharacterData.WalkSpeed;
                //Play("走路1.mp3");
            }
            else if (state == 3)
            {
                speedNow = m_CharacterData.RunSpeed;
            }
            else
            {
                return;
            }

            float vertical = Input.GetAxis("Vertical"); //W S 上 下
            float horizontal = Input.GetAxis("Horizontal"); //A D 左右
            transform.Translate(Vector3.up * vertical * speedNow * Time.deltaTime);//W S 上 下
            transform.Translate(Vector3.right * horizontal * speedNow * Time.deltaTime);//A D 左右
        }

        private void DinosaurVoiceControl()
        {
            if (AS != null)
            {
                AS.clip = (AudioClip)Resources.Load("Sounds/走路2", typeof(AudioClip));
                if (state == 2)
                {
                    if (!AS.isPlaying)
                    {
                        //按下按键P后进行播放  
                        AS.Play();
                    }
                    AS.pitch = 1;
                }
                else if (state == 3)
                {
                    if (!AS.isPlaying)
                    {
                        //按下按键P后进行播放  
                        AS.Play();
                    }
                    AS.pitch = 1.5f;
                }
                else
                {
                    //按下按键S停止播放  
                    AS.Stop();
                }
            }
        }
    }
}