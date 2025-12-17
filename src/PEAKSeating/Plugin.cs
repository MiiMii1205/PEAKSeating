using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PEAKSeating.Patchers;

namespace PEAKSeating;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    private static ConfigEntry<SeatingOrders>? _configSeatingOrder;
    private static Harmony _harmonyPatch = new(Id);
    internal static ManualLogSource Log { get; private set; } = null!;

    public static SeatingOrders SeatingOrder => _configSeatingOrder?.Value ?? SeatingOrders.VANILLA;

    private void Awake()
    {
        Log = Logger;

        _configSeatingOrder = Config.Bind(
            "General",
            "Seating Order",
            SeatingOrders.VANILLA,
            ((List<string>) [
                "The seating order on the helicopter.",
                "Possible values :",
                "VANILLA - Scouts sits in vanilla order, i.e the order they joined the game",
                "DISTANCE - Scouts sits according to how close they are to the helicopter",
                "RANDOM - Scouts sits in a completely random order",
            ]).Join(null,"/n")
        );

        _configSeatingOrder.SettingChanged += RepatchHarmony;

        PatchHarmony();
        
        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private void OnDestroy()
    {
        Log.LogInfo("Unpatching Harmony Patches");
        _harmonyPatch.UnpatchSelf();
        Log.LogInfo($"Plugin {Name} unloaded!");
    }


    private static void PatchHarmony()
    {
        _harmonyPatch.PatchAll(typeof(PeakHandlerPatcher));
        _harmonyPatch.PatchAll(typeof(HelicopterPatcher));
    }
    
    private static void RepatchHarmony(object sender, EventArgs e)
    {
        Log.LogInfo("Unpatching Harmony Patches");
        _harmonyPatch.UnpatchSelf();

        Log.LogInfo("Repatching Harmony Patches");
        PatchHarmony();
    }
}