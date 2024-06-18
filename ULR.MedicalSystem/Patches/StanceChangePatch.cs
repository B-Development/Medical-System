using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace ULR.MedicalSystem.Patches
{
    [HarmonyPatch(typeof(PlayerStance))]
    [HarmonyPatch("simulate")]
    class StanceChangePatch
    {
        public static void Prefix(PlayerStance instance)
        {
            var player = UnturnedPlayer.FromPlayer(instance.player);
            if (instance.stance != EPlayerStance.PRONE && Main.Instance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                if (player.IsInVehicle)
                {
                    player.CurrentVehicle.forceRemovePlayer(out var seat, player.CSteamID, out var pos, out var angle);
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
