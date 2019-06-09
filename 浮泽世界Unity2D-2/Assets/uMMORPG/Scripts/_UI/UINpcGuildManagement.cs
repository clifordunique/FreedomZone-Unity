using UnityEngine;
using UnityEngine.UI;

namespace E.Game
{
    public partial class UINpcGuildManagement : UIBase
    {
        public Text createPriceText;
        public InputField createNameInput;
        public Button createButton;
        public Button terminateButton;

        void Update()
        {
            Player player = Player.localPlayer;

            // use collider point(s) to also work with big entities
            if (player != null &&
                player.Target != null && player.Target is Npc &&
                Utils.ClosestDistance(player.collider, player.Target.collider) <= player.interactionRange)
            {
                createNameInput.interactable = !player.InGuild() &&
                                               player.Money >= GuildSystem.CreationPrice;
                createNameInput.characterLimit = GuildSystem.NameMaxLength;

                createPriceText.text = GuildSystem.CreationPrice.ToString();

                createButton.interactable = !player.InGuild() && GuildSystem.IsValidGuildName(createNameInput.text);
                createButton.onClick.SetListener(() =>
                {
                    player.CmdCreateGuild(createNameInput.text);
                    createNameInput.text = ""; // clear the input afterwards
                });

                terminateButton.interactable = player.guild.CanTerminate(player.name);
                terminateButton.onClick.SetListener(() =>
                {
                    player.CmdTerminateGuild();
                });
            }
            else panel.SetActive(false);
        }
    }
}