using PerfectRandom.Sulfur.Gameplay;

namespace Archipelago.Patches;

public class ChurchCollectionState
{
    public static ChurchCollectionLootable Instance { get; set; }
    public static bool IsEnabled = false;
    public static bool IsLooting = false;
}