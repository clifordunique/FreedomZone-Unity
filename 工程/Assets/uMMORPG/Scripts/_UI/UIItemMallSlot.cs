﻿// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;

namespace E.Game
{
    public class UIItemMallSlot : UIBaseSlot
    {
        public Image image;
        public UIDragAndDropable dragAndDropable;
        public Text nameText;
        public Text priceText;
        public Button unlockButton;
    }
}