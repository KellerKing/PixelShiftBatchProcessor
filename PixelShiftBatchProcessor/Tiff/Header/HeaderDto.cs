namespace PixelShiftBatchProcessor.Tiff.Header
{
    public class HeaderDto
    {
        public bool IsTiff { get; set; }
        public bool IsLittleEndian { get; set; }
        public int Offset { get; set; }
    }
}
