using UnityEngine;

namespace Archipelago.Archipelago;

public class StampLocation
{
    [SerializeField]
    public string Name;
    [SerializeField]
    public string Description;

    public StampLocation(string name, string description)
    {
        Name = name;
        Description = description;
    }
}