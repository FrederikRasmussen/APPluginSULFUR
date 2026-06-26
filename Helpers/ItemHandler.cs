using System;
using System.Collections.Generic;
using Archipelago.Archipelago;
using Archipelago.Patches;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.DataStorage;

namespace Archipelago.Helpers;

public static class ItemHandler
{
    public static void HandleSulf(ArchipelagoItems.Item item, int amount)
    {
        Plugin.Logger.LogInfo($"Handling {item.Name} as {amount} of Sulf");
        Plugin.Client.State.AddSulf(amount);
        if (ChurchCollectionState.IsEnabled)
            ChurchCollectionState.Instance.SetVisualEffects(true);
    }
    
    public static void HandleTeliaUnlock(ArchipelagoItems.Item item)
    {
        Plugin.Logger.LogInfo($"Handling {item.Name} as Telia unlock of {item.ItemId().GetAsset().GetName()}");
        HandleSulf(item, item.ItemId().GetAsset().basePrice);
        Plugin.Client.State.AddPrintableWeapon(item.Id);
    }

    public static void UnlockCheckpoint(ArchipelagoItems.Item item)
    {
        Plugin.Logger.LogInfo($"Handling {item.Name} as Checkpoint {item.Checkpoint()}");
        Plugin.Client.State.AddCheckpoint(item.Id);
        PlayerProgress.SetCheckpointReached(item.Checkpoint(), true);
    }

    public static void HandleScholarUnlock(ArchipelagoItems.Item item)
    {
        Plugin.Logger.LogInfo($"Handling {item.Name} as Scroll Unlock {item.ItemId().GetAsset().GetName()}");
        Plugin.Client.State.AddScrollUnlock(item.Id);
    }
    
    public static void HandleActualItem(ArchipelagoItems.Item item)
    {
        Plugin.Logger.LogInfo($"Handling {item.Name} as Actual Item {item.ItemId().GetAsset().GetName()}");
        Plugin.Client.State.AddWaitingItem(item.Id);
        if (ChurchCollectionState.IsEnabled)
            ChurchCollectionState.Instance.SetVisualEffects(true);
    }
}