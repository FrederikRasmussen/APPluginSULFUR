using HarmonyLib;
using PerfectRandom.Sulfur.Core.Items;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(LootManager), nameof(LootManager.GetChurchCollectionAmount))]
public class GetChurchCollectionAmount
{
    public static void Postfix(ref int __result)
    {
        __result += Plugin.Client.State.UnlockedSulf;
    }
}