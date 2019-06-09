using System;
using UnityEngine;

namespace E.Game
{
    [DisallowMultipleComponent]
    public class ThirdPersonController : MonoBehaviour
    {
        private void Start()
        {
            transform.position = CameraManager.Singleton.m_FirstCamera.transform.position;
            transform.eulerAngles = CameraManager.Singleton.m_FirstCamera.transform.eulerAngles;
        }
        private void Update()
        {
            if (!Cursor.visible)
            {
                CharacterManager.Singleton.Player.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
            }
        }
    }
}