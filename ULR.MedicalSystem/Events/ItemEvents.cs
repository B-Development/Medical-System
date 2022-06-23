using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULR.MedicalSystem.Events
{
    public static class ItemEvents
    {
        public static void OnEquipItem(this EventManager manager, PlayerEquipment equipment, ItemJar jar, ItemAsset asset, ref bool shouldAllow)
        {
            var player = UnturnedPlayer.FromPlayer(equipment.player);

            if (manager.pluginInstance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                shouldAllow = false;
            }
        }

        public static void OnPickupItem(this EventManager manager, Player player, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            var uplayer = UnturnedPlayer.FromPlayer(player);

            if (Main.Instance.DownedPlayers.ContainsKey(uplayer.CSteamID))
            {
                shouldAllow = false;
            }
        }
    }
}
