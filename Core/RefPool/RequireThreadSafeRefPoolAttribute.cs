using System;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Attribute used for any type that needs a <see cref="ThreadSafeRefPool{TObject}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RequireThreadSafeRefPoolAttribute : Attribute
    {
        // Empty.
    }
}