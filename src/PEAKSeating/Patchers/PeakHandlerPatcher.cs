using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace PEAKSeating.Patchers;

[HarmonyPatch(typeof(PeakHandler))]
[HarmonyPatch(nameof(PeakHandler.SetCosmetics))]
public static class PeakHandlerPatcher
{
    public static IList<int>? ScoutOrder { get; set; }

    static int SortCharsDistance(Character a, Character b)
    {
        var helicopterPosition = PeakHandler.Instance.endCutsceneAnimator.transform.position;

        var positionA = Vector3.Distance(a.lastLivingPosition, helicopterPosition);
        var positionB = Vector3.Distance(b.lastLivingPosition, helicopterPosition);

        return positionA.CompareTo(positionB);
    }

    static int SortCharsRandom(Character a, Character b)
    {
        if (ScoutOrder == null)
        {
            //  Not initialized.. 
            Plugin.Log.LogError("The random order was not initialized. Using the default view");

            return a?.view.ViewID.CompareTo(b?.view.ViewID) ?? 0;
        }
        else
        {
            var viewIdA = a?.view.ViewID ?? 0;
            var viewIdB = b?.view.ViewID ?? 0;

            var indexOfa = ScoutOrder.IndexOf(viewIdA);
            var indexOfb = ScoutOrder.IndexOf(viewIdB);

            return indexOfa.CompareTo(indexOfb);
        }
    }

    static int SortCharsVanilla(Character a, Character b)
    {
        return a.view.ViewID - b.view.ViewID;
    }

    static string GetOrderingFuncName()
    {
        switch (Plugin.SeatingOrder)
        {
            case SeatingOrders.CLOSEST:
                return nameof(SortCharsDistance);
            case SeatingOrders.RANDOM:
                return nameof(SortCharsRandom);
            case SeatingOrders.VANILLA:
                return nameof(SortCharsVanilla);
            default:
                throw new ArgumentOutOfRangeException(nameof(Plugin.SeatingOrder));
        }
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeMatcher = new CodeMatcher(instructions);

        // No need to patch if we use the vanilla order
        if (Plugin.SeatingOrder != SeatingOrders.VANILLA)
        {
            Plugin.Log.LogInfo($"Patching sort for {Plugin.SeatingOrder.ToString().ToLower()} sorting...");
            
            // We replace the standard Sort comparator to whatever function the player wants 
            return codeMatcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldftn),
                    new CodeMatch(OpCodes.Newobj),
                    new CodeMatch(OpCodes.Dup)
                )
                .ThrowIfInvalid("Could not find command")
                .SetOperandAndAdvance(typeof(PeakHandlerPatcher).GetMethod(GetOrderingFuncName(),
                    BindingFlags.Static | BindingFlags.NonPublic))
                .InstructionEnumeration();
        }

        return codeMatcher.InstructionEnumeration();
    }
}