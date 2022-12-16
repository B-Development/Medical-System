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
using ULR.MedicalSystem.Events;
using ULR.MedicalSystem.Patches;
using UnityEngine;

namespace ULR.MedicalSystem
{
    public class Main : RocketPlugin<Configuration>
    {
        public static Main Instance { get; set; }
        public Dictionary<CSteamID, bool> DownedPlayers = new Dictionary<CSteamID, bool>();
        public Dictionary<CSteamID, bool> DownedInvincivility = new Dictionary<CSteamID, bool>();
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
            EffectManager.onEffectButtonClicked += Manager.OnButtonClicked;
            UnturnedPlayerEvents.OnPlayerUpdatePosition += Manager.OnPlayerMoved;
            UnturnedPlayerEvents.OnPlayerDeath += Manager.OnPlayerDie;
        }

        protected override void Unload()
        {
            Instance = null;

            DownedPlayers = null;
            DownedInvincivility = null;
            RevivedPlayers = null;

            StopAllCoroutines();
            harmony.UnpatchAll("unturnedliferp.medicalsystem");

            U.Events.OnPlayerConnected -= Manager.OnPlayerJoined;
            U.Events.OnPlayerDisconnected -= Manager.OnPlayerLeave;

            ItemManager.onTakeItemRequested -= Manager.OnPickupItem;
            EffectManager.onEffectButtonClicked -= Manager.OnButtonClicked;
            UnturnedPlayerEvents.OnPlayerUpdatePosition -= Manager.OnPlayerMoved;
            UnturnedPlayerEvents.OnPlayerDeath -= Manager.OnPlayerDie;
        }
    }
}
