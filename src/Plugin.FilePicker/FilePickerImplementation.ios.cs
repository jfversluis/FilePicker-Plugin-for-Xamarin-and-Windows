using Foundation;
using MobileCoreServices;
using Plugin.FilePicker.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for file picking on iOS
    /// </summary>
    public class FilePickerImplementation : NSObject, IFilePicker
    {
        /// <summary>
        /// Request ID for current picking call
        /// </summary>
        private int requestId;

        /// <summary>
        /// Task completion source for task when finished picking
        /// </summary>
        private TaskCompletionSource<FileData> completionSource;

        /// <summary>
        /// Event which is invoked when a file was picked
        /// </summary>
        internal EventHandler<FilePickerEventArgs> Handler { get; set; }

        /// <summary>
        /// Called when file has been picked successfully
        /// </summary>
        /// <param name="args">file picker event args</param>
        private void OnFilePicked(FilePickerEventArgs args)
        {
            this.Handler?.Invoke(null, args);
        }

        /// <summary>
        /// Callback method called by document picker when file has been picked; this is called
        /// starting from iOS 11.
        /// </summary>
        /// <param name="sender">sender object (document picker)</param>
        /// <param name="args">event args</param>
        private void DocumentPicker_DidPickDocumentAtUrls(object sender, UIDocumentPickedAtUrlsEventArgs args)
        {
            var control = (UIDocumentPickerViewController)sender;
            foreach (var url in args.Urls)
            {
                this.DocumentPicker_DidPickDocument(control, new UIDocumentPickedEventArgs(url));
            }

            control.Dispose();
        }

        /// <summary>
        /// Callback method called by document picker when file has been picked; this is called
        /// up to iOS 10.
        /// </summary>
        /// <param name="sender">sender object (document picker)</param>
        /// <param name="args">event args</param>
        private void DocumentPicker_DidPickDocument(object sender, UIDocumentPickedEventArgs args)
        {
            try
            {
                var securityEnabled = args.Url.StartAccessingSecurityScopedResource();
                var doc = new UIDocument(args.Url);

                string filename = doc.LocalizedName;
                string pathname = doc.FileUrl?.Path;

                args.Url.StopAccessingSecurityScopedResource();

                // iCloud drive can return null for LocalizedName.
                if (filename == null && pathname != null)
                {
                    filename = Path.GetFileName(pathname);
                }

                this.OnFilePicked(new FilePickerEventArgs(filename, pathname));
            }
            catch (Exception ex)
            {
                // pass exception to task so that it doesn't get lost in the UI main loop
                var tcs = Interlocked.Exchange(ref this.completionSource, null);
                tcs.SetException(ex);
            }
        }

        /// <summary>
        /// Handles when the file picker was cancelled. Either in the
        /// popup menu or later on.
        /// </summary>
        /// <param name="sender">sender object (document picker)</param>
        /// <param name="args">event args</param>
        public void DocumentPicker_WasCancelled(object sender, EventArgs args)
        {
            var tcs = Interlocked.Exchange(ref this.completionSource, null);
            tcs.SetResult(null);
        }

        /// <summary>
        /// Lets the user pick a file with the systems default file picker.
        /// For iOS iCloud drive needs to be configured.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be selected while picking.
        /// On iOS you can specify UTType constants, e.g. UTType.Image.
        /// </param>
        /// <returns>
        /// File data object, or null when user cancelled picking file
        /// </returns>
        public async Task<FileData> PickFile(string[] allowedTypes)
        {
            var fileData = await this.PickMediaAsync(allowedTypes);

            return fileData;
        }

        public Task<FileData> CreateOrOverwriteFile(string[] allowedTypes = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// File picking implementation
        /// </summary>
        /// <param name="allowedTypes">list of allowed types; may be null</param>
        /// <returns>picked file data, or null when picking was cancelled</returns>
        private Task<FileData> PickMediaAsync(string[] allowedTypes)
        {
            var id = this.GetRequestId();

            var ntcs = new TaskCompletionSource<FileData>(id);

            if (Interlocked.CompareExchange(ref this.completionSource, ntcs, null) != null)
            {
                throw new InvalidOperationException("Only one operation can be active at a time");
            }

            var allowedUtis = new string[]
            {
                UTType.Content,
                UTType.Item,
                "public.data"
            };

            if (allowedTypes != null)
            {
                allowedUtis = allowedTypes;
            }

            // NOTE: Importing (UIDocumentPickerMode.Import) makes a local copy of the document,
            // while opening (UIDocumentPickerMode.Open) opens the document directly. We do the
            // first, so the user has to read the file immediately.
            var documentPicker = new UIDocumentPickerViewController(allowedUtis, UIDocumentPickerMode.Import);

            documentPicker.DidPickDocument += this.DocumentPicker_DidPickDocument;
            documentPicker.WasCancelled += this.DocumentPicker_WasCancelled;
            documentPicker.DidPickDocumentAtUrls += this.DocumentPicker_DidPickDocumentAtUrls;

            UIViewController viewController = GetActiveViewController();
            viewController.PresentViewController(documentPicker, true, null);

            this.Handler = (sender, args) =>
            {
                var tcs = Interlocked.Exchange(ref this.completionSource, null);

                tcs?.SetResult(new FileData(
                    args.FilePath,
                    args.FileName,
                    () =>
                    {
                        return new FileStream(args.FilePath, FileMode.Open, FileAccess.Read);
                    }));
            };

            return this.completionSource.Task;
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

        /// <summary>
        /// Returns a new request ID for a new call to PickFile()
        /// </summary>
        /// <returns>new request ID</returns>
        private int GetRequestId()
        {
            var id = this.requestId;

            if (this.requestId == int.MaxValue)
            {
                this.requestId = 0;
            }
            else
            {
                this.requestId++;
            }

            return id;
        }
    }
}
