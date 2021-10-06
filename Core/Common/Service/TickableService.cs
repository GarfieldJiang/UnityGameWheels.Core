using System;

namespace COL.UnityGameWheels.Core
{
    public abstract class TickableService : ITickableService, IDisposable
    {
        protected ITickService m_TickService;

        public int TickOrder { get; private set; } = 0;

        public bool IsTicking { get; private set; }

        public TickableService(ITickService tickService)
        {
            m_TickService = tickService;
        }

        public virtual bool StartTicking()
        {
            if (IsTicking)
            {
                return false;
            }

            m_TickService.AddUpdateCallback(OnUpdate, TickOrder);
            IsTicking = true;
            return true;
        }

        public virtual bool StopTicking()
        {
            if (!IsTicking)
            {
                return false;
            }

            m_TickService.RemoveUpdateCallback(OnUpdate);
            return true;
        }

        public void RefreshTickOrder(int newOrder)
        {
            if (newOrder == TickOrder)
            {
                return;
            }

            TickOrder = newOrder;
            if (!IsTicking)
            {
                return;
            }

            // 既然在 Ticking 就拿出来，重新加入（排序）。
            m_TickService.RemoveUpdateCallback(OnUpdate);
            m_TickService.AddUpdateCallback(OnUpdate, TickOrder);
        }

        protected abstract void OnUpdate(TimeStruct timeStruct);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopTicking();
                m_TickService = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}