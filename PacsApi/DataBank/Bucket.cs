namespace PacsApi.DataBank
{
    public class Bucket : IDisposable
    {
        private Stream _fileStream;
        private string _ownerId;
        public Bucket(Stream fileStream)
        {
            _fileStream = fileStream;
        }
        public string OwnerId
        {
            get => _ownerId;
        }
        public Stream GetStream()
        {
            return _fileStream ?? throw new NullReferenceException("stream is empty");
        }
        public void Dispose()
        {
            _fileStream?.Dispose();
            _fileStream = null;
        }
        public void SetOwner(string ownerId)
        {
            _ownerId = ownerId;
        }
    }
}
