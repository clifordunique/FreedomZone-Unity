﻿using UnityEngine;
using UnityEngine.UI;

public partial class UINpcRevive : MonoBehaviour
{
    public static UINpcRevive singleton;
    public GameObject panel;
    public UIDragAndDropable itemSlot;
    public Text costsText;
    public Button reviveButton;
    [HideInInspector] public int itemIndex = -1;

    public UINpcRevive()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    void Update()
    {
        Player player = Player.localPlayer;

        // use collider point(s) to also work with big entities
        if (player != null &&
            player.Target != null && player.Target is Npc &&
            Utils.ClosestDistance(player.collider, player.Target.collider) <= player.interactionRange)
        {
            Npc npc = (Npc)player.Target;

            // revive
            if (itemIndex != -1 && itemIndex < player.inventory.Count &&
                player.inventory[itemIndex].amount > 0 &&
                player.inventory[itemIndex].item.data is PetItem)
            {
                ItemSlot slot = player.inventory[itemIndex];
                PetItem itemData = (PetItem)slot.item.data;
                if (itemData.summonPrefab != null) {
                    itemSlot.GetComponent<Image>().color = Color.white;
                    itemSlot.GetComponent<Image>().sprite = slot.item.image;
                    itemSlot.GetComponent<UIShowToolTip>().enabled = true;
                    itemSlot.GetComponent<UIShowToolTip>().text = slot.ToolTip();
                    itemSlot.dragable = true;
                    costsText.text = itemData.revivePrice.ToString();
                    reviveButton.interactable = slot.item.summonedHealth == 0 && player.Money >= itemData.revivePrice;
                    reviveButton.onClick.SetListener(() => {
                        player.CmdNpcReviveSummonable(itemIndex);
                        itemIndex = -1;
                    });
                }
            }
            else
            {
                itemSlot.GetComponent<Image>().color = Color.clear;
                itemSlot.GetComponent<Image>().sprite = null;
                itemSlot.GetComponent<UIShowToolTip>().enabled = false;
                itemSlot.dragable = false;
                costsText.text = "0";
                reviveButton.interactable = false;
            }
        }
        else panel.SetActive(false);
    }
}
