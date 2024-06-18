using HarmonyLib;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using ULR.MedicalSystem.Events;

namespace ULR.MedicalSystem
{
    public class Main : RocketPlugin<Configuration>
    {
        public static Main Instance { get; private set; }
        public Dictionary<CSteamID, bool> DownedPlayers = new Dictionary<CSteamID, bool>();
        public Dictionary<CSteamID, bool> DownedInvincibility = new Dictionary<CSteamID, bool>();
        public Dictionary<CSteamID, bool> RevivedPlayers = new Dictionary<CSteamID, bool>();
        public readonly Dictionary<CSteamID, bool> ByPassMedical = new Dictionary<CSteamID, bool>();
        public EventManager Manager { get; set; }
        private Harmony _harmony;

        protected override void Load()
        {
            Instance = this;

            _harmony = new Harmony("unturnedliferp.medicalsystem");
            _harmony.PatchAll();

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
            DownedInvincibility = null;
            RevivedPlayers = null;

            StopAllCoroutines();
            _harmony.UnpatchAll("unturnedliferp.medicalsystem");

            U.Events.OnPlayerConnected -= Manager.OnPlayerJoined;
            U.Events.OnPlayerDisconnected -= Manager.OnPlayerLeave;

            ItemManager.onTakeItemRequested -= Manager.OnPickupItem;
            EffectManager.onEffectButtonClicked -= Manager.OnButtonClicked;
            UnturnedPlayerEvents.OnPlayerUpdatePosition -= Manager.OnPlayerMoved;
            UnturnedPlayerEvents.OnPlayerDeath -= Manager.OnPlayerDie;

        }
    }
}
