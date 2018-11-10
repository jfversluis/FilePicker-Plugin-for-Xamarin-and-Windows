using System;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Database;
using Java.IO;
using AN = Android.Net;
using Android.Webkit;

namespace Plugin.FilePicker
{
    public static class IOUtil
    {
        public static string getPath(Context context, AN.Uri uri)
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

                    if (!string.IsNullOrEmpty(id) &&
                        id.StartsWith("raw:"))
                    {
                        return id.Substring(4);
                    }

                    string[] contentUriPrefixesToTry = new string[]
                    {
                        "content://downloads/public_downloads",
                        "content://downloads/my_downloads"
                    };

                    foreach (string contentUriPrefix in contentUriPrefixesToTry)
                    {
                        Android.Net.Uri contentUri = ContentUris.WithAppendedId(
                            Android.Net.Uri.Parse(contentUriPrefix), long.Parse(id));

                        try
                        {
                            var path = getDataColumn(context, contentUri, null, null);
                            if (path != null)
                            {
                                return path;
                            }
                        }
                        catch (Exception)
                        {
                            // ignore exception; path can't be retrieved using ContentResolver
                        }
                    }
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
            // MediaStore (and general)
            if (isMediaStore(uri.Scheme))
            {
                return getDataColumn(context, uri, null, null);
            }
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        /// <summary>
        /// Checks if the scheme part of the URL matches the content:// scheme
        /// </summary>
        /// <param name="scheme">scheme part of URL</param>
        /// <returns>true when it matches, false when not</returns>
        public static bool isMediaStore(string scheme)
        {
            return scheme.StartsWith("content");
        }

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
                    int column_index = cursor.GetColumnIndex(column);
                    if (column_index == -1)
                        return null;

                    string path = cursor.GetString(column_index);

                    // When the path has no root (i.e. is relative), better return null so that
                    // the content uri is used and the file contents can be read
                    if (path != null && !System.IO.Path.IsPathRooted(path))
                    {
                        return null;
                    }

                    return path;
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
