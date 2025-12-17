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
    static void SummonHelicopterPatch(PeakHandler __instance)
    {
        var order = __instance.endCutsceneAnimator.gameObject.GetOrAddComponent<HelicopterOrderController>();

        // Only the host gets to generate a seating order and will replicate to other clients
        
        if (PhotonNetwork.IsMasterClient && Plugin.SeatingOrder == SeatingOrders.RANDOM)
        {
            Plugin.Log.LogInfo("Generating a random player order...");
            var seatingOrder = PlayerHandler.GetAllPlayerCharacters().ConvertAll(c => c.view.ViewID).Shuffle();
            PeakHandlerPatcher.ScoutOrder = seatingOrder;
            order.photonView.RPC("SetupCutsceneOrder", RpcTarget.Others, seatingOrder);
        }
    }

}