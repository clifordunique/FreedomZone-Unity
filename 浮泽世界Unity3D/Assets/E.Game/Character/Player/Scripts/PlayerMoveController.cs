using System;
using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace E.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerMoveController : MonoBehaviour
    {
        [Header("当前帧按键输入")]
        [SerializeField, Tooltip("移动")] private Vector2 m_InputMove;
        [SerializeField, Tooltip("跳跃")] private bool m_InputJump;

        [Header("当前帧运动状态")]
        [SerializeField, Tooltip("移动速度")] private float m_CurrentSpeed;
        [SerializeField, Tooltip("移动方向")] private Vector3 m_MoveDir = Vector3.zero;
        [SerializeField, Tooltip("是否正在跳跃")] private bool m_IsJumping;
        [SerializeField, Tooltip("是否正在行走或站立")] private bool m_IsWalking;
        [NonSerialized, Tooltip("使用的速度")] public float m_UsingSpeed;
        [NonSerialized, Tooltip("碰撞方式")] private CollisionFlags m_CollisionFlags;
        [NonSerialized, Tooltip("当前步")] private float m_StepCycle;
        [NonSerialized, Tooltip("下一步")] private float m_NextStep;

        [Header("上一帧运动状态")]
        [NonSerialized, Tooltip("是否正在行走")] private bool m_WasWalking;
        [NonSerialized, Tooltip("是否落地")] private bool m_WasGrounding;

        [Header("运动属性")]
        [NonSerialized, Tooltip("步行速度")] private float m_WalkSpeed = 2;
        [NonSerialized, Tooltip("奔跑速度")] private float m_RunSpeed = 5;
        [NonSerialized, Tooltip("跳跃速度")] private float m_JumpSpeed = 5;
        [NonSerialized, Tooltip("下落速度")] private float m_StickToGroundForce = 10;
        [NonSerialized, Tooltip("重力系数")] private float m_GravityMultiplier = 1.5f;
        [NonSerialized, Tooltip("奔跑步长"), Range(0, 1)] private float m_RunstepLenghten = 0.5f;
        [NonSerialized, Tooltip("步间隔")] private float m_StepInterval = 2;

        [Header("运动音效")]
        [SerializeField, Tooltip("步行声音")] private AudioClip[] m_FootstepSounds;
        [SerializeField, Tooltip("跳跃声音")] private AudioClip m_JumpSound;
        [SerializeField, Tooltip("落地声音")] private AudioClip m_LandSound;

        //获取组件
        private CharacterController m_CharacterController;
        private PlayerController m_PlayerController;
        private AudioSource m_AudioSource;


        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_PlayerController = GetComponent<PlayerController>();
            m_AudioSource = GetComponent<AudioSource>();

            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_IsJumping = false;

        }
        private void Update()
        {
            if (!Cursor.visible)
            {
                // 获取跳跃按键输入
                if (!m_InputJump)
                {
                    m_InputJump = Input.GetButtonDown("Jump");
                }
                //刚落地
                if (!m_WasGrounding && m_CharacterController.isGrounded)
                {
                    PlayLandingSound();
                    m_MoveDir.y = 0f;
                    m_IsJumping = false;
                }
                if (!m_CharacterController.isGrounded && !m_IsJumping && m_WasGrounding)
                {
                    m_MoveDir.y = 0f;
                }
                //记录状态
                m_WasGrounding = m_CharacterController.isGrounded;
                m_CurrentSpeed = m_CharacterController.velocity.magnitude;
            }
        }
        private void FixedUpdate()
        {
            if (!Cursor.visible)
            {
                //重置
                m_UsingSpeed = 0;
                GetInput(out m_UsingSpeed);
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = transform.forward * m_InputMove.y + transform.right * m_InputMove.x;

                // get a normal for the surface that is being touched to move along it
                RaycastHit hitInfo;
                Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                                   m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

                m_MoveDir.x = desiredMove.x * m_UsingSpeed;
                m_MoveDir.z = desiredMove.z * m_UsingSpeed;

                if (m_CharacterController.isGrounded)
                {
                    m_MoveDir.y = -m_StickToGroundForce;
                    //起跳
                    if (m_InputJump)
                    {
                        m_MoveDir.y = m_JumpSpeed;
                        PlayJumpSound();
                        m_InputJump = false;
                        m_IsJumping = true;
                    }
                }
                else
                {
                    m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
                }

                m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
                ProgressStepCycle(m_UsingSpeed);
            }
        }
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }
            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * 2f, hit.point, ForceMode.Impulse);
        }

        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            m_WasWalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_InputMove = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_InputMove.sqrMagnitude > 1)
            {
                m_InputMove.Normalize();
            }
        }
        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_InputMove.x != 0 || m_InputMove.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;

            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }
        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }
        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }
        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }
    }
}