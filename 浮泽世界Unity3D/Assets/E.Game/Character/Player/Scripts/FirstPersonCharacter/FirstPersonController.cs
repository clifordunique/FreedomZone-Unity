using System;
using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("运动状态")]
        [SerializeField, Tooltip("移动速度")] private float m_CurrentSpeed;
        [SerializeField, Tooltip("是否步行或奔跑")] private bool m_IsWalking;

        [Header("运动属性")]
        [SerializeField, Tooltip("步行速度")] private float m_WalkSpeed;
        [SerializeField, Tooltip("奔跑速度")] private float m_RunSpeed;
        [SerializeField, Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField, Tooltip("跳跃速度")] private float m_JumpSpeed;
        [SerializeField, Tooltip("下落速度")] private float m_StickToGroundForce;
        [SerializeField, Tooltip("重力系数")] private float m_GravityMultiplier;

        [Header("相机视角")]
        [SerializeField] private MouseLook m_MouseLook;

        [Header("相机视场角变动")]
        [SerializeField, Tooltip("是否启用相机视场角变动")] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();

        [Header("相机视角摇晃")]
        [SerializeField, Tooltip("是否启用相机视角摇晃")] private bool m_UseHeadBob;
        [SerializeField, Tooltip("曲线摇晃控制器")] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField, Tooltip("插值摇晃控制器")] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        
        [Header("运动音效")]
        [SerializeField, Tooltip("步间隔")] private float m_StepInterval;
        [SerializeField, Tooltip("步行声音")] private AudioClip[] m_FootstepSounds;
        [SerializeField, Tooltip("跳跃声音")] private AudioClip m_JumpSound;
        [SerializeField, Tooltip("落地声音")] private AudioClip m_LandSound;

        private CharacterController m_CharacterController;
        private AudioSource m_AudioSource;
        private Camera m_Camera;
        private Vector3 m_OriginalCameraPosition;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CollisionFlags m_CollisionFlags;
        private float m_YRotation;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_PreviouslyGrounded;
        private bool m_Jumping;
        private bool m_Jump;


        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_AudioSource = GetComponent<AudioSource>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;

            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
			m_MouseLook.Init(transform , m_Camera.transform);
        }

        private void Update()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // the jump state needs to read here to make sure it is not missed
                if (!m_Jump)
                {
                    m_Jump = Input.GetButtonDown("Jump");
                }

                if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
                {
                    StartCoroutine(m_JumpBob.DoBobCycle());
                    PlayLandingSound();
                    m_MoveDir.y = 0f;
                    m_Jumping = false;
                }
                if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
                {
                    m_MoveDir.y = 0f;
                }

                m_PreviouslyGrounded = m_CharacterController.isGrounded;
                m_CurrentSpeed = m_CharacterController.velocity.magnitude;
            }
        }
        
        private void FixedUpdate()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                RotateView();

                float speed;
                GetInput(out speed);
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

                // get a normal for the surface that is being touched to move along it
                RaycastHit hitInfo;
                Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                                   m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

                m_MoveDir.x = desiredMove.x * speed;
                m_MoveDir.z = desiredMove.z * speed;


                if (m_CharacterController.isGrounded)
                {
                    m_MoveDir.y = -m_StickToGroundForce;

                    if (m_Jump)
                    {
                        m_MoveDir.y = m_JumpSpeed;
                        PlayJumpSound();
                        m_Jump = false;
                        m_Jumping = true;
                    }
                }
                else
                {
                    m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
                }
                m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

                ProgressStepCycle(speed);
                UpdateCameraPosition(speed);
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


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }
        
        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }
        
        private void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);
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
        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
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
