using Rocket.API;

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
        public byte RevivedPlayerHealth;
        public int ReviveTime;

        public ushort DefibID;
        public ushort DefibZapID;
        public ushort DefibChargeID;
        public string DefibPermission;
        public byte DefibedPlayerHealth;
        public int DefibTime;

        public void LoadDefaults()
        {
            DownedReviveCall = "medical.revivecall";

            KillDownedPlayers = true;
            DownedPlayerHealth = 10;
            DownedPlayerMovementSpeed = 0;
            DownedTimer = 120;
            DownedInvicibilityTimer = 10;

            RevivePermission = "medical.revive";
            RevivedPlayerHealth = 50;
            ReviveTime = 10;

            DefibID = 9800;
            DefibZapID = 9802;
            DefibChargeID = 9801;
            DefibPermission = "medical.defib";
            DefibedPlayerHealth = 75;
            DefibTime = 5;
        }
    }
}
