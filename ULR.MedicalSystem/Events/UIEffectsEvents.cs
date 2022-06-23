using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULR.MedicalSystem.Events
{
    public static class UIEffectsEvents
    {
        public static void OnButtonClicked(this EventManager manager, Player player, string buttonName)
        {
            var ePlayer = UnturnedPlayer.FromPlayer(player);
            if (buttonName == "Suicide_Button" && Main.Instance.DownedPlayers.ContainsKey(ePlayer.CSteamID))
            {
                Main.Instance.DownedPlayers.Remove(ePlayer.CSteamID);
                EffectManager.askEffectClearByID(9770, ePlayer.CSteamID);
                ePlayer.Player.life.askSuicide(ePlayer.CSteamID);
                ePlayer.Player.movement.pluginSpeedMultiplier = 1;
                ePlayer.Player.movement.pluginJumpMultiplier = 1;
            }
        }
    }
}
