using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace ULR.MedicalSystem.Components
{
    public class DownedPlayerComonpent : UnturnedPlayerComponent
    {
        public EDeathCause newCause;
        public ELimb newLimb;
        public CSteamID killer;
    }
}
