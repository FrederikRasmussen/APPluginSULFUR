using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.UI;

namespace Archipelago.Patches;

[HarmonyPatch]
public class AddItem
{
    private static bool HasReplacedOnboardingMelee = false;
    private static bool HasReplaceOnboardingGun = false;
    private static void PickedUp(ItemId id)
    {
        if (GameManager.Instance.currentEnvironment.id is WorldEnvironmentIds.Onboarding
            or WorldEnvironmentIds.Onboarding_LoopAround)
        {
            var paperdoll = UIManager.Instance.Paperdoll;
            #pragma warning disable Harmony003
            if (!HasReplaceOnboardingGun && id.GetAsset().slotType == SlotType.Weapon)
            #pragma warning restore Harmony003
            {
                HasReplaceOnboardingGun = true;
                var gunId = Plugin.Client.StartingGun();
                var gunDefinition = gunId.GetAsset();
                var weaponSlot = paperdoll.GetSlot(InventorySlot.Weapon0);
                weaponSlot.itemInSlot.DestroyFromInventory();
                UIManager.Instance.PlayerBackpackGrid.AddItem(gunDefinition, isPickup: true, announce: false);
                var gunItem = weaponSlot.itemInSlot;
                if (gunItem.CurrentCaliber != CaliberTypes._9mm)
                    gunItem.ChangeWeaponCaliber(CaliberTypes._9mm);
                gunItem.ModifyDurability(gunItem.DurabilityMax);
                return;
            }
            
            #pragma warning disable Harmony003
            if (!HasReplacedOnboardingMelee && id.GetAsset().slotType == SlotType.BasicMelee)
            #pragma warning restore Harmony003
            {
                HasReplacedOnboardingMelee = true;
                var meleeId = Plugin.Client.StartingMelee();
                var meleeDefinition = meleeId.GetAsset();
                var meleeSlot = paperdoll.GetSlot(InventorySlot.BasicMelee);
                meleeSlot.itemInSlot.DestroyFromInventory();
                UIManager.Instance.PlayerBackpackGrid.AddItem(meleeDefinition, isPickup: true, announce: false);
                return;
            }
        }
        Plugin.Logger.LogInfo($"Found {id}");
        if (!ArchipelagoItems.Has(id)) return;
        if (id == ItemIds.Item_Suitcase)
            Plugin.Client.CompleteLocations("Find your Suitcase");
        else if (id == ItemIds.Item_Refrigerator)
            Plugin.Client.CompleteLocations("Find a Refrigerator");
        else if (id == ItemIds.Item_ChestOfDrawers)
            Plugin.Client.CompleteLocations("Find the Chest of Drawers");
        else
        {
            var item = ArchipelagoItems.Get(id);
            var locationName = $"Find {item.Name}";
            Plugin.Client.CompleteLocations(locationName);
        }
    }
    
    [HarmonyPatch(typeof(ItemGrid), nameof(ItemGrid.AddItem), typeof(ItemDefinition), typeof(bool), typeof(bool), typeof(InventoryData))]    
    public static void Postfix(ItemDefinition itemSO, ItemGrid __instance)
    {
        if (!__instance.IsPlayerInventory) return;
        PickedUp(itemSO.id);
    }
    
    [HarmonyPatch(typeof(ItemGrid), nameof(ItemGrid.AddItem), typeof(InventoryData))]    
    public static void Postfix(InventoryData data, ItemGrid __instance)
    {
        if (!__instance.IsPlayerInventory) return;
        PickedUp(data.id);
    }
}