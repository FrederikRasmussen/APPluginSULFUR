using Archipelago.Helpers;
using HarmonyLib;
using PerfectRandom.Sulfur.Gameplay;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(ChurchCollectionLootable), nameof(ChurchCollectionLootable.SetVisualEffects))]
public class SetVisualEffects
{
    public static void Prefix(ref bool state)
    {
        if (Plugin.Client.State.ChurchCollectionHasSomething() && !ChurchCollectionState.IsLooting)
            state = true;
    }
}