using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using BepInEx;
using Pathfinding.Graphs.Grid.Rules;
using PerfectRandom.Sulfur.Core;

namespace Archipelago.Archipelago;

public class Client
{
    public ArchipelagoSession Session { get; private set; }
    public static Config Config { get; private set; }
    public string Hostname { get; private set; }
    public int Port { get; private set; }
    public string Slot { get; private set; }
    public string Password { get; private set; }

    private string RecentConnectionErrorMessage { get; set; } = "";

    public GameState State { get; private set; }
    public bool NewFileRequired { get; set; } = false;

    public Client(Config config)
    {
        Config = config;
        Hostname = Config.Hostname;
        Port = Config.Port;
        Slot = Config.Slot;
        Password = Config.Password;
        MakeNewSession(Hostname, Port);
    }

    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        try
        {
            Plugin.Logger.LogInfo($"Receiving item index {helper.Index}, comparing with {State.Index}");
            if (State.Index >= helper.Index)
            {
                helper.DequeueItem();
                return;
            }
            var itemId = helper.PeekItem().ItemId;
            var item = ArchipelagoItems.Get(itemId);
            Plugin.Logger.LogInfo($"Attempting to handle item {itemId} as {item.Name}");
            item.Action.Invoke();
            State.IncrementIndex();
            helper.DequeueItem();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }
    }

    private void OnMessageReceived(LogMessage message)
    {
        Plugin.Logger.LogInfo(message.ToString());
    }

    private static string PrettyConnectionString(string hostname, int port, string slot, string password)
    {
        password = !password.IsNullOrWhiteSpace() ? ":***" : "";
        return $"{slot}{password}@{hostname}:{port}";
    }

    public string CurrentSessionAsString()
    {
        return PrettyConnectionString(Hostname, Port, Slot, Password);
    }

    public string CurrentSessionConfigAsString()
    {
        return PrettyConnectionString(Config.Hostname, Config.Port, Config.Slot, Config.Password);
    }

    public string ConnectionStatus()
    {
        if (Session.Socket.Connected)
            return $"Currently connected to {CurrentSessionAsString()}.";
        if (RecentConnectionErrorMessage.IsNullOrWhiteSpace())
            return "Disconnected from Archipelago Multi World.";

        var message = RecentConnectionErrorMessage;
        RecentConnectionErrorMessage = null;
        return message;
    }

    public bool Connect()
    {
        Disconnect();

        if (Hostname != Config.Hostname || Port != Config.Port)
            MakeNewSession(Config.Hostname, Config.Port);
        
        Hostname = Config.Hostname;
        Port = Config.Port;
        Slot = Config.Slot;
        Password = Config.Password;
        
        State = new GameState(NewFileRequired);
        
        LoginResult loginResult = null;
        try
        {
            loginResult = Session.TryConnectAndLogin(
                "SULFUR",
                Slot,
                ItemsHandlingFlags.AllItems,
                password: Password,
                requestSlotData: true
            );
            if (!loginResult.Successful) throw new SocketException();
        }
        catch (Exception e)
        {
            loginResult ??= new LoginFailure(e.GetBaseException().Message);
            var failure = (LoginFailure)loginResult;
            var errorMessage = $"Failed to Connect to {CurrentSessionAsString()}:";
            errorMessage = failure.Errors.Aggregate(errorMessage, (current, error) => current + $"\n    {error}");
            errorMessage = failure.ErrorCodes.Aggregate(errorMessage, (current, error) => current + $"\n    {error}");
            RecentConnectionErrorMessage = errorMessage; 
            Plugin.Logger.LogError(errorMessage);
            return false;
        }
        
        var checksCompletedWhileDisconnected = new HashSet<long>(State.AllCompletedLocations);
        checksCompletedWhileDisconnected.ExceptWith(Session.Locations.AllLocationsChecked);
        if (checksCompletedWhileDisconnected.Count > 0)
            Session.Locations.CompleteLocationChecksAsync(checksCompletedWhileDisconnected.ToArray());
        
        Plugin.Logger.LogInfo("Completing waiting locations");
        CompleteLocations(waitingLocations);
        waitingLocations = new HashSet<string>();
        
        return true;
    }

    private void MakeNewSession(string hostname, int port)
    {
        if (Session is not null)
        {
            Session.MessageLog.OnMessageReceived -= OnMessageReceived;
            Session.Items.ItemReceived -= OnItemReceived;
        }
        Session = ArchipelagoSessionFactory.CreateSession(hostname, port);
        Session.MessageLog.OnMessageReceived += OnMessageReceived;
        Session.Items.ItemReceived += OnItemReceived;
    }

    public void Disconnect()
    {
        if (Session is not null && Session.Socket.Connected)
            Session.Socket.DisconnectAsync();
    }

    public ISet<string> waitingLocations = new HashSet<string>();
    public void CompleteLocations(params ICollection<string> locationNames)
    {
        if (State == null || !State.SulfurSave.initialized)
        {
            Plugin.Logger.LogInfo("No state, saving to waiting locations");
            Plugin.Logger.LogInfo($"Saving locations {string.Join(",", locationNames)}");
            waitingLocations.UnionWith(locationNames);
            return;
        }
        try
        {
            var locations = locationNames.Select(locationName => Session.Locations.GetLocationIdFromName("SULFUR", locationName)).Where(locationId => locationId >= 0).ToHashSet();
            Plugin.Logger.LogInfo($"Adding locations {waitingLocations.Count}");
            if (locations.Count == 0) return;
            Plugin.Logger.LogInfo($"Completing locations {string.Join(",", locations)}");
            var genericWeaponLocations = GenericWeaponLocationsUnlocked(locations);
            locations.UnionWith(genericWeaponLocations);
            Session.Locations.CompleteLocationChecks(locations.ToArray());
            Plugin.Logger.LogInfo($"Saving locations completed {string.Join(",", locations)}");
            State.CompleteLocations(locations);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }
    }
    
    private ISet<long> GenericWeaponLocationsUnlocked(HashSet<long> locations)
    {
        var allCurrentLocations = 
            locations
                .Union(State.AllCompletedLocations)
                .Select(
                    id => Session.Locations.GetLocationNameFromId(id)
                )
                .ToHashSet();
        var locationGroups = Session.DataStorage.GetLocationNameGroups();
        var findSpecificWeaponLocations = locationGroups["Find specific weapon"].ToHashSet();
        var rankUpSpecificWeaponLocations = locationGroups["Rank up specific weapon"].ToHashSet();
        var sacrificeSpecificWeaponLocations = locationGroups["Sacrifice specific weapon"].ToHashSet();
        var findWeaponModelLocations = locationGroups["Find unique weapon model"].ToHashSet();
        var rankUpWeaponModelLocations = locationGroups["Rank up unique weapon model"].ToHashSet();
        var sacrificeWeaponModelLocations = locationGroups["Sacrifice unique weapon model"].ToHashSet();
        var rank1Locations = locationGroups["Rank 1"].ToHashSet();
        var rank2Locations = locationGroups["Rank 1"].ToHashSet();
        var rank3Locations = locationGroups["Rank 1"].ToHashSet();
        var rank4Locations = locationGroups["Rank 1"].ToHashSet();
        var rank5Locations = locationGroups["Rank 1"].ToHashSet();
        
        var returnLocations = new HashSet<string>();
        var meleeLocations = locationGroups["Melee Weapon"].ToHashSet();
        returnLocations.UnionWith(WeaponModelLocationsCompleted(
            allCurrentLocations,
            meleeLocations,
            findSpecificWeaponLocations,
            findWeaponModelLocations
        ));
        
        var weaponTypes = new List<string>
        {
            "Assault Rifle",
            "Light Machine Gun",
            "Pistol",
            "Revolver",
            "Rifle",
            "Shotgun",
            "Sniper Rifle",
            "Sub-Machine Gun"
        };
        foreach (var weaponType in weaponTypes)
        {
            var weaponTypeLocations = locationGroups[weaponType].ToHashSet();
            returnLocations.UnionWith(WeaponModelLocationsCompleted(
                allCurrentLocations,
                weaponTypeLocations,
                findSpecificWeaponLocations,
                findWeaponModelLocations
            ));
            returnLocations.UnionWith(WeaponModelLocationsCompleted(
                allCurrentLocations,
                weaponTypeLocations,
                weaponTypeLocations.Intersect(rankUpSpecificWeaponLocations).Intersect(rank1Locations).ToHashSet(),
                weaponTypeLocations.Intersect(rankUpWeaponModelLocations).Intersect(rank1Locations).ToHashSet()
            ));
            returnLocations.UnionWith(WeaponModelLocationsCompleted(
                allCurrentLocations,
                weaponTypeLocations,
                weaponTypeLocations.Intersect(rankUpSpecificWeaponLocations).Intersect(rank2Locations).ToHashSet(),
                weaponTypeLocations.Intersect(rankUpWeaponModelLocations).Intersect(rank2Locations).ToHashSet()
            ));
            returnLocations.UnionWith(WeaponModelLocationsCompleted(
                allCurrentLocations,
                weaponTypeLocations,
                weaponTypeLocations.Intersect(rankUpSpecificWeaponLocations).Intersect(rank3Locations).ToHashSet(),
                weaponTypeLocations.Intersect(rankUpWeaponModelLocations).Intersect(rank3Locations).ToHashSet()
            ));
            returnLocations.UnionWith(WeaponModelLocationsCompleted(
                allCurrentLocations,
                weaponTypeLocations,
                weaponTypeLocations.Intersect(rankUpSpecificWeaponLocations).Intersect(rank4Locations).ToHashSet(),
                weaponTypeLocations.Intersect(rankUpWeaponModelLocations).Intersect(rank4Locations).ToHashSet()
            ));
            returnLocations.UnionWith(WeaponModelLocationsCompleted(
                allCurrentLocations,
                weaponTypeLocations,
                weaponTypeLocations.Intersect(rankUpSpecificWeaponLocations).Intersect(rank5Locations).ToHashSet(),
                weaponTypeLocations.Intersect(rankUpWeaponModelLocations).Intersect(rank5Locations).ToHashSet()
            ));
            returnLocations.UnionWith(WeaponModelLocationsCompleted(
                allCurrentLocations,
                weaponTypeLocations,
                sacrificeSpecificWeaponLocations,
                sacrificeWeaponModelLocations
            ));
        }

        return returnLocations.Select(name => Session.Locations.GetLocationIdFromName("SULFUR", name)).ToHashSet();
    }

    private ISet<string> WeaponModelLocationsCompleted(
        ISet<string> allCurrentLocations,
        HashSet<string> weaponTypeLocations,
        HashSet<string> specificWeaponLocations,
        HashSet<string> weaponModelLocations
    )
    {
        var locationsCompleted = new HashSet<string>();
        var specificLocations = allCurrentLocations
            .Intersect(weaponTypeLocations)
            .Intersect(specificWeaponLocations)
            .ToList();
        Plugin.Logger.LogInfo("Sorted items: " + string.Join(",", specificLocations));
        var specificLocationsAmount =
            allCurrentLocations
                .Intersect(weaponTypeLocations)
                .Intersect(specificWeaponLocations)
                .Count();
        Plugin.Logger.LogInfo(specificLocationsAmount);
        var sortedModelLocations =
            weaponTypeLocations
                .Intersect(weaponModelLocations)
                .OrderBy(name => Session.Locations.GetLocationIdFromName("SULFUR", name))
                .ToList();
        Plugin.Logger.LogInfo("Sorted items: " + string.Join(",", sortedModelLocations));
        for (var i = 0; i < specificLocationsAmount; i++)
        {
            Plugin.Logger.LogInfo("Adding location");
            locationsCompleted.Add(sortedModelLocations[i]);
        }

        return locationsCompleted;
    }

    public bool Connected()
    {
        return Session.Socket.Connected;
    }

    public ItemId StartingGun()
    {
        return ArchipelagoItems.Get((long)Session.DataStorage.GetSlotData()["StartGun"]).ItemId();
    }

    public ItemId StartingMelee()
    {
        return ArchipelagoItems.Get((long)Session.DataStorage.GetSlotData()["StartMelee"]).ItemId();
    }
}