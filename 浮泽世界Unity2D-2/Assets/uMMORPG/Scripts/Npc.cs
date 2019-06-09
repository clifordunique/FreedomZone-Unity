// The Npc class is rather simple. It contains state Update functions that do
// nothing at the moment, because Npcs are supposed to stand around all day.
//
// Npcs first show the welcome text and then have options for item trading and
// quests.
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// talk-to-npc quests work by adding the same quest to two npcs, one with
// accept=true and complete=false, the other with accept=false and complete=true
[Serializable]
public class ScriptableQuestOffer
{
    public ScriptableQuest quest;
    public bool acceptHere = true;
    public bool completeHere = true;
}

[RequireComponent(typeof(NetworkNavMeshAgent2D))]
public partial class Npc : Entity
{
    [Header("【欢迎文本】")]
    [TextArea(1, 30)] public string welcome;

    [Header("【出售物品】")]
    public ScriptableItem[] saleItems;

    [Header("【任务】")]
    public ScriptableQuestOffer[] quests;

    [Header("【传送】")]
    public Transform teleportTo;

    [Header("【是否提供公会管理】")]
    public bool offersGuildManagement = true;

    [Header("【是否可召唤】")]
    public bool offersSummonableRevive = true;

    // networkbehaviour ////////////////////////////////////////////////////////
    public override void OnStartServer()
    {
        base.OnStartServer();

        // all npcs should spawn with full health and mana
        Health = HealthMax;
        Mind = MindMax;

        // addon system hooks
        Utils.InvokeMany(GetType(), this, "OnStartServer_");
    }

    // finite state machine states /////////////////////////////////////////////
    [Server] protected override string UpdateServer() { return State; }
    [Client]
    [Obsolete]
    protected override void UpdateClient()
    {
        // addon system hooks
        Utils.InvokeMany(GetType(), this, "UpdateClient_");
    }

    // overlays ////////////////////////////////////////////////////////////////
    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();

        if (panQuestMark != null)
        {
            panQuestMark.SetActive(true);
            // find local player (null while in character selection)
            if (Player.localPlayer != null)
            {
                if (quests.Any(entry => entry.completeHere && Player.localPlayer.CanCompleteQuest(entry.quest.name)))
                    panQuestMark.GetComponentInChildren<Text>().text = "!";
                else if (quests.Any(entry => entry.acceptHere && Player.localPlayer.CanAcceptQuest(entry.quest)))
                    panQuestMark.GetComponentInChildren<Text>().text = "?";
                else
                    panQuestMark.GetComponentInChildren<Text>().text = "";
            }
        }
    }

    // skills //////////////////////////////////////////////////////////////////
    public override bool CanAttack(Entity entity) { return false; }

    // quests //////////////////////////////////////////////////////////////////
    // helper function to filter the quests that are shown for a player
    // -> all quests that:
    //    - can be started by the player
    //    - or were already started but aren't completed yet
    public List<ScriptableQuest> QuestsVisibleFor(Player player)
    {
        return quests.Where(entry => (entry.acceptHere && player.CanAcceptQuest(entry.quest)) ||
                                     (entry.completeHere && player.HasActiveQuest(entry.quest.name)))
                     .Select(entry => entry.quest)
                     .ToList();
    }
}
