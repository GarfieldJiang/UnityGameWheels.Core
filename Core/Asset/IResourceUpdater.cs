using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Resource updater interface.
    /// </summary>
    public interface IResourceUpdater
    {
        bool IsReady { get; }

        int[] GetAvailableResourceGroupIds();

        void GetAvailableResourceGroupIds(List<int> groupIds);

        ResourceGroupStatus GetResourceGroupStatus(int groupId);

        void StartUpdatingResourceGroup(int groupId, ResourceGroupUpdateCallbackSet callbackSet, object context);

        bool StopUpdatingResourceGroup(int groupId);

        ResourceGroupUpdateSummary GetResourceGroupUpdateSummary(int groupId);
    }
}