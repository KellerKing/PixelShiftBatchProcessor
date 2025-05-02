using System;
using System.Buffers.Binary;

namespace PixelShiftBatchProcessor.Tiff.Image_File_Directory
{
    internal class IfdService
    {
        public static IfdEntryDto GetIfdEntry(Span<byte> bytes, bool isLittleEndian)
        {
            var typ = GetIfdType(bytes, isLittleEndian);
            var isKnownType = IsKnownType(typ);
            throw new NotImplementedException("//TODO: Baustelle. Wird nach hinten geschoben weils nur um Performance geht");
        }

        private static bool IsKnownType(ushort type)
        {
            return false;
        }

        private static ushort GetIfdType(Span<byte> bytes, bool isLittleEndian) 
        {
            var span = bytes.Slice(0, 2);
            return isLittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(span) : BinaryPrimitives.ReadUInt16BigEndian(span);
        }
    }
}
