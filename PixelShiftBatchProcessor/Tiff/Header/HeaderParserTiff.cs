using PixelShiftBatchProcessor.NewFolder;
using System.Buffers.Binary;
using System.Text;

namespace PixelShiftBatchProcessor.Tiff.Header
{
    class HeaderParserTiff
    {
        public static HeaderDto ParseHeader(Span<byte> bytes)
        {

            if (bytes == null) throw new ArgumentNullException("bytes");
            if (bytes.Length < 8) return new HeaderDto();
            
            
            var endian = Encoding.ASCII.GetString(bytes.Slice(0, 2));
            var isLittleEndian = Endian.LITTLE_ENDIAN.Equals(endian, StringComparison.OrdinalIgnoreCase);
            var magicNumber = isLittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(2,2)) : BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(2,2));
            
            if (magicNumber != 42) return new HeaderDto();

            var byteOffset = isLittleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(4, 4)) : BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(4, 4));

            return new HeaderDto
            {
                IsLittleEndian = isLittleEndian,
                IsTiff = true,
                ByteOffset = byteOffset,
            };
        }
    }
}

