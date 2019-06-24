using UnityEngine;
using UnityEngine.UI;

namespace E.Tool
{
    public partial class UIPortrait : UIBase
    {
        public Image image;

        void Update()
        {
            Player player = Player.localPlayer;

            if (player)
            {
                panel.SetActive(true);
                image.sprite = player.portraitIcon;
            }
            else panel.SetActive(false);
        }
    }
}