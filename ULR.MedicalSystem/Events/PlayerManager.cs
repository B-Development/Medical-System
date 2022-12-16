using Rocket.API;
using Rocket.Core.Utils;
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
using static UnityEngine.GraphicsBuffer;

namespace ULR.MedicalSystem.Events
{
    public static class PlayerManager
    {
        public static void OnPlayerLeave(this EventManager manager, UnturnedPlayer player)
        {
            player.Player.equipment.onEquipRequested -= manager.OnEquipItem;

            if (Main.Instance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                Main.Instance.DownedPlayers.Remove(player.CSteamID);
                player.Player.life.tellHealth(player.CSteamID, 0);
            }
        }

        public static void OnPlayerJoined(this EventManager manager, UnturnedPlayer player)
        {
            player.Player.equipment.onEquipRequested += manager.OnEquipItem;
        }

        public static void OnPlayerDie(this EventManager manager, UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (manager.pluginInstance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                player.Player.movement.pluginJumpMultiplier = 1;
                player.Player.movement.pluginSpeedMultiplier = 1;

                manager.pluginInstance.RevivedPlayers.Add(player.CSteamID, true);

                TaskDispatcher.QueueOnMainThread(() =>
                {
                    RemoveRevive(player);
                }, 1.0f);
            }
        }

        public static void OnPlayerMoved(this EventManager manager, UnturnedPlayer player, Vector3 position)
        {
            List<Player> players = new List<Player>();
            PlayerTool.getPlayersInRadius(player.Position, 5, players);
            EffectManager.askEffectClearByID(9771, player.CSteamID);
            foreach (var p in players)
            {
                var steamid = UnturnedPlayer.FromPlayer(p).CSteamID;
                if (steamid != player.CSteamID)
                {
                    if (Main.Instance.DownedPlayers.ContainsKey(steamid))
                    {
                        EffectManager.sendUIEffect(9771, 9771, player.CSteamID, false, "Player in need", $"Use surrender to revive {UnturnedPlayer.FromPlayer(p).DisplayName}");

                        if (player.Stance == EPlayerStance.CROUCH && player.Player.animator.gesture == EPlayerGesture.SURRENDER_START)
                        {
                            UnturnedPlayer.FromCSteamID(steamid).Teleport(player);
                        }
                        if (player.Player.animator.gesture == EPlayerGesture.SURRENDER_START)
                        {
                            Main.Instance.StartCoroutine(ReviveTime(UnturnedPlayer.FromPlayer(p), player, false, Main.Instance.Configuration.Instance.ReviveTime));
                        }
                        else if (player.Player.equipment.itemID == Main.Instance.Configuration.Instance.DefibID)
                        {
                            EffectManager.sendEffect(Main.Instance.Configuration.Instance.DefibChargeID, 3, player.Position);
                            Main.Instance.StartCoroutine(ReviveTime(UnturnedPlayer.FromPlayer(p), player, true, Main.Instance.Configuration.Instance.DefibTime));
                        }
                    }
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

        public static IEnumerator ReviveTime(UnturnedPlayer target, UnturnedPlayer reviver, bool defib, int time)
        {
            while (time > 0)
            {
                time--;
                yield return new WaitForSeconds(1);
            }

            if (Main.Instance.DownedPlayers.ContainsKey(target.CSteamID))
            {
                if (defib && reviver.Player.equipment.itemID == Main.Instance.Configuration.Instance.DefibID)
                {
                    EffectManager.sendEffect(Main.Instance.Configuration.Instance.DefibZapID, 3, target.Position);
                    target.Player.movement.pluginSpeedMultiplier = 1;
                    target.Player.movement.pluginJumpMultiplier = 1;

                    Main.Instance.DownedPlayers.Remove(target.CSteamID);
                    Main.Instance.RevivedPlayers.Add(target.CSteamID, true);
                    target.Player.life.tellHealth(target.CSteamID, Main.Instance.Configuration.Instance.DefibedPlayerHealth);

                    List<Player> players = new List<Player>();
                    PlayerTool.getPlayersInRadius(target.Position, 5, players);
                    foreach (var player in players)
                    {
                        var ePlayer = UnturnedPlayer.FromPlayer(player);
                        EffectManager.askEffectClearByID(9771, ePlayer.CSteamID);
                    }
                    PayReviver(reviver, target);
                }
                if (reviver.Player.animator.gesture == EPlayerGesture.SURRENDER_START && reviver.Stance != EPlayerStance.CROUCH)
                {
                    target.Player.movement.pluginSpeedMultiplier = 1;
                    target.Player.movement.pluginJumpMultiplier = 1;

                    Main.Instance.DownedPlayers.Remove(target.CSteamID);
                    Main.Instance.RevivedPlayers.Add(target.CSteamID, true);
                    target.Player.life.tellHealth(target.CSteamID, Main.Instance.Configuration.Instance.RevivedPlayerHealth);

                    List<Player> players = new List<Player>();
                    PlayerTool.getPlayersInRadius(target.Position, 5, players);
                    foreach (var player in players)
                    {
                        var ePlayer = UnturnedPlayer.FromPlayer(player);
                        EffectManager.askEffectClearByID(9771, ePlayer.CSteamID);
                    }
                    PayReviver(reviver, target);
                }
            }
        }

        private static void PayReviver(UnturnedPlayer reviver, UnturnedPlayer target)
        {
            if (reviver.HasPermission(Main.Instance.Configuration.Instance.RewardPerm))
            {
                var amountToBeRewarded = Main.Instance.Configuration.Instance.RewardAmount;

                if (Main.Instance.Configuration.Instance.TakeFromPlayer)
                {
                    if (target.Experience <= amountToBeRewarded)
                    {
                        amountToBeRewarded = target.Experience;
                        target.Experience = 0;
                    }
                    else
                    {
                        target.Experience -= amountToBeRewarded;
                    }
                }

                reviver.Experience += amountToBeRewarded;
            }
        }
    }
}
