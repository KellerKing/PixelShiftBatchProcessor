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
        {
            m_Fielpath = fielpath;
        }

        public void Process()
        {
            // Hier wird die Raw-Datei verarbeitet
            Console.WriteLine($"Verarbeite Raw-Datei: {m_Fielpath}");
            
            var bytes = File.ReadAllBytes(m_Fielpath);
            var span  = bytes.AsSpan();
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


            var ifd0Span = span.Slice((int)offsetIfd0InByte, 12);
            //BinaryPrimitives --> https://learn.microsoft.com/de-de/dotnet/api/system.buffers.binary.binaryprimitives?view=net-7.0 --> Bessere Klasse als BitConverter.
        }

    }
}
