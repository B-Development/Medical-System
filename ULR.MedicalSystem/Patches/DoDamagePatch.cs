using HarmonyLib;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ULR.MedicalSystem.Patches
{
    [HarmonyPatch(typeof(PlayerLife))]
    [HarmonyPatch("doDamage")]
    class DoDamagePatch
    {
        public static bool Prefix(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, ref EPlayerKill kill, bool trackKill, ERagdollEffect newRagdollEffect, bool canCauseBleeding, PlayerLife __instance)
        {
            Player ply = __instance.channel.owner.player;
            UnturnedPlayer uplayer = UnturnedPlayer.FromPlayer(ply);
            if (ply is null) return false;
            if (Main.Instance.DownedPlayers.ContainsKey(newKiller))
            {
                UnturnedChat.Say(newKiller, $"You cannot damage {uplayer.CharacterName} while downed.", Color.red);
                return false;
            }
            if(newCause == EDeathCause.SUICIDE)
            {
                return true;
            }
            if (Main.Instance.DownedInvincivility.ContainsKey(uplayer.CSteamID))
            {
                return false;
            }
            if (amount >= ply.life.health)
            {
                if (!Main.Instance.DownedPlayers.ContainsKey(uplayer.CSteamID))
                {
                    Main.Instance.DownedPlayers.Add(uplayer.CSteamID, true);
                    Main.Instance.DownedInvincivility.Add(uplayer.CSteamID, true);

                    ply.equipment.dequip();
                    ply.life.tellHealth(uplayer.CSteamID, Main.Instance.Configuration.Instance.DownedPlayerHealth);

                    uplayer.Player.channel.send("tellStance", uplayer.CSteamID, (ESteamPacket)15, new object[1]
                    {
                        //The Stance ID
                        (object)5
                    });

                    ply.movement.pluginSpeedMultiplier = Main.Instance.Configuration.Instance.DownedPlayerMovementSpeed;
                    ply.movement.pluginJumpMultiplier = 0;

                    Main.Instance.StartCoroutine(DownedInvincibility(uplayer.CSteamID, Main.Instance.Configuration.Instance.DownedInvicibilityTimer));
                    Main.Instance.StartCoroutine(DownTimer(uplayer.CSteamID, Main.Instance.Configuration.Instance.DownedTimer));

                    EffectManager.sendUIEffect(9770, 9770, uplayer.CSteamID, false, "You are injured", string.Empty, "Respawn in: " + Main.Instance.Configuration.Instance.DownedTimer + "s", "Suicide");
                    EffectManager.sendEffectClicked("Suicide_Button");

                    List<Player> players = new List<Player>();
                    PlayerTool.getPlayersInRadius(uplayer.Position, 5, players);
                    foreach (var p in players)
                    {
                        var steamid = UnturnedPlayer.FromPlayer(p).CSteamID;
                        if (steamid != uplayer.CSteamID)
                        {
                            EffectManager.sendUIEffect(9771, 9771, steamid, false, "Player in need", $"Use surrender to revive {uplayer.DisplayName}");
                        }
                    }
                    return false;
                }
                else
                {
                    if (Main.Instance.Configuration.Instance.KillDownedPlayers)
                    {
                        Main.Instance.DownedPlayers.Remove(uplayer.CSteamID);

                        uplayer.Player.movement.pluginSpeedMultiplier = 1;
                        uplayer.Player.movement.pluginJumpMultiplier = 1;

                        List<Player> players = new List<Player>();
                        PlayerTool.getPlayersInRadius(uplayer.Position, 5, players);
                        foreach (var player in players)
                        {
                            var ePlayer = UnturnedPlayer.FromPlayer(player);
                            EffectManager.askEffectClearByID(9771, ePlayer.CSteamID);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static IEnumerator DownedInvincibility(CSteamID steamid, int time)
        {
            var pl = UnturnedPlayer.FromCSteamID(steamid);

            while (time > 0)
            {
                time--;
                yield return new WaitForSeconds(1);
            }

            Main.Instance.DownedInvincivility.Remove(steamid);
        }

        public static void changeTimer(CSteamID steamid, int time)
        {
            EffectManager.askEffectClearByID(9770, steamid);
            EffectManager.sendUIEffect(9770, 9770, steamid, false, "You are injured", string.Empty, "Respawn in: " + time + "s", "Suicide");
            EffectManager.sendEffectClicked("Suicide_Button");
        }

        public static IEnumerator DownTimer(CSteamID steamid, int time)
        {
            var pl = UnturnedPlayer.FromCSteamID(steamid);

            while (time > 0)
            {
                if (Main.Instance.DownedPlayers.ContainsKey(steamid))
                {
                    changeTimer(steamid, (time));
                    time--;
                    yield return new WaitForSeconds(1);
                }
                else
                {
                    Main.Instance.DownedPlayers.Remove(steamid);
                    EffectManager.askEffectClearByID(9770, steamid);
                    pl.Player.movement.pluginSpeedMultiplier = 1;
                    pl.Player.movement.pluginJumpMultiplier = 1;
                    pl.Suicide();

                    List<Player> players = new List<Player>();
                    PlayerTool.getPlayersInRadius(UnturnedPlayer.FromCSteamID(steamid).Position, 5, players);
                    foreach (var player in players)
                    {
                        var ePlayer = UnturnedPlayer.FromPlayer(player);
                        EffectManager.askEffectClearByID(9771, ePlayer.CSteamID);
                    }
                    break;
                }
            }
            if (Main.Instance.DownedPlayers.ContainsKey(steamid))
            {
                Main.Instance.DownedPlayers.Remove(steamid);
                EffectManager.askEffectClearByID(9770, steamid);
                pl.Player.movement.pluginSpeedMultiplier = 1;
                pl.Player.movement.pluginJumpMultiplier = 1;
                pl.Suicide();

                List<Player> players = new List<Player>();
                PlayerTool.getPlayersInRadius(UnturnedPlayer.FromCSteamID(steamid).Position, 5, players);
                foreach (var player in players)
                {
                    var ePlayer = UnturnedPlayer.FromPlayer(player);
                    EffectManager.askEffectClearByID(9771, ePlayer.CSteamID);
                }
            }
        }
    }
}
