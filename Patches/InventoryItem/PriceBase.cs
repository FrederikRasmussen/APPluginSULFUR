using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(InventoryItem), nameof(InventoryItem.PriceBase))]
[HarmonyPatch(MethodType.Getter)]
public class PriceBase
{
    public static bool Prefix(ref int __result, InventoryItem __instance)
    {
        if (__instance.SlotType is SlotType.Weapon or SlotType.BasicMelee)
        {
            __result = 0;
            return false;
        }

        return true;
    }
}