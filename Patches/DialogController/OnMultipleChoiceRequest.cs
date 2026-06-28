using System;
using Archipelago.Helpers;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using I2.Loc;
using NodeCanvas.DialogueTrees;
using PerfectRandom.Sulfur.Core;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(DialogController), nameof(DialogController.OnMultipleChoiceRequest))]
public class OnMultipleChoiceRequest
{
    
    
    public static void Postfix(MultipleChoiceRequestInfo info, DialogController __instance)
    {
        if (Plugin.Client.Connected()) return;
        LocalizationManager.TryGetTranslation("UnitNames/Amulet", out var translatedAmuletName);
        if (__instance.CurrentSpeakable.ActorName != translatedAmuletName) return;
        Plugin.Logger.LogInfo("Achievement popup!");
        AchievementUIManager.Instance.TestAchievementPopup(AchievementManager.Instance.AllAchievements[0]);
        foreach (var button in __instance.playerDialogButtons)
        {
            var option = button.GetComponent<DialogOption>();
            const string translatePath = "Dialog/Amulet_PLA_WhereamIgoingnext";
            if (option.optionText.text != LocalizationManager.GetTranslation(translatePath)) continue;
            option.SetDialogOption($"Connect to Multi World {Plugin.Client.CurrentSessionConfigAsString()}.");
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate
            {
                Plugin.Client.Connect();
                __instance.OnSubtitlesRequest(Internal_OnSubtitlesRequestInfo.CurrentInfo);
            });
        }
    }
}