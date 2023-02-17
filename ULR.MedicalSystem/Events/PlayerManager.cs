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
            EffectManager.askEffectClearByID(9771, player.Player.channel.GetOwnerTransportConnection());
            foreach (var p in players)
            {
                var steam_id = UnturnedPlayer.FromPlayer(p).CSteamID;
                if (steam_id != player.CSteamID)
                {
                    if (Main.Instance.DownedPlayers.ContainsKey(steam_id))
                    {
                        EffectManager.sendUIEffect(9771, 9771, player.Player.channel.GetOwnerTransportConnection(), false, "Player in need", $"Use {(Assets.find(EAssetType.ITEM, Main.Instance.Configuration.Instance.ReviveItem) as ItemAsset)?.itemName} to revive {UnturnedPlayer.FromCSteamID(steam_id).DisplayName}.");

                        if (player.Stance == EPlayerStance.CROUCH && player.Player.animator.gesture == EPlayerGesture.SURRENDER_START)
                        {
                            UnturnedPlayer.FromCSteamID(steam_id).Teleport(player);
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
    }
}
