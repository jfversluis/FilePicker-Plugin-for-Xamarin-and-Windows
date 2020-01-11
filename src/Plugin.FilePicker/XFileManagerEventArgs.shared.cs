using System;

namespace Plugin.XFileManager.Abstractions
{
    /// <summary>
    /// Event arguments for the event when file picking was completed.
    /// </summary>
    //internal class XFileManagerEventArgs : EventArgs
    //{
    //    /// <summary>
    //    /// File name part of picked file, without path
    //    /// </summary>
    //    public string FileName { get; set; }

    //    /// <summary>
    //    /// Complete file path of picked file; on some OS this may contain an Uri
    //    /// </summary>
    //    public string FilePath { get; set; }

    //    /// <summary>
    //    /// Creates a new and empty file picker event args object
    //    /// </summary>
    //    public XFileManagerEventArgs()
    //    {
    //    }

    //    /// <summary>
    //    /// Creates new file picker event args
    //    /// </summary>
    //    /// <param name="fileName">file name part of picked file</param>
    //    public XFileManagerEventArgs(string fileName)
    //    {
    //        this.FileName = fileName;
    //    }

    //    /// <summary>
    //    /// Creates new file picker event args
    //    /// </summary>
    //    /// <param name="fileName">file name part of picked file</param>
    //    /// <param name="filePath">complete file path of picked file</param>
    //    public XFileManagerEventArgs(string fileName, string filePath)
    //        : this(fileName)
    //    {
    //        this.FilePath = filePath;
    //    }
    //}

    /// <summary>
    /// Event arguments for the event when file picking was cancelled, either
    /// by the user or when an exception occured
    /// </summary>
    public class FilePickerCancelledEventArgs : EventArgs
    {
        /// <summary>
        /// Exception that occured that led to cancelling file picking; may be
        /// null when file picking was cancelled by the user
        /// </summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// Event arguments for the event when folder picking was cancelled, either
    /// by the user or when an exception occured
    /// </summary>
    public class FolderPickerCancelledEventArgs : EventArgs
    {
        /// <summary>
        /// Exception that occured that led to cancelling folder picking; may be
        /// null when folder picking was cancelled by the user
        /// </summary>
        public Exception Exception { get; set; }
    }

    public class PermissionRequestEventArgs : EventArgs
    {
        public bool success { get; set; }

        public PermissionRequestEventArgs()
        {

        }
    }

    /// <summary>
    /// Event arguments for the event when file picking was completed.
    /// </summary>
    public class FilePickerEventArgs : EventArgs
    {
        /// <summary>
        /// File name part of picked file, without path
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Complete file path of picked file; on some OS this may contain an Uri
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Complete folder path of picked folder; on some OS this may contain an Uri
        /// </summary>
        public string FolderPath { get; set; }



        /// <summary>
        /// Creates a new and empty file picker event args object
        /// </summary>
        public FilePickerEventArgs()
        {
        }

        /// <summary>
        /// Creates new file picker event args
        /// </summary>
        /// <param name="fileName">file name part of picked file</param>
        public FilePickerEventArgs(string fileName)
        {
            this.FileName = fileName;
        }

        /// <summary>
        /// Creates new file picker event args
        /// </summary>
        /// <param name="fileName">file name part of picked file</param>
        /// <param name="filePath">complete file path of picked file</param>
        public FilePickerEventArgs(string fileName, string filePath)
            : this(fileName)
        {
            this.FilePath = filePath;
        }
    }

    /// <summary>
    /// Event arguments for the event when folder picking was completed.
    /// </summary>
    public class FolderPickerEventArgs : EventArgs
    {
        /// <summary>
        /// Folder name part of picked folder, without path
        /// </summary>
        public string FolderName { get; set; }

        /// <summary>
        /// Complete folder path of picked folder; on some OS this may contain an Uri
        /// </summary>
        public string FolderPath { get; set; }



        /// <summary>
        /// Creates a new and empty folder picker event args object
        /// </summary>
        public FolderPickerEventArgs()
        {
        }

        /// <summary>
        /// Creates new folder picker event args
        /// </summary>
        /// <param name="folderName">folder name part of picked folder</param>
        public FolderPickerEventArgs(string folderName)
        {
            this.FolderName = folderName;
        }

        /// <summary>
        /// Creates new folder picker event args
        /// </summary>
        /// <param name="folderName">folder name part of picked folder</param>
        /// <param name="folderPath">complete folder path of picked folder</param>
        public FolderPickerEventArgs(string folderName, string folderPath)
            : this(folderName)
        {
            this.FolderPath = folderPath;
        }
    }

}
