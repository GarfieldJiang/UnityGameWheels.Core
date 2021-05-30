using System;

namespace COL.UnityGameWheels.Core
{
    public abstract class TickableService : ITickableService, IDisposable
    {
        protected ITickService m_TickService;

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

            // TODO: More flexibility with tick order?
            m_TickService.AddUpdateCallback(OnUpdate, 0);
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