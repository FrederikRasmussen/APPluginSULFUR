using System.Collections.Generic;
using Archipelago.Helpers;

namespace Archipelago.Archipelago;

public class GameState
{
    public readonly SulfurSave_Threadsafe SulfurSave;
    public int Index { get; private set; }
    public HashSet<long> AllCompletedLocations { get; }
    public HashSet<long> PrintableWeaponIds { get; }

    public List<long> ScrollUnlocks { get; }

    public HashSet<long> CheckpointUnlocks { get; }

    public int UnlockedSulf { get; private set; }

    public Queue<long> WaitingItems { get; }

    public GameState(bool newFileRequired)
    {
        SulfurSave = new SulfurSave_Threadsafe(newFileRequired);
        var coroutine = SulfurSave.Setup();
        while (coroutine.MoveNext()) ;
        Index = SulfurSave.Load(SaveKeys.Index, 0);
        AllCompletedLocations = SulfurSave.Load(SaveKeys.AllCompletedLocations, new HashSet<long>());
        PrintableWeaponIds = SulfurSave.Load(SaveKeys.PrintableWeaponIds, new HashSet<long>());
        ScrollUnlocks = SulfurSave.Load(SaveKeys.ScrollUnlocks, new List<long>());
        CheckpointUnlocks = SulfurSave.Load(SaveKeys.CheckpointUnlocks, new HashSet<long>());
        UnlockedSulf = SulfurSave.Load(SaveKeys.UnlockedSulf, 0);
        WaitingItems = SulfurSave.Load(SaveKeys.ChurchCollectionItems, new Queue<long>());
    }
    
    public void IncrementIndex()
    {
        Index++;
        SulfurSave.SaveToDisk(SaveKeys.Index, Index);
    }

    public void CompleteLocations(params ICollection<long> locations)
    {
        Plugin.Logger.LogInfo($"{AllCompletedLocations.Count} union with {locations.Count}");
        AllCompletedLocations.UnionWith(locations);
        Plugin.Logger.LogInfo($"{AllCompletedLocations.Count} is the new count");
        SulfurSave.SaveToDisk(SaveKeys.AllCompletedLocations, AllCompletedLocations);
    }

    public void AddPrintableWeapon(long id)
    {
        PrintableWeaponIds.Add(id);
        SulfurSave.SaveToDisk(SaveKeys.PrintableWeaponIds, PrintableWeaponIds);
    }
    
    public void AddScrollUnlock(long id)
    {
        ScrollUnlocks.Add(id);
        SulfurSave.SaveToDisk(SaveKeys.ScrollUnlocks, ScrollUnlocks);
    }

    public void AddCheckpoint(long id)
    {
        CheckpointUnlocks.Add(id);
        SulfurSave.SaveToDisk(SaveKeys.CheckpointUnlocks, CheckpointUnlocks);
    }

    public void AddSulf(int amount)
    {
        UnlockedSulf += amount;
        SulfurSave.SaveToDisk(SaveKeys.UnlockedSulf, UnlockedSulf);
    }
    
    public void ResetSulf()
    {
        UnlockedSulf = 0;
        SulfurSave.SaveToDisk(SaveKeys.UnlockedSulf, UnlockedSulf);
    }
    
    public bool ChurchCollectionHasSomething()
    {
        return UnlockedSulf > 0 || WaitingItems.Count > 0;
    }

    public void AddWaitingItem(long id)
    {
        WaitingItems.Enqueue(id);
        SulfurSave.SaveToDisk(SaveKeys.ChurchCollectionItems, WaitingItems);
    }
    
    public void SpawnedAllWaitingItems()
    {
        SulfurSave.SaveToDisk(SaveKeys.ChurchCollectionItems, WaitingItems);
        UnlockedSulf = 0;
        SulfurSave.SaveToDisk(SaveKeys.UnlockedSulf, UnlockedSulf);
    }
        
    private static class SaveKeys
    {
        private const string Prefix = "Archipelago_";
        internal const string Index = $"{Prefix}Index";
        internal const string AllCompletedLocations = $"{Prefix}AllCompletedLocations";
        internal const string PrintableWeaponIds = $"{Prefix}PrintableWeaponIds";
        internal const string ScrollUnlocks = $"{Prefix}ScrollUnlocks";
        internal const string CheckpointUnlocks = $"{Prefix}CheckpointUnlocks";
        internal const string UnlockedSulf = $"{Prefix}UnlockedSulf";
        internal const string ChurchCollectionItems = $"{Prefix}ChurchCollectionItems";
    }
}