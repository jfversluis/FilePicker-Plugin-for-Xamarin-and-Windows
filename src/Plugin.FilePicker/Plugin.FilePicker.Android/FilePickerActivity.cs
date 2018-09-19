using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System.Threading.Tasks;
using Plugin.FilePicker.Abstractions;
using Android.Provider;
using System.Net;

namespace Plugin.FilePicker
{
    [Activity (ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [Preserve (AllMembers = true)]
    public class FilePickerActivity : Activity
    {
        private Context context;

        protected override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate (savedInstanceState);

            context = Application.Context;

            string[] allowedTypes = Intent.GetStringArrayExtra("allowedTypes") ?? null;

            var intent = new Intent (Intent.ActionGetContent);

            if (allowedTypes != null)
            {
                var typeString = "";
                for (var i = 0; i < allowedTypes.Length; i++)
                {
                    if (allowedTypes[i].Contains("/"))
                    {
                        typeString += allowedTypes[i];

                        if (i != allowedTypes.Length - 1)
                        {
                            typeString += "|";
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(typeString))
                {
                    typeString = "*/*";
                }
                intent.SetType(typeString);
            }
            else
            {
                intent.SetType("*/*");
            }

            intent.AddCategory (Intent.CategoryOpenable);
            try {
                StartActivityForResult (Intent.CreateChooser (intent, "Select file"), 0);
            } catch (Exception exAct) {
                System.Diagnostics.Debug.Write (exAct);
            }
        }

        protected override void OnActivityResult (int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult (requestCode, resultCode, data);

            if (resultCode == Result.Canceled) {
                // Notify user file picking was cancelled.
                OnFilePickCancelled ();
                Finish ();
            } else {
                System.Diagnostics.Debug.Write (data.Data);
                try {
                    var _uri = data.Data;

                    var filePath = IOUtil.getPath (context, _uri);

                    if (string.IsNullOrEmpty (filePath))
                        filePath = IOUtil.isMediaStore(_uri.Scheme) ? _uri.ToString() : _uri.Path;
                    byte[] file;
                    if (IOUtil.isMediaStore(_uri.Scheme))
                        file = IOUtil.readFile(context, _uri);
                    else
                        file = IOUtil.readFile (filePath);

                    var fileName = GetFileName (context, _uri);

                    OnFilePicked (new FilePickerEventArgs (file, fileName, filePath));
                } catch (Exception readEx) {
                    // Notify user file picking failed.
                    OnFilePickCancelled ();
                    System.Diagnostics.Debug.Write (readEx);
                } finally {
                    Finish ();
                }
            }
        }

        string GetFileName (Context ctx, Android.Net.Uri uri)
        {

            string [] projection = { MediaStore.MediaColumns.DisplayName };

            var cr = ctx.ContentResolver;
            var name = "";
            var metaCursor = cr.Query (uri, projection, null, null, null);

            if (metaCursor != null) {
                try {
                    if (metaCursor.MoveToFirst ()) {
                        name = metaCursor.GetString (0);
                    }
                } finally {
                    metaCursor.Close ();
                }
            }

            if (!string.IsNullOrWhiteSpace(name))
                return name;
            else
                return System.IO.Path.GetFileName(WebUtility.UrlDecode(uri.ToString()));
        }

        internal static event EventHandler<FilePickerEventArgs> FilePicked;
        internal static event EventHandler<EventArgs> FilePickCancelled;

        private static void OnFilePickCancelled ()
        {
            FilePickCancelled?.Invoke (null, null);
        }

        private static void OnFilePicked (FilePickerEventArgs e)
        {
            FilePicked?.Invoke(null, e);
        }
    }
}