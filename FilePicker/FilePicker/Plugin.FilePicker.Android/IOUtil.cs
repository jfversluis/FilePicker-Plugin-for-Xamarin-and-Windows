using System;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Database;
using Java.IO;
using Android.Webkit;
using System.IO;
using System.Collections.Generic;
using Plugin.FilePicker.Abstractions;

namespace Plugin.FilePicker
{
    public class IOUtil
    {

        public static string getPath (Context context, Android.Net.Uri uri)
        {
            bool isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            // DocumentProvider
            if (isKitKat && DocumentsContract.IsDocumentUri (context, uri)) {
                // ExternalStorageProvider
                if (isExternalStorageDocument (uri)) {
                    var docId = DocumentsContract.GetDocumentId (uri);
                    string [] split = docId.Split (':');
                    var type = split [0];

                    if ("primary".Equals (type, StringComparison.OrdinalIgnoreCase)) {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split [1];
                    }

                    // TODO handle non-primary volumes
                }
                // DownloadsProvider
                else if (isDownloadsDocument (uri)) {

                    string id = DocumentsContract.GetDocumentId (uri);
                    Android.Net.Uri contentUri = ContentUris.WithAppendedId (
                            Android.Net.Uri.Parse ("content://downloads/public_downloads"), long.Parse (id));

                    return getDataColumn (context, contentUri, null, null);
                }
                // MediaProvider
                else if (isMediaDocument (uri)) {
                    var docId = DocumentsContract.GetDocumentId (uri);
                    string [] split = docId.Split (':');
                    var type = split [0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals (type)) {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    } else if ("video".Equals (type)) {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    } else if ("audio".Equals (type)) {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    var selection = "_id=?";
                    var selectionArgs = new string [] {
                        split[1]
                    };

                    return getDataColumn (context, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            if (isMediaStore(uri.Scheme)) {
                return getDataColumn (context, uri, null, null);
            }
            // File
            else if ("file".Equals (uri.Scheme, StringComparison.OrdinalIgnoreCase)) {
                return uri.Path;
            }

            return null;
        }

        public static bool isMediaStore(String scheme)
        {
            return scheme.StartsWith("content");//.Equals(scheme, StringComparison.OrdinalIgnoreCase);
        }

        public static string getDataColumn (Context context, Android.Net.Uri uri, 
            string selection, string [] selectionArgs)
        {

            ICursor cursor = null;
            var column = MediaStore.Files.FileColumns.Data;
            string [] projection = { column };
            try
            {
                cursor = context.ContentResolver.Query(uri,
                    projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int column_index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(column_index);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
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
        public static bool isExternalStorageDocument (Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals (uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is DownloadsProvider.
         */
        public static bool isDownloadsDocument (Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals (uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is MediaProvider.
         */
        public static bool isMediaDocument (Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals (uri.Authority);
        }

        public static Byte[] readFile(Context context, Android.Net.Uri uri)
        {
            using (var inStream = context.ContentResolver.OpenInputStream(uri))
                return FileData.ReadFully(inStream);
        }

        public static byte [] readFile (string file)
        {
            try {
                return readFile (new Java.IO.File(file));
            } catch (Exception ex) {
                System.Diagnostics.Debug.Write (ex);
                return new byte [0];
            }
        }

        public static byte [] readFile (Java.IO.File file)
        {
            // Open file
            var f = new RandomAccessFile (file, "r");

            try {
                // Get and check length
                long longlength = f.Length ();
                var length = (int)longlength;

                if (length != longlength)
                    throw new Java.IO.IOException("Filesize exceeds allowed size");
                // Read file and return data
                byte [] data = new byte [length];
                f.ReadFully (data);
                return data;
            } catch (Exception ex) {
                System.Diagnostics.Debug.Write (ex);
                return new byte [0];
            } finally {
                f.Close ();
            }
        }

        public static string GetMimeType (string url)
        {
            string type = null;
            var extension = MimeTypeMap.GetFileExtensionFromUrl (url);

            if (extension != null) {
                type = MimeTypeMap.Singleton.GetMimeTypeFromExtension (extension);
            }

            return type;
        }
    }
}