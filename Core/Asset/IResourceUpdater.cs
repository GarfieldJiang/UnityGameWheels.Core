using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Resource updater interface.
    /// </summary>
    public interface IResourceUpdater
    {
        bool IsReady { get; }

        IEnumerable<int> GetAvailableResourceGroupIds();

        void GetAvailableResourceGroupIds(List<int> groupIds);

        bool ResourceGroupIdIsAvailable(int groupId);

        ResourceGroupStatus GetResourceGroupStatus(int groupId);

        void StartUpdatingResourceGroup(int groupId, ResourceGroupUpdateCallbackSet callbackSet, object context);

        bool StopUpdatingResourceGroup(int groupId);

        void StopAllUpdatingResourceGroups();

        ResourceGroupUpdateSummary GetResourceGroupUpdateSummary(int groupId);
    }
}