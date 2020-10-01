using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;
using Plugin.FilePicker.Abstractions;
using UIKit;

namespace Plugin.FilePicker
{
    public class DocumentBrowser : UIDocumentBrowserViewControllerDelegate
    {
        /// <summary>
        /// Request ID for current picking call
        /// </summary>
        private int requestId;

        /// <summary>
        /// Task completion source for task when finished picking
        /// </summary>
        private TaskCompletionSource<FilePlaceholder> completionSource;

        /// <summary>
        /// Event which is invoked when a file was picked
        /// </summary>
        internal EventHandler<FilePlaceholder> Handler { get; set; }

        private string[] _extensions;

        /// <summary>
        /// Called when file has been picked successfully
        /// </summary>
        /// <param name="destination">file picker picked destination</param>
        /// /// <param name="creation">we want to create a file</param>
        private async void OnFilePicked(NSUrl destination, bool creation = false)
        {
            if (destination == null || !destination.IsFileUrl)
            {
                this.Handler?.Invoke(this, null);
                return;
            }

            var document = new GenericDocument(destination);
            var success = await document.OpenAsync();

            if (!success)
            {
                this.Handler?.Invoke(this, null);
                return;
            }

            async Task StreamSetter(Stream stream, FilePlaceholder placeholder)
            {
                document.DataStream = stream;

                try
                {
                    if (!await document.SaveAsync(destination, creation ? UIDocumentSaveOperation.ForCreating : UIDocumentSaveOperation.ForOverwriting))
                    {
                        throw new Exception("Failed to Save Document.");
                    }
                }
                finally
                {
                    await document.CloseAsync();
                }
            }

            var placeHolder = new FilePlaceholder(destination.AbsoluteString, destination.LastPathComponent, StreamSetter, b => document.Dispose());

            this.Handler?.Invoke(null, placeHolder);
        }

        public override void DidRequestDocumentCreation(UIDocumentBrowserViewController controller, Action<NSUrl, UIDocumentBrowserImportMode> importHandler)
        {
            var editController = new FileNameInputViewController(_extensions);

            void OnEditControllerOnOnViewDidDisappear(object sender, EventArgs args)
            {
                editController.OnViewDidDisappear -= OnEditControllerOnOnViewDidDisappear;

                if (string.IsNullOrEmpty(editController.FileName))
                {
                    importHandler(null, UIDocumentBrowserImportMode.None);
                    return;
                }

                var documentFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var tempFileName = editController.FileName;

                var path = Path.Combine(documentFolder, tempFileName);
                var tempFile = File.Create(path);
                tempFile.Dispose();

                importHandler(NSUrl.CreateFileUrl(path, false, null), UIDocumentBrowserImportMode.Move);
            }

            editController.OnViewDidDisappear += OnEditControllerOnOnViewDidDisappear;

            controller.PresentViewController(editController, true, null);
             
        }

        public override void DidPickDocumentUrls(UIDocumentBrowserViewController controller, NSUrl[] documentUrls)
        {
            OnFilePicked(documentUrls[0]);
        }

        public override void DidPickDocumentsAtUrls(UIDocumentBrowserViewController controller, NSUrl[] documentUrls)
        {
            OnFilePicked(documentUrls[0]);
        }

        public override void DidImportDocument(UIDocumentBrowserViewController controller, NSUrl sourceUrl, NSUrl destinationUrl)
        {
            OnFilePicked(destinationUrl);
        }


        /// <summary>
        /// File picking implementation
        /// </summary>
        /// <param name="allowedTypes">list of allowed types; may be null</param>
        /// <returns>picked file data, or null when picking was cancelled</returns>
        public Task<FilePlaceholder> PickMediaAsync(string[] allowedTypes)
        {
            var id = this.GetRequestId();

            var ntcs = new TaskCompletionSource<FilePlaceholder>(id);

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
                allowedUtis = allowedTypes.Where(x => x[0] != '.').ToArray();
                _extensions = allowedTypes.Where(x => x[0] == '.').ToArray();
            }
            else
            {
                _extensions = null;
            }


            var documentBrowser = new CustomDocumentBrowserViewController(allowedUtis)
            {
                AllowsDocumentCreation = true,
                AllowsPickingMultipleItems = false,
                Delegate = this,
            };

            void OnDocumentBrowserOnOnViewDidDisappear(object sender, EventArgs args)
            {
                OnFilePicked(null);
            }

            documentBrowser.OnViewDidDisappear += OnDocumentBrowserOnOnViewDidDisappear;
            
            UIViewController viewController = GetActiveViewController();
            viewController.PresentViewController(documentBrowser, false, null);

            this.Handler = (sender, args) =>
            {
                documentBrowser.OnViewDidDisappear -= OnDocumentBrowserOnOnViewDidDisappear;
                documentBrowser.DismissViewController(false, null);

                var tcs = Interlocked.Exchange(ref this.completionSource, null);
                tcs?.SetResult(args);
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

        private class CustomDocumentBrowserViewController : UIDocumentBrowserViewController
        {
            public event EventHandler OnViewDidDisappear;
            public override void ViewDidDisappear(bool animated)
            {
                base.ViewDidDisappear(animated);
                OnViewDidDisappear?.Invoke(this, null);
            }

            public CustomDocumentBrowserViewController(string[] contentTypes) : base(contentTypes)
            {

            }
        }
    }
}