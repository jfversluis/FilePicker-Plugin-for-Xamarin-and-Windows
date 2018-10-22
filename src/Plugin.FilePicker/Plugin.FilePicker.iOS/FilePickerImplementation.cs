using Foundation;
using MobileCoreServices;
using Plugin.FilePicker.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using System.Diagnostics;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for FilePicker
    /// </summary>
    public class FilePickerImplementation : NSObject, IFilePicker
    {
        private int _requestId;
        private TaskCompletionSource<FileData> _completionSource;

        /// <summary>
        /// Event which is invoked when a file was picked
        /// </summary>
        public EventHandler<FilePickerEventArgs> Handler
        {
            get;
            set;
        }

        private void OnFilePicked (FilePickerEventArgs e)
        {
            Handler?.Invoke (null, e);
        }

        private void DocumentPicker_DidPickDocumentAtUrls(object sender, UIDocumentPickedAtUrlsEventArgs e)
        {
            var control = (UIDocumentPickerViewController)sender;
            foreach (var url in e.Urls)
                DocumentPicker_DidPickDocument(control, new UIDocumentPickedEventArgs(url));

            control.Dispose();
        }

        private void DocumentPicker_DidPickDocument (object sender, UIDocumentPickedEventArgs e)
        {
            var securityEnabled = e.Url.StartAccessingSecurityScopedResource ();
            var doc = new UIDocument (e.Url);
            var data = NSData.FromUrl (e.Url);
            var dataBytes = new byte [data.Length];

            System.Runtime.InteropServices.Marshal.Copy (data.Bytes, dataBytes, 0, Convert.ToInt32 (data.Length));

            string filename = doc.LocalizedName;
            string pathname = doc.FileUrl?.ToString();

            e.Url.StopAccessingSecurityScopedResource();

            // iCloud drive can return null for LocalizedName.
            if (filename == null)
            {
                // Retrieve actual filename by taking the last entry after / in FileURL.
                // e.g. /path/to/file.ext -> file.ext

                // filesplit is either:
                // 0 (pathname is null, or last / is at position 0)
                // -1 (no / in pathname)
                // positive int (last occurence of / in string)
                var filesplit = pathname?.LastIndexOf ('/') ?? 0;

                filename = pathname?.Substring (filesplit + 1);
            }

            OnFilePicked(new FilePickerEventArgs(dataBytes, filename, pathname));
        }
        
        /// <summary>
        /// Handles when the file picker was cancelled. Either in the
        /// popup menu or later on.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DocumentPicker_WasCancelled(object sender, EventArgs e)
        {
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                tcs.SetResult(null);
            }
        }

        /// <summary>
        /// Lets the user pick a file with the systems default file picker
        /// For iOS iCloud drive needs to be configured
        /// </summary>
        /// <returns></returns>
        public async Task<FileData> PickFile (string[] allowedTypes)
        {
            var media = await TakeMediaAsync (allowedTypes);

            return media;
        }

        private Task<FileData> TakeMediaAsync (string[] allowedTypes)
        {
            var id = GetRequestId ();

            var ntcs = new TaskCompletionSource<FileData> (id);

            if (Interlocked.CompareExchange (ref _completionSource, ntcs, null) != null)
                throw new InvalidOperationException ("Only one operation can be active at a time");

            var allowedUtis = new string [] {
                UTType.Content,
                UTType.Item,
                "public.data"
            };

            if (allowedTypes != null)
            {
                allowedUtis = allowedTypes;
            }

            // NOTE: Importing makes a local copy of the document, while opening opens the document directly
            var documentPicker = new UIDocumentPickerViewController(allowedUtis, UIDocumentPickerMode.Import);
            //var documentPicker = new UIDocumentPickerViewController(allowedUtis, UIDocumentPickerMode.Open);

            documentPicker.DidPickDocument += DocumentPicker_DidPickDocument;
            documentPicker.WasCancelled += DocumentPicker_WasCancelled;
            documentPicker.DidPickDocumentAtUrls += DocumentPicker_DidPickDocumentAtUrls;

            UIViewController viewController = GetActiveViewController();
            viewController.PresentViewController(documentPicker, true, null);

            Handler = null;

            Handler = (s, e) =>
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);

                tcs?.SetResult(new FileData(e.FilePath, e.FileName, () =>
                {
                    return new FileStream(e.FilePath, FileMode.Open, FileAccess.Read);
                }));
            };

            return _completionSource.Task;
        }

        /// <summary>
        /// Finds active view controller to use to present document picker
        /// </summary>
        /// <returns>view controller to use</returns>
        private static UIViewController GetActiveViewController()
        {
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            UIViewController viewController = window.RootViewController;

            while (viewController.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }

            return viewController;
        }

        private int GetRequestId ()
        {
            var id = _requestId;

            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }

        public Task<bool> SaveFile (FileData fileToSave)
        {
            try
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = Path.Combine(documents, fileToSave.FileName);

                File.WriteAllBytes (fileName, fileToSave.DataArray);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Task.FromResult(false);
            }
        }

        public void OpenFile (NSUrl fileUrl)
        {
            var docControl = UIDocumentInteractionController.FromUrl (fileUrl);

            var window = UIApplication.SharedApplication.KeyWindow;
            var subViews = window.Subviews;
            var lastView = subViews.Last ();
            var frame = lastView.Frame;

            docControl.PresentOpenInMenu (frame, lastView, true);
        }

        public void OpenFile (string fileToOpen)
        {
            var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);

            var fileName = Path.Combine (documents, fileToOpen);

            if (NSFileManager.DefaultManager.FileExists(fileName))
            {
                var url = new NSUrl(fileName, true);
                OpenFile(url);
            }
        }

        public async void OpenFile (FileData fileToOpen)
        {
            var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);

            var fileName = Path.Combine (documents, fileToOpen.FileName);

            if (NSFileManager.DefaultManager.FileExists(fileName))
            {
                var url = new NSUrl(fileName, true);

                OpenFile(url);
            }
            else
            {
                await SaveFile(fileToOpen);
                OpenFile(fileToOpen);
            }
        }
    }
}
