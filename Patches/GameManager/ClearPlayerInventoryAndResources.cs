using System.Collections.Generic;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.Stats;
using PerfectRandom.Sulfur.Core.UI;
using PerfectRandom.Sulfur.Core.Units;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.ClearPlayerInventoryAndResources))]
public class ClearPlayerInventoryAndResources
{
    public static bool Prefix(GameManager __instance)
    {
        if (!GlobalSettings.Debug.KeepInventoryOnDeath)
        {
            Plugin.Logger.LogInfo("Clearing player inventory");
            var gunId = Plugin.Client.StartingGun();
            var gunDefinition = gunId.GetAsset();
            var paperdoll = UIManager.Instance.Paperdoll;
            var weaponSlot = paperdoll.GetSlot(InventorySlot.Weapon0);
            weaponSlot.itemInSlot.DestroyFromInventory();
            UIManager.Instance.PlayerBackpackGrid.AddItem(gunDefinition, isPickup: true, announce: false);
            var gunItem = weaponSlot.itemInSlot;
            if (gunItem.CurrentCaliber != CaliberTypes._9mm)
                gunItem.ChangeWeaponCaliber(CaliberTypes._9mm);
            gunItem.ModifyDurability(gunItem.DurabilityMax);
            var gunData = new InventoryData(
                gunId,
                InventorySlot.Weapon0,
                1,
                gunItem.currentAmmo,
                gunItem.CurrentCaliber,
                gunItem.stats.SerializedAttributeData(),
                gunItem.GetSerializedAttachments(),
                gunItem.GetSerializedEnchantments(),
                gunItem.BoughtForPrice,
                gunItem.InventorySize.x,
                gunItem.InventorySize.y
            );

            var meleeId = Plugin.Client.StartingMelee();
            var meleeDefinition = meleeId.GetAsset();
            var meleeSlot = paperdoll.GetSlot(InventorySlot.BasicMelee);
            meleeSlot.itemInSlot.DestroyFromInventory();
            UIManager.Instance.PlayerBackpackGrid.AddItem(meleeDefinition, isPickup: true, announce: false);
            var meleeItem = meleeSlot.itemInSlot;
            var meleeData = new InventoryData(
                meleeId,
                InventorySlot.BasicMelee,
                1,
                meleeItem.currentAmmo,
                meleeItem.CurrentCaliber,
                meleeItem.stats.SerializedAttributeData(),
                meleeItem.GetSerializedAttachments(),
                meleeItem.GetSerializedEnchantments(),
                meleeItem.BoughtForPrice,
                meleeItem.InventorySize.x,
                meleeItem.InventorySize.y
            );
            
            SulfurSave.Imp.SaveToDisk_OnlyToCacheOnConsole("Inventory", new List<InventoryData>
            {
                gunData,
                meleeData
            });
            SulfurSave.Imp.SaveToDisk_OnlyToCacheOnConsole("WorldResources", new WorldResourceData());
            __instance.savedStats = null;
            __instance.savedBuffs = null;
        }
        return false;
    }
}