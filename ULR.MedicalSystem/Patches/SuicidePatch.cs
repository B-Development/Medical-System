using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULR.MedicalSystem.Patches
{
    [HarmonyPatch(typeof(PlayerLife))]
    [HarmonyPatch("ReceiveSuicideRequest")]
    class SuicidePatch
    {
        [HarmonyLib.HarmonyPrefix]
        public static bool suicide(PlayerLife __instance)
        {
            var player = UnturnedPlayer.FromPlayer(__instance.player);
            if (!Main.Instance.RevivedPlayers.ContainsKey(player.CSteamID))
            {
                if (Main.Instance.DownedPlayers.ContainsKey(player.CSteamID))
                {
                    Main.Instance.DownedPlayers.Remove(player.CSteamID);

                    List<Player> players = new List<Player>();
                    PlayerTool.getPlayersInRadius(player.Position, 5, players);
                    foreach (var eplayer in players)
                    {
                        var ePlayer = UnturnedPlayer.FromPlayer(eplayer);
                        EffectManager.askEffectClearByID(9771, ePlayer.CSteamID);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
