using System.Collections.Generic;
using PEAKSeating.Patchers;
using Photon.Pun;

namespace PEAKSeating.Behaviours;

public class HelicopterOrderController: MonoBehaviourPun
{
    [PunRPC]
    public void SetupCutsceneOrder(List<int> indexOrder)
    {
        // Update the scout order if you aren't the host
        PeakHandlerPatcher.ScoutOrder = indexOrder;
    }
}