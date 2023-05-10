using Lotlab.PluginCommon.FFXIV.Parser;
using System.Runtime.InteropServices;

namespace IslandCraftworkHelper
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MJICraftworksInfo
    {
        public IPCHeader ipc; // IPC 包头

        public byte currPopPattern;
        public byte nextPopPattern;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 78)]
        public byte[] demands;

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
