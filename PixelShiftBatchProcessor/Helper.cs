namespace PixelShiftBatchProcessor
{
    public class Helper
    {
        public static ushort GetSpeicherbedarfInByte(uint feldtyp)
        {
            return feldtyp switch
            {
                Feldtyp.BYTE => 1,
                Feldtyp.ASCII => 1,
                Feldtyp.SHORT => 2,
                Feldtyp.LONG => 4,
                Feldtyp.RATIONAL => 8,
                Feldtyp.SBYTE => 1,
                Feldtyp.UNDEFINED => 1,
                Feldtyp.SSHORT => 2,
                Feldtyp.SLONG => 4,
                Feldtyp.SRATIONAL => 8,
                Feldtyp.FLAOT => 4,
                Feldtyp.DOUBLE => 8,
                _ => throw new ArgumentException("Invalid field type.", nameof(feldtyp)),
            };
        }
    }
}
