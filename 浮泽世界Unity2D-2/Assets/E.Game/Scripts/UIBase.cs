using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E.Utility;

namespace E.Game
{
    public class UIBase : MonoBehaviour
    {
        public GameObject panel;

        private void Awake()
        {
            panel = transform.GetChild(0).gameObject;
        }
    }
}
