using System;
using System.IO;

namespace Plugin.FilePicker.Abstractions
{
    /// <summary>
    /// The object used as a wrapper for the file picked by the user
    /// </summary>
    public class FileData : IDisposable
    {
        private string _fileName;
        private string _filePath;
        private bool _isDisposed;
        private readonly Action<bool> _dispose;
        private readonly Func<Stream> _streamGetter;

        public FileData()
        { }

        public FileData(string filePath, string fileName, Func<Stream> streamGetter, Action<bool> dispose = null)
        {
            _filePath = filePath;
            _fileName = fileName;
            _dispose = dispose;
            _streamGetter = streamGetter;
        }

        //Updated with BUG Fix for Content picked files.See Original Repo: https://github.com/jfversluis/FilePicker-Plugin-for-Xamarin-and-Windows/commit/997f85f309f939c6ca2a87efedd4d8bb7618b6df#diff-2cc24c90e81a2832630bd9d47d6948e1
        /// <summary>
        /// Completely reads all bytes from the input stream and returns it as byte array. Can be
        /// used when the returned file data consists of a stream, not a real filename.
        /// </summary>
        /// <param name="input">input stream</param>
        /// <returns>byte array</returns>
        public static byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        //END

        public byte[] DataArray
        {
            get
            {
                using (var stream = GetStream())
                {
                    //Updated with BUG Fix for Content picked files. See Original Repo: https://github.com/jfversluis/FilePicker-Plugin-for-Xamarin-and-Windows/commit/997f85f309f939c6ca2a87efedd4d8bb7618b6df#diff-2cc24c90e81a2832630bd9d47d6948e1
                    return ReadFully(stream);
                    //END
                }
            }
        }

        /// <summary>
        /// Filename of the picked file
        /// </summary>
        public string FileName
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(null);

                return _fileName;
            }

            set
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(null);

                _fileName = value;
            }
        }

        /// <summary>
        /// Full filepath of the picked file
        /// </summary>
        public string FilePath
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(null);

                return _filePath;
            }

            set
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(null);

                _filePath = value;
            }
        }

        /// <summary>
        /// Get stream if available
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(null);

            return _streamGetter();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _dispose?.Invoke(disposing);
        }

        ~FileData()
        {
            Dispose(false);
        }
    }
}