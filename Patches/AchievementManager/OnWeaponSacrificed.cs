using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.OnWeaponSacrificed))]
public class OnWeaponSacrificed
{
    public static void Postfix(ItemId weaponId)
    {
        #pragma warning disable Harmony003
        Plugin.Logger.LogInfo($"Sacrificed weapon {weaponId.GetAsset().name}");
        #pragma warning restore Harmony003
        var item = ArchipelagoItems.Get(weaponId);
        var locationName = $"Contribute {item.Name} to the cause";
        Plugin.Client.CompleteLocations(locationName);
    }
}