using System;

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

        public static string GetKennzeichnung(UInt16 kennzeichnungDezimal)
        {
            return kennzeichnungDezimal switch
            {
                254 => "NewSubfileType",
                255 => "SubfileType",
                256 => "ImageWidth",
                257 => "ImageLength",
                258 => "BitsPerSample",
                259 => "Compression",
                262 => "PhotometricInterpretation",
                263 => "Threshholding",
                264 => "CellWidth",
                265 => "CellLength",
                266 => "FillOrder",
                269 => "DocumentName",
                270 => "ImageDescription",
                271 => "Make",
                272 => "Model",
                273 => "StripOffsets",
                274 => "Orientation",
                277 => "SamplesPerPixel",
                278 => "RowsPerStrip",
                279 => "StripByteCounts",
                280 => "MinSampleValue",
                281 => "MaxSampleValue",
                282 => "XResolution",
                283 => "YResolution",
                284 => "PlanarConfiguration",
                285 => "PageName",
                286 => "XPosition",
                287 => "YPosition",
                288 => "FreeOffsets",
                289 => "FreeByteCounts",
                290 => "GrayResponseUnit",
                291 => "GrayResponseUnit",
                _ => string.Empty
            };

        }
    }
}
