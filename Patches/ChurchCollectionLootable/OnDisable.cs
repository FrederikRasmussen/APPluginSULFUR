using HarmonyLib;
using PerfectRandom.Sulfur.Gameplay;

namespace Archipelago.Patches;
[HarmonyPatch(typeof(ChurchCollectionLootable), nameof(ChurchCollectionLootable.OnDisable))]
public class OnDisable
{
    public static void Postfix()
    {
        ChurchCollectionState.IsEnabled = false;
        ChurchCollectionState.Instance = null;
    }
}