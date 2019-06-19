using UnityEngine;
using UnityEngine.UI;

namespace E.Game
{
    public partial class UIHealthMana : UIBase
    {
        public Slider healthSlider;
        public Text healthStatus;
        public Slider manaSlider;
        public Text manaStatus;

        void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                panel.SetActive(true);

                healthSlider.value = player.HealthPercent();
                healthStatus.text = player.Health + " / " + player.HealthMax;

                manaSlider.value = player.MindPercent();
                manaStatus.text = player.Mind + " / " + player.MindMax;
            }
            else panel.SetActive(false);
        }
    }
}