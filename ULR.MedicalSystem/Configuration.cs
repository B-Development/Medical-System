using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULR.MedicalSystem
{
    public class Configuration : IRocketPluginConfiguration
    {
        public string DownedReviveCall;

        public bool KillDownedPlayers;
        public byte DownedPlayerHealth;
        public float DownedPlayerMovementSpeed;
        public int DownedTimer;
        public int DownedInvicibilityTimer;

        public string RevivePermission;
        public ushort ReviveItem;
        public byte RevivedPlayerHealth;

        public void LoadDefaults()
        {
            DownedReviveCall = "medical.revivecall";

            KillDownedPlayers = true;
            DownedPlayerHealth = 10;
            DownedPlayerMovementSpeed = 0;
            DownedTimer = 120;
            DownedInvicibilityTimer = 10;

            RevivePermission = "medical.revive";
            ReviveItem = 387;
            RevivedPlayerHealth = 50;
        }
    }
}
