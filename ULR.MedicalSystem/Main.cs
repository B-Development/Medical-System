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

namespace ULR.MedicalSystem
{
    public class Main : RocketPlugin
    {
        public static Main Instance { get; set; }
        public Dictionary<CSteamID, bool> DownedPlayers { get; set; }
        public EventManager Manager { get; set; }

        protected override void Load()
        {
            Instance = this;

            DownedPlayers = new Dictionary<CSteamID, bool>();
            Manager = new EventManager(this);

            U.Events.OnPlayerConnected += Manager.OnPlayerJoined;
            U.Events.OnPlayerDisconnected += Manager.OnPlayerLeave;
            PlayerPatches.onDeath += OnPlayerDeath;
        }

        private bool OnPlayerDeath(EDeathCause newCause, ELimb newLimb, CSteamID newKiller)
        {
            return false;
        }

        protected override void Unload()
        {
            Instance = null;

            DownedPlayers = null;

            U.Events.OnPlayerConnected -= Manager.OnPlayerJoined;
            U.Events.OnPlayerDisconnected -= Manager.OnPlayerLeave;
            PlayerPatches.onDeath -= OnPlayerDeath;
        }
    }
}
