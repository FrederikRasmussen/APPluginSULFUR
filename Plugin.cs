using Archipelago.Archipelago;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using Ryuka.Sulfur.NativeUI;

namespace Archipelago;

[BepInDependency("ryuka.sulfur.nativeui", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin("dk.nuzzles.archipelago", "Archipelago", "0.2.0")]
[BepInProcess("Sulfur.exe")]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    public static Client Client { get; set; }

    private void Awake()
    {
        // Initialise Harmony instance
        var harmony = new Harmony("dk.nuzzles.archipelago");
        HarmonyFileLog.Enabled = true;
        harmony.PatchAll();
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin dk.nuzzles.archipelago is loaded!");

        var hostnameBind = Config.Bind(
            "Archipelago",
            "Hostname",
            "archipelago.gg",
            "Which server to connect to"
        );
        var portBind = Config.Bind(
            "Archipelago",
            "Port",
            123456,
            "Which port to connect to"
        );
        var slotBind = Config.Bind(
            "Archipelago",
            "Slot",
            "Father",
            "Which slot to connect to"
        );
        var passwordBind = Config.Bind(
            "Archipelago",
            "Password",
            "",
            "Which password to use"
        );
        
        Logger.LogInfo("Making config");
        var config = new Config(
            hostnameBind,
            portBind,
            slotBind,
            passwordBind
        );
        Logger.LogInfo("Made config");
        
        Client = new Client(config);
        
        SulfurOptionsApi.RegisterPage(new SulfurOptionsPage
        {
            PageId = "dk.nuzzles.archipelago",
            DisplayName = "Archipelago",
            SortOrder = 1000,
            GetDisplayName = () => "Archipelago",
            BuildPage = ctx => BuildPage(ctx, hostnameBind, portBind, slotBind, passwordBind)
        });
    }
    
    private void OnDestroy()
    {
        SulfurOptionsApi.UnregisterPage("ryuka.example.mod");
    }
    
    private void BuildPage(
        SulfurOptionsContext ctx,
        ConfigEntry<string> hostnameEntry,
        ConfigEntry<int> portEntry,
        ConfigEntry<string> slotEntry,
        ConfigEntry<string> passwordEntry
    )
    {
        ctx.AddSection("Archipelago");
        ctx.AddDescription("Archipelago connection options.");

        var hostnameRow = new SulfurSettingRow
        {
            Label = hostnameEntry.Definition.Key,
            IndentLevel = 1,

            DirtyText = "Pending",
            CleanText = "Unchanged",
            RestartRequiredText = "Restart Required",
            LiveApplyText = "Live Apply",
            AdvancedText = "Advanced",
            HiddenText = "Hidden",
            DangerousText = "Dangerous",
            
            ShowCleanBadge = false,
            ShowDefaultButton = false,
        };
        ctx.AddSettingText(
            hostnameRow,
            hostnameEntry.Value,
            newValue => hostnameEntry.Value = newValue
        );
        
        var portRow = new SulfurSettingRow
        {
            Label = portEntry.Definition.Key,
            IndentLevel = 1,

            DirtyText = "Pending",
            CleanText = "Unchanged",
            RestartRequiredText = "Restart Required",
            LiveApplyText = "Live Apply",
            AdvancedText = "Advanced",
            HiddenText = "Hidden",
            DangerousText = "Dangerous",
            
            ShowCleanBadge = false,
            ShowDefaultButton = false,
        };
        ctx.AddSettingNumber(
            portRow,
            portEntry.Value,
            0,
            65535,
            0,
            newValue => portEntry.Value = (int)newValue
        );
        
        var slotRow = new SulfurSettingRow
        {
            Label = slotEntry.Definition.Key,
            IndentLevel = 1,

            DirtyText = "Pending",
            CleanText = "Unchanged",
            RestartRequiredText = "Restart Required",
            LiveApplyText = "Live Apply",
            AdvancedText = "Advanced",
            HiddenText = "Hidden",
            DangerousText = "Dangerous",

            ShowCleanBadge = false,
            ShowDefaultButton = false,
        };
        ctx.AddSettingText(
            slotRow,
            slotEntry.Value,
            newValue => slotEntry.Value = newValue
        );
        
        var passwordRow = new SulfurSettingRow
        {
            Label = passwordEntry.Definition.Key,
            IndentLevel = 1,

            DirtyText = "Pending",
            CleanText = "Unchanged",
            RestartRequiredText = "Restart Required",
            LiveApplyText = "Live Apply",
            AdvancedText = "Advanced",
            HiddenText = "Hidden",
            DangerousText = "Dangerous",
            
            ShowCleanBadge = false,
            ShowDefaultButton = false,
        };
        ctx.AddSettingText(
            passwordRow,
            passwordEntry.Value,
            newValue => passwordEntry.Value = newValue
        );
    }
}
