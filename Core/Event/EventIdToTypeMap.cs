using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Event ID to type bijection.
    /// </summary>
    public static class EventIdToTypeMap
    {
        private static int s_CurrentId = 1;

        private static Dictionary<int, Type> s_EventIdToType = new Dictionary<int, Type>();
        private static Dictionary<Type, int> s_EventTypeToId = new Dictionary<Type, int>();

        /// <summary>
        /// Generate a new event ID for a given event type.
        /// </summary>
        /// <typeparam name="EventType">Event type.</typeparam>
        /// <returns>Event ID.</returns>
        public static int Generate<EventType>() where EventType : BaseEventArgs
        {
            var type = typeof(EventType);
            if (type.IsAbstract || !type.IsClass)
            {
                throw new ArgumentException(Utility.Text.Format("Type '{0}' is not a concrete class.", type.FullName));
            }

            var eventId = s_CurrentId++;
            s_EventIdToType.Add(eventId, type);
            s_EventTypeToId.Add(type, eventId);

            return eventId;
        }

        /// <summary>
        /// Check whether a given event type has an ID.
        /// </summary>
        /// <typeparam name="EventType">Event type.</typeparam>
        /// <returns>Whether a given event type has an ID.</returns>
        public static bool HasEventType<EventType>() where EventType : BaseEventArgs
        {
            var type = typeof(EventType);
            return s_EventTypeToId.ContainsKey(type);
        }

        /// <summary>
        /// Check whether an event ID is used by some event type.
        /// </summary>
        /// <param name="eventId">Event ID.</param>
        /// <returns>Whether an event ID is used by some event type.</returns>
        public static bool HasEventId(int eventId)
        {
            return s_EventIdToType.ContainsKey(eventId);
        }

        /// <summary>
        /// Get event type for a given event ID.
        /// </summary>
        /// <param name="eventId">Event ID.</param>
        /// <returns>Event Type.</returns>
        public static Type GetEventType(int eventId)
        {
            return s_EventIdToType[eventId];
        }

        /// <summary>
        /// Get event ID for a given event type.
        /// </summary>
        /// <typeparam name="EventType">Event type.</typeparam>
        /// <returns>Event ID.</returns>
        public static int GetEventId<EventType>() where EventType : BaseEventArgs
        {
            return s_EventTypeToId[typeof(EventType)];
        }
    }
}