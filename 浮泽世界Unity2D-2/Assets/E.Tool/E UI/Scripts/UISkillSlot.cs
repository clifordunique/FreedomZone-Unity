// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;

namespace E.Tool
{
    public class UISkillSlot : UIBaseSlot
    {
        public UIDragAndDropable dragAndDropable;
        public Image image;
        public Button button;
        public GameObject cooldownOverlay;
        public Text cooldownText;
        public Image cooldownCircle;
        public Text descriptionText;
        public Button upgradeButton;
    }
}