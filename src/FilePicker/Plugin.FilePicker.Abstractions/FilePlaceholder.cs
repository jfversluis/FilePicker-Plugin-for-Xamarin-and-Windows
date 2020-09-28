using System;
using System.IO;

namespace Plugin.FilePicker.Abstractions
{
    public class FilePlaceholder : IDisposable
    {
        /// <summary>
        /// Backing store for the FileName property
        /// </summary>
        private string _fileName;

        /// <summary>
        /// Backing store for the FilePath property
        /// </summary>
        private string _filePath;

        /// <summary>
        /// Indicates if the object is already disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Action to dispose of underlying resources of the picked file.
        /// </summary>
        private readonly Action<bool> _dispose;

        /// <summary>
        /// Function to get a stream to the picked file.
        /// </summary>
        private readonly Action<Stream, FilePlaceholder> _streamSetter;

        /// <summary>
        /// Creates a new and empty file data object
        /// </summary>
        public FilePlaceholder()
        {
        }

        /// <summary>
        /// Creates a new file data object with property values
        /// </summary>
        /// <param name="filePath">
        /// Full file path to the picked file.
        /// </param>
        /// <param name="fileName">
        /// File name of the picked file.
        /// </param>
        /// <param name="streamSetter">app 
        /// Function to get a stream to the picked file.
        /// </param>
        /// <param name="dispose">
        /// Action to dispose of the underlying resources of the picked file.
        /// </param>
        public FilePlaceholder(string filePath, string fileName, Action<Stream, FilePlaceholder> streamSetter, Action<bool> dispose = null)
        {
            _filePath = filePath;
            _fileName = fileName;
            _dispose = dispose;
            _streamSetter = streamSetter;
        }

        /// <summary>
        /// Completely reads all bytes from the input stream and returns it as byte array. Can be
        /// used when the returned file data consists of a stream, not a real filename.
        /// </summary>
        /// <param name="input">input stream</param>
        /// <returns>byte array</returns>
        internal static byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Filename of the picked file, without path
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
        /// Full filepath of the picked file.
        /// Note that on specific platforms this can also contain an URI that
        /// can't be opened with file related APIs. Use DataArray property or
        /// GetStream() method in this cases.
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
        /// Gets stream to access the picked file.
        /// Note that when DataArray property was already accessed, the stream
        /// must be rewinded to the beginning.
        /// </summary>
        /// <returns>stream object</returns>
        public void SetStream(Stream stream)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(null);

            _streamSetter(stream, this);
        }

        #region IDispose implementation
        /// <summary>
        /// Disposes of all resources in the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of managed resources
        /// </summary>
        /// <param name="disposing">
        /// True when called from Dispose(), false when called from the destructor
        /// </param>
        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _dispose?.Invoke(disposing);
        }

        /// <summary>
        /// Finalizer for this object
        /// </summary>
        ~FilePlaceholder()
        {
            this.Dispose(false);
        }
        #endregion
    }
}