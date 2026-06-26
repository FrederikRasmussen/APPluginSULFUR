using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.UI.Inventory;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(StampPurchaseListItem), nameof(StampPurchaseListItem.DoPurchase))]
public class DoPurchase
{
    public static bool Prefix(StampPurchaseListItem __instance)
    {
        if (__instance.Item.isEndlessUpgrade)
            return true;
        GameManager.Instance.PlayerUnit.Stats.ModifyStamps(-__instance.PurchasePrice, __instance);
        var locationName = $"Trade stamps for the {ArchipelagoItems.Get(__instance.Item.id).Name}";
        Plugin.Client.CompleteLocations(locationName);
        __instance.priceButton.transform.parent.gameObject.SetActive(false);
        __instance.checkmark.SetActive(true);
        __instance.canvasGroup.alpha = (__instance.purchasedAlpha);
        __instance.UpdateBackgroundColor();
        __instance.UpdateStampRotation();
        __instance.purchaseSound?.Play(__instance.transform);
        return false;
    }
}