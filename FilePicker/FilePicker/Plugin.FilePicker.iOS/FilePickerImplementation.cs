using Foundation;
using MobileCoreServices;
using Plugin.FilePicker.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace Plugin.FilePicker
{
		private Task<FileData> TakeMediaAsync()
		{

			int id = GetRequestId();

			var ntcs = new TaskCompletionSource<FileData>(id);
			if (Interlocked.CompareExchange(ref this.completionSource, ntcs, null) != null)
				throw new InvalidOperationException("Only one operation can be active at a time");

			var allowedUTIs = new string[] {
				UTType.UTF8PlainText,
				UTType.PlainText,
				UTType.RTF,
				UTType.PNG,
				UTType.Text,
				UTType.PDF,
				UTType.Image,
				UTType.UTF16PlainText,
				UTType.FileURL
			};

			UIDocumentMenuViewController importMenu =
				new UIDocumentMenuViewController(allowedUTIs, UIDocumentPickerMode.Import);
			importMenu.Delegate = this;

			importMenu.ModalPresentationStyle = UIModalPresentationStyle.Popover;

			UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(importMenu, true, null);

			UIPopoverPresentationController presPopover = importMenu.PopoverPresentationController;

			if (presPopover != null)
			{
				presPopover.SourceView = UIApplication.SharedApplication.KeyWindow.RootViewController.View;
				presPopover.PermittedArrowDirections = UIPopoverArrowDirection.Down;
			}

			handler = null;

			handler = (s, e) =>
			{
				var tcs = Interlocked.Exchange(ref this.completionSource, null);

				tcs.SetResult(new FileData
				{
					DataArray = e.FileByte,
					FileName = e.FileName
				});
			};


			return completionSource.Task;

		}

		public void WasCancelled(UIDocumentMenuViewController documentMenu)
		{

		}

		private int GetRequestId()
		{
			int id = this.requestId;
			if (this.requestId == Int32.MaxValue)
				this.requestId = 0;
			else
				this.requestId++;

			return id;
		}

		public async Task<bool> SaveFile(FileData fileToSave)
		{
			try
			{
				var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				var fileName = Path.Combine(documents, fileToSave.FileName);

				File.WriteAllBytes(fileName, fileToSave.DataArray);

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public void OpenFile(NSUrl fileUrl)
		{
			var docControl = UIDocumentInteractionController.FromUrl(fileUrl);

			var window = UIApplication.SharedApplication.KeyWindow;
			var subViews = window.Subviews;
			var lastView = subViews.Last();
			var frame = lastView.Frame;

			docControl.PresentOpenInMenu(frame, lastView, true);
		}

		public void OpenFile(string fileToOpen)
		{
			var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			var fileName = Path.Combine(documents, fileToOpen);

			if (NSFileManager.DefaultManager.FileExists(fileName))
			{
				var url = new NSUrl(fileName, true);
				OpenFile(url);
			}
		}

		public async void OpenFile(FileData fileToOpen)
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
				await SaveFile(fileToOpen);
				OpenFile(fileToOpen);
			}
		}
	}
}
=======
    /// <summary>
    /// Implementation for FilePicker
    /// </summary>
    public class FilePickerImplementation : NSObject, IUIDocumentMenuDelegate, IFilePicker
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

        private void OnFilePicked(FilePickerEventArgs e)
        {
            Handler?.Invoke(null, e);
        }

        public void DidPickDocumentPicker(UIDocumentMenuViewController documentMenu, UIDocumentPickerViewController documentPicker)
        {
            documentPicker.DidPickDocument += DocumentPicker_DidPickDocument;

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(documentPicker, true, null);
        }

        private void DocumentPicker_DidPickDocument(object sender, UIDocumentPickedEventArgs e)
        {
            var securityEnabled = e.Url.StartAccessingSecurityScopedResource();
			      var doc = new UIDocument(e.Url);
			      var data = NSData.FromUrl(e.Url);
			      var dataBytes = new byte[data.Length];

            System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));

			      string filename = doc.LocalizedName;

            // iCloud drive can return null for LocalizedName.
            if (filename == null)
            {
              // Retrieve actual filename by taking the last entry after / in FileURL.
              // e.g. /path/to/file.ext -> file.ext

              // pathname is either a string or null.
              var pathname = doc.FileUrl?.ToString();
              
              // filesplit is either:
              // 0 (pathname is null, or last / is at position 0)
              // -1 (no / in pathname)
              // positive int (last occurence of / in string)
              var filesplit = pathname?.LastIndexOf('/') ?? 0;

              filename = pathname?.Substring(filesplit + 1);
            }

            OnFilePicked(new FilePickerEventArgs(dataBytes, filename));
        }

        /// <summary>
        /// Lets the user pick a file with the systems default file picker
        /// For iOS iCloud drive needs to be configured
        /// </summary>
        /// <returns></returns>
        public async Task<FileData> PickFile()
        {
            var media = await TakeMediaAsync();

            return media;
        }

        private Task<FileData> TakeMediaAsync()
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<FileData>(id);

            if (Interlocked.CompareExchange(ref _completionSource, ntcs, null) != null)
                throw new InvalidOperationException("Only one operation can be active at a time");

            var allowedUtis = new string[] {
                UTType.UTF8PlainText,
                UTType.PlainText,
                UTType.RTF,
                UTType.PNG,
                UTType.Text,
                UTType.PDF,
                UTType.Image,
                UTType.UTF16PlainText,
                UTType.FileURL
            };

            var importMenu =
                new UIDocumentMenuViewController(allowedUtis, UIDocumentPickerMode.Import)
                {
                    Delegate = this,
                    ModalPresentationStyle = UIModalPresentationStyle.Popover
                };

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(importMenu, true, null);

            var presPopover = importMenu.PopoverPresentationController;

            if (presPopover != null)
            {
                presPopover.SourceView = UIApplication.SharedApplication.KeyWindow.RootViewController.View;
                presPopover.PermittedArrowDirections = UIPopoverArrowDirection.Down;
            }

            Handler = null;

            Handler = (s, e) =>
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);

                tcs.SetResult(new FileData(e.FilePath, e.FileName, () => File.OpenRead(e.FilePath)));
            };

            return _completionSource.Task;
        }

        public void WasCancelled(UIDocumentMenuViewController documentMenu)
        { }

        private int GetRequestId()
        {
            var id = _requestId;

            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }

        public async Task<bool> SaveFile(FileData fileToSave)
        {
            try
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = Path.Combine(documents, fileToSave.FileName);

                File.WriteAllBytes(fileName, fileToSave.DataArray);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void OpenFile(NSUrl fileUrl)
        {
            var docControl = UIDocumentInteractionController.FromUrl(fileUrl);

            var window = UIApplication.SharedApplication.KeyWindow;
            var subViews = window.Subviews;
            var lastView = subViews.Last();
            var frame = lastView.Frame;

            docControl.PresentOpenInMenu(frame, lastView, true);
        }

        public void OpenFile(string fileToOpen)
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var fileName = Path.Combine(documents, fileToOpen);

            if (NSFileManager.DefaultManager.FileExists(fileName))
            {
                var url = new NSUrl(fileName, true);
                OpenFile(url);
            }
        }

        public async void OpenFile(FileData fileToOpen)
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
                await SaveFile(fileToOpen);
                OpenFile(fileToOpen);
            }
        }
    }
}