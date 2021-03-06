﻿using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Net
{
    /// <summary>
    /// Interface of the network service that manages persistent network connections as <see cref="INetChannel"/>s.
    /// </summary>
    public interface INetService : ILifeCycle
    {
        INetChannelFactory ChannelFactory { get; set; }

        IList<INetChannel> GetChannels();

        void GetChannels(List<INetChannel> channels);

        INetChannel GetChannel(string name);

        bool TryGetChannel(string name, out INetChannel channel);

        bool HasChannel(string name);

        INetChannel AddChannel(string name, string typeKey, INetChannelHandler handler, int receivePacketHeaderLength);
    }
}