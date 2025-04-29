namespace PixelShiftBatchProcessor.Tiff.Header
{
    public class HeaderDto
    {
        public bool IsTiff { get; set; } //TODO: Irgendwann generischer mit Enumeration
        public bool IsLittleEndian { get; set; }
        public uint ByteOffset { get; set; }
    }
}
