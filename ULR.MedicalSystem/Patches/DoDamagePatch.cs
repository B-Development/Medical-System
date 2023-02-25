using System;
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
        public static bool Prefix(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, ref EPlayerKill kill, bool trackKill, ERagdollEffect newRagdollEffect, bool canCauseBleeding, PlayerLife __instance)
        {
            var ply = __instance.channel.owner.player;
            var unturned_player = UnturnedPlayer.FromPlayer(ply);

            if (ply is null) return false;

            if (Main.Instance.ByPassMedical.ContainsKey(unturned_player.CSteamID))
            {
                Main.Instance.ByPassMedical.Remove(unturned_player.CSteamID);
                return true;
            }

            if (Main.Instance.RevivedPlayers.ContainsKey(unturned_player.CSteamID))
            {
                Main.Instance.RevivedPlayers.Remove(unturned_player.CSteamID);
                return false;
            }

            if (Main.Instance.DownedPlayers.ContainsKey(newKiller))
            {
                UnturnedChat.Say(newKiller, $"You cannot damage {unturned_player.CharacterName} while downed.", Color.red);
                return false;
            }

            if (ply.life.health - amount > 0) return true;
            
            if (!Main.Instance.DownedPlayers.ContainsKey(unturned_player.CSteamID))
            {
                Main.Instance.DownedPlayers.Add(unturned_player.CSteamID, true);
                Main.Instance.DownedInvincibility.Add(unturned_player.CSteamID, true);

                ply.equipment.dequip();
                ply.life.tellHealth(unturned_player.CSteamID, Main.Instance.Configuration.Instance.DownedPlayerHealth);
                ply.life.tellBleeding(unturned_player.CSteamID, false);

                unturned_player.Player.channel.send("tellStance", unturned_player.CSteamID, (ESteamPacket)15, 5);

                ply.movement.pluginSpeedMultiplier = Main.Instance.Configuration.Instance.DownedPlayerMovementSpeed;
                ply.movement.pluginJumpMultiplier = 0;

                Main.Instance.StartCoroutine(DownedInvincibility(unturned_player.CSteamID, Main.Instance.Configuration.Instance.DownedInvicibilityTimer));
                Main.Instance.StartCoroutine(DownTimer(unturned_player.CSteamID, Main.Instance.Configuration.Instance.DownedTimer));

                EffectManager.sendUIEffect(9772, 9772, unturned_player.Player.channel.GetOwnerTransportConnection(), false, "You are injured", string.Empty, "Respawn in: " + Main.Instance.Configuration.Instance.DownedTimer + "s", "Suicide");

                var players = new List<Player>();
                PlayerTool.getPlayersInRadius(unturned_player.Position, 5, players);
                foreach (var steam_id in players.Select(p => UnturnedPlayer.FromPlayer(p).CSteamID).Where(steam_id => steam_id != unturned_player.CSteamID && !Main.Instance.DownedPlayers.ContainsKey(steam_id)))
                {
                    EffectManager.sendUIEffect(9771, 9771, steam_id, false, "Player in need", $"Use {(Assets.find(EAssetType.ITEM, Main.Instance.Configuration.Instance.ReviveItem) as ItemAsset)?.itemName} to revive {unturned_player.DisplayName}.");
                }
            }
            else
            {
                if (!Main.Instance.Configuration.Instance.KillDownedPlayers && Main.Instance.DownedPlayers.ContainsKey(unturned_player.CSteamID)) return false;

                Main.Instance.DownedPlayers.Remove(unturned_player.CSteamID);

                unturned_player.Player.movement.pluginSpeedMultiplier = 1;
                unturned_player.Player.movement.pluginJumpMultiplier = 1;

                var players = new List<Player>();
                PlayerTool.getPlayersInRadius(unturned_player.Position, 5, players);
                foreach (var e_player in players.Select(UnturnedPlayer.FromPlayer))
                {
                    EffectManager.askEffectClearByID(9771, e_player.CSteamID);
                }
                return true;
            }

            return true;
        }
        
        public static IEnumerator DownedInvincibility(CSteamID steam_id, int time)
        {
            while (time > 0)
            {
                time--;
                yield return new WaitForSeconds(1);
            }

            Main.Instance.DownedInvincibility.Remove(steam_id);
        }

        public static void ChangeTimer(CSteamID steam_id, int time)
        {
            EffectManager.askEffectClearByID(9772, steam_id);
            EffectManager.sendUIEffect(9772, 9772, steam_id, false, "You are injured", string.Empty, "Respawn in: " + time + "s", "Suicide");
        }

        public static IEnumerator DownTimer(CSteamID steam_id, int time)
        {
            var pl = UnturnedPlayer.FromCSteamID(steam_id);

            while (time > 0)
            {
                if (Main.Instance.DownedPlayers.ContainsKey(steam_id))
                {
                    ChangeTimer(steam_id, (time));
                    time--;
                    yield return new WaitForSeconds(1);
                }
                else
                {
                    Main.Instance.DownedPlayers.Remove(steam_id);
                    EffectManager.askEffectClearByID(9772, UnturnedPlayer.FromCSteamID(steam_id).Player.channel.GetOwnerTransportConnection());
                    pl.Player.movement.pluginSpeedMultiplier = 1;
                    pl.Player.movement.pluginJumpMultiplier = 1;

                    var component = pl.GetComponent<DownedPlayerComonpent>();
                    DamageTool.damage(component.Player.Player, component.newCause, component.newLimb, component.killer, pl.Position, 1000, 1, out EPlayerKill kill, false, false);

                    Main.Instance.ByPassMedical.Add(pl.CSteamID, true);

                    var players = new List<Player>();
                    PlayerTool.getPlayersInRadius(UnturnedPlayer.FromCSteamID(steam_id).Position, 5, players);
                    foreach (var player in players.Select(UnturnedPlayer.FromPlayer))
                    {
                        EffectManager.askEffectClearByID(9771, player.Player.channel.GetOwnerTransportConnection());
                    }
                    break;
                }
            }

            if (!Main.Instance.DownedPlayers.ContainsKey(steam_id)) yield break;
            {
                Main.Instance.DownedPlayers.Remove(steam_id);
                EffectManager.askEffectClearByID(9772, UnturnedPlayer.FromCSteamID(steam_id).Player.channel.GetOwnerTransportConnection());
                pl.Player.movement.pluginSpeedMultiplier = 1;
                pl.Player.movement.pluginJumpMultiplier = 1;

                var component = pl.GetComponent<DownedPlayerComonpent>();
                DamageTool.damage(component.Player.Player, component.newCause, component.newLimb, component.killer, pl.Position, 1000, 1, out _, false);

                var players = new List<Player>();
                PlayerTool.getPlayersInRadius(UnturnedPlayer.FromCSteamID(steam_id).Position, 5, players);
                foreach (var player in players.Select(UnturnedPlayer.FromPlayer))
                {
                    EffectManager.askEffectClearByID(9771, player.Player.channel.GetOwnerTransportConnection());
                }
            }
        }
    }
}
