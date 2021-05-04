using System;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Life cycle with an OnUpdate tick method.
    /// </summary>
    public abstract class TickableLifeCycleService : BaseLifeCycleService
    {
        /// <summary>
        /// The tick service instance used.
        /// </summary>
        [Ioc.Inject]
        public ITickService TickService { get; set; }

        private int? m_TickOrder = null;

        /// <summary>
        /// Tick order.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public int TickOrder
        {
            get
            {
                if (m_TickOrder == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_TickOrder.Value;
            }

            set
            {
                if (m_TickOrder != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_TickOrder = value;
            }
        }

        protected abstract void OnUpdate(TimeStruct timeStruct);

        public override void OnInit()
        {
            base.OnInit();
            TickService.AddUpdateCallback(OnUpdate, TickOrder);
        }

        public override void OnShutdown()
        {
            TickService.RemoveUpdateCallback(OnUpdate);
            base.OnShutdown();
        }
    }
}