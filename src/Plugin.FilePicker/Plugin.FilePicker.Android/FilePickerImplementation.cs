using Android.App;
using Android.Content;
using Android.Runtime;
using Java.IO;
using Plugin.FilePicker.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    ///
    [Preserve (AllMembers = true)]
    public class FilePickerImplementation : IFilePicker
    {
        private readonly Context _context;
        private int _requestId;
        private TaskCompletionSource<FileData> _completionSource;

        public FilePickerImplementation ()
        {
            _context = Application.Context;
        }

        public async Task<FileData> PickFile (string[] allowedTypes)
        {
            //var media = await TakeMediaAsync ("file/*", Intent.ActionGetContent);
            var media = await TakeMediaAsync(allowedTypes, Intent.ActionGetContent);

            return media;
        }

        //private Task<FileData> TakeMediaAsync (string type, string action)
        private Task<FileData> TakeMediaAsync(string[] allowedTypes, string action)
        {
            var id = GetRequestId ();

            var ntcs = new TaskCompletionSource<FileData> (id);

            if (Interlocked.CompareExchange (ref _completionSource, ntcs, null) != null)
                throw new InvalidOperationException ("Only one operation can be active at a time");

            try {
                var pickerIntent = new Intent (this._context, typeof (FilePickerActivity));
                pickerIntent.SetFlags (ActivityFlags.NewTask);

                pickerIntent.PutExtra(nameof(allowedTypes), allowedTypes);

                this._context.StartActivity (pickerIntent);

                EventHandler<FilePickerEventArgs> handler = null;
                EventHandler<EventArgs> cancelledHandler = null;

                handler = (s, e) => {
                    var tcs = Interlocked.Exchange (ref _completionSource, null);

                    FilePickerActivity.FilePicked -= handler;

                    tcs?.SetResult (new FileData (e.FilePath, e.FileName,
                        () =>
                        {
                            if (IOUtil.isMediaStore(e.FilePath))
                                return new System.IO.MemoryStream(e.FileByte);
                            else
                                return System.IO.File.OpenRead (e.FilePath);
                        }));
                };

                cancelledHandler = (s, e) => {
                    var tcs = Interlocked.Exchange (ref _completionSource, null);

                    FilePickerActivity.FilePickCancelled -= cancelledHandler;

                    tcs?.SetResult (null);
                };

                FilePickerActivity.FilePickCancelled += cancelledHandler;
                FilePickerActivity.FilePicked += handler;
            } catch (Exception exAct) {
                Debug.Write (exAct);
            }

            return _completionSource.Task;
        }

        private int GetRequestId ()
        {
            int id = _requestId;

            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }

        public Task<bool> SaveFile (FileData fileToSave)
        {
            try {
                var myFile = new File (Android.OS.Environment.ExternalStorageDirectory, fileToSave.FileName);

                if (myFile.Exists ())
                    return Task.FromResult(true);

                var fos = new FileOutputStream (myFile.Path);

                fos.Write (fileToSave.DataArray);
                fos.Close ();

                return Task.FromResult(true);
            } catch (Exception ex) {
                Debug.WriteLine (ex.Message);
                return Task.FromResult(false);
            }
        }

        public void OpenFile (File fileToOpen)
        {
            var uri = Android.Net.Uri.FromFile (fileToOpen);
            var intent = new Intent ();
            var mime = IOUtil.GetMimeType (uri.ToString ());

            intent.SetAction (Intent.ActionView);
            intent.SetDataAndType (uri, mime);
            intent.SetFlags (ActivityFlags.NewTask);

            _context.StartActivity (intent);
        }

        public void OpenFile (string fileToOpen)
        {
            var myFile = new File (Android.OS.Environment.ExternalStorageState, fileToOpen);

            OpenFile (myFile);
        }

        public async void OpenFile (FileData fileToOpen)
        {
            var myFile = new File (Android.OS.Environment.ExternalStorageState, fileToOpen.FileName);

            if (!myFile.Exists ())
                await SaveFile (fileToOpen);

            OpenFile (fileToOpen);
        }
    }
}