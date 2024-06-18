using Rocket.Unturned.Player;
using SDG.Unturned;

namespace ULR.MedicalSystem.Events
{
    public static class ItemEvents
    {
        public static void OnEquipItem(this EventManager manager, PlayerEquipment equipment, ItemJar jar, ItemAsset asset, ref bool shouldAllow)
        {
            var player = UnturnedPlayer.FromPlayer(equipment.player);

            if (manager.PluginInstance.DownedPlayers.ContainsKey(player.CSteamID))
            {
                shouldAllow = false;
            }
        }

        public static void OnPickupItem(this EventManager manager, Player player, byte x, byte y, uint instanceID, byte toX, byte toY, byte toRot, byte toPage, ItemData itemData, ref bool shouldAllow)
        {
            var uplayer = UnturnedPlayer.FromPlayer(player);

            if (Main.Instance.DownedPlayers.ContainsKey(uplayer.CSteamID))
            {
                shouldAllow = false;
            }
        }
    }
}
