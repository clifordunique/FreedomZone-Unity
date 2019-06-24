using UnityEngine;
using UnityEngine.UI;

namespace E.Tool
{
    public partial class UIExperienceBar : UIBase
    {
        public Slider slider;
        public Text statusText;

        void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                panel.SetActive(true);
                slider.value = player.ExperiencePercent();
                statusText.text = "Lv." + player.level + " (" + (player.ExperiencePercent() * 100).ToString("F2") + "%)";
            }
            else panel.SetActive(false);
        }
    }
}