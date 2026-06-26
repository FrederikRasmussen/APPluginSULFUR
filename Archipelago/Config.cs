using BepInEx.Configuration;

namespace Archipelago.Archipelago;

public class Config(
    ConfigEntry<string> hostnameBind,
    ConfigEntry<int> portBind,
    ConfigEntry<string> slotBind,
    ConfigEntry<string> passwordBind)
{
    public string Hostname => hostnameBind.Value;
    public int Port => portBind.Value;
    public string Slot => slotBind.Value;
    public string Password => passwordBind.Value;
}