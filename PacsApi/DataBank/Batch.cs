using System.Runtime.InteropServices.Marshalling;
using static System.Net.WebRequestMethods;

namespace PacsApi.DataBank
{
    public class Batch : IDisposable
    {
        private string _id;
        private bool _disposedValue;
        private bool _isCompleted;
        private List<Bucket> _dicomStreams;
        private IEnumerator<Bucket> _streamEnumerator;
        public Action<Batch> OnCompleted;
        public Batch()
        {
            _dicomStreams = new List<Bucket>();
            _id = Guid.NewGuid().ToString();
            _isCompleted = false;
        }
        public List<Bucket> GetBuckets()
        {
            if (_dicomStreams == null)
            {
                throw new InvalidOperationException("Batch not created. Call Create() first.");
            }
            return _dicomStreams;
        }
        public int Count => _dicomStreams?.Count ?? 0;
        public string Id => _id;
        public bool IsCompleted
        {
            get => _isCompleted;
            internal set => _isCompleted = value;
        }
        public void Create(List<IFormFile> files)
        {
            
            foreach (var file in files)
            {
                using var ms = new MemoryStream();
                file.CopyTo(ms);

                var bytes = ms.ToArray();

                var stream = new MemoryStream(bytes); // 🔥 safe copy

                _dicomStreams.Add(new Bucket(stream));
            }
        }
        public IEnumerator<Bucket> GetEnumerator()
        {
            if (_dicomStreams == null)
                throw new InvalidOperationException("Batch not created. Call Create() first.");
            _streamEnumerator = _dicomStreams.GetEnumerator();
            return _streamEnumerator;
        }
        public void ResetEnumerator()
        {
            _streamEnumerator?.Reset();
        }
        //we will pass UserId here so that we can track which user uploaded which batch of DICOM files. This is important for auditing and security purposes.
        //or its easy for us to implement user-specific features like allowing users to view or manage only their own uploaded batches.
        //or when multiple users will upload files we will track which batch belongs to which user and we can implement user-specific features like allowing users to view or manage only their own uploaded batches.
        public void SetOwner(string ownerId)
        {
            if (_dicomStreams == null)
                throw new InvalidOperationException("Batch not created. Call Create() first.");
            _dicomStreams.ForEach(ds => ds.SetOwner(ownerId));
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_dicomStreams != null)
                    {
                        _dicomStreams.ForEach(ds => ds.Dispose());
                        _dicomStreams.Clear();
                        _dicomStreams = null;
                    }
                    if (_streamEnumerator != null)
                    {
                        _streamEnumerator.Dispose();
                        _streamEnumerator = null;
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
