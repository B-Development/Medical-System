using HarmonyLib;
using Rocket.Core.Plugins;
using Rocket.Unturned;
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

namespace ULR.MedicalSystem
{
    public class Main : RocketPlugin<Configuration>
    {
        public static Main Instance { get; set; }
        public Dictionary<CSteamID, bool> DownedPlayers { get; set; }
        public Dictionary<CSteamID, bool> DownedInvincivility { get; set; }
        public EventManager Manager { get; set; }
        public Harmony harmony;

        protected override void Load()
        {
            Instance = this;

            harmony = new Harmony("unturnedliferp.medicalsystem");
            harmony.PatchAll();

            DownedPlayers = new Dictionary<CSteamID, bool>();
            DownedInvincivility = new Dictionary<CSteamID, bool>();
            Manager = new EventManager(this);

            U.Events.OnPlayerConnected += Manager.OnPlayerJoined;
            U.Events.OnPlayerDisconnected += Manager.OnPlayerLeave;

            ItemManager.onTakeItemRequested += Manager.OnPickupItem;
            EffectManager.onEffectButtonClicked += Manager.OnButtonClicked;
        }

        protected override void Unload()
        {
            Instance = null;

            DownedPlayers = null;
            DownedInvincivility = null;

            StopAllCoroutines();
            harmony.UnpatchAll("unturnedliferp.medicalsystem");

            U.Events.OnPlayerConnected -= Manager.OnPlayerJoined;
            U.Events.OnPlayerDisconnected -= Manager.OnPlayerLeave;

            ItemManager.onTakeItemRequested -= Manager.OnPickupItem;
            EffectManager.onEffectButtonClicked -= Manager.OnButtonClicked;
        }
    }
}
