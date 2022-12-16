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
        public static Main Instance { get; set; }
        public Dictionary<CSteamID, bool> DownedPlayers { get; set; }
        public Dictionary<CSteamID, bool> DownedInvincivility { get; set; }
        public Dictionary<CSteamID, bool> RevivedPlayers { get; set; }
        public EventManager Manager { get; set; }
        public Harmony harmony;

        protected override void Load()
        {
            Instance = this;

            harmony = new Harmony("unturnedliferp.medicalsystem");
            harmony.PatchAll();

            DownedPlayers = new Dictionary<CSteamID, bool>();
            DownedInvincivility = new Dictionary<CSteamID, bool>();
            RevivedPlayers = new Dictionary<CSteamID, bool>();

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
