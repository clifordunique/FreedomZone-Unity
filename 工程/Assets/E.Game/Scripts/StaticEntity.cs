using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class StaticEntity : Entity
{
    [Header("【信息】")]
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
