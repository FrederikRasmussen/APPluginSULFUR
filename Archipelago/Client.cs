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
using PerfectRandom.Sulfur.Core.DataStorage;
using PerfectRandom.Sulfur.Core.LevelGeneration;

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
    public Queue<LogMessage> LogMessages { get; } = new();

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
        Plugin.Logger.LogInfo("added to queue " +message);
        LogMessages.Enqueue(message);
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
            Session.Locations.CompleteLocationChecks(locations.ToArray());
            Plugin.Logger.LogInfo($"Saving locations completed {string.Join(",", locations)}");
            State.CompleteLocations(locations);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }
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