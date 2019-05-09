namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Base class of all modules.
    /// </summary>
    public abstract class BaseModule : IModule
    {
        private bool m_Inited = false;
        private bool m_HasShutDown = false;

        /// <summary>
        /// Initialize this module.
        /// </summary>
        public virtual void Init()
        {
            if (m_Inited)
            {
                throw new System.InvalidOperationException("Trying to initialize twice.");
            }

            m_Inited = true;
        }

        /// <summary>
        /// Shut down this module.
        /// </summary>
        public virtual void ShutDown()
        {
            m_HasShutDown = true;
        }

        /// <summary>
        /// Generic tick method.
        /// </summary>
        /// <param name="timeStruct">Time struct.</param>
        public virtual void Update(TimeStruct timeStruct)
        {
            CheckStateOrThrow();
        }

        /// <summary>
        /// Check whether the module is in an available state.
        /// </summary>
        protected internal virtual void CheckStateOrThrow()
        {
            if (!m_Inited)
            {
                throw new System.InvalidOperationException("Not initialized.");
            }

            if (m_HasShutDown)
            {
                throw new System.InvalidOperationException("Already shut down.");
            }
        }
    }
}