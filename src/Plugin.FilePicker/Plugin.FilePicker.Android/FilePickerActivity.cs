using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System.Threading.Tasks;
using Plugin.FilePicker.Abstractions;
using Android.Provider;

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


            var intent = new Intent (Intent.ActionGetContent);
            intent.SetType ("*/*");

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
                        filePath = _uri.Path;

                    var file = IOUtil.readFile (filePath);

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
            return name;
        }

        internal static event EventHandler<FilePickerEventArgs> FilePicked;
        internal static event EventHandler<EventArgs> FilePickCancelled;

        private static void OnFilePickCancelled ()
        {
            FilePickCancelled?.Invoke (null, null);
        }

        private static void OnFilePicked (FilePickerEventArgs e)
        {
            var picked = FilePicked;

            if (picked != null)
                picked (null, e);
        }
    }
}