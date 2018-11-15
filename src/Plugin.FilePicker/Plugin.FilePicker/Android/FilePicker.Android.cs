using Android.App;
using Android.Content;
using Java.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Plugin.FilePicker
{
    static partial class FilePicker
    {
        static readonly Context _context = Application.Context;
        static int _requestId;
        static TaskCompletionSource<FileData> _completionSource;

        static async Task<FileData> PlataformPickFile(string[] allowedTypes)
        {
            var media = await TakeMediaAsync(allowedTypes, Intent.ActionGetContent);

            return media;
        }

        static Task<bool> PlataformSaveFile(FileData fileToSave)
        {
            try
            {
                var myFile = new File(Android.OS.Environment.ExternalStorageDirectory, fileToSave.FileName);

                if (myFile.Exists())
                    return Task.FromResult(true);

                var fos = new FileOutputStream(myFile.Path);

                fos.Write(fileToSave.DataArray);
                fos.Close();

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
            var myFile = new File(Android.OS.Environment.ExternalStorageDirectory, fileToOpen);

            OpenFile(myFile);
        }

        static void PlataformOpenFile(FileData fileToOpen)
        {
            var myFile = new File(Android.OS.Environment.ExternalStorageDirectory, fileToOpen.FileName);

            if (!myFile.Exists())
                SaveFile(fileToOpen).GetAwaiter().GetResult();

            OpenFile(myFile);
        }

        static Task<FileData> TakeMediaAsync(string[] allowedTypes, string action)
        {
            if (_context.PackageManager.CheckPermission(
                Android.Manifest.Permission.ReadExternalStorage,
                _context.PackageName) == Android.Content.PM.Permission.Denied)
            {
                throw new InvalidOperationException("Android permission READ_EXTERNAL_STORAGE is missing or was denied by user");
            }

            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<FileData>(id);

            if (Interlocked.CompareExchange(ref _completionSource, ntcs, null) != null)
                throw new InvalidOperationException("Only one operation can be active at a time");

            try
            {
                var pickerIntent = new Intent(_context, typeof(FilePickerActivity));
                pickerIntent.SetFlags(ActivityFlags.NewTask);

                pickerIntent.PutExtra(nameof(allowedTypes), allowedTypes);

                _context.StartActivity(pickerIntent);

                EventHandler<FilePickerEventArgs> handler = null;
                EventHandler<FilePickerCancelledEventArgs> cancelledHandler = null;

                handler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref _completionSource, null);

                    FilePickerActivity.FilePickCancelled -= cancelledHandler;
                    FilePickerActivity.FilePicked -= handler;

                    tcs?.SetResult(new FileData(e.FilePath, e.FileName,
                        () =>
                        {
                            if (IOUtil.isMediaStore(e.FilePath))
                            {
                                var contentUri = Android.Net.Uri.Parse(e.FilePath);
                                return Application.Context.ContentResolver.OpenInputStream(contentUri);
                            }
                            else
                                return System.IO.File.OpenRead(e.FilePath);
                        }));
                };

                cancelledHandler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref _completionSource, null);

                    FilePickerActivity.FilePickCancelled -= cancelledHandler;
                    FilePickerActivity.FilePicked -= handler;

                    if (e?.Exception != null)
                    {
                        tcs?.SetException(e.Exception);
                    }
                    else
                    {
                        tcs?.SetResult(null);
                    }
                };

                FilePickerActivity.FilePickCancelled += cancelledHandler;
                FilePickerActivity.FilePicked += handler;
            }
            catch (Exception exAct)
            {
                Debug.Write(exAct);
                _completionSource.SetException(exAct);
            }

            return _completionSource.Task;
        }

        static int GetRequestId()
        {
            int id = _requestId;

            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }

        static void OpenFile(File fileToOpen)
        {
            var uri = Android.Net.Uri.FromFile(fileToOpen);
            var intent = new Intent();
            var mime = IOUtil.GetMimeType(uri.ToString());

            intent.SetAction(Intent.ActionView);
            intent.SetDataAndType(uri, mime);
            intent.SetFlags(ActivityFlags.NewTask);

            _context.StartActivity(intent);
        }
    }
}
