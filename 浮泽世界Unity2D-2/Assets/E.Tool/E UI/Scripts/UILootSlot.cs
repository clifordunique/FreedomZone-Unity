// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;

namespace E.Tool
{
    public class UILootSlot : UIBaseSlot
    {
        public Button button;
        public UIDragAndDropable dragAndDropable;
        public Image image;
        public Text nameText;
        public GameObject amountOverlay;
        public Text amountText;
    }
}