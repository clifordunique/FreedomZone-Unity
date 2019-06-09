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
using HighlightingSystem;
using UnityEngine.Networking;
using System;

namespace E.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerData))]
    public class PlayerController : MonoBehaviour
    {
        [Header("交互相关")]
        [SerializeField, Tooltip("交互模式")] private InteractiveMode m_InteractiveMode = InteractiveMode.普通;
        [SerializeField, Tooltip("左手")] private GameObject m_LHand;
        [SerializeField, Tooltip("右手")] private GameObject m_RHand;

        [Header("其它")]
        [SerializeField, Tooltip("动画控制器")] private Animator m_Animator;
        private CharacterController m_CharacterController;
        private PlayerData m_PlayerModel;


        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            m_CharacterController = GetComponent<CharacterController>();
            m_PlayerModel = GetComponent<PlayerData>();
        }
        private void Start()
        {
            InitPlayerModel();
            UIManager.Singleton.m_UIMessageController.ShowMessage("欢迎光临浮泽世界！亲爱的" + m_PlayerModel.PlayerName);
            CameraManager.Singleton.UseFirstPersonView();
            CameraManager.Singleton.SetMirrorsCamera();
        }
        private void Update()
        {
            //记录本次游戏时间
            m_PlayerModel.TotalOnline += TimeSpan.FromSeconds(1f / 60);

            if (!Cursor.visible)
            {
                //按键检测
                if (Input.GetKeyUp(KeyCode.Q))
                {
                    PickUp(m_LHand);
                }
                if (Input.GetKeyUp(KeyCode.E))
                {
                    PickUp(m_RHand);
                }
                if (Input.GetKeyUp(KeyCode.F))
                {
                    StoryManager.Singleton.SurveyOrDialog();
                }
                if (Input.GetMouseButtonUp(0))
                {
                    if (m_InteractiveMode == InteractiveMode.普通)
                    {
                        Use(m_LHand);
                    }
                    else if (m_InteractiveMode == InteractiveMode.战斗)
                    {
                        Attack(m_LHand);
                    }
                    else if (m_InteractiveMode == InteractiveMode.投掷)
                    {
                        Throw(m_LHand);
                    }
                }
                if (Input.GetMouseButtonUp(1))
                {
                    if (m_InteractiveMode == InteractiveMode.普通)
                    {
                        Use(m_RHand);
                    }
                    else if (m_InteractiveMode == InteractiveMode.战斗)
                    {
                        Attack(m_RHand);
                    }
                    else if (m_InteractiveMode == InteractiveMode.投掷)
                    {
                        Throw(m_RHand);
                    }
                }
                if (Input.GetKeyUp(KeyCode.Tab))
                {
                    switch (m_InteractiveMode)
                    {
                        case InteractiveMode.普通:
                            m_InteractiveMode = InteractiveMode.战斗;
                            Debug.Log("准备攻击");
                            break;
                        case InteractiveMode.战斗:
                            m_InteractiveMode = InteractiveMode.投掷;
                            Debug.Log("准备投掷");
                            break;
                        case InteractiveMode.投掷:
                            m_InteractiveMode = InteractiveMode.普通;
                            Debug.Log("准备使用");
                            break;
                    }
                }
                if (Input.GetKeyUp(KeyCode.V))
                {
                    CameraManager.Singleton.SwitchCamera();
                }

                SetAnimation();
            }
            else
            {
                if (Input.GetKeyUp(KeyCode.F) | Input.GetKeyUp(KeyCode.Space) |
                    Input.GetKeyUp(KeyCode.Return) | Input.GetMouseButtonUp(0))
                {
                    StoryManager.Singleton.NextSentence();
                }

            }
        }

        /// <summary>
        /// 初始化玩家信息
        /// </summary>
        private void InitPlayerModel()
        {
            m_PlayerModel.PlayerName = "E";
            m_PlayerModel.PlayerID = "000001";
            m_PlayerModel.FirstLogin = DateTime.Now;
            m_PlayerModel.LastLogin = DateTime.Now;
            m_PlayerModel.LastLogout = DateTime.Now;

            m_PlayerModel.TotalOnline = TimeSpan.Zero;
            m_PlayerModel.Health = 100;
            m_PlayerModel.Stamina = 100;
            m_PlayerModel.Mentality = 100;
            m_PlayerModel.Satiety = 1;
            m_PlayerModel.Cleanness = 1;
            m_PlayerModel.Strength = 40;
            m_PlayerModel.RunSpeed = 5;

            m_PlayerModel.CarryingProps = null;
            m_PlayerModel.OwnedSkills = null;
        }

        /// <summary>
        /// 设置动画
        /// </summary>
        private void SetAnimation()
        {
            if (m_CharacterController.isGrounded)
            {
                //移动动画
                if (!Input.GetKey(KeyCode.W) & !Input.GetKey(KeyCode.A) & !Input.GetKey(KeyCode.S) & !Input.GetKey(KeyCode.D))
                {
                    if (m_Animator.GetInteger("animation") != 19)
                    {
                        m_Animator.GetComponent<AnimatorController>().SetInt("animation,19");
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        if (Input.GetKey(KeyCode.W))
                        {
                            if (m_Animator.GetInteger("animation") != 6)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,6");
                            }
                        }
                        if (Input.GetKey(KeyCode.A))
                        {
                            if (m_Animator.GetInteger("animation") != 7)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,7");

                            }
                        }
                        else if (Input.GetKey(KeyCode.D))
                        {
                            if (m_Animator.GetInteger("animation") != 8)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,8");
                            }
                        }
                        else if (Input.GetKey(KeyCode.S))
                        {
                            if (m_Animator.GetInteger("animation") != 9)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,9");
                            }
                        }
                    }
                    else if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (Input.GetKey(KeyCode.W))
                        {
                            if (m_Animator.GetInteger("animation") != 10)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,10");
                            }
                        }
                        if (Input.GetKey(KeyCode.A))
                        {
                            if (m_Animator.GetInteger("animation") != 11)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,11");
                            }
                        }
                        else if (Input.GetKey(KeyCode.D))
                        {
                            if (m_Animator.GetInteger("animation") != 12)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,12");
                            }
                        }
                        else if (Input.GetKey(KeyCode.S))
                        {
                            if (m_Animator.GetInteger("animation") != 10)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,10");
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.W))
                        {
                            if (m_Animator.GetInteger("animation") != 6)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,6");
                            }
                        }
                        if (Input.GetKey(KeyCode.A))
                        {
                            if (m_Animator.GetInteger("animation") != 7)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,7");
                            }
                        }
                        else if (Input.GetKey(KeyCode.D))
                        {
                            if (m_Animator.GetInteger("animation") != 8)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,8");
                            }
                        }
                        else if (Input.GetKey(KeyCode.S))
                        {
                            if (m_Animator.GetInteger("animation") != 9)
                            {
                                m_Animator.GetComponent<AnimatorController>().SetInt("animation,9");
                            }
                        }
                    }
                }

                //跳跃动画
                if (Input.GetButton("Jump"))
                {
                    if (m_Animator.GetInteger("animation") != 13)
                    {
                        m_Animator.GetComponent<AnimatorController>().SetInt("animation,13");
                    }
                }
            }
        }
        //物品系统
        /// <summary>
        /// 拾取物品
        /// </summary>
        /// <param name="hand"></param>
        private void PickUp(GameObject hand)
        {
            if (CameraManager.Singleton.m_CurrentCamera == null) { return; }
            if (hand.transform.childCount == 0)
            {
                GameObject target = CameraManager.Singleton.m_CurrentCamera.GetComponent<RaycastController>().m_HitTarget;
                if (target == null) { return; }
                //当射线碰撞目标为props类型的物品，执行拾取操作
                if (target.layer == 10)
                {
                    target.transform.SetParent(hand.transform);

                    target.GetComponent<InteractorData>().PickUp();
                    //target.transform.localPosition = new Vector3(0, 0, 0);
                    float y = target.transform.eulerAngles.y;
                    target.transform.eulerAngles = new Vector3(0, y, 0);

                    target.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    Debug.Log("拾取了：" + target.name);
                }
                else
                {
                    Debug.Log("无法拾取：" + target.name);
                }
            }
            else
            {
                hand.transform.GetChild(0).GetComponent<InteractorData>().m_V1 = Vector3.zero;
                StopCoroutine(hand.transform.GetChild(0).GetComponent<InteractorData>().MoveTo(new Vector3(0, 0, 0)));

                Debug.Log("丢弃了：" + hand.transform.GetChild(0).name);
                hand.transform.GetChild(0).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                hand.transform.GetChild(0).SetParent(GameObject.Find("Props").transform);
            }
        }
        /// <summary>
        /// 使用
        /// </summary>
        /// <param name="hand">使用道具的手</param>
        private void Use(GameObject hand)
        {
            Debug.Log("正在使用由：" + hand.name);
        }
        /// <summary>
        /// 攻击
        /// </summary>
        /// <param name="hand">攻击的手</param>
        private void Attack(GameObject hand)
        {
            Debug.Log("正在攻击由：" + hand.name);
        }
        /// <summary>
        /// 投掷
        /// </summary>
        /// <param name="hand">投掷道具的手</param>
        private void Throw(GameObject hand)
        {
            if (hand.transform.childCount > 0)
            {
                Transform target = hand.transform.GetChild(0);
                Debug.Log("正在投掷：" + target.GetComponent<InteractorData>().Name);
                target.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                float temp = (float)Math.Sqrt(target.GetComponent<Rigidbody>().mass);
                target.GetComponent<Rigidbody>().AddForce(CameraManager.Singleton.m_CurrentCamera.transform.forward * m_PlayerModel.Strength * temp * 20);
                target.SetParent(GameObject.Find("Props").transform);
            }
            else
            {
                Debug.Log("没有可以投掷的物品：" + hand.name);
            }
        }
    }
}