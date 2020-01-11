using Android.App;
using Android.Content;
using Android.Runtime;
using Java.IO;
using Plugin.XFileManager.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using File = Java.IO.File;
using Xamarin.Essentials;
using Android.Support.V4.Provider;
using Android;

// Adds permission for READ_EXTERNAL_STORAGE to the AndroidManifest.xml of the app project without
// the user of the plugin having to add it by himself/herself.
[assembly: UsesPermission(Android.Manifest.Permission.ReadExternalStorage)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]

//[assembly: Xamarin.Forms.Dependency(typeof(Plugin.Droid.FileManagerImplementation))]

namespace Plugin.XFileManager
{
    /// <summary>
    /// Implementation for file picking on Android
    /// </summary>
    [Preserve(AllMembers = true)]
    public class FileManagerImplementation : IXFileManager
    {
        /// <summary>
        /// Android context to use for picking
        /// </summary>
        private readonly Context context;

        //Context context = Android.App.Application.Context;
        WeakReference<Context> weakContext { get; set; } = new WeakReference<Context>(Android.App.Application.Context);

        /// <summary>
        /// Request ID for current picking call
        /// </summary>
        private int requestId;

        /// <summary>
        /// Task completion source for task when finished picking file
        /// </summary>
        private TaskCompletionSource<FileData> completionSource;

        /// <summary>
        /// Task completion source for task when finished picking folder
        /// </summary>
        private TaskCompletionSource<string> tcs_string;

        private TaskCompletionSource<bool> tcs_bool_as_int;

        public class FileSaved : EventArgs
        {
            public bool success { get; set; }
        }

        /// <summary>
        /// Creates a new file picker implementation
        /// </summary>
        public FileManagerImplementation()
        {
            this.context = Application.Context;
        }

        /// <summary>
        /// Implementation for picking a file on Android.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be selected while picking.
        /// On Android you can specify one or more MIME types, e.g.
        /// "image/png"; also wild card characters can be used, e.g. "image/*".
        /// </param>
        /// <returns>
        /// File data object, or null when user cancelled picking file
        /// </returns>
        public async Task<FileData> PickFile(string[] allowedTypes)
        {
            var fileData = await this.PickFileAsync(allowedTypes, Intent.ActionGetContent);

            return fileData;
        }

        /// <summary>
        /// File picking implementation
        /// </summary>
        /// <param name="allowedTypes">list of allowed types; may be null</param>
        /// <param name="action">Android intent action to use; unused</param>
        /// <returns>picked file data, or null when picking was cancelled</returns>
        private Task<FileData> PickFileAsync(string[] allowedTypes, string action)
        {
            var id = this.GetRequestId();

            var ntcs = new TaskCompletionSource<FileData>(id);

            var previousTcs = Interlocked.Exchange(ref this.completionSource, ntcs);
            if (previousTcs != null)
            {
                previousTcs.TrySetResult(null);
            }

            try
            {
                var pickerIntent = new Intent(this.context, typeof(FilePickActivity));
                pickerIntent.SetFlags(ActivityFlags.NewTask);

                pickerIntent.PutExtra(FilePickActivity.ExtraAllowedTypes, allowedTypes);

                this.context.StartActivity(pickerIntent);

                EventHandler<FilePickerEventArgs> handler = null;
                EventHandler<FilePickerCancelledEventArgs> cancelledHandler = null;

                handler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref this.completionSource, null);

                    FilePickActivity.FilePickCancelled -= cancelledHandler;
                    FilePickActivity.FilePicked -= handler;

                    tcs?.SetResult(new FileData(
                        e.FilePath,
                        e.FolderPath,
                        e.FileName,
                        () => GetStream(e.FilePath).Result));
                };

                cancelledHandler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref this.completionSource, null);

                    FilePickActivity.FilePickCancelled -= cancelledHandler;
                    FilePickActivity.FilePicked -= handler;

                    if (e?.Exception != null)
                    {
                        tcs?.SetException(e.Exception);
                    }
                    else
                    {
                        tcs?.SetResult(null);
                    }
                };

                FilePickActivity.FilePickCancelled += cancelledHandler;
                FilePickActivity.FilePicked += handler;
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                this.completionSource.SetException(ex);
            }

            return this.completionSource.Task;
        }


        /// <summary>
        /// Implementation for getting a stream of a file on Android.
        /// </summary>
        /// <param name="filePath">
        /// Specifies the file from which the stream should be opened.
        /// <returns>stream object</returns>
        public Task<Stream> GetStream(string filePath)
        {
            return Task.Run(() =>
            {
                if (IOUtil.IsMediaStore(filePath))
                {
                    var contentUri = Android.Net.Uri.Parse(filePath);
                    return Application.Context.ContentResolver.OpenInputStream(contentUri);
                }
                else
                {
                    return System.IO.File.OpenRead(filePath);
                }
            });
        }


        /// <summary>
        /// Returns a new request ID for a new call to PickFile()
        /// </summary>
        /// <returns>new request ID</returns>
        private int GetRequestId()
        {
            int id = this.requestId;

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

        /// <summary>
        /// Android implementation of saving a picked file to the external storage directory.
        /// </summary>
        /// <param name="fileToSave">picked file data for file to save</param>
        /// <returns>true when file was saved successfully, false when not</returns>
        public Task<bool> SaveFileToLocalAppStorage(FileData fileToSave)
        {
            try
            {
                var myFile = new File(Android.OS.Environment.ExternalStorageDirectory, fileToSave.FileName);

                if (myFile.Exists())
                {
                    myFile.Delete();
                }

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

        /// <summary>
        /// Android implementation of opening a file by using ActionView intent.
        /// </summary>
        /// <param name="fileToOpen">file to open in viewer</param>
        public void OpenFile(File fileToOpen)
        {
            var uri = Android.Net.Uri.FromFile(fileToOpen);
            var intent = new Intent();
            var mime = IOUtil.GetMimeType(uri.ToString());

            intent.SetAction(Intent.ActionView);
            intent.SetDataAndType(uri, mime);
            intent.SetFlags(ActivityFlags.NewTask);

            this.context.StartActivity(intent);
        }

        /// <summary>
        /// Android implementation of OpenFile(), opening a file already stored on external
        /// storage.
        /// </summary>
        /// <param name="fileToOpen">relative filename of file to open</param>
        public void OpenFile(string fileToOpen)
        {
            var myFile = new File(Android.OS.Environment.ExternalStorageDirectory, fileToOpen);

            this.OpenFile(myFile);
        }

        /// <summary>
        /// Android implementation of OpenFile(), opening a picked file in an external viewer. The
        /// picked file is saved to external storage before opening.
        /// </summary>
        /// <param name="fileToOpen">picked file data</param>
        public async void OpenFile(FileData fileToOpen)
        {
            var myFile = new File(Android.OS.Environment.ExternalStorageDirectory, fileToOpen.FileName);

            if (!myFile.Exists())
            {
                await this.SaveFileToLocalAppStorage(fileToOpen);
            }

            this.OpenFile(myFile);
        }

        public async void OpenFileViaEssentials(string fileToOpen)
        {
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(fileToOpen)
            });
        }

        /// <summary>
        /// Implementation for picking a folder on Android.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all folder types
        /// can be selected while picking.
        /// On Android you can specify one or more MIME types, e.g.
        /// "image/png"; also wild card characters can be used, e.g. "image/*".
        /// </param>
        /// <returns>
        /// Folder data object, or null when user cancelled picking folder
        /// </returns>
        public async Task<string> PickFolder()
        {
            var folderName = await this.PickFolderAsync(Intent.ActionOpenDocumentTree);

            return folderName;
        }


        public Task<string> PickFolderAsync(string action)
        {
            {
                var id = this.GetRequestId();

                var next = new TaskCompletionSource<string>(id);

                // Interlocked.CompareExchange(ref object location1, object value, object comparand)
                // Compare location1 with comparand.
                // If equal replace location1 by value.
                // Returns the original value of location1.
                // ---
                // In this context, tcs is compared to null, if equal tcs is replaced by next,
                // and original tcs is returned.
                // We then compare original tcs with null, if not null it means that a task was 
                // already started.
                if (Interlocked.CompareExchange(ref tcs_string, next, null) != null)
                {
                    return Task.FromResult<string>(null);
                }
                try 
                {
                    var pickIntent = new Intent(this.context, typeof(FolderPickActivity));
                    //weakContext.TryGetTarget(out Context newContext);
                    //var pickIntent = new Intent(newContext, typeof(FolderPickActivity));
                    pickIntent.SetFlags(ActivityFlags.NewTask);
                    //newContext.StartActivity(pickIntent);
                    this.context.StartActivity(pickIntent);


                    EventHandler<FolderPickerEventArgs> handler = null;
                    EventHandler<FolderPickerCancelledEventArgs> cancelledHandler = null;

                    handler = (sender, e) =>
                    {

                        // Interlocaked.Exchange(ref object location1, object value)
                        // Sets an object to a specified value and returns a reference to the original object.
                        // ---
                        // In this context, sets tcs to null and returns it.
                        var task = Interlocked.Exchange(ref tcs_string, null);

                        FolderPickActivity.FolderPickCancelled -= cancelledHandler;
                        FolderPickActivity.FolderPicked -= handler;

                        task?.SetResult(e.FolderPath);
                        //if (e.FolderPath != null)
                        //{
                        //    task.SetResult(e.FolderPath);

                        //}
                        //else
                        //{
                        //    task.SetCanceled();
                        //}
                    };

                    cancelledHandler = (s, e) =>
                    {
                        var tcs = Interlocked.Exchange(ref this.completionSource, null);

                        FolderPickActivity.FolderPickCancelled -= cancelledHandler;
                        FolderPickActivity.FolderPicked -= handler;

                        if (e?.Exception != null)
                        {
                            tcs?.SetException(e.Exception);
                        }
                        else
                        {
                            tcs?.SetResult(null);
                        }
                    };

                    FolderPickActivity.FolderPickCancelled += cancelledHandler;
                    FolderPickActivity.FolderPicked += handler;

                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                    this.completionSource.SetException(ex);
                }

                return tcs_string.Task;
            }
        }

        public Task<Stream> GetStreamFromPath(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveFileInFolder(FileData fileToSave)
        {
            var uniqueId = Guid.NewGuid();
            var next = new TaskCompletionSource<bool>(uniqueId);

            // Interlocked.CompareExchange(ref object location1, object value, object comparand)
            // Compare location1 with comparand.
            // If equal replace location1 by value.
            // Returns the original value of location1.
            // ---
            // In this context, tcs is compared to null, if equal tcs is replaced by next,
            // and original tcs is returned.
            // We then compare original tcs with null, if not null it means that a task was 
            // already started.
            if (Interlocked.CompareExchange(ref tcs_bool_as_int, next, null) != null)
            {
                return Task.FromResult<bool>(false);
            }

            EventHandler<PermissionRequestEventArgs> handler = null;

            weakContext.TryGetTarget(out Context newContext);
            
            var requestPermissionIntent = new Intent(newContext, typeof(RequestPermissionActivity));
            requestPermissionIntent.SetFlags(ActivityFlags.NewTask);
            requestPermissionIntent.PutExtra(RequestPermissionActivity.RequestedPermission, Manifest.Permission.WriteExternalStorage);




            handler = (sender, e) =>
            {

                // Interlocaked.Exchange(ref object location1, object value)
                // Sets an object to a specified value and returns a reference to the original object.
                // ---
                // In this context, sets tcs to null and returns it.
                var task = Interlocked.Exchange(ref tcs_bool_as_int, null);

                RequestPermissionActivity.OnPermissionGranted -= handler;

                if (e.success)
                {
                    try
                    {
                        var test = Android.Net.Uri.Parse(fileToSave.FolderPath);// + "/test.pdf");
                        weakContext.TryGetTarget(out Context newContext);


                        var documentFile = DocumentFile.FromTreeUri(newContext, test);

                        DocumentFile newFile = documentFile.FindFile(fileToSave.FileName);
                        if (newFile == null) 
                        {
                            newFile = documentFile.CreateFile("*/*", fileToSave.FileName);
                        }


                        //var documentFile = DocumentFile.FromTreeUri(activity, test);


                        var outputstream = newContext.ContentResolver.OpenOutputStream(newFile.Uri);

                        fileToSave.GetStream().CopyTo(outputstream);

                        outputstream.Flush();
                        outputstream.Close();

                        task.SetResult(e.success);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        task.SetCanceled();
                    }

                }
            };
            RequestPermissionActivity.OnPermissionGranted += handler;

            //proably need a try statement here
            newContext.StartActivity(requestPermissionIntent);



            return tcs_bool_as_int.Task;
        }
    }
}
