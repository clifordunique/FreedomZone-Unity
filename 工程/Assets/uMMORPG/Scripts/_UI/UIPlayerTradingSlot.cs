﻿// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;

namespace E.Game
{
    public class UIPlayerTradingSlot : UIBaseSlot
    {
        public UIDragAndDropable dragAndDropable;
        public Image image;
        public GameObject amountOverlay;
        public Text amountText;
    }
}