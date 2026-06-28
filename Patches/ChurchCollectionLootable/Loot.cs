using System.Collections;
using Archipelago.Archipelago;
using Archipelago.Helpers;
using HarmonyLib;
using ParadoxNotion;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.LevelGeneration;
using PerfectRandom.Sulfur.Gameplay;
using UnityEngine;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(ChurchCollectionLootable), nameof(ChurchCollectionLootable.Loot))]
public class Loot
{
    public static void Prefix(ChurchCollectionLootable __instance)
    {
        Plugin.Logger.LogInfo("Looking for playerHUDs");
        foreach (var playerHUD in PlayerHUD.instances)
        {
            Plugin.Logger.LogInfo("Found one hud");
            Plugin.Logger.LogInfo(playerHUD.GetName());
            Plugin.Logger.LogInfo(playerHUD.pickupNotificationRoot.GetName());
        }
        Plugin.Logger.LogInfo("Found all playerHUDs");
        
        __instance.totalWorth = LootManager.Instance.GetChurchCollectionAmount();
        __instance.moneyToSpawn = LootManager.Instance.GetMoneyItemsForAmount(__instance.totalWorth, shuffle: true);
        if (Plugin.Client.State.WaitingItems.Count <= 0 && Plugin.Client.State.UnlockedSulf <= 0) return;
        __instance.hasContents = true;
        if (Plugin.Client.State.WaitingItems.Count <= 0) return;
        __instance.StartCoroutine(LootSpawnRoutine(__instance.lootSpawnTransform, __instance));
    }
    
    private static IEnumerator LootSpawnRoutine(Transform transformForSpawning, ChurchCollectionLootable __instance)
    {
        ChurchCollectionState.IsLooting = true;
        
        var church = StaticInstance<GameManager>.Instance.orderedRooms[0];
        var startCount = Plugin.Client.State.WaitingItems.Count;
        var waiter = __instance.lootBetweenWaiter;
        if (startCount * __instance.lootBetweenWaiter.m_Seconds > 10.0f)
        {
            waiter = new WaitForSeconds(10.0f / startCount);
            Plugin.Logger.LogInfo($"Waiting waits {10.0f / startCount} between spawns");
        }
        StaticInstance<InteractionManager>.Instance.SpawnPickup(transformForSpawning.position, motionTowardsPlayer: false, ArchipelagoItems.Get(Plugin.Client.State.WaitingItems.Dequeue()).ItemId().GetAsset(), church, null, null, 0.75f);
        yield return __instance.lootBetweenWaiter;
        while(Plugin.Client.State.WaitingItems.Count > 0)
        {
            var item = Plugin.Client.State.WaitingItems.Dequeue();
            StaticInstance<InteractionManager>.Instance.SpawnPickup(transformForSpawning.position, motionTowardsPlayer: false, ArchipelagoItems.Get(item).ItemId().GetAsset(), church, null, null, 0.75f);
            yield return waiter;
        }

        Plugin.Client.State.SpawnedAllWaitingItems();
        ChurchCollectionState.IsLooting = false;
        
        __instance.animator.ResetTrigger("Interact");
    }
}