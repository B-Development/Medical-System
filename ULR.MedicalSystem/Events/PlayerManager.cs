using Rocket.Unturned.Player;
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
        }

        public static void OnPlayerJoined(this EventManager manager, UnturnedPlayer player)
        {
            player.Player.equipment.onEquipRequested += manager.OnEquipItem;
        }
    }
}
