using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System.Threading.Tasks;
using Plugin.FilePicker.Abstractions;
using Android.Provider;
using System.Net;
using Android;
using Android.Content.PM;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Activity that is shown in order to start Android file picking using ActionGetContent
    /// intent.
    /// </summary>
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [Preserve(AllMembers = true)]
    public class FileSaverActivity : Activity
    {
        /// <summary>
        /// Intent Extra constant to pass list of allowed types to FilePicker activity.
        /// </summary>
        public const string ExtraAllowedTypes = "EXTRA_ALLOWED_TYPES";

        /// <summary>
        /// This variable gets passed when the request for the permission to access storage
        /// gets send and then gets again read whne the request gets answered.
        /// </summary>
        private const int RequestStorage = 1;


        /// <summary>
        /// Android context to be used for opening file picker
        /// </summary>
        private Context context;

        /// <summary>
        /// Called when activity is about to be created; immediately starts file picker intent
        /// when permission is available, otherwise requests permission on API level >= 23 or
        /// throws an error if the API level is below.
        /// </summary>
        /// <param name="savedInstanceState">saved instance state; unused</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.context = Application.Context;

            /*If OnCreate is called with savedInstanceState, it's an attempt to restore the activity after it was killed by the system,
            but the picker has already been presented before and we don't want to present it again. Reproducible with DKA.*/
            if (savedInstanceState != null)
                return;

            var readPermission = this.context.PackageManager.CheckPermission(
                Manifest.Permission.ReadExternalStorage,
                this.context.PackageName);

            var writePermission = this.context.PackageManager.CheckPermission(
                Manifest.Permission.WriteExternalStorage,
                this.context.PackageName);

            if (readPermission == Permission.Granted && writePermission == Permission.Granted)
            {
                this.StartPicker();
            }
            else
            {
                if ((int)Build.VERSION.SdkInt >= 23)
                {
                    this.RequestPermissions(new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, RequestStorage);
                }
                else
                {
                    throw new InvalidOperationException(
                        "Android permission READ_EXTERNAL_STORAGE is missing and API level lower than 23, so it can't be requested");
                }
            }
        }

        /// <summary>
        /// Receives the answer from the dialog that asks for the READ_EXTERNAL_STORAGE permission
        /// and starts the FilePicker if it's granted or otherwise closes this activity.
        /// </summary>
        /// <param name="requestCode">requestCode; shows us that the dialog we requested is responsible for this answer</param>
        /// <param name="permissions">permissions; unused</param>
        /// <param name="grantResults">grantResults; contains the result of the dialog to request the permission</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == RequestStorage)
            {
                if (grantResults.Length == 2 &&
                    grantResults[0] == Permission.Granted &&
                    grantResults[1] == Permission.Granted)
                {
                    this.StartPicker();
                }
                else
                {
                    OnFilePickCancelled();
                    this.Finish();
                }
            }
        }

        /// <summary>
        /// Sends an intent to start the FilePicker
        /// </summary>
        private void StartPicker()
        {
            var intent = new Intent(
                Build.VERSION.SdkInt < BuildVersionCodes.Kitkat ? throw new Java.Lang.Exception("OS to low... how did you get here???") : Intent.ActionCreateDocument);

            var allowedTypes = Intent.GetStringArrayExtra(ExtraAllowedTypes)?.
                Where(o => !string.IsNullOrEmpty(o) && o.Contains("/")).ToList();

            if (allowedTypes != null && allowedTypes.Count > 0)
            {
                intent.SetType(allowedTypes[0]);
                allowedTypes.RemoveAt(0);

                if (allowedTypes.Count > 0)
                {
                    intent.PutExtra(Intent.ExtraMimeTypes, allowedTypes.ToArray());
                }
            }
            else
            {
                intent.SetType("*/*");
            }

            intent.AddFlags(ActivityFlags.GrantPersistableUriPermission);
            
            try
            {
                this.StartActivityForResult(Intent.CreateChooser(intent, "Save file"), 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
        }

        /// <summary>
        /// Called when activity started with StartActivityForResult() returns.
        /// </summary>
        /// <param name="requestCode">request code used in StartActivityForResult()</param>
        /// <param name="resultCode">result code</param>
        /// <param name="data">intent data from file picking</param>
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Canceled)
            {
                // Notify user file picking was cancelled.
                OnFilePickCancelled();
                this.Finish();
            }
            else
            {
                try
                {
                    if (data?.Data == null)
                        throw new Exception("File picking returned no valid data");

                    System.Diagnostics.Debug.Write(data.Data);

                    var uri = data.Data;

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                    {
                        context.ContentResolver.TakePersistableUriPermission(
                            uri,
                            ActivityFlags.GrantReadUriPermission);
                    }

                    // The scoped storage feature was introduced on Android Q and it prevents direct file access on external storage. 
                    // Thus, using the IOUtil.GetPath should be avoided because a System.UnauthorizedAccessException
                    // will be thrown when calling File.OpenRead() on the local path.
                    // For more information, see: https://developer.android.com/training/data-storage/#scoped-storage
                    var filePath = (int)Build.VERSION.SdkInt >= 29
                        ? uri.ToString()
                        : IOUtil.GetPath(this.context, uri);

                    if (string.IsNullOrEmpty(filePath))
                    {
                        filePath = IOUtil.IsMediaStore(uri.Scheme) ? uri.ToString() : uri.Path;
                    }

                    var fileName = this.GetFileName(this.context, uri);

                    OnFilePicked(new FilePickerEventArgs(fileName, filePath));
                }
                catch (Exception readEx)
                {
                    System.Diagnostics.Debug.Write(readEx);

                    // Notify user file picking failed.
                    FilePickCancelled?.Invoke(
                        this,
                        new FilePickerCancelledEventArgs
                        {
                            Exception = readEx
                        });
                }
                finally
                {
                    this.Finish();
                }
            }
        }

        /// <summary>
        /// Retrieves file name part from given Uri
        /// </summary>
        /// <param name="context">Android context to access content resolver</param>
        /// <param name="uri">Uri to get filename for</param>
        /// <returns>file name part</returns>
        private string GetFileName(Context context, Android.Net.Uri uri)
        {
            string[] projection = { MediaStore.MediaColumns.DisplayName };

            var resolver = context.ContentResolver;
            var name = string.Empty;
            var metaCursor = resolver.Query(uri, projection, null, null, null);

            if (metaCursor != null)
            {
                try
                {
                    if (metaCursor.MoveToFirst())
                    {
                        name = metaCursor.GetString(0);
                    }
                }
                finally
                {
                    metaCursor.Close();
                }
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!Path.HasExtension(name))
                    name = name.TrimEnd('.') + '.' + IOUtil.GetExtensionFromUri(context, uri);

                return name;
            }
            else
            {
                var extension = IOUtil.GetExtensionFromUri(context, uri);
                if (!string.IsNullOrEmpty(extension))
                    return "filename." + extension;
                else
                    return Path.GetFileName(WebUtility.UrlDecode(uri.ToString()));
            }
        }

        /// <summary>
        /// Event that gets signaled when file has successfully been picked
        /// </summary>
        internal static event EventHandler<FilePickerEventArgs> FilePicked;

        /// <summary>
        /// Event that gets signaled when file picking has been cancelled by the user
        /// </summary>
        internal static event EventHandler<FilePickerCancelledEventArgs> FilePickCancelled;

        /// <summary>
        /// Signals event that file picking was cancelled
        /// </summary>
        private static void OnFilePickCancelled()
        {
            FilePickCancelled?.Invoke(null, null);
        }

        /// <summary>
        /// Signals event that file picking has finished
        /// </summary>
        /// <param name="args">file picker event args</param>
        private static void OnFilePicked(FilePickerEventArgs args)
        {
            FilePicked?.Invoke(null, args);
        }
    }
}