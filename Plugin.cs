using Archipelago.Archipelago;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;

namespace Archipelago;

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
    }
}
