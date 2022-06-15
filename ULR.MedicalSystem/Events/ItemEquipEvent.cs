using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULR.MedicalSystem.Events
{
    public static class ItemEquipEvent
    {
        public static void OnEquipItem(this EventManager manager, PlayerEquipment equipment, ItemJar jar, ItemAsset asset, ref bool shouldAllow)
        {
            var player = UnturnedPlayer.FromPlayer(equipment.player);

            if (manager.pluginInstance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                shouldAllow = false;
            }
        }
    }
}
