using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULR.MedicalSystem.Components;

namespace ULR.MedicalSystem.Events
{
    public static class UIEffectsEvents
    {
        public static void OnButtonClicked(this EventManager manager, Player player, string buttonName)
        {
            var uPlayer = UnturnedPlayer.FromPlayer(player);
            if (buttonName != "Suicide_Button" || !Main.Instance.DownedPlayers.ContainsKey(uPlayer.CSteamID)) return;

            Main.Instance.DownedPlayers.Remove(uPlayer.CSteamID);
            EffectManager.askEffectClearByID(9770, uPlayer.Player.channel.GetOwnerTransportConnection());
            uPlayer.Player.movement.pluginSpeedMultiplier = 1;
            uPlayer.Player.movement.pluginJumpMultiplier = 1;

            Main.Instance.ByPassMedical.Add(uPlayer.CSteamID, true);
            var component = uPlayer.GetComponent<DownedPlayerComonpent>();
            DamageTool.damage(component.Player.Player, component.newCause, component.newLimb, component.killer, uPlayer.Position, 1000, 1, out var kill, false, false);

            List<Player> players = new List<Player>();
            PlayerTool.getPlayersInRadius(uPlayer.Position, 5, players);

            foreach (var ePlayer in players.Select(UnturnedPlayer.FromPlayer))
            {
                EffectManager.askEffectClearByID(9771, ePlayer.Player.channel.GetOwnerTransportConnection());
            }
        }
    }
}
