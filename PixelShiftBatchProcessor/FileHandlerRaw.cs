using PixelShiftBatchProcessor.NewFolder;
using PixelShiftBatchProcessor.Tiff.Header;
using System.Buffers.Binary;
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
            Console.WriteLine($"Verarbeite Raw-Datei: {m_Fielpath}");

            var testresult = new List<IFd>();
            var bytes = File.ReadAllBytes(m_Fielpath);
            var span = bytes.AsSpan();

            var header = HeaderParserTiff.ParseHeader(span.Slice(0, 8));

            m_IsLittleEndian = header.IsLittleEndian;
            m_IsTiff = header.IsTiff;
            var offsetIfd0InByte = header.ByteOffset;

            var anzahlIfd = BitConverter.ToUInt16(span.Slice((int)offsetIfd0InByte, 2));

            var currentOffset = offsetIfd0InByte + 2;
            for (var i = 0; i < anzahlIfd; i++)
            {
                var ifdSpan = span.Slice((int)currentOffset, 12);

                var kennzeichnungsnummer = m_IsLittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(ifdSpan.Slice(0, 2)) : BinaryPrimitives.ReadUInt16BigEndian(ifdSpan.Slice(0, 2));
                var feldtyp = m_IsLittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(ifdSpan.Slice(2, 2)) : BinaryPrimitives.ReadUInt16BigEndian(ifdSpan.Slice(2, 2));

                var anzahlElemente = m_IsLittleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(ifdSpan.Slice(4, 4)) : BinaryPrimitives.ReadUInt32BigEndian(ifdSpan.Slice(4, 4));

                var byteProElement = Helper.GetSpeicherbedarfInByte(feldtyp);
                var byteFuerIfdContent = byteProElement * anzahlElemente;

                var offset = currentOffset  + 8;
                var sprungweite = 4;
                if(byteFuerIfdContent > 4)
                {
                    offset = m_IsLittleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(ifdSpan.Slice(8, 4)) : BinaryPrimitives.ReadUInt32BigEndian(ifdSpan.Slice(8, 4));
                    sprungweite = byteProElement;
                }

                if (feldtyp == Feldtyp.ASCII)
                {
                    var asciiContent = new StringBuilder();

                    for (var j = 0; j < anzahlElemente; j++)
                    {
                        var content = GetContent(span.Slice((int)offset, sprungweite), feldtyp, m_IsLittleEndian);
                        asciiContent.Append(content);

                        if (byteFuerIfdContent > 4)
                            offset += byteProElement;
                    }

                    var ifd = new IFd()
                    {
                        Kennzeichnungsnummer = kennzeichnungsnummer,
                        Feldtyp = feldtyp,
                        Content = asciiContent.ToString(),
                    };

                    testresult.Add(ifd);
                    currentOffset += 12;
                    continue;
                }

                for (var j = 0; j < anzahlElemente; j++)
                {
                    var content = GetContent(span.Slice((int)offset, sprungweite), feldtyp, m_IsLittleEndian);

                    var ifd = new IFd()
                    {
                        Kennzeichnungsnummer = kennzeichnungsnummer,
                        Feldtyp = feldtyp,
                        Content = content,
                    };
                    testresult.Add(ifd);

                    if (byteFuerIfdContent > 4)
                        offset += byteProElement;
                }

                currentOffset += 12;
            }

            //BinaryPrimitives --> https://learn.microsoft.com/de-de/dotnet/api/system.buffers.binary.binaryprimitives?view=net-7.0 --> Bessere Klasse als BitConverter.
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

    class IFd
    {
        public int Kennzeichnungsnummer { get; set; }
        public ushort Feldtyp { get; set; }
        public object Content { get; set; }

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
                    _ => string.Empty
                };
            }
        }
    }
}
