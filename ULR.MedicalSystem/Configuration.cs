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
        public bool KillDownedPlayers;
        public byte DownedPlayerHealth;
        public float DownedPlayerMovementSpeed;
        public int DownedTimer;
        public int DownedInvicibilityTimer;
        public void LoadDefaults()
        {
            KillDownedPlayers = true;
            DownedPlayerHealth = 10;
            DownedPlayerMovementSpeed = 0;
            DownedTimer = 120;
            DownedInvicibilityTimer = 10;
        }
    }
}
