using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ULR.MedicalSystem.Patches
{
    [HarmonyPatch(typeof(PlayerStance))]
    [HarmonyPatch("simulate")]
    class StanceChangePatch
    {
        public static void Prefix(PlayerStance __instance)
        {
            var player = UnturnedPlayer.FromPlayer(__instance.player);
            if (__instance.stance != EPlayerStance.PRONE && Main.Instance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                if (player.IsInVehicle)
                {
                    player.CurrentVehicle.forceRemovePlayer(out byte seat, player.CSteamID, out Vector3 pos, out byte angle);
                    VehicleManager.sendExitVehicle(player.CurrentVehicle, seat, pos, angle, true);
                }
                player.Player.channel.send("tellStance", player.CSteamID, (ESteamPacket)15, new object[1]
                {
                (object) 5
                });
            }
        }
    }
}
