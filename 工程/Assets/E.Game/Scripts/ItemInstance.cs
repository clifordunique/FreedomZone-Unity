using UnityEngine;
using Mirror;
using System.Linq;

public class ItemInstance : Entity
{
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
    protected override void UpdateClient()
    {
    }
}
