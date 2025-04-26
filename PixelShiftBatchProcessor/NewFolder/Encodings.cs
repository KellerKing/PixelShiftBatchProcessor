namespace PixelShiftBatchProcessor.NewFolder
{
    internal class Encodings
    {
        private const string LittleEndian = "49-49";
        private const string BigEndian = "4D-4D";
        private const string LittleEndianTiff = "2A-00";
        private const string BigEndianTiff = "00-2A";
        public static bool IsLittleEndian(string info) //UTF8
        {
            return info.Equals(LittleEndian, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTiff(string info, bool isLittleEndian)
        {
            var hexNummer = isLittleEndian ? LittleEndianTiff : BigEndianTiff;
            return info.Equals(LittleEndianTiff, StringComparison.OrdinalIgnoreCase);
        }
    }
}
