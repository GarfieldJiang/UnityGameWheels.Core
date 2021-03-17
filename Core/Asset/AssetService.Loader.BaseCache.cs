using System;

namespace COL.UnityGameWheels.Core.Asset
{
    public partial class AssetService
    {
        internal partial class Loader
        {
            internal abstract class BaseCache
            {
                protected internal Loader Owner;

                public string Path { get; internal set; }

                public string ErrorMessage { get; protected set; }

                private int m_RetainCount = 0;

                private bool m_Ticking = false;

                public abstract float LoadingProgress { get; }

                private readonly Action<TimeStruct> m_UpdateFunc = null;

                internal BaseCache()
                {
                    m_UpdateFunc = Update;
                }

                protected void StartTicking()
                {
                    if (m_Ticking)
                    {
                        return;
                    }

                    m_Ticking = true;
                    Owner.m_TickDelegates.Add(m_UpdateFunc);
                }

                protected void StopTicking()
                {
                    if (!m_Ticking)
                    {
                        return;
                    }

                    Owner.m_TickDelegates.Remove(m_UpdateFunc);
                    m_Ticking = false;
                }

                internal abstract void Init();

                protected abstract void MarkAsUnretained();

                protected abstract void UnmarkAsUnretained();

                public int RetainCount => m_RetainCount;

                internal virtual void IncreaseRetainCount()
                {
                    m_RetainCount++;
                    InternalLog.DebugFormat("[{0} IncreaseRetainCount] '{2}' to {1}", GetType().Name, m_RetainCount, Path);
                }

                internal virtual void ReduceRetainCount()
                {
                    if (m_RetainCount <= 0)
                    {
                        throw new InvalidOperationException(Utility.Text.Format("Reducing retain count to negative, on '{0}' ({1}).", Path,
                            GetType().Name));
                    }

                    --m_RetainCount;
                    InternalLog.DebugFormat("[{0} ReduceRetainCount] '{2}' to {1}", GetType().Name, m_RetainCount, Path);
                }

                protected abstract void Update(TimeStruct timeStruct);

                internal virtual void Reset()
                {
                    if (m_RetainCount != 0)
                    {
                        throw new InvalidOperationException(Utility.Text.Format("Try to reset '{0}' ({1}) when retain count is non-zero.",
                            Path, GetType().Name));
                    }

                    Path = null;
                    ErrorMessage = null;
                    Owner = null;
                }

                internal abstract void OnSlotReady();

                /// <summary>
                /// Whether the cache can be safely released when it's <see cref="RetainCount"/> is 0.
                /// </summary>
                /// <returns></returns>
                internal abstract bool CanRelease();
            }
        }
    }
}