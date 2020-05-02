using System;

namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Attribute used for property injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
    }
}