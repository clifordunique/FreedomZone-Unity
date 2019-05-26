using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using E.Game;
using E.Tool;
using E.Utility;

public partial class Entity
{

    void Awake_E() {}
    void OnStartServer_E() {}
    void Update_E()
    {
        if (UIManager.Singleton.entityInfoDisplayMode == UIManager.EntityInfoDisplayMode.AlwaysHide)
        {
            panName.gameObject.SetActive(false);
        }
        else
        {
            panName.gameObject.SetActive(true);
        }
    }
    [Server] void DealDamageAt_E(Entity entity, int amount) {}
    [Client] void OnDamageReceived_E(int amount, DamageType damageType) {}
    [Server] void OnDeath_E() {}


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

public partial class NetworkManagerMMO
{
    public GameState gameState;


    void Start_E() { }
    void OnStartServer_E() { }
    void OnStopServer_E() { }
    void OnClientConnect_E(NetworkConnection conn) { }
    void OnServerLogin_E(LoginMsg message) { }
    void OnClientCharactersAvailable_E(CharactersAvailableMsg message) { }
    void OnServerAddPlayer_E(string account, GameObject player, NetworkConnection conn, AddPlayerMessage message) { }
    void OnServerCharacterCreate_E(CharacterCreateMsg message, Player player) { }
    void OnServerCharacterDelete_E(CharacterDeleteMsg message) { }
    void OnServerDisconnect_E(NetworkConnection conn) { }
    void OnClientDisconnect_E(NetworkConnection conn) { }


    public enum GameState
    {
        Loading,
        Ready,
        Gaming
    }
}
