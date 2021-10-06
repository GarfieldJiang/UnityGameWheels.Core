using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Net
{
    public class NetService : TickableService, INetService
    {
        private readonly List<INetChannel> m_Channels = new List<INetChannel>();
        private readonly List<INetChannel> m_CopiedChannels = new List<INetChannel>();

        public INetChannelFactory ChannelFactory { get; } = null;

        public NetService(INetChannelFactory channelFactory, ITickService tickService) : base(tickService)
        {
            ChannelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
        }

        public INetChannel AddChannel(string name, string typeKey, INetChannelHandler handler, int receivePacketHeaderLength)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid name.", nameof(name));
            }

            if (HasChannel(name))
            {
                throw new InvalidOperationException($"Already exists a channel with name '{name}'.");
            }

            var channel = ChannelFactory.Create(name, typeKey, handler, receivePacketHeaderLength);
            m_Channels.Add(channel);
            return channel;
        }

        public INetChannel GetChannel(string name)
        {
            foreach (var channel in m_Channels)
            {
                if (channel.Name == name)
                {
                    return channel;
                }
            }

            throw new KeyNotFoundException($"Channel with name '{name}' doesn't exist.");
        }

        public IList<INetChannel> GetChannels()
        {
            return m_Channels.ToArray();
        }

        public void GetChannels(List<INetChannel> channels)
        {
            channels.Clear();
            channels.AddRange(m_Channels);
        }

        public bool HasChannel(string name)
        {
            foreach (var channel in m_Channels)
            {
                if (channel.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var channel in m_Channels)
                {
                    channel.Close();
                }

                m_Channels.Clear();
            }

            base.Dispose(disposing);
        }

        public bool TryGetChannel(string name, out INetChannel channel)
        {
            channel = null;
            foreach (var ch in m_Channels)
            {
                if (ch.Name == name)
                {
                    channel = ch;
                    return true;
                }
            }

            return false;
        }

        protected override void OnUpdate(TimeStruct timeStruct)
        {
            GetChannels(m_CopiedChannels);
            foreach (var channel in m_CopiedChannels)
            {
                channel.Update(timeStruct);
            }
        }
    }
}