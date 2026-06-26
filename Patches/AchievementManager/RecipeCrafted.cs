using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.RecipeCrafted))]
public class RecipeCrafted
{
    public static void Postfix(RecipeData thisRecipe)
    {
        #pragma warning disable Harmony003
        Plugin.Logger.LogInfo($"Crafted recipe {thisRecipe.name}");
        var item = ArchipelagoItems.Get(thisRecipe.createsItem);
        #pragma warning restore Harmony003
        var locationName = $"Mix the magic '{item.Name}'";
        Plugin.Client.CompleteLocations(locationName);
    }
}