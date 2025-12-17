using System.Collections.Generic;
using HarmonyLib;
using PEAKSeating.Behaviours;
using Photon.Pun;
using pworld.Scripts.Extensions;

namespace PEAKSeating.Patchers;

public static class HelicopterPatcher
{
    [HarmonyPatch(typeof(PeakHandler), "SummonHelicopter")]
    [HarmonyPostfix]
    private static void SummonHelicopterPatch(PeakHandler __instance)
    {
        var order = __instance.endCutsceneAnimator.gameObject.GetOrAddComponent<HelicopterOrderController>();

        // We're using a "fist passed the poll" system to replicate the sorting order

        Plugin.Log.LogInfo("Generating a backup random player order...");
        var seatingOrder = PlayerHandler.GetAllPlayerCharacters().ConvertAll(c => c.view.ViewID).Shuffle();
        
        Plugin.Log.LogInfo("Replicating random player order...");
        order.photonView.RPC("SetupCutsceneOrder", RpcTarget.All, order.photonView.ViewID, seatingOrder);
    }    
    
}