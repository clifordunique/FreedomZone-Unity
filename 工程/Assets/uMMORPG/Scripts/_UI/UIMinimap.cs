﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace E.Game
{
    public partial class UIMinimap : UIBase
    {
        public float zoomMin = 5;
        public float zoomMax = 50;
        public float zoomStepSize = 5;
        public Text sceneText;
        public Button plusButton;
        public Button minusButton;
        public Camera minimapCamera;

        void Start()
        {
            plusButton.onClick.SetListener(() =>
            {
                minimapCamera.orthographicSize = Mathf.Max(minimapCamera.orthographicSize - zoomStepSize, zoomMin);
            });
            minusButton.onClick.SetListener(() =>
            {
                minimapCamera.orthographicSize = Mathf.Min(minimapCamera.orthographicSize + zoomStepSize, zoomMax);
            });
        }

        void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                panel.SetActive(true);
                sceneText.text = SceneManager.GetActiveScene().name;
            }
            else panel.SetActive(false);
        }
    }
}