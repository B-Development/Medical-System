using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULR.MedicalSystem.Components
{
    public class DownedPlayerComonpent : UnturnedPlayerComponent
    {
        public EDeathCause newCause;
        public ELimb newLimb;
        public CSteamID killer;
    }
}
