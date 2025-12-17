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

    private static int SortCharsDistance(Character a, Character b)
    {
        var helicopterPosition = PeakHandler.Instance.endCutsceneAnimator.transform.position;

        var positionA = Vector3.Distance(a.lastLivingPosition, helicopterPosition);
        var positionB = Vector3.Distance(b.lastLivingPosition, helicopterPosition);

        return positionA.CompareTo(positionB);
    }

    private static int SortCharsRandom(Character a, Character b)
    {
        if (ScoutOrder == null)
        {
            //  Not initialized, so we'll fall back to vanilla sorting
            Plugin.Log.LogError("Random scout seating order not initialized! Using the default seating order...");

            return a?.view.ViewID.CompareTo(b?.view.ViewID) ?? 0;
        }
        else
        {
            var indexOfa = ScoutOrder.IndexOf(a?.view.ViewID ?? 0);
            var indexOfb = ScoutOrder.IndexOf(b?.view.ViewID ?? 0);

            // Sorting again the index so we'll eventually place every scout according to the random order
            return indexOfa.CompareTo(indexOfb);
        }
    }

    private static int SortCharsVanilla(Character a, Character b)
    {
        return a.view.ViewID - b.view.ViewID;
    }

    private static string GetOrderingFuncName()
    {
        return Plugin.SeatingOrder switch
        {
            SeatingOrders.CLOSEST => nameof(SortCharsDistance),
            SeatingOrders.RANDOM => nameof(SortCharsRandom),
            SeatingOrders.VANILLA => nameof(SortCharsVanilla),
            _ => throw new ArgumentOutOfRangeException(nameof(Plugin.SeatingOrder))
        };
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeMatcher = new CodeMatcher(instructions);

        // No need to patch if we use vanilla ordering
        if (Plugin.SeatingOrder != SeatingOrders.VANILLA)
        {
            Plugin.Log.LogInfo($"Patching sort for {Plugin.SeatingOrder.ToString().ToLower()} sorting...");
            
            //Replacing the Sort lambda comparator to whatever order the player wants 
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

        Plugin.Log.LogInfo($"Not applying patch, because it's {Plugin.SeatingOrder.ToString().ToLower()} sorting...");
        
        return codeMatcher.InstructionEnumeration();
    }
}