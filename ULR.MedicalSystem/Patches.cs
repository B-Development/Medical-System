using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULR.MedicalSystem
{
    [HarmonyPatch(typeof(PlayerLife), "ReceiveDeath")]
    public class PlayerPatches
    {
        internal static DeathHandler onDeath;
        [HarmonyPrefix]
        private static bool deathHandler(EDeathCause newCause, ELimb newLimb, CSteamID newKiller) => onDeath.Invoke(newCause, newLimb, newKiller);

        internal delegate bool DeathHandler(EDeathCause newCause, ELimb newLimb, CSteamID newKiller);
    }
}
