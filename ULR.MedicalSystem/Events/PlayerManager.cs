using Rocket.Core.Utils;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using UnityEngine;

namespace ULR.MedicalSystem.Events
{
    public static class PlayerManager
    {
        public static void OnPlayerLeave(this EventManager manager, UnturnedPlayer player)
        {
            player.Player.equipment.onEquipRequested -= manager.OnEquipItem;

            if (!Main.Instance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                return;
            }
            
            Main.Instance.DownedPlayers.Remove(player.CSteamID);
            player.Player.life.tellHealth(player.CSteamID, 0);
        }

        public static void OnPlayerJoined(this EventManager manager, UnturnedPlayer player) => 
            player.Player.equipment.onEquipRequested += manager.OnEquipItem;

        public static void OnPlayerDie(this EventManager manager, UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (!manager.PluginInstance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                return;
            }
            
            player.Player.movement.pluginJumpMultiplier = 1;
            player.Player.movement.pluginSpeedMultiplier = 1;

            manager.PluginInstance.RevivedPlayers.Add(player.CSteamID, true);

            TaskDispatcher.QueueOnMainThread(() =>
            {
                RemoveRevive(player);
            }, 1.0f);
        }

        public static void OnPlayerMoved(this EventManager manager, UnturnedPlayer player, Vector3 position)
        {
            var players = new List<Player>();
            PlayerTool.getPlayersInRadius(player.Position, 5, players);
            EffectManager.askEffectClearByID(9771, player.Player.channel.GetOwnerTransportConnection());
            foreach (var p in players)
            {
                var steamID = UnturnedPlayer.FromPlayer(p).CSteamID;
                if (steamID == player.CSteamID)
                {
                    continue;
                }

                if (!Main.Instance.DownedPlayers.ContainsKey(steamID))
                {
                    continue;
                }
                EffectManager.sendUIEffect(9771, 9771, player.Player.channel.GetOwnerTransportConnection(), false, "Player in need", $"Use surrender to revive {UnturnedPlayer.FromPlayer(p).DisplayName}");

                if (player.Stance == EPlayerStance.CROUCH && player.Player.animator.gesture == EPlayerGesture.SURRENDER_START)
                {
                    UnturnedPlayer.FromCSteamID(steamID).Teleport(player);
                }
                if (player.Player.animator.gesture == EPlayerGesture.SURRENDER_START)
                {
                    Main.Instance.StartCoroutine(ReviveTime(UnturnedPlayer.FromPlayer(p), player, false, Main.Instance.Configuration.Instance.ReviveTime));
                }
                if (player.Player.equipment.itemID == Main.Instance.Configuration.Instance.DefibID)
                {
                    EffectManager.sendEffect(Main.Instance.Configuration.Instance.DefibChargeID, 3, player.Position);
                    Main.Instance.StartCoroutine(ReviveTime(UnturnedPlayer.FromPlayer(p), player, true, Main.Instance.Configuration.Instance.DefibTime));
                }
            }
        }

        private static void RemoveRevive(UnturnedPlayer target)
        {
            if (target != null)
            {
                Main.Instance.RevivedPlayers.Remove(target.CSteamID);
            }
        }

        private static IEnumerator ReviveTime(UnturnedPlayer target, UnturnedPlayer reviver, bool defib, int time)
        {
            while (time > 0)
            {
                time--;
                yield return new WaitForSeconds(1);
            }

            if (!Main.Instance.DownedPlayers.ContainsKey(target.CSteamID)) yield break;

            if (defib && reviver.Player.equipment.itemID == Main.Instance.Configuration.Instance.DefibID && reviver.HasPermission(Main.Instance.Configuration.Instance.DefibPermission))
            {
                EffectManager.sendEffect(Main.Instance.Configuration.Instance.DefibZapID, 3, target.Position);
                target.Player.movement.pluginSpeedMultiplier = 1;
                target.Player.movement.pluginJumpMultiplier = 1;

                Main.Instance.RevivedPlayers.Add(target.CSteamID, true);
                Main.Instance.DownedPlayers.Remove(target.CSteamID);
                target.Player.life.tellHealth(target.CSteamID, Main.Instance.Configuration.Instance.DefibedPlayerHealth);

                var players = new List<Player>();
                PlayerTool.getPlayersInRadius(target.Position, 5, players);

                foreach (var ePlayer in players.Select(UnturnedPlayer.FromPlayer))
                {
                    EffectManager.askEffectClearByID(9771, ePlayer.Player.channel.GetOwnerTransportConnection());
                }
            }

            if (reviver.Player.animator.gesture != EPlayerGesture.SURRENDER_START ||
                reviver.Stance != EPlayerStance.CROUCH || !reviver.HasPermission(Main.Instance.Configuration.Instance.RevivePermission)) yield break;
            {
                target.Player.movement.pluginSpeedMultiplier = 1;
                target.Player.movement.pluginJumpMultiplier = 1;

                Main.Instance.RevivedPlayers.Add(target.CSteamID, true);
                Main.Instance.DownedPlayers.Remove(target.CSteamID);
                target.Player.life.tellHealth(target.CSteamID, Main.Instance.Configuration.Instance.RevivedPlayerHealth);

                var players = new List<Player>();
                PlayerTool.getPlayersInRadius(target.Position, 5, players);
                foreach (var ePlayer in players.Select(UnturnedPlayer.FromPlayer))
                {
                    EffectManager.askEffectClearByID(9771, ePlayer.Player.channel.GetOwnerTransportConnection());
                }
            }
        }
    }
}
