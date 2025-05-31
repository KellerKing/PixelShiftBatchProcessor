using FileHandler.Filestream;

namespace FileHandler.ByteReader
{
    internal class MemoryByteReader : IByteReader
    {
        private readonly string m_Name;
        
        private int m_FileLength = -1;
        private Task<byte[]> m_ReadBytesTask;

        public MemoryByteReader(string fullFilepath)
        {
            m_Name = fullFilepath;
            m_ReadBytesTask = CreateReadFileAsyncTask();
            
            m_ReadBytesTask.ContinueWith(t => { 
                m_FileLength = t.Result.Length;
            });
        }

        public int Length => m_FileLength;

        public bool CanRead()
        {
            if (string.IsNullOrEmpty(m_Name)) return false;
            if (!File.Exists(m_Name)) return false;

            return true;
        }

        public void Dispose()
        {
            m_ReadBytesTask?.Dispose();
        }

        public Span<byte> GetBytes(int startindex, int length)
        {
            if (m_ReadBytesTask.IsCompleted)
            {
                return m_ReadBytesTask.Result.AsSpan(startindex, length);
            }

            m_ReadBytesTask.Wait();
            return m_ReadBytesTask.Result.AsSpan(startindex, length);
        }

        private Task<byte[]> CreateReadFileAsyncTask()
        {
            if (!CanRead())
                return Task.FromResult<byte[]>([]);

            return Task.Run(() => File.ReadAllBytesAsync(m_Name));
        }
    }
}
