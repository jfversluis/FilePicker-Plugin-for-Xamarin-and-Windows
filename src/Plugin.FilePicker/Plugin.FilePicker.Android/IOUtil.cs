using System;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Database;
using Java.IO;
using Android.Webkit;
using Plugin.FilePicker.Abstractions;

namespace Plugin.FilePicker
{
    public class IOUtil
    {

        public static string getPath(Context context, Android.Net.Uri uri)
        {
            bool isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            // DocumentProvider
            if (isKitKat && DocumentsContract.IsDocumentUri(context, uri))
            {
                // ExternalStorageProvider
                if (isExternalStorageDocument(uri))
                {
                    var docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(':');
                    var type = split[0];

                    if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }

                    // TODO handle non-primary volumes
                }
                // DownloadsProvider
                else if (isDownloadsDocument(uri))
                {

                    string id = DocumentsContract.GetDocumentId(uri);
                    Android.Net.Uri contentUri = ContentUris.WithAppendedId(
                            Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    return getDataColumn(context, contentUri, null, null);
                }
                // MediaProvider
                else if (isMediaDocument(uri))
                {
                    var docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(':');
                    var type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    var selection = "_id=?";
                    var selectionArgs = new string[] {
                        split[1]
                    };

                    return getDataColumn(context, contentUri, selection, selectionArgs);
                }
            }
            //Updated with BUG Fix for Content picked files. See Original Repo: https://github.com/jfversluis/FilePicker-Plugin-for-Xamarin-and-Windows/commit/997f85f309f939c6ca2a87efedd4d8bb7618b6df#diff-2cc24c90e81a2832630bd9d47d6948e1
            // MediaStore (and general)
            if (isMediaStore(uri.Scheme))
            {
                return getDataColumn(context, uri, null, null);
            }
            //END Bug fix
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        //Updated with BUG Fix for Content picked files. See Original Repo: https://github.com/jfversluis/FilePicker-Plugin-for-Xamarin-and-Windows/commit/997f85f309f939c6ca2a87efedd4d8bb7618b6df#diff-2cc24c90e81a2832630bd9d47d6948e1
        /// <summary>
        /// Checks if the scheme part of the URL matches the content:// scheme
        /// </summary>
        /// <param name="scheme">scheme part of URL</param>
        /// <returns>true when it matches, false when not</returns>
        public static bool isMediaStore(string scheme)
        {
            return scheme.StartsWith("content");
        }
        //END BUG FIX

        public static string getDataColumn(Context context, Android.Net.Uri uri, string selection,
        string[] selectionArgs)
        {

            ICursor cursor = null;
            string column = MediaStore.Files.FileColumns.Data;
            string[] projection = { column };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs,
                        null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int column_index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(column_index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is ExternalStorageProvider.
         */
        public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is DownloadsProvider.
         */
        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is MediaProvider.
         */
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        public static Byte[] readFile(Context context, Android.Net.Uri uri)
        {
            using (var inStream = context.ContentResolver.OpenInputStream(uri))
                return FileData.ReadFully(inStream);
        }

        public static byte[] readFile(string file)
        {
            try
            {
                return readFile(new File(file));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return new byte[0];
            }
        }

        public static byte[] readFile(File file)
        {
            // Open file
            var f = new RandomAccessFile(file, "r");

            try
            {
                // Get and check length
                long longlength = f.Length();
                var length = (int)longlength;

                if (length != longlength)
                    throw new IOException("Filesize exceeds allowed size");
                // Read file and return data
                byte[] data = new byte[length];
                f.ReadFully(data);
                return data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return new byte[0];
            }
            finally
            {
                f.Close();
            }
        }

        public static string GetMimeType(string url)
        {
            string type = null;
            var extension = MimeTypeMap.GetFileExtensionFromUrl(url);

            if (extension != null)
            {
                type = MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);
            }

            return type;
        }
    }
}