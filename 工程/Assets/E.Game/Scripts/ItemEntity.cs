using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class ItemEntity : Entity
{
    [Header("【物品信息】")]
    public ScriptableItem ItemInfo;

    private void Update()
    {
        
    }

    [Server]
    protected override string UpdateServer()
    {
        return "IDLE";
    }
    [Client]
    [Obsolete]
    protected override void UpdateClient()
    {
    }
}
