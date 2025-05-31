using FileHandler.ByteReader;

namespace FileHandler
{
    public class Testklasse
    {
        private readonly string m_path;
        private readonly MemoryByteReader m_reader;
        public Testklasse(string path)
        {
            m_path = path;
            m_reader = new MemoryByteReader(m_path);
        }



        public void Test1(int start, int length)
        {
            m_reader.GetBytes(start, length);
        }
    }
}
