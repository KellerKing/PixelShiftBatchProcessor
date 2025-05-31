namespace FileHandler.Filestream
{
    internal interface IByteReader : IDisposable
    {
        int Length { get; }
        bool CanRead();
        Span<byte> GetBytes(int startindex,  int length);
    }
}
