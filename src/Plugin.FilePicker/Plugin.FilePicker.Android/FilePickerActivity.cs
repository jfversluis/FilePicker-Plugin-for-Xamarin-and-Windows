using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System.Threading.Tasks;
using Plugin.FilePicker.Abstractions;
using Android.Provider;
using System.Net;
using System.Linq;

namespace Plugin.FilePicker
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [Preserve(AllMembers = true)]
    public class FilePickerActivity : Activity
    {
        private Context context;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            context = Application.Context;

            var intent = new Intent(Intent.ActionGetContent);

            //Determine Allowed MIME Types
            var mimeTypesAllowed = Intent.GetStringArrayListExtra("MIMETypesAllowed");
            if (mimeTypesAllowed != null && mimeTypesAllowed.Count > 0)
            {
                if (mimeTypesAllowed.Count == 1)
                {
                    //There's only one MIME Type, perhaps there is an App
                    //which best suits this type of file so specify it explicitly
                    intent.SetType(mimeTypesAllowed.First());
                }
                else
                {
                    //Let the App which responds to our Intent know that
                    //we only want files specific MIME types.
                    intent.PutExtra(Intent.ExtraMimeTypes, mimeTypesAllowed.ToArray());
                    intent.SetType("*/*");
                }
            }
            else
            {
                intent.SetType("*/*");
            }

            intent.AddCategory(Intent.CategoryOpenable);
            try
            {
                StartActivityForResult(Intent.CreateChooser(intent, "Select file"), 0);
            }
            catch (Exception exAct)
            {
                System.Diagnostics.Debug.Write(exAct);
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Canceled)
            {
                // Notify user file picking was cancelled.
                OnFilePickCancelled();
                Finish();
            }
            else
            {
                System.Diagnostics.Debug.Write(data.Data);
                try
                {
                    var _uri = data.Data;

                    var filePath = IOUtil.getPath(context, _uri);

                    if (string.IsNullOrEmpty(filePath))
                        filePath = _uri.Path;

                    //Updated with BUG Fix for Content picked files. See Original Repo: https://github.com/jfversluis/FilePicker-Plugin-for-Xamarin-and-Windows/commit/997f85f309f939c6ca2a87efedd4d8bb7618b6df#diff-2cc24c90e81a2832630bd9d47d6948e1
                    filePath = IOUtil.isMediaStore(_uri.Scheme) ? _uri.ToString() : _uri.Path;
                    byte[] file;
                    if (IOUtil.isMediaStore(_uri.Scheme))
                        file = IOUtil.readFile(context, _uri);
                    else
                        file = IOUtil.readFile(filePath);
                    //End Bug fix code

                    var fileName = GetFileName(context, _uri);
                    
                    OnFilePicked(new FilePickerEventArgs(file, fileName, filePath));
                }
                catch (Exception readEx)
                {
                    // Notify user file picking failed.
                    OnFilePickCancelled();
                    System.Diagnostics.Debug.Write(readEx);
                }
                finally
                {
                    Finish();
                }
            }
        }

        string GetFileName(Context ctx, Android.Net.Uri uri)
        {

            string[] projection = { MediaStore.MediaColumns.DisplayName };

            var cr = ctx.ContentResolver;
            var name = "";
            var metaCursor = cr.Query(uri, projection, null, null, null);

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
                return name;
            else
                return System.IO.Path.GetFileName(WebUtility.UrlDecode(uri.ToString()));
        }

        internal static event EventHandler<FilePickerEventArgs> FilePicked;
        internal static event EventHandler<EventArgs> FilePickCancelled;

        private static void OnFilePickCancelled()
        {
            FilePickCancelled?.Invoke(null, null);
        }

        private static void OnFilePicked(FilePickerEventArgs e)
        {
            FilePicked?.Invoke(null, e);
        }
    }
}