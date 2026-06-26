using HarmonyLib;
using PerfectRandom.Sulfur.Gameplay;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(ChurchCollectionLootable), nameof(ChurchCollectionLootable.OnEnable))]
public class OnEnable
{
    public static void Postfix(ChurchCollectionLootable __instance)
    {
        ChurchCollectionState.IsEnabled = true;
        ChurchCollectionState.Instance = __instance;
    }
}