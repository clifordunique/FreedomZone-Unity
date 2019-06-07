using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace E.Game
{
    public partial class UIBuffs : UIBase
    {
        public UIBuffSlot slotPrefab;

        void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                panel.SetActive(true);

                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, player.buffs.Count, panel.transform);

                // refresh all
                for (int i = 0; i < player.buffs.Count; ++i)
                {
                    UIBuffSlot slot = panel.transform.GetChild(i).GetComponent<UIBuffSlot>();

                    // refresh
                    slot.image.color = Color.white;
                    slot.image.sprite = player.buffs[i].image;
                    slot.tooltip.text = player.buffs[i].ToolTip();
                    slot.slider.maxValue = player.buffs[i].buffTime;
                    slot.slider.value = player.buffs[i].BuffTimeRemaining();
                }
            }
            else panel.SetActive(false);
        }
    }
}