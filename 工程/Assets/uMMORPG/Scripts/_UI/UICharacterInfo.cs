// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;
using System;

public partial class UICharacterInfo : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.T;
    public GameObject panel;
    public Text damageText;
    public Text defenseText;
    public Text healthText;
    public Text manaText;
    public Text criticalChanceText;
    public Text blockChanceText;
    public Text speedText;
    public Text levelText;
    public Text currentExperienceText;
    public Text maximumExperienceText;
    public Text skillExperienceText;
    public Text strengthText;
    public Text intelligenceText;
    public Button strengthButton;
    public Button intelligenceButton;

    public Entity entity;

    [Obsolete]
    private void Update()
    {
        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
        {
            SetPlayerEntity();
        }
        // only refresh the panel while it's active
        if (entity)
        {
            RefreshCharacterInfo();
        }
        else
        {
            panel.SetActive(false);
        }
    }

    public void SetPlayerEntity()
    {
        entity = Player.localPlayer;
        panel.SetActive(!panel.activeSelf);
    }
    public void SetCharacterEntity(Entity e)
    {
        entity = e;
        panel.SetActive(!panel.activeSelf);
    }
    public void RefreshCharacterInfo()
    {
        if (panel.activeSelf)
        {
            damageText.text = entity.damage.ToString();
            defenseText.text = entity.defense.ToString();
            healthText.text = entity.healthMax.ToString();
            manaText.text = entity.manaMax.ToString();
            criticalChanceText.text = (entity.criticalChance * 100).ToString("F0") + "%";
            blockChanceText.text = (entity.blockChance * 100).ToString("F0") + "%";
            speedText.text = entity.speed.ToString();
            levelText.text = entity.level.ToString();

            if (entity.GetType() == typeof(Player))
            {
                Player player = (Player)entity;
                currentExperienceText.text = player.experience.ToString();
                maximumExperienceText.text = player.experienceMax.ToString();
                skillExperienceText.text = player.skillExperience.ToString();

                strengthText.text = player.strength.ToString();
                strengthButton.interactable = player.AttributesSpendable() > 0;
                strengthButton.onClick.SetListener(() => { player.CmdIncreaseStrength(); });

                intelligenceText.text = player.intelligence.ToString();
                intelligenceButton.interactable = player.AttributesSpendable() > 0;
                intelligenceButton.onClick.SetListener(() => { player.CmdIncreaseIntelligence(); });
            }
            else if (entity.GetType() == typeof(Npc))
            {
                Npc npc = (Npc)entity;
                //currentExperienceText.text = npc.experience.ToString();
                //maximumExperienceText.text = npc.experienceMax.ToString();
                //skillExperienceText.text = npc.skillExperience.ToString();

                //strengthText.text = npc.strength.ToString();
                //strengthButton.interactable = npc.AttributesSpendable() > 0;
                //strengthButton.onClick.SetListener(() => { npc.CmdIncreaseStrength(); });

                //intelligenceText.text = npc.intelligence.ToString();
                //intelligenceButton.interactable = npc.AttributesSpendable() > 0;
                //intelligenceButton.onClick.SetListener(() => { npc.CmdIncreaseIntelligence(); });
            }
            else if (entity.GetType() == typeof(Monster))
            {
                Monster monster = (Monster)entity;
                //currentExperienceText.text = monster.experience.ToString();
                //maximumExperienceText.text = monster.experienceMax.ToString();
                //skillExperienceText.text = monster.skillExperience.ToString();

                //strengthText.text = monster.strength.ToString();
                //strengthButton.interactable = monster.AttributesSpendable() > 0;
                //strengthButton.onClick.SetListener(() => { monster.CmdIncreaseStrength(); });

                //intelligenceText.text = monster.intelligence.ToString();
                //intelligenceButton.interactable = monster.AttributesSpendable() > 0;
                //intelligenceButton.onClick.SetListener(() => { monster.CmdIncreaseIntelligence(); });
            }
            else if (entity.GetType() == typeof(Item))
            {

            }
        }
    }
}
