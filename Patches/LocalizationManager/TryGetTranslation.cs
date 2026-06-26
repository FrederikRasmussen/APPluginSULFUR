using HarmonyLib;
using I2.Loc;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.TryGetTranslation))]
public class TryGetTranslation
{
    public static void Postfix(string Term, ref string Translation)
    {
        if (Term is "Dialog/Amulet_NPC_Whatsonyourmind" or "Dialog/Amulet_NPC_Imfilledtothebrimand")
        {
            Translation = Plugin.Client.ConnectionStatus();
        }
    }
}