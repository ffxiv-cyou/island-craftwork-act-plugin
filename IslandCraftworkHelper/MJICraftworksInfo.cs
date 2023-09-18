using Lotlab.PluginCommon.FFXIV.Parser;
using System.Runtime.InteropServices;

namespace IslandCraftworkHelper
{
    public class MJICraftworksInfo
    {
        public IPCHeader ipc; // IPC 包头

        public byte currPopPattern;
        public byte nextPopPattern;
        public byte[] demands;

        public MJICraftworksInfo(NetworkParser parser, byte[] packet, uint packetLen)
        {
            ipc = parser.ParseAsPacket<IPCHeader>(packet);
            if (packetLen < 2)
                return;

            int offset = Marshal.SizeOf<IPCHeader>();
            currPopPattern = packet[offset++];
            nextPopPattern = packet[offset++];
            demands = new byte[packetLen - 2];
            for (int i = 0; i < packetLen - 2; i++)
            {
                demands[i] = packet[offset++];
            }
        }

        public bool IsValid()
        {
            if (currPopPattern > 101 || nextPopPattern > 101) return false;
            if (demands[0] != 0) return false;
            for (int i = 0; i < demands.Length; i++)
            {
                if (demands[i] >> 4 > 4 || (demands[i] & 0x0F) > 5) return false;
            }
            return true;
        }

        public string ToArray()
        {
            var bytes = new byte[demands.Length + 2];
            bytes[0] = currPopPattern;
            bytes[1] = nextPopPattern;
            demands.CopyTo(bytes, 2);
            return bytes.ToHexString();
        }
    }
}
