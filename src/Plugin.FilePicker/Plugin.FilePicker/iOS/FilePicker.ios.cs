using Foundation;
using MobileCoreServices;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using System.Diagnostics;

namespace Plugin.FilePicker
{
    static partial class FilePicker
    {
        static int _requestId;
        static TaskCompletionSource<FileData> _completionSource;

        public static EventHandler<FilePickerEventArgs> Handler
        {
            get;
            set;
        }

        static void OnFilePicked(FilePickerEventArgs e)
        {
            Handler?.Invoke(null, e);
        }

        static void DocumentPicker_DidPickDocumentAtUrls(object sender, UIDocumentPickedAtUrlsEventArgs e)
        {
            var control = (UIDocumentPickerViewController)sender;
            foreach (var url in e.Urls)
                DocumentPicker_DidPickDocument(control, new UIDocumentPickedEventArgs(url));

            control.Dispose();
        }

        static void DocumentPicker_DidPickDocument(object sender, UIDocumentPickedEventArgs e)
        {
            try
            {
                var securityEnabled = e.Url.StartAccessingSecurityScopedResource();
                var doc = new UIDocument(e.Url);

                string filename = doc.LocalizedName;
                string pathname = doc.FileUrl?.Path;

                e.Url.StopAccessingSecurityScopedResource();

                // iCloud drive can return null for LocalizedName.
                if (filename == null && pathname != null)
                {
                    filename = Path.GetFileName(pathname);
                }

                OnFilePicked(new FilePickerEventArgs(null, filename, pathname));
            }
            catch (Exception ex)
            {
                // pass exception to task so that it doesn't get lost in the UI main loop
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                tcs.SetException(ex);
            }
        }

        public static void DocumentPicker_WasCancelled(object sender, EventArgs e)
        {
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                tcs.SetResult(null);
            }
        }



        static async Task<FileData> PlataformPickFile(string[] allowedTypes)
        {
            var media = await TakeMediaAsync(allowedTypes);

            return media;
        }

        static Task<bool> PlataformSaveFile(FileData fileToSave)
        {
            try
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = Path.Combine(documents, fileToSave.FileName);

                File.WriteAllBytes(fileName, fileToSave.DataArray);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Task.FromResult(false);
            }

        }

        static void PlataformOpenFile(string fileToOpen)
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var fileName = Path.Combine(documents, fileToOpen);

            if (NSFileManager.DefaultManager.FileExists(fileName))
            {
                var url = new NSUrl(fileName, true);
                OpenFile(url);
            }
        }

        static void PlataformOpenFile(FileData fileToOpen)
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var fileName = Path.Combine(documents, fileToOpen.FileName);

            if (NSFileManager.DefaultManager.FileExists(fileName))
            {
                var url = new NSUrl(fileName, true);

                OpenFile(url);
            }
            else
            {
                SaveFile(fileToOpen).GetAwaiter().GetResult();
                OpenFile(fileToOpen);
            }
        }

        static Task<FileData> TakeMediaAsync(string[] allowedTypes)
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<FileData>(id);

            if (Interlocked.CompareExchange(ref _completionSource, ntcs, null) != null)
                throw new InvalidOperationException("Only one operation can be active at a time");

            var allowedUtis = new string[] {
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

        static int GetRequestId()
        {
            var id = _requestId;

            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }

        static UIViewController GetActiveViewController()
        {
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            UIViewController viewController = window.RootViewController;

            while (viewController.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }

            return viewController;
        }

        static void OpenFile(NSUrl fileUrl)
        {
            var docControl = UIDocumentInteractionController.FromUrl(fileUrl);

            var window = UIApplication.SharedApplication.KeyWindow;
            var subViews = window.Subviews;
            var lastView = subViews.Last();
            var frame = lastView.Frame;

            docControl.PresentOpenInMenu(frame, lastView, true);
        }
    }
}
