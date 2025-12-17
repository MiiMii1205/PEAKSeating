using System.Collections.Generic;
using PEAKSeating.Patchers;
using Photon.Pun;

namespace PEAKSeating.Behaviours;

public class HelicopterOrderController : MonoBehaviourPun
{
    private bool m_replicated = false;
    private int m_lowestViewID = -1;

    [PunRPC]
    public void SetupCutsceneOrder(int id, List<int> indexOrder)
    {
        if (!m_replicated)
        {
            // Update the scout order
            Plugin.Log.LogInfo($"Received order {indexOrder} from view ${id}. Saving random seating order...");
            PeakHandlerPatcher.ScoutOrder = indexOrder;
            m_replicated = true;
            m_lowestViewID = id;
        }
        else if (id < m_lowestViewID)
        {
            // The earliest player's random order has priority so override the previous random order
            Plugin.Log.LogInfo(
                $"Received order {indexOrder} from view ${id}, which has priority over view ${m_lowestViewID}. Saving random seating order...");
            PeakHandlerPatcher.ScoutOrder = indexOrder;
            m_lowestViewID = id;
        }
    }
}