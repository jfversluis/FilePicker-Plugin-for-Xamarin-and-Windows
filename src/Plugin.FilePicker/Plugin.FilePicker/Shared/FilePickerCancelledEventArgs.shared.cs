using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.FilePicker
{
    public class FilePickerCancelledEventArgs : EventArgs
    {
        /// <summary>
        /// Exception that occured that led to cancelling file picking; may be
        /// null when file picking was cancelled by the user
        /// </summary>
        public Exception Exception { get; set; }
    }
}
