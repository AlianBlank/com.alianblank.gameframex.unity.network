using System.IO;
using GameFrameX.Runtime;
using ProtoBuf;

namespace GameFrameX.Network.Runtime
{
    public sealed class DefaultPacketSendHeaderHandler : IPacketSendHeaderHandler, IPacketHandler
    {
        /// <summary>
        /// 网络包长度
        /// </summary>
        private const int NetPacketLength = 4;

        /// <summary>
        /// 消息码
        /// </summary>
        private const int NetCmdIdLength = 4;

        /// <summary>
        /// 消息体长度
        /// </summary>
        private const int NetBodyLength = 4;

        /// <summary>
        /// 消息编号
        /// </summary>
        private const int NetUniqueIdLength = 8;


        public DefaultPacketSendHeaderHandler()
        {
            // 4 + 4 + 4 + 4 
            PacketHeaderLength = NetPacketLength + NetUniqueIdLength + NetCmdIdLength + NetBodyLength;
            m_CachedByte = new byte[PacketHeaderLength];
        }

        /// <summary>
        /// 消息包头长度
        /// </summary>
        public int PacketHeaderLength { get; }

        /// <summary>
        /// 获取网络消息包协议编号。
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        public int PacketLength { get; private set; }


        int m_Count = 0;
        private int m_Offset = 0;
        private readonly byte[] m_CachedByte;

        public bool Handler<T>(T messageObject, MemoryStream destination, out byte[] messageBodyBuffer) where T : MessageObject
        {
            m_Offset = 0;
            messageBodyBuffer = SerializerHelper.Serialize(messageObject);
            var messageType = messageObject.GetType();
            Id = ProtoMessageIdHandler.GetReqMessageIdByType(messageType);
            var messageLength = messageBodyBuffer.Length;
            PacketLength = PacketHeaderLength + messageLength;
            // 数据包总大小
            m_CachedByte.WriteInt(PacketLength, ref m_Offset);
            // 消息编号
            m_CachedByte.WriteLong(messageObject.UniqueId, ref m_Offset);
            // 消息ID
            m_CachedByte.WriteInt(Id, ref m_Offset);
            // 消息体长度
            m_CachedByte.WriteInt(messageLength, ref m_Offset);
            destination.Write(m_CachedByte, 0, PacketHeaderLength);
            return true;
        }
    }
}