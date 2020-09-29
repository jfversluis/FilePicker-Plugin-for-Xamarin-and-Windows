using Android.App;
using Android.Content;
using Android.Runtime;
using Java.IO;
using Plugin.FilePicker.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Debug = System.Diagnostics.Debug;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    ///
    [Preserve(AllMembers = true)]
    public class FilePickerImplementation : IFilePicker
    {
        /// <summary>
        /// Android context to use for picking
        /// </summary>
        private readonly Context context;

        /// <summary>
        /// Request ID for current picking call
        /// </summary>
        private int requestId;

        /// <summary>
        /// Task completion source for task when finished picking
        /// </summary>
        private TaskCompletionSource<FileData> completionPickerSource;

        /// <summary>
        /// Task completion source for task when finished creating or overwriting
        /// </summary>
        private TaskCompletionSource<FilePlaceholder> completionCreationSource;

        /// <summary>
        /// Creates a new file picker implementation
        /// </summary>
        public FilePickerImplementation()
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
            var fileData = await this.PickFileAsync(allowedTypes);

            return fileData;
        }

        /// <summary>
        /// Implementation for saving a file on Android.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be saved to while picking.
        /// On Android you can specify one or more MIME types, e.g.
        /// "image/png"; also wild card characters can be used, e.g. "image/*".
        /// </param>
        /// <returns>
        /// File Placeholder object, or null when user cancelled picking file
        /// </returns>
        public async Task<FilePlaceholder> CreateOrOverwriteFile(string[] allowedTypes = null)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
            {
                throw new KeyCharacterMap.UnavailableException("Android version must be kitkat or higher");
            }

            var placeHolder = await this.CreatOrOverwriteFileAsync(allowedTypes);

            return placeHolder;
        }

        /// <summary>
        /// File picking implementation
        /// </summary>
        /// <param name="allowedTypes">list of allowed types; may be null</param>
        /// <param name="action">Android intent action to use; unused</param>
        /// <returns>picked file data, or null when picking was cancelled</returns>
        private Task<FileData> PickFileAsync(string[] allowedTypes)
        {
            var id = this.GetRequestId();

            var ntcs = new TaskCompletionSource<FileData>(id);

            var previousTcs = Interlocked.Exchange(ref this.completionPickerSource, ntcs);
            if (previousTcs != null)
            {
                previousTcs.TrySetResult(null);
            }

            try
            {
                var pickerIntent = new Intent(this.context, typeof(FilePickerActivity));
                pickerIntent.SetFlags(ActivityFlags.NewTask);

                pickerIntent.PutExtra(FilePickerActivity.ExtraAllowedTypes, allowedTypes);

                this.context.StartActivity(pickerIntent);

                EventHandler<FilePickerEventArgs> handler = null;
                EventHandler<FilePickerCancelledEventArgs> cancelledHandler = null;

                handler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref this.completionPickerSource, null);

                    FilePickerActivity.FilePickCancelled -= cancelledHandler;
                    FilePickerActivity.FilePicked -= handler;

                    tcs?.SetResult(new FileData(
                        e.FilePath,
                        e.FileName,
                        () =>
                        {
                            if (IOUtil.IsMediaStore(e.FilePath))
                            {
                                var contentUri = Android.Net.Uri.Parse(e.FilePath);
                                return Application.Context.ContentResolver.OpenInputStream(contentUri);
                            }
                            else
                            {
                                return System.IO.File.OpenRead(e.FilePath);
                            }
                        }));
                };

                cancelledHandler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref this.completionPickerSource, null);

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
            catch (Exception ex)
            {
                Debug.Write(ex);
                this.completionPickerSource.SetException(ex);
            }

            return this.completionPickerSource.Task;
        }

        /// <summary>
        /// File saving implementation
        /// </summary>
        /// <param name="allowedTypes">list of allowed types; may be null</param>
        /// <param name="action">Android intent action to use; unused</param>
        /// <returns>picked file data, or null when picking was cancelled</returns>
        private Task<FilePlaceholder> CreatOrOverwriteFileAsync(string[] allowedTypes)
        {
            var id = this.GetRequestId();

            var ntcs = new TaskCompletionSource<FilePlaceholder>(id);

            var previousTcs = Interlocked.Exchange(ref this.completionCreationSource, ntcs);
            if (previousTcs != null)
            {
                previousTcs.TrySetResult(null);
            }

            try
            {
                var pickerIntent = new Intent(this.context, typeof(FileSaverActivity));
                pickerIntent.SetFlags(ActivityFlags.NewTask);

                pickerIntent.PutExtra(FilePickerActivity.ExtraAllowedTypes, allowedTypes);

                this.context.StartActivity(pickerIntent);

                EventHandler<FilePickerEventArgs> handler = null;
                EventHandler<FilePickerCancelledEventArgs> cancelledHandler = null;

                handler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref this.completionCreationSource, null);

                    FilePickerActivity.FilePickCancelled -= cancelledHandler;
                    FilePickerActivity.FilePicked -= handler;

                    tcs?.SetResult(new FilePlaceholder(
                        e.FilePath,
                        e.FileName,
                        null));
                };

                cancelledHandler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref this.completionCreationSource, null);

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
            catch (Exception ex)
            {
                Debug.Write(ex);
                this.completionPickerSource.SetException(ex);
            }

            return this.completionCreationSource.Task;
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
    }
}