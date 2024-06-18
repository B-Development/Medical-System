using HarmonyLib;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ULR.MedicalSystem.Components;
using UnityEngine;

namespace ULR.MedicalSystem.Patches
{
    [HarmonyPatch(typeof(PlayerLife))]
    [HarmonyPatch("doDamage")]
    class DoDamagePatch
    {
        public static bool Prefix(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, ref EPlayerKill kill, bool trackKill, ERagdollEffect newRagdollEffect, bool canCauseBleeding, PlayerLife instance)
        {
            var ply = instance.channel.owner.player;
            var uPlayer = UnturnedPlayer.FromPlayer(ply);
            if (Main.Instance.ByPassMedical.ContainsKey(uPlayer.CSteamID))
            {
                Main.Instance.ByPassMedical.Remove(uPlayer.CSteamID);
                return true;
            }

            if (Main.Instance.RevivedPlayers.ContainsKey(uPlayer.CSteamID))
            {
                Main.Instance.RevivedPlayers.Remove(uPlayer.CSteamID);
                return false;
            }
            if (ply is null) return false;

            if (Main.Instance.DownedPlayers.ContainsKey(newKiller))
            {
                UnturnedChat.Say(newKiller, $"You cannot damage {uPlayer.CharacterName} while downed.", Color.red);
                return false;
            }

            if (amount < ply.life.health) return true;
            

            if (!Main.Instance.DownedPlayers.ContainsKey(uPlayer.CSteamID))
            {
                Main.Instance.DownedPlayers.Add(uPlayer.CSteamID, true);
                Main.Instance.DownedInvincibility.Add(uPlayer.CSteamID, true);

                ply.equipment.dequip();
                ply.life.tellHealth(uPlayer.CSteamID, Main.Instance.Configuration.Instance.DownedPlayerHealth);

                uPlayer.Player.channel.send("tellStance", uPlayer.CSteamID, (ESteamPacket)15, (object)5);

                ply.movement.pluginSpeedMultiplier = Main.Instance.Configuration.Instance.DownedPlayerMovementSpeed;
                ply.movement.pluginJumpMultiplier = 0;

                Main.Instance.StartCoroutine(DownedInvincibility(uPlayer.CSteamID, Main.Instance.Configuration.Instance.DownedInvicibilityTimer));
                Main.Instance.StartCoroutine(DownTimer(uPlayer.CSteamID, Main.Instance.Configuration.Instance.DownedTimer));

                EffectManager.sendUIEffect(9770, 9770, uPlayer.Player.channel.GetOwnerTransportConnection(), false, "You are injured", string.Empty, "Respawn in: " + Main.Instance.Configuration.Instance.DownedTimer + "s", "Suicide");
                EffectManager.sendEffectClicked("Suicide_Button");

                var players = new List<Player>();
                PlayerTool.getPlayersInRadius(uPlayer.Position, 5, players);
                foreach (var p in players)
                {
                    var steamid = UnturnedPlayer.FromPlayer(p).CSteamID;
                    if (steamid != uPlayer.CSteamID)
                    {
                        EffectManager.sendUIEffect(9771, 9771, steamid, false, "Player in need", $"Use surrender to revive {uPlayer.DisplayName}");
                    }
                }
            }
            else
            {
                if (!Main.Instance.Configuration.Instance.KillDownedPlayers && Main.Instance.DownedPlayers.ContainsKey(uPlayer.CSteamID)) return false;

                Main.Instance.DownedPlayers.Remove(uPlayer.CSteamID);

                uPlayer.Player.movement.pluginSpeedMultiplier = 1;
                uPlayer.Player.movement.pluginJumpMultiplier = 1;

                var uPlayers = new List<Player>();
                PlayerTool.getPlayersInRadius(uPlayer.Position, 5, uPlayers);
                foreach (var player in uPlayers)
                {
                    var ePlayer = UnturnedPlayer.FromPlayer(player);
                    EffectManager.askEffectClearByID(9771, ePlayer.CSteamID);
                }
                return true;
            }

            return true;
        }


        public static IEnumerator DownedInvincibility(CSteamID steamID, int time)
        {
            var pl = UnturnedPlayer.FromCSteamID(steamID);

            while (time > 0)
            {
                time--;
                yield return new WaitForSeconds(1);
            }

            Main.Instance.DownedInvincibility.Remove(steamID);
        }

        public static void ChangeTimer(CSteamID steamID, int time)
        {
            EffectManager.askEffectClearByID(9770, steamID);
            EffectManager.sendUIEffect(9770, 9770, steamID, false, "You are injured", string.Empty, "Respawn in: " + time + "s", "Suicide");
            EffectManager.sendEffectClicked("Suicide_Button");
        }

        public static IEnumerator DownTimer(CSteamID steamID, int time)
        {
            var pl = UnturnedPlayer.FromCSteamID(steamID);

            while (time > 0)
            {
                if (Main.Instance.DownedPlayers.ContainsKey(steamID))
                {
                    ChangeTimer(steamID, (time));
                    time--;
                    yield return new WaitForSeconds(1);
                }
                else
                {
                    Main.Instance.DownedPlayers.Remove(steamID);
                    EffectManager.askEffectClearByID(9770, UnturnedPlayer.FromCSteamID(steamID).Player.channel.GetOwnerTransportConnection());
                    pl.Player.movement.pluginSpeedMultiplier = 1;
                    pl.Player.movement.pluginJumpMultiplier = 1;

                    var component = pl.GetComponent<DownedPlayerComonpent>();
                    DamageTool.damage(component.Player.Player, component.newCause, component.newLimb, component.killer, pl.Position, 1000, 1, out var kill, false, false);

                    Main.Instance.ByPassMedical.Add(pl.CSteamID, true);

                    var players = new List<Player>();
                    PlayerTool.getPlayersInRadius(UnturnedPlayer.FromCSteamID(steamID).Position, 5, players);
                    foreach (var player in players)
                    {
                        var ePlayer = UnturnedPlayer.FromPlayer(player);
                        EffectManager.askEffectClearByID(9771, ePlayer.Player.channel.GetOwnerTransportConnection());
                    }
                    break;
                }
            }

            if (!Main.Instance.DownedPlayers.ContainsKey(steamID)) yield break;
            {
                Main.Instance.DownedPlayers.Remove(steamID);
                EffectManager.askEffectClearByID(9770, UnturnedPlayer.FromCSteamID(steamID).Player.channel.GetOwnerTransportConnection());
                pl.Player.movement.pluginSpeedMultiplier = 1;
                pl.Player.movement.pluginJumpMultiplier = 1;
                pl.Suicide();

                var players = new List<Player>();
                PlayerTool.getPlayersInRadius(UnturnedPlayer.FromCSteamID(steamID).Position, 5, players);
                foreach (var ePlayer in players.Select(player => UnturnedPlayer.FromPlayer(player)))
                {
                    EffectManager.askEffectClearByID(9771, ePlayer.Player.channel.GetOwnerTransportConnection());
                }
            }
        }
    }
}
