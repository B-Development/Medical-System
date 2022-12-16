using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ULR.MedicalSystem.Commands
{
    public class CallCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "112";

        public string Help => "Calls the EMS to your location";

        public string Syntax => "/112";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var uPlayer = caller as UnturnedPlayer;

            if (!Main.Instance.DownedPlayers.ContainsKey(uPlayer.CSteamID))
            {
                UnturnedChat.Say(caller, $"You cannot call the ems unless downed.", Color.red);
                return;
            }

            foreach(var client in Provider.clients)
            {
                var user = UnturnedPlayer.FromSteamPlayer(client);

                if(user.GetPermissions().Any(perm => perm.Name == Main.Instance.Configuration.Instance.DownedReviveCall))
                {
                    user.Player.quests.tellSetMarker(user.CSteamID, true, uPlayer.Position, "EMS Call!");
                }
            }

            UnturnedChat.Say(uPlayer, "You have now called the ems to your location");
        }
    }
}
