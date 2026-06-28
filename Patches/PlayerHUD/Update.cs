using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using I2.Loc;
using NodeCanvas.Tasks.Actions;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.Units;
using PerfectRandom.Sulfur.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(PlayerHUD), nameof(Update))]
public class Update
{
    private static RectTransform _ourRoot = null;
    private static PickupNotification _ourPickupNotifs = null;
    private static readonly List<PickupNotification> ActivePickupNotifications = [];
    private static readonly Sprite artwork = ItemIds.Currency_SulfCoin.GetAsset().artwork;

    private const int MaximumLines = 10;
    
    public static void Prefix(PlayerHUD __instance)
    {
        if (__instance.pickupNotificationRoot is null)
            return;
        
        if (_ourRoot is null)
        {
            _ourRoot = UnityEngine.Object.Instantiate(__instance.pickupNotificationRoot, __instance.transform);
            _ourRoot.localPosition = new Vector3(_ourRoot.GetParentSize().x, _ourRoot.GetParentSize().y, 0);

            var verticalLayoutGroup = _ourRoot.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.spacing = -7f;
        }
        if (_ourPickupNotifs is null)
        {
            _ourPickupNotifs = UnityEngine.Object.Instantiate(__instance.pickupNotificationPrefab, _ourRoot);

            var image = _ourPickupNotifs.GetComponent<Image>();
            Component.Destroy(image);

            var layoutElement = _ourPickupNotifs.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = _ourRoot.GetParentSize().x;
            GameObject.Destroy(_ourPickupNotifs.amount.gameObject);
            
            _ourPickupNotifs.artwork.sprite = artwork;
            
            _ourPickupNotifs.gameObject.SetActive(false);
        }
        
        if (ActivePickupNotifications.Count <= MaximumLines && Plugin.Client.LogMessages.TryPeek(out var message))
        {
            var pickupNotification = Object.Instantiate(_ourPickupNotifs, _ourRoot);
            pickupNotification.gameObject.SetActive(value: false);
            var builder = new StringBuilder();
            foreach (var messagePart in message.Parts)
            {
                var color = messagePart.Color;
                var colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                builder.Append($"<color={colorHex}>{messagePart.Text}</color>");
            }
            pickupNotification.name.text = builder.ToString();
            pickupNotification.endTime = Time.time + __instance.notificationDuration * 5;
            pickupNotification.gameObject.SetActive(value: true);
            ActivePickupNotifications.Add(pickupNotification);
            Plugin.Client.LogMessages.Dequeue();
        }

        var toRemove = new List<PickupNotification>();
        foreach (var activePickupNotification in ActivePickupNotifications)
        {
            if (!(Time.time > activePickupNotification.endTime)) continue;
            Plugin.Logger.LogInfo("Removing notif");
            activePickupNotification.gameObject.SetActive(false);
            toRemove.Add(activePickupNotification);
        }
        foreach (var notification in toRemove)
        {
            ActivePickupNotifications.Remove(notification);
            Plugin.Logger.LogInfo("Removed notif");
        }
    }
}