using HarmonyLib;
using I2.Loc;
using NodeCanvas.DialogueTrees;
using PerfectRandom.Sulfur.Core;

namespace Archipelago.Patches;
[HarmonyPatch(typeof(DialogController), nameof(DialogController.Internal_OnSubtitlesRequestInfo))]
public class Internal_OnSubtitlesRequestInfo
{
    public static SubtitlesRequestInfo CurrentInfo = null;
    
    public static void Postfix(SubtitlesRequestInfo info, DialogController __instance)
    {
        LocalizationManager.TryGetTranslation("UnitNames/Amulet", out var translatedAmuletName);
        if (__instance.CurrentSpeakable.ActorName != translatedAmuletName) return;
        var currentLine =
            string.Concat("Dialog/" + __instance.currentDialogueTree.name.Replace("Dialog_", "") + "_NPC_",
                info.statement.meta);
        if (currentLine is "Dialog/Amulet_NPC_Whatsonyourmind" or "Dialog/Amulet_NPC_Imfilledtothebrimand")
        {
            CurrentInfo = info;
        }
    }
}