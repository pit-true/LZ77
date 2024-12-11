using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalapagosItemManager
{
    internal class LZ77Handler
    {
        public static byte[] GetDecompressedData(byte[] RomAllData, int index)
        {
            if (RomAllData == null || index < 0 || index + 4 > RomAllData.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of bounds for RomAllData of length {RomAllData.Length}.");
            }

            uint decompressedLength = (uint)((RomAllData[index + 2] << 8) | RomAllData[index + 1]);

            byte[] destBuffer = new byte[decompressedLength];

            int bytesWritten = 0;
            int offset = index + 4;

            while (bytesWritten < decompressedLength)
            {
                if (offset >= RomAllData.Length)
                {
                    throw new InvalidOperationException($"Offset {offset} exceeds the bounds of RomAllData (length {RomAllData.Length}).");
                }

                byte flags = RomAllData[offset];
                offset++;

                for (int i = 0; i < 8 && bytesWritten < decompressedLength; ++i)
                {
                    if (offset >= RomAllData.Length)
                    {
                        throw new InvalidOperationException($"Offset {offset} exceeds the bounds of RomAllData (length {RomAllData.Length}).");
                    }

                    bool type = (flags & (0x80 >> i)) != 0;

                    if (!type)
                    {
                        byte value = RomAllData[offset];
                        offset++;
                        destBuffer[bytesWritten] = value;
                        bytesWritten++;
                    }
                    else
                    {
                        if (offset + 2 > RomAllData.Length)
                        {
                            throw new InvalidOperationException($"Offset {offset} exceeds the bounds of RomAllData for 2-byte read.");
                        }

                        ushort value = BitConverter.ToUInt16(RomAllData, offset);
                        offset += 2;

                        ushort disp = (ushort)(((value & 0xf) << 8) | (value >> 8));
                        byte n = (byte)((value >> 4) & 0xf);

                        if (bytesWritten - disp - 1 < 0)
                        {
                            throw new InvalidOperationException($"Invalid displacement {disp}. Cannot reference before the start of destBuffer.");
                        }

                        if (bytesWritten + n + 3 > decompressedLength)
                        {
                            n = (byte)(decompressedLength - bytesWritten - 3);
                        }

                        for (int j = 0; j < n + 3; ++j)
                        {
                            destBuffer[bytesWritten] = destBuffer[bytesWritten - disp - 1];
                            bytesWritten++;
                        }
                    }
                }
            }

            return destBuffer;
        }
    }
}
