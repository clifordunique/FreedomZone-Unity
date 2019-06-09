using System;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;

namespace E.Game
{
    [DisallowMultipleComponent]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private Vector3 m_OriginalCameraPosition;

        [Header("相机视角")]
        [Tooltip("X灵敏度")] public float XSensitivity = 2f;
        [Tooltip("Y灵敏度")] public float YSensitivity = 2f;
        [Tooltip("X最小值")] public float MinimumX = -80f;
        [Tooltip("X最大值")] public float MaximumX = 50f;


        private void Start()
        {
            transform.position = CameraManager.Singleton.m_ThirdCamera.transform.position;
            transform.eulerAngles = CameraManager.Singleton.m_ThirdCamera.transform.eulerAngles;
        }
        private void FixedUpdate()
        {
            if (!Cursor.visible)
            {
                transform.position = m_OriginalCameraPosition + CharacterManager.Singleton.Player.transform.position;
                float yRot = Input.GetAxis("Mouse X") * XSensitivity;
                float xRot = Input.GetAxis("Mouse Y") * YSensitivity;
                CharacterManager.Singleton.Player.transform.eulerAngles += new Vector3(0, yRot, 0);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, CharacterManager.Singleton.Player.transform.eulerAngles.y, transform.eulerAngles.z);
                float x = transform.eulerAngles.x;
                if (x > 180)
                {
                    if (x < MinimumX + 360)
                    {
                        transform.eulerAngles = new Vector3(MinimumX, transform.eulerAngles.y, transform.eulerAngles.z);
                    }
                    else
                    {
                        transform.eulerAngles += new Vector3(-xRot, 0, 0);
                    }

                }
                else if (x < 180)
                {
                    if (x > MaximumX)
                    {
                        transform.eulerAngles = new Vector3(MaximumX - 0.01f, transform.eulerAngles.y, transform.eulerAngles.z);
                    }
                    else
                    {
                        transform.eulerAngles += new Vector3(-xRot, 0, 0);
                    }
                }
            }
        }


        /// <summary>
        /// 设置画面高斯模糊
        /// </summary>
        /// <param name="b"></param>
        public void SetRapidBluer(bool b)
        {
            if (b)
            {
                GetComponent<RapidBlurEffect>().DownSampleNum = 1;
                GetComponent<RapidBlurEffect>().BlurIterations = 4;
            }
            else
            {
                GetComponent<RapidBlurEffect>().DownSampleNum = 0;
                GetComponent<RapidBlurEffect>().BlurIterations = 0;
            }
        }
    }
}