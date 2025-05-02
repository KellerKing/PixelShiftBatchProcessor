using System;

namespace PixelShiftBatchProcessor.Tiff.Image_File_Directory
{
    internal class IfdEntryDto
    {
        public ushort TagDezimal { get; init; }
        public ushort Feldtyp { get; init; }
        
        public string TagHexadezimal 
        {
            get
            {
                return TagDezimal.ToString("X4");
            }
        }
    }
}
