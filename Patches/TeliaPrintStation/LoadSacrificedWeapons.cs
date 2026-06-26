using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core.World;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(TeliaPrintStation), nameof(TeliaPrintStation.LoadSacrificedWeapons))]
public class LoadSacrificedWeapons
{
    public static void Postfix(TeliaPrintStation __instance)
    {
        foreach (var id in Plugin.Client.State.PrintableWeaponIds)
        {
            var weapon = ArchipelagoItems.Get(id).ItemId();
            __instance.sacrificedWeapons.Remove(weapon);
            __instance.sacrificedWeapons.Add(weapon);
            __instance.timesPrintedRegister[weapon] = 0;
        }
    }
}