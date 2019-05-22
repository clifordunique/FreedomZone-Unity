using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Entity
{
    void Awake_Entity() {}
    void OnStartServer_Entity() {}
    void Update_Entity() {}
    [Server] void DealDamageAt_Entity(Entity entity, int amount) {}
    [Client] void OnDamageReceived_Entity(int amount, DamageType damageType) {}
    [Server] void OnDeath_Entity() {}


    private void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f);
    }
    private void OnMouseOver()
    {
    }
    private void OnMouseDown()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.6f, 0.6f);
    }
    private void OnMouseDrag()
    {

    }
    private void OnMouseUp()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f);
    }
    private void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
    }
}
