﻿using Lidgren.Network;

namespace Jazz2.Networking.Packets.Server
{
    public struct PlayerAddHealth : IServerPacket
    {
        public NetConnection SenderConnection { get; set; }

        byte IServerPacket.Type => 21;


        public byte Index;
        public byte Amount;

        void IServerPacket.Read(NetIncomingMessage msg)
        {
            Index = msg.ReadByte();

            Amount = msg.ReadByte();
        }

        void IServerPacket.Write(NetOutgoingMessage msg)
        {
            msg.Write((byte)Index);

            msg.Write((byte)Amount);
        }
    }
}