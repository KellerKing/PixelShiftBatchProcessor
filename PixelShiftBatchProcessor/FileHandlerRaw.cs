using PixelShiftBatchProcessor.NewFolder;
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
            m_Fielpath = fielpath;
        }

        public void Process()
        {
            // Hier wird die Raw-Datei verarbeitet
            Console.WriteLine($"Verarbeite Raw-Datei: {m_Fielpath}");

            var testresult = new List<IFd>();
            var bytes = File.ReadAllBytes(m_Fielpath);
            var span = bytes.AsSpan();
            //var str = System.Text.Encoding.Default.GetString(bytes);
            //File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "res.txt"), str);

            m_IsLittleEndian = Encodings.IsLittleEndian(BitConverter.ToString(bytes, 0, 2));
            m_IsTiff = Encodings.IsTiff(BitConverter.ToString(bytes, 2, 2), m_IsLittleEndian);

            var offsetIfd0Span = span.Slice(4, 4);

            if (!m_IsLittleEndian)
            {
                offsetIfd0Span.Reverse();
            }

            var offsetIfd0InByte = BitConverter.ToUInt32(offsetIfd0Span);
            var anzahlIfd = BitConverter.ToUInt16(span.Slice((int)offsetIfd0InByte, 2));

            var currentOffset = offsetIfd0InByte + 2;
            for (var i = 0; i < anzahlIfd; i++)
            {
                var ifdSpan = span.Slice((int)currentOffset, 12);
                var ifd = m_IsLittleEndian ? new IFd()
                {
                    Kennzeichnungsnummer = BinaryPrimitives.ReadUInt16LittleEndian(ifdSpan.Slice(0, 2)),
                    Feldtyp = BinaryPrimitives.ReadUInt16LittleEndian(ifdSpan.Slice(2, 2)),
                    Content = GetContent(ifdSpan.Slice(4, 4), BinaryPrimitives.ReadUInt16LittleEndian(ifdSpan.Slice(2, 2)), m_IsLittleEndian)
                } :
                new IFd()
                {
                    Kennzeichnungsnummer = BinaryPrimitives.ReadUInt16BigEndian(ifdSpan.Slice(0, 2))
                };
                testresult.Add(ifd);
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
                    5 => BinaryPrimitives.ReadUInt32LittleEndian(content),
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
                    _ => string.Empty
                };
            }
        }
    }
}
