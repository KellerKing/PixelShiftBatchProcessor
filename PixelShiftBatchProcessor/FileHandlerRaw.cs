using PixelShiftBatchProcessor.NewFolder;
using PixelShiftBatchProcessor.Tiff.Header;
using PixelShiftBatchProcessor.Tiff.Image_File_Directory;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace PixelShiftBatchProcessor
{
    internal class FileHandlerRaw //Temp Klasse | Logger übergeben statt Konsolenausgabe
    {
        private readonly string m_Fielpath;

        private bool m_IsLittleEndian;
        private bool m_IsTiff;
        public FileHandlerRaw(string fielpath) //https://docs.fileformat.com/image/tiff/
        { //https://dpb587.me/entries/tiff-ifd-and-subifd-20240226
            //https://files.dnb.de/nestor/kurzartikel/thema_06-TIFF.pdf
            //https://www.loc.gov/preservation/digital/formats/content/tiff_tags.shtml



            //https://www.itu.int/itudoc/itu-t/com16/tiff-fx/docs/tiff6.pdf
            m_Fielpath = fielpath;
        }

        public void Process()
        {
            // Hier wird die Raw-Datei verarbeitet
            Console.WriteLine($"Verarbeite Raw-Datei: {m_Fielpath}"); //https://www.exiftool.org/TagNames/Sony.html

            var testresult = new List<IFd>();
            var bytes = File.ReadAllBytes(m_Fielpath);
            var span = bytes.AsSpan();

            var header = HeaderParserTiff.ParseHeader(span.Slice(0, 8));

            m_IsLittleEndian = header.IsLittleEndian;
            m_IsTiff = header.IsTiff;
            var offsetIfd0InByte = header.ByteOffset;
            var offsetCurrentIfd = offsetIfd0InByte;
            var hasNextIfd = false;

            do
            {
                var ifd = GetIfd(m_IsLittleEndian, testresult.Count, span, offsetCurrentIfd);
                var byteWoOffsetNaechsteIfdAusgelesenWird = OffsetNextifdStart(offsetCurrentIfd, ifd.Entries.Count);
                offsetCurrentIfd = Convert.ToUInt32(GetContent(span.Slice((int)byteWoOffsetNaechsteIfdAusgelesenWird, 4), Feldtyp.LONG, m_IsLittleEndian));
                hasNextIfd = offsetCurrentIfd > 0;
                testresult.Add(ifd);

            } while (hasNextIfd);


            var ifdsToCheck = new Queue<IFd>(testresult);

            while (ifdsToCheck.Count > 0) 
            {
                var current  = ifdsToCheck.Dequeue();
                var subIfs = GetSubIfds(current.Entries, m_IsLittleEndian, span, current.Nummer);
                current.SubIfds = subIfs;

               foreach ( var subIfd in subIfs )
                    ifdsToCheck.Enqueue(subIfd);
            }
            //BinaryPrimitives --> https://learn.microsoft.com/de-de/dotnet/api/system.buffers.binary.binaryprimitives?view=net-7.0 --> Bessere Klasse als BitConverter.
        }

        private static IEnumerable<IFd> GetSubIfds(IEnumerable<IfdEntry> entries, bool isLittleEndian, Span<byte> span, int ifdNummer)
        {
            var result = new List<IFd>();
            foreach (var entry in entries)
            {
                if (new[] { 34665, 330 }.Any(x => x == entry.Kennzeichnungsnummer))
                {
                    Type valueType = entry.Content.GetType();

                    if (valueType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var arrayContent = (List<object>)entry.Content;
                        var resultContent = new List<IFd>();
                        foreach (var item in arrayContent)
                        {
                           result.Add(GetIfd(isLittleEndian, ifdNummer, span, (uint)item));
                        }
                    }
                    else
                    {
                        result.Add(GetIfd(isLittleEndian, ifdNummer, span, Convert.ToUInt32(entry.Content)));
                    }
                }
            }
            return result;
        }

        private static uint OffsetNextifdStart(uint offsetCurrentIfd, int anzahlEintraegeInCurrentIfd)
        {
            var groesseCurrentOffset = anzahlEintraegeInCurrentIfd * 12;
            return (uint)(offsetCurrentIfd + 2 + groesseCurrentOffset);
        }

        private static IFd GetIfd(bool isLittleEndian, int nummer, Span<byte> fullFile, uint offset)
        {
            var ifd = new IFd { IndexAbsolut = (int)offset, Nummer = nummer, Entries = new List<IfdEntry>() };
            var anzahlEntriesInIfd = BitConverter.ToUInt16(fullFile.Slice((int)offset, 2));
            var currentOffset = offset + 2;

            for (var i = 0; i < anzahlEntriesInIfd; i++)
            {
                var entry = GetEntry(isLittleEndian, fullFile, currentOffset);
                ifd.Entries.Add(entry);
                currentOffset += 12;
            }

            return ifd;
        }

        private static IfdEntry GetEntry(bool isLittleEndian, Span<Byte> fullFile, uint indexEntry)
        {
            var entrySpan = fullFile.Slice((int)indexEntry, 12);
            var kennzeichnungsnummer = isLittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(entrySpan.Slice(0, 2)) : BinaryPrimitives.ReadUInt16BigEndian(entrySpan.Slice(0, 2));
            var feldtyp = isLittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(entrySpan.Slice(2, 2)) : BinaryPrimitives.ReadUInt16BigEndian(entrySpan.Slice(2, 2));

            var anzahlElemente = isLittleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(entrySpan.Slice(4, 4)) : BinaryPrimitives.ReadUInt32BigEndian(entrySpan.Slice(4, 4));

            var byteProElement = Helper.GetSpeicherbedarfInByte(feldtyp);
            var byteFuerIfdContent = byteProElement * anzahlElemente;

            var offset = indexEntry + 8;
            var sprungweite = 4;
            if (byteFuerIfdContent > 4)
            {
                offset = isLittleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(entrySpan.Slice(8, 4)) : BinaryPrimitives.ReadUInt32BigEndian(entrySpan.Slice(8, 4));
                sprungweite = byteProElement;
            }

            if (feldtyp == Feldtyp.ASCII)
            {
                var asciiContent = new StringBuilder();

                for (var j = 0; j < anzahlElemente; j++)
                {
                    var content = GetContent(fullFile.Slice((int)offset, sprungweite), feldtyp, isLittleEndian);
                    asciiContent.Append(content);

                    if (byteFuerIfdContent > 4)
                        offset += byteProElement;
                }

                var ifd = new IfdEntry()
                {
                    Kennzeichnungsnummer = kennzeichnungsnummer,
                    Feldtyp = feldtyp,
                    Content = asciiContent.ToString(),
                    IndexAbsolut = indexEntry
                };

                return ifd;
            }
            else
            {
                var contentList = new List<object>();
                for (var j = 0; j < anzahlElemente; j++)
                {
                    var content = GetContent(fullFile.Slice((int)offset, sprungweite), feldtyp, isLittleEndian);
                    contentList.Add(content);

                    if (byteFuerIfdContent > 4)
                        offset += byteProElement;
                }

                var ifd = new IfdEntry()
                {
                    Kennzeichnungsnummer = kennzeichnungsnummer,
                    Feldtyp = feldtyp,
                    Content = contentList,
                    IndexAbsolut = indexEntry
                };

                return ifd;
            }
        }

        private static object GetContent(Span<byte> content, ushort feldtyp, bool isLittleEndian) //https://www.loc.gov/preservation/digital/formats/content/tiff_tags.shtml
        {
            if (content.Length == 0)
            {
                throw new ArgumentException("Content cannot be empty.", nameof(content));
            }

            if (isLittleEndian)
            {

                return feldtyp switch
                {
                    1 => content[0],
                    2 => Encoding.ASCII.GetString(content),
                    3 => BinaryPrimitives.ReadUInt16LittleEndian(content),
                    4 => BinaryPrimitives.ReadUInt32LittleEndian(content),
                    5 => new Rational(
                        BinaryPrimitives.ReadUInt32LittleEndian(content.Slice(0, 4)),
                        BinaryPrimitives.ReadUInt32LittleEndian(content.Slice(4, 4))),
                    6 => content[0],
                    7 => content[0],
                    8 => content[0],
                    9 => content[0],
                    10 => content[0],
                    _ => throw new ArgumentException("Content cannot be empty.", nameof(content)),
                };

            }

            return feldtyp switch
            {
                1 => content[0],
                2 => Encoding.ASCII.GetString(content),
                3 => BinaryPrimitives.ReadUInt16BigEndian(content),
                4 => BinaryPrimitives.ReadUInt32BigEndian(content),
                5 => BinaryPrimitives.ReadUInt32BigEndian(content),
                6 => content[0],
                7 => content[0],
                8 => content[0],
                9 => content[0],
                10 => content[0],
                _ => throw new ArgumentException("Content cannot be empty.", nameof(content)),
            };
        }

    }

    struct Rational
    {
        public Rational(uint zaehler, uint nenner)
        {
            Zaehler = zaehler;
            Nenner = nenner;
        }

        public uint Zaehler { get; set; }
        public uint Nenner { get; set; }

        public double Wert => Nenner == 0 ? double.NaN : (double)Zaehler / Nenner;

        public override readonly string ToString()
        {
            return $"{Zaehler}/{Nenner}";
        }
    }

    class IfdEntry
    {
        public int Kennzeichnungsnummer { get; set; }
        public ushort Feldtyp { get; set; }
        public object Content { get; set; }
        public uint IndexAbsolut { get; set; }

        public string KennzeichnungHex
        {
            get
            {
                return Kennzeichnungsnummer.ToString("X4");
            }
        }

        public string KennzeichnungName
        {
            get
            {
                return Kennzeichnungsnummer switch
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
                    296 => "ResolutionUnit",
                    305 => "Software",
                    306 => "DateTime",
                    315 => "Artist",
                    330 => "SubIFDs",
                    513 => "JPEGInterchangeFormat",
                    514 => "JPEGInterchangeFormatLength",
                    531 => "YCbCrPositioning",
                    700 => "XMP",
                    33432 => "Copyright",
                    33434 => "ExposureTime",
                    33437 => "FNumber",
                    34665 => "Exif IFD",
                    34853 => "GPSInfo",
                    34850 => "ExposureProgram",
                    34855 => "ISOSpeedRatings",
                    34864 => "SensitivityType",
                    34866 => "RecommendedExposureIndex",
                    36864 => "ExifVersion",
                    36867 => "DateTimeOriginal",
                    36868 => "DateTimeDigitized",
                    36880 => "???",
                    36881 => "???",
                    36882 => "???",
                    37121 => "ComponentsConfiguration",
                    37122 => "CompressedBitsPerPixel",
                    37379 => "BrightnessValue",
                    37380 => "ExposureBiasValue",
                    37381 => "MaxApertureValue",
                    37383 => "MeteringMode",
                    37384 => "LightSource",
                    37385 => "Flash",
                    37386 => "FocalLength",
                    37500 => "MakerNote", //Hier bin ich richtig
                    37510 => "UserComment",
                    40960 => "FlashpixVersion",
                    40961 => "ColorSpace",
                    40962 => "PixelXDimension",
                    40963 => "PixelYDimension",
                    40965 => "Interoperability IFD",
                    41728 => "FileSource",
                    41729 => "SceneType",
                    41985 => "CustomRendered",
                    41986 => "ExposureMode",
                    41987 => "WhiteBalance",
                    41988 => "DigitalZoomRatio",
                    41989 => "FocalLengthIn35mmFilm",
                    41990 => "SceneCaptureType",
                    41992 => "Contrast",
                    41993 => "Saturation",
                    41994 => "Sharpness",
                    42034 => "LensSpecification",
                    42036 => "LensModel",
                    50341 => "PrintImageMatching",
                    50740 => "DNGPrivateData",
                    _ => string.Empty
                };
            }
        }
    }

    class IFd
    {
        public int IndexAbsolut { get; set; }
        public int Nummer { get; set; }
        public List<IfdEntry> Entries { get; set; }
        public IEnumerable<IFd> SubIfds { get; set; } = Enumerable.Empty<IFd>();
    }
}
