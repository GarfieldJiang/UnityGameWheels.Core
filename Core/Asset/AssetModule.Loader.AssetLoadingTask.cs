namespace COL.UnityGameWheels.Core.Asset
{
    public partial class AssetModule
    {
        internal partial class Loader
        {
            private class AssetLoadingTask : IAssetLoadingTaskImpl
            {
                private IAssetLoadingTaskImpl Impl = null;

                public AssetLoadingTask()
                {
                }

                public object ResourceObject { get => Impl.ResourceObject; set => Impl.ResourceObject = value; }

                public string AssetPath { get => Impl.AssetPath; set => Impl.AssetPath = value; }

                public object AssetObject => Impl.AssetObject;

                public bool IsDone => Impl.IsDone;

                public float Progress => Impl.Progress;

                public string ErrorMessage => Impl.ErrorMessage;

                public void OnCreate(ISimpleFactory<IAssetLoadingTaskImpl> implFactory)
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