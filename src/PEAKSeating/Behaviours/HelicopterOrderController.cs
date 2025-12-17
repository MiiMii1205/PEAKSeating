using System.Collections.Generic;
using PEAKSeating.Patchers;
using Photon.Pun;

namespace PEAKSeating.Behaviours;

public class HelicopterOrderController: MonoBehaviourPun
{
    private bool m_replicated = false;
    [PunRPC]
    public void SetupCutsceneOrder(List<int> indexOrder)
    {
        if (!m_replicated)
        {
            // Update the scout order if you aren't the host
            Plugin.Log.LogInfo($"Received order {indexOrder}. Saving random seating order...");
            PeakHandlerPatcher.ScoutOrder = indexOrder;
            m_replicated = true;
        }
    }
}