namespace COL.UnityGameWheels.Core
{
    public abstract class BaseLifeCycleService : ILifeCycle
    {
        private bool m_IsInited = false;
        private bool m_IsShut = false;

        /// <summary>
        /// Initialize this module.
        /// </summary>
        public virtual void OnInit()
        {
            if (m_IsInited)
            {
                throw new System.InvalidOperationException("Trying to initialize twice.");
            }

            m_IsInited = true;
        }

        /// <summary>
        /// Shut down this module.
        /// </summary>
        public virtual void OnShutdown()
        {
            m_IsShut = true;
        }


        /// <summary>
        /// Check whether the module is in an available state.
        /// </summary>
        protected internal virtual void CheckStateOrThrow()
        {
            if (!m_IsInited)
            {
                throw new System.InvalidOperationException("Not initialized.");
            }

            if (m_IsShut)
            {
                throw new System.InvalidOperationException("Already shut.");
            }
        }
    }
}