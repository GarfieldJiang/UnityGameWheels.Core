namespace COL.UnityGameWheels.Core.Asset
{
    public partial class AssetService
    {
        internal partial class Loader
        {
            private class ResourceLoadingTask : IResourceLoadingTaskImpl
            {
                private IResourceLoadingTaskImpl Impl = null;

                public string ResourcePath { get => Impl.ResourcePath; set => Impl.ResourcePath = value; }

                public string ResourceParentDir { get => Impl.ResourceParentDir; set => Impl.ResourceParentDir = value; }

                public object ResourceObject => Impl.ResourceObject;

                public bool IsDone => Impl.IsDone;

                public float Progress => Impl.Progress;

                public string ErrorMessage => Impl.ErrorMessage;

                public ResourceLoadingTask()
                {
                }

                public void OnCreate(ISimpleFactory<IResourceLoadingTaskImpl> implFactory)
                {
                    if (Impl == null)
                    {
                        Impl = implFactory.Get();
                    }
                }

                public void OnReset()
                {
                    Impl.OnReset();
                }

                public void OnStart()
                {
                    Impl.OnStart();
                }

                public void OnUpdate(TimeStruct timeStruct)
                {
                    Impl.OnUpdate(timeStruct);
                }
            }
        }
    }
}