//using System;
//using System.IO;
//using Android.Content;
//using Java.IO;
//using Xamarin.Forms;
//using System.Threading.Tasks;
//using Plugin.Droid;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Android.Graphics;

//using Android.App;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Android.Provider;
//using System.Threading.Tasks;
//using Android.Support.V4.Provider;
//using Android;
//using Android.Content.PM;
//using Android.Support.V4.Content;
//using Android.Support.V4.App;

//[assembly: Dependency(typeof(SaveAndroid))]

//class SaveAndroid: IDocumentsSaveLoadPick
//    {
//    public static readonly int REQUEST_CODE_OPEN_DIRECTORY = 1;
//    MainActivity activity = new MainActivity();
//    Android.Net.Uri FolderPath;
//    string OriginalPath;
//    string RealPath;
//    Stream str = null;

//    public static readonly int READ_REQUEST_CODE = 3;
//    public static readonly int REQUEST_WRITE_EXTERN_DIR = 2;





//    //Method to save document as a file in Android and view the saved document
//    public async Task SaveAndView(string fileName, String contentType, MemoryStream stream)
//        {
//            string root = null;
//            //Get the root path in android device.
//            if (Android.OS.Environment.IsExternalStorageEmulated)
//            {
//                root = Android.OS.Environment.ExternalStorageDirectory.ToString();
//            }
//            else
//                root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

//            //Create directory and file 
//            Java.IO.File myDir = new Java.IO.File(root + "/Syncfusion");
//            myDir.Mkdir();

//            Java.IO.File file = new Java.IO.File(myDir, fileName);

//            //Remove if the file exists
//            if (file.Exists()) file.Delete();

//            //Write the stream into the file
//            FileOutputStream outs = new FileOutputStream(file);
//            outs.Write(stream.ToArray());

//            outs.Flush();
//            outs.Close();

//            //Invoke the created file for viewing 
//            if (file.Exists())
//            {
//                Android.Net.Uri path = Android.Net.Uri.FromFile(file);
//                string extension = Android.Webkit.MimeTypeMap.GetFileExtensionFromUrl(Android.Net.Uri.FromFile(file).ToString());
//                string mimeType = Android.Webkit.MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);
//                Intent intent = new Intent(Intent.ActionView);
//                intent.SetDataAndType(path, mimeType);
//                Forms.Context.StartActivity(Intent.CreateChooser(intent, "Choose App"));
//            }
//        }




//    public void SelectDirectory(Stream stream)
//    {

//        activity = Xamarin.Forms.Forms.Context as MainActivity;
//        activity.Intent = new Intent();

//        activity.Intent.SetAction(Intent.ActionOpenDocumentTree);
//        //activity.Intent.PutExtra("android.content.extra.SHOW_ADVANCED", true);
//        activity.StartActivityForResult(activity.Intent, REQUEST_CODE_OPEN_DIRECTORY);
//        //activity.StartActivityForResult(Intent.CreateChooser(activity.Intent, "Select Save Location"), REQUEST_CODE_OPEN_DIRECTORY);




//        activity.ActivityResult += (object sender, ActivityResultEventArgs e) =>
//        {

//            FolderPath = e.Intent.Data;
//            var documentFile = DocumentFile.FromTreeUri(activity, FolderPath);
//            //var test = GetActualPathFromFile(FolderPath, xx);
//            string DummyPath = FolderPath.Path;
//            OriginalPath = DummyPath.Split(':')[1];
//            RealPath = "/storage/emulated/0/" + OriginalPath;

//            if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
//            {
//                str = stream;



//                //Save(test);
//                str.Position = 0;
//                var fileBytes = ReadFully(str);
//                Bitmap bitmap = BitmapFactory.DecodeByteArray(fileBytes, 0, fileBytes.Length);



//                //ExportBitmapAsJPG(bitmap, documentFile);
//                var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
//                //var filePath = System.IO.Path.Combine(RealPath, "image.jpg");


//                //var stream = new MemoryStream();
//                var testg = documentFile.CreateFile("image/jpeg", "test.jpg");
//                //var ftest = test.
//                var test2 = documentFile.Uri.Path;
//                //var stream2 = new FileStream(testg.Uri.Path, FileMode.Create);
//                //bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream2);


//                stream.Close();


//                var outputstream = activity.ContentResolver.OpenOutputStream(testg.Uri);
//                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, outputstream);
//                //.Write(bitmap.ToArray(), 0,100);
//                //out.write(infos.get(i).getContent().getBytes());
//                outputstream.Flush();
//                outputstream.Close();

//                Toast.MakeText(activity, "Image has been saved into" + RealPath, ToastLength.Short).Show();
//            }
//            else
//            {
//                ActivityCompat.RequestPermissions(activity, new String[] { Manifest.Permission.WriteExternalStorage }, REQUEST_WRITE_EXTERN_DIR);
//            }

//            //activity.ContentResolver
//        };

//    }







//    public static byte[] ReadFully(Stream input)
//    {
//        byte[] buffer = new byte[16 * 1024];
//        using (MemoryStream ms = new MemoryStream())
//        {
//            int read;
//            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
//            {
//                ms.Write(buffer, 0, read);
//            }
//            return ms.ToArray();
//        }
//    }

//}
