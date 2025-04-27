namespace PixelShiftBatchProcessor
{
    public class Feldtyp
    {
        public const ushort BYTE = 1; // 8-Bit unsigned integer
        public const ushort ASCII = 2; // 8-Bit Zeichen (ASCII) – 1 Byte pro Zeichen, letzter Buchstabe ist meist NUL-Terminator	
        public const ushort SHORT = 3; // 16-Bit unsigned integer
        public const ushort LONG = 4; // 32-Bit unsigned Integer
        public const ushort RATIONAL = 5; // Zwei LONGs (Zähler und Nenner, beide jeweils 4 Bytes) → Bruchdarstellung	
        public const ushort SBYTE = 6; // 8-Bit signed integer
        public const ushort UNDEFINED = 7; // 8-Bit unbekannt (Rohdaten)
        public const ushort SSHORT = 8; // 16-Bit signed integer
        public const ushort SLONG = 9; // 32-Bit signed integer
        public const ushort SRATIONAL = 10; // Zwei SLONGs (Zähler und Nenner als signed, jeweils 4 Bytes) → Bruchdarstellung	
        public const ushort FLAOT = 11; // 32 Bit floating point
        public const ushort DOUBLE = 12; // 64 Bit floating point
    }
}
