using HarmonyLib;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.Unturned.Chat;
using ULR.MedicalSystem.Events;
using ULR.MedicalSystem.Patches;
using UnityEngine;

namespace ULR.MedicalSystem
{
    public class Main : RocketPlugin<Configuration>
    {
        public static Main Instance { get; set; }
        public Dictionary<CSteamID, bool> DownedPlayers = new Dictionary<CSteamID, bool>();
        public Dictionary<CSteamID, bool> DownedInvincibility = new Dictionary<CSteamID, bool>();
        public Dictionary<CSteamID, bool> RevivedPlayers = new Dictionary<CSteamID, bool>();
        public Dictionary<CSteamID, bool> ByPassMedical = new Dictionary<CSteamID, bool>();
        public EventManager Manager { get; set; }
        public Harmony harmony;

        protected override void Load()
        {
            Instance = this;

            harmony = new Harmony("unturnedliferp.medicalsystem");
            harmony.PatchAll();

            Manager = new EventManager(this);

            U.Events.OnPlayerConnected += Manager.OnPlayerJoined;
            U.Events.OnPlayerDisconnected += Manager.OnPlayerLeave;

            ItemManager.onTakeItemRequested += Manager.OnPickupItem;
            UnturnedPlayerEvents.OnPlayerUpdatePosition += Manager.OnPlayerMoved;
            UnturnedPlayerEvents.OnPlayerDeath += Manager.OnPlayerDie;

            UseableConsumeable.onPerformingAid += OnAided;
        }

        private void OnAided(Player instigator, Player target, ItemConsumeableAsset asset, ref bool shouldallow)
        {
            var unturned_target = UnturnedPlayer.FromPlayer(target);
            var unturned_instigator = UnturnedPlayer.FromPlayer(instigator);

            if (unturned_instigator.GetPermissions().Select(perm => perm.Name)
                .Contains(Instance.Configuration.Instance.RevivePermission))
            {
                if (asset.id == Instance.Configuration.Instance.ReviveItem)
                {
                    if (DownedPlayers.ContainsKey(unturned_instigator.CSteamID))
                    {
                        UnturnedChat.Say(unturned_instigator, $"You cannot");
                        shouldallow = false;
                        return;
                    }

                    if (DownedPlayers.ContainsKey(unturned_target.CSteamID))
                    {
                        unturned_target.Player.movement.pluginSpeedMultiplier = 1;
                        unturned_target.Player.movement.pluginJumpMultiplier = 1;
                        
                        Instance.RevivedPlayers.Add(unturned_target.CSteamID, true);
                        Instance.DownedPlayers.Remove(unturned_target.CSteamID);

                        unturned_target.Player.life.tellHealth(unturned_target.CSteamID, Instance.Configuration.Instance.RevivedPlayerHealth);
                        
                        var players = new List<Player>();
                        PlayerTool.getPlayersInRadius(unturned_target.Position, 5, players);

                        foreach (var player in players.Select(UnturnedPlayer.FromPlayer))
                        {
                            EffectManager.askEffectClearByID(9771, player.Player.channel.GetOwnerTransportConnection());
                        }
                    }
                }
            }
        }

        protected override void Unload()
        {
            Instance = null;

            DownedPlayers = null;
            DownedInvincibility = null;
            RevivedPlayers = null;

            StopAllCoroutines();
            harmony.UnpatchAll("unturnedliferp.medicalsystem");

            U.Events.OnPlayerConnected -= Manager.OnPlayerJoined;
            U.Events.OnPlayerDisconnected -= Manager.OnPlayerLeave;

            ItemManager.onTakeItemRequested -= Manager.OnPickupItem;
            UnturnedPlayerEvents.OnPlayerUpdatePosition -= Manager.OnPlayerMoved;
            UnturnedPlayerEvents.OnPlayerDeath -= Manager.OnPlayerDie;

            UseableConsumeable.onPerformingAid -= OnAided;
        }
    }
}
