using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULR.MedicalSystem.Events
{
    public static class PlayerManager
    {
        public static void OnPlayerLeave(this EventManager manager, UnturnedPlayer player)
        {
            player.Player.equipment.onEquipRequested -= manager.OnEquipItem;

            if (Main.Instance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                Main.Instance.DownedPlayers.Remove(player.CSteamID);
                player.Player.life.tellHealth(player.CSteamID, 0);
            }
        }

        public static void OnPlayerJoined(this EventManager manager, UnturnedPlayer player)
        {
            player.Player.equipment.onEquipRequested += manager.OnEquipItem;
            player.Player.life.onHealthUpdated = (byte newHealth) =>
            {
                if (Main.Instance.DownedPlayers.ContainsKey(player.CSteamID))
                {
                    if(newHealth > Main.Instance.Configuration.Instance.DownedPlayerHealth)
                    {
                        player.Player.life.tellHealth(player.CSteamID, Main.Instance.Configuration.Instance.DownedPlayerHealth);
                    }
                }
            };
        }
    }
}
