//using Foundation;
//using MobileCoreServices;
//using QuickLook;
//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Threading;
//using System.Threading.Tasks;
//using UIKit;

//[assembly: Xamarin.Forms.Dependency(typeof(Plugin.iOS.SaveIOS))]
//namespace Plugin.iOS
//{
//    class SaveIOS : NSObject, IDocumentsSaveLoadPick
//    {
//        private int requestId;

//        private TaskCompletionSource<string> completionSource;

//        internal EventHandler<XFileManagerEventArgs> Handler { get; set; }


//        private void OnFilePicked(XFileManagerEventArgs args)
//        {
//            this.Handler?.Invoke(null, args);
//        }

//        private void DocumentPicker_DidPickDocumentAtUrls(object sender, UIDocumentPickedAtUrlsEventArgs args)
//        {
//            var control = (UIDocumentPickerViewController)sender;
//            foreach (var url in args.Urls)
//            {
//                this.DocumentPicker_DidPickDocument(control, new UIDocumentPickedEventArgs(url));
//            }

//            control.Dispose();
//        }

//        private void DocumentPicker_DidPickDocument(object sender, UIDocumentPickedEventArgs args)
//        {
//            try
//            {
//                var securityEnabled = args.Url.StartAccessingSecurityScopedResource();
//                var doc = new UIDocument(args.Url);

//                string filename = doc.LocalizedName;
//                string pathname = doc.FileUrl?.Path;

//                args.Url.StopAccessingSecurityScopedResource();

//                // iCloud drive can return null for LocalizedName.
//                if (filename == null && pathname != null)
//                {
//                    filename = Path.GetFileName(pathname);
//                }

//                this.OnFilePicked(new XFileManagerEventArgs(filename, pathname));
//            }
//            catch (Exception ex)
//            {
//                // pass exception to task so that it doesn't get lost in the UI main loop
//                var tcs = Interlocked.Exchange(ref this.completionSource, null);
//                tcs.SetException(ex);
//            }
//        }

//        public void DocumentPicker_WasCancelled(object sender, EventArgs args)
//        {
//            var tcs = Interlocked.Exchange(ref this.completionSource, null);
//            tcs.SetResult(null);
//        }

//        public async Task<string> PickFilePathNOTUSED(string[] allowedTypes)
//        {
//            var fileData = await this.PickMediaAsync(allowedTypes);

//            return fileData;
//        }

//        private Task<string> PickMediaAsync(string[] allowedTypes)
//        {
//            var id = this.GetRequestId();

//            var ntcs = new TaskCompletionSource<string>(id);

//            if (Interlocked.CompareExchange(ref this.completionSource, ntcs, null) != null)
//            {
//                throw new InvalidOperationException("Only one operation can be active at a time");
//            }

//            var allowedUtis = new string[]
//            {
//                UTType.Content,
//                UTType.Item,
//                "public.data"
//            };

//            if (allowedTypes != null)
//            {
//                allowedUtis = allowedTypes;
//            }

//            // NOTE: Importing (UIDocumentPickerMode.Import) makes a local copy of the document,
//            // while opening (UIDocumentPickerMode.Open) opens the document directly. We do the
//            // first, so the user has to read the file immediately.
//            var documentPicker = new UIDocumentPickerViewController(allowedUtis, UIDocumentPickerMode.Import);

//            documentPicker.DidPickDocument += this.DocumentPicker_DidPickDocument;
//            documentPicker.WasCancelled += this.DocumentPicker_WasCancelled;
//            documentPicker.DidPickDocumentAtUrls += this.DocumentPicker_DidPickDocumentAtUrls;

//            UIViewController viewController = GetActiveViewController();
//            viewController.PresentViewController(documentPicker, true, null);

//            this.Handler = (sender, args) =>
//            {
//                var tcs = Interlocked.Exchange(ref this.completionSource, null);
//                tcs?.SetResult(args.FilePath + "/" + args.FileName);
//            };

//            return this.completionSource.Task;
//        }

//        private static UIViewController GetActiveViewController()
//        {
//            UIWindow window = UIApplication.SharedApplication.KeyWindow;
//            UIViewController viewController = window.RootViewController;

//            while (viewController.PresentedViewController != null)
//            {
//                viewController = viewController.PresentedViewController;
//            }

//            return viewController;
//        }

//        private int GetRequestId()
//        {
//            var id = this.requestId;

//            if (this.requestId == int.MaxValue)
//            {
//                this.requestId = 0;
//            }
//            else
//            {
//                this.requestId++;
//            }

//            return id;
//        }

//        public Task<bool> SaveFileStream(string path, string name, string mimetype, Stream stream)
//        {
//            string fileToSave = path + "/" + name;
//            try
//            {
//                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//                var fileName = Path.Combine(documents, fileToSave);

//                var test = File.Create(fileName);
//                stream.CopyTo(test);
//                stream.Flush();
//                stream.Close();
//                test.Flush();
//                test.Close();

//                return Task.FromResult(true);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex.Message);
//                return Task.FromResult(false);
//            }
//        }

//        public Task<string> PickFolderPath()
//        {
//            return Task.FromResult<string>(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
//        }

//        public Task<bool> OpenFile(NSUrl fileUrl)
//        {
//            var docController = UIDocumentInteractionController.FromUrl(fileUrl);

//            var window = UIApplication.SharedApplication.KeyWindow;
//            var subViews = window.Subviews;
//            var lastView = subViews.Last();
//            var frame = lastView.Frame;

//            docController.PresentOpenInMenu(frame, lastView, true);
//            return Task.FromResult<bool>(true);
//        }

//        public Task<bool> OpenFileFromPathNOTUSED(string fileToOpen)
//        {
//            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

//            var fileName = Path.Combine(documents, fileToOpen);

//            if (NSFileManager.DefaultManager.FileExists(fileName))
//            {
//                var url = new NSUrl(fileName, true);
//                return this.OpenFile(url);
//            }
//            else
//            {
//                return Task.FromResult<bool>(false);
//            }
//        }

//        public Task<MemoryStream> LoadFileToStreamNOTUSED()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<MemoryStream> LoadFileToStreamNOTUSED(string fullpath)
//        {
//            throw new NotImplementedException();
//        }

//        //public async void OpenFile(FileData fileToOpen)
//        //{
//        //    var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

//        //    var fileName = Path.Combine(documents, fileToOpen.FileName);

//        //    if (NSFileManager.DefaultManager.FileExists(fileName))
//        //    {
//        //        var url = new NSUrl(fileName, true);

//        //        this.OpenFile(url);
//        //    }
//        //    else
//        //    {
//        //        await this.SaveFile(fileToOpen);
//        //        this.OpenFile(fileToOpen);
//        //    }
//        //}
//        //public Task<bool> SaveFile(string path, string name, string mimetype, Stream stream)
//        //{
//        //    //Get the root path in iOS device.

//        //    string filePath = Path.Combine(path, name);

//        //    //Create a file and write the stream into it.
//        //    FileStream fileStream = File.Open(filePath, FileMode.Create);
//        //    stream.Position = 0;
//        //    stream.CopyTo(fileStream);
//        //    fileStream.Flush();
//        //    fileStream.Close();

//        //    //Invoke the saved document for viewing
//        //    UIViewController currentController = UIApplication.SharedApplication.KeyWindow.RootViewController;
//        //    while (currentController.PresentedViewController != null)
//        //        currentController = currentController.PresentedViewController;
//        //    UIView currentView = currentController.View;

//        //    QLPreviewController qlPreview = new QLPreviewController();
//        //    QLPreviewItem item = new QLPreviewItemBundle(name, filePath);
//        //    qlPreview.DataSource = new PreviewControllerDS(item);

//        //    currentController.PresentViewController(qlPreview, true, null);

//        //    return Task.FromResult<bool>(true);
//        //}




//    }
//}

