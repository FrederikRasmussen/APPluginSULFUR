using System.Collections.Generic;
using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.DataStorage;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(PlayerProgress), nameof(PlayerProgress.SetCheckpointReached))]
public class SetCheckpointReached
{
    private static readonly HashSet<string> LockedCheckpoints = [
        "ReachedEnvironment_WorldEnvironment_" + WorldEnvironmentIds.Act_01_Shanty,
        "ReachedEnvironment_WorldEnvironment_" + WorldEnvironmentIds.Act_01_Sewers,
        "ReachedEnvironment_WorldEnvironment_" + WorldEnvironmentIds.Act_01_Hedgemaze,
        "ReachedEnvironment_WorldEnvironment_" + WorldEnvironmentIds.Act_01_Dungeon,
        "ReachedEnvironment_WorldEnvironment_" + WorldEnvironmentIds.Act_01_Castle,
        "ReachedEnvironment_WorldEnvironment_" + WorldEnvironmentIds.Act_02_Forest,
        "ReachedEnvironment_WorldEnvironment_" + WorldEnvironmentIds.Act_02_Bridge,
        "ReachedEnvironment_WorldEnvironment_" + WorldEnvironmentIds.Act_03_Desert,
        "ReachedEnvironment_WorldEnvironment_" + WorldEnvironmentIds.Act_03_EndChurch,
    ];

    private static readonly Dictionary<string, string> BossCheckpoints = new()
    {
        { "BossDead_Cousin", "Defeat Cousin in the Sulfur Caves" },
        { "BossDead_Lucia", "Defeat St. Lucia in the Castle" },
        { "BossDead_Emperor", "Defeat the Terrorbaum in the Forest" },
        { "BossDead_Terrorbaum", "Defeat the Emperor in the Shav'Wani Fortress" },
        { "BossDead_DesertClause", "Defeat Desert Claus in the Desert" },
        { "HasCompletedMainStory", "Defeat the Witch" }
    };
    
    public static void Prefix(string identifier, ref bool reached)
    {
        if (LockedCheckpoints.Contains(identifier) &&
            !Plugin.Client.State.CheckpointUnlocks.Contains(ArchipelagoItems.Get(identifier).Id)) reached = false;
        if (!reached) return;
        switch (identifier)
        {
            case "ItemBroughtToChurch_Item_Suitcase":
                Plugin.Client.CompleteLocations("Bring your Suitcase to the Church");
                break;
            case "ItemBroughtToChurch_Item_Refrigerator":
                Plugin.Client.CompleteLocations("Plug in your new Refrigerator");
                break;
            case "ItemBroughtToChurch_Item_ChestOfDrawers":
                Plugin.Client.CompleteLocations("Put the Chest of Drawers in the Church");
                break;
        }
        if (BossCheckpoints.ContainsKey(identifier))
            Plugin.Client.CompleteLocations(BossCheckpoints[identifier]);
    }
}