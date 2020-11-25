# FilePicker Plugin for Xamarin.Forms

Simple cross-platform plug-in that allows you to pick files and work with them.

The original project can be found [here](https://github.com/Studyxnet/FilePicker-Plugin-for-Xamarin-and-Windows/), but seems abandoned, this one was forked and further developed.

### The future: [Xamarin.Essentials](https://docs.microsoft.com/en-us/xamarin/essentials/)

Since version 1.6.0 the [Xamarin.Essentials](https://github.com/xamarin/Essentials)
project also supports file picking! See the [Migration Guide](#migration-guide)
on how to migrate from this plugin to the official Xamarin.Essentials API!

## Build status
### Stable [![Build status](https://jfversluis.visualstudio.com/FilePicker%20plugin/_apis/build/status/FilePicker%20Plugin)](https://jfversluis.visualstudio.com/FilePicker%20plugin/_build/latest?definitionId=36) [![NuGet version](https://badge.fury.io/nu/Xamarin.Plugin.FilePicker.svg)](https://badge.fury.io/nu/Xamarin.Plugin.FilePicker)
 
NuGet: [https://www.nuget.org/packages/Xamarin.Plugin.FilePicker/](https://www.nuget.org/packages/Xamarin.Plugin.FilePicker/)
 
### Development feed (possibly instable)

Add this as a source to your IDE to find the latest packages: [https://www.myget.org/F/filepicker-plugin/api/v3/index.json](https://www.myget.org/F/filepicker-plugin/api/v3/index.json)

## Setup

* Install into your Xamarin.Android, Xamarin.iOS, Xamarin.Forms, Xamarin.Mac, Xamarin.WPF project and Client projects.

**Platform Support**

| Platform              |Supported| Version |Remarks|
| ------------------------ | :-: | :------: | :------------------: |
|Xamarin.iOS Classic       | Yes | iOS 8+   ||
|Xamarin.iOS Unified       | Yes | iOS 8+   ||
|Xamarin.Android           | Yes | API 10+  ||
|Windows Phone Silverlight | No  |          ||
|Windows Phone RT          | Yes | 8.1+     |Up to package version 1.4.x|
|Windows Store RT          | Yes | 8.1+     |Up to package version 1.4.x|
|Windows 10 UWP            | Yes | 10+      ||
|Xamarin.Mac               | Yes | * 10.12+ ||
|WPF                       | Yes | N/A      |Using .NET Framework 4.5|

\* The Xamarin.Mac implementation has only been tested on MacOS 10.12.

### API Usage

Call **CrossFilePicker.Current** from any platform or .NET Standard project to gain access to APIs.

### Example

    try
    {
        FileData fileData = await CrossFilePicker.Current.PickFile();
        if (fileData == null)
            return; // user canceled file picking

        string fileName = fileData.FileName;
        string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);

        System.Console.WriteLine("File name chosen: " + fileName);
        System.Console.WriteLine("File data: " + contents);
    }
    catch (Exception ex)
    {
        System.Console.WriteLine("Exception choosing file: " + ex.ToString());
    }

### Methods

#### `async Task<FileData> PickFile(string[] allowedTypes = null)`

Starts file picking and returns file data for picked file. File types can be
specified in order to limit files that can be selected. Note that this method
may throw exceptions that occured during file picking.

Note that on Android it can happen that PickFile() can be called twice. In
this case the first PickFile() call will return null as it is effectively
cancelled.

Parameter `allowedTypes`:
Specifies one or multiple allowed types. When null, all file types can be
selected while picking.

On Android you can specify one or more MIME types, e.g. "image/png"; also wild
card characters can be used, e.g. "image/*".

On iOS you can specify UTType constants, e.g. UTType.Image.

On UWP, specify a list of extensions, like this: `".jpg", ".png"`.

On WPF, specify strings like this: `"Data type (*.ext)|*.ext"`, which
corresponds how the Windows file open dialog specifies file types.

### Data structures

The returned `FileData` object contains multiple properties that can be accessed:

    public class FileData
    {
        /// When accessed, reads all data from the picked file and returns it.
        public byte[] DataArray { get; }

        /// Filename of the picked file; doesn't contain any path.
        public string FileName { get; }

        /// Full file path of the picked file; note that on some platforms the
        /// file path may not be a real, accessible path but may contain an
        /// platform specific URI; may also be null.
        public string FilePath { get; }

        /// Returns a stream to the picked file; this is the most reliable way
        /// to access the data of the picked file.
        public Stream GetStream();
    }

Note that `DataArray` is filled on first access, so be sure to rewind the stream when
you access it via GetStream() afterwards.

### **IMPORTANT**
**Android:**
The FilePath property may contain a content URI that starts with `content://`.
The plugin tries hard to find out an actual filename, but when it can't, the
file can only be accessed using `GetStream()` or `DataArray`. On Android
`ContentProvider` classes are used to share data between apps. The resource
that is accessed may not even be a file, streamed over the internet or loaded
from a database.

The plugin also tries to get a persistable content URI that can be stored for
later access. Be prepared that this may fail, though. Content could have been
moved to a different location or could be deleted.

The `READ_EXTERNAL_STORAGE` permission is automatically added to your Android
app project. Starting from Android 6.0 (Marshmallow) the user is asked for the
permission when not granted yet. When the user denies the permission,
`PickFile()` returns null.

The `WRITE_EXTERNAL_STORAGE` is required if you call SaveFile() and must be
added to your Android app project by yourself.

**iOS:** 
You need to [Configure iCloud Driver for your app](https://developer.xamarin.com/guides/ios/platform_features/intro_to_cloudkit).

## Migration guide

Migrating the usage of this FilePicker plugin to `Xamarin.Essentials.FilePicker`
isn't straight-forward. It's similar, though, since the Essentials code
originated from this plugin. Here's a little guide on how to do it.

1. If you don't have the Xamarin.Essentials NuGet package installed yet, install
   it into the Android, iOS and UWP projects. Also install it into your Forms
   project, if you're calling the FilePicker from there. Be sure to also
   properly [initialize Essentials](https://docs.microsoft.com/en-us/xamarin/essentials/get-started).
   You can remove the Xamarin.Plugin.FilePicker NuGet package now or afterwards.

2. Rename the namespaces, types and method calls. Replace

       using Plugin.FilePicker;
       using Plugin.FilePicker.Abstractions;

   with

       using Xamarin.Essentials;

   Replace `await CrossFilePicker.Current.PickFile()` with
   `await FilePicker.PickAsync()`. Replace `FileData` with `FileResult` (or
   use the `var` keyword).

3. Use `Xamarin.Essentials.PickOptions` if you specified file types for picking.
   Replace code like this:

       string[] fileTypes = null;
       if (Device.RuntimePlatform == Device.Android)
           fileTypes = new string[] { "image/png", "image/jpeg" };

       if (Device.RuntimePlatform == Device.iOS)
           fileTypes = new string[] { "public.image" };

       if (Device.RuntimePlatform == Device.UWP)
           fileTypes = new string[] { ".jpg", ".png" };

    with:

       var options = new PickOptions
       {
           FileTypes = new FilePickerFileType(
               new Dictionary<DevicePlatform, IEnumerable<string>>
               {
                   { DevicePlatform.Android, new string[] { "image/png", "image/jpeg"} },
                   { DevicePlatform.iOS, new string[] { "public.image" } },
                   { DevicePlatform.UWP, new string[] { ".jpg", ".png" } },
               }),
           PickerTitle = "Select a file to import"
       };

    If you can't specify a list of MIME types or file extensions, use `null`
    as the value of the dictionary entry, or else file picking on that platform
    won't be available:

       { DevicePlatform.Android, null },

    Note also that the `PickerTitle` is a new property, but the title is only
    shown on Android.

4. Replace usage of `FileData` with `Xamarin.Essentials.FileResult`. The new
   FileResult structure has some properties and methods named differently. The
   biggest change is that you should not (and in some cases on Android can't)
   rely on `FileResult.FullPath` to be a file system filename. Always use
   `FileResult.OpenStreamAsync()` to get a stream to the picked file. From
   there you can either read from the stream directly (e.g. using a
   `StreamReader`), or copy the file into your app folder. This can be done
   using `Stream.CopyToAsync()` and has the advantage that you do the copying
   in a background task, and you can specify a `CancellationToken` that can
   be used to cancel the operation. You could even show a progress dialog to
   the user that allows cancelling the transfer.


## Troubleshooting

### All platforms

**InvalidOperationException "Only one operation can be active at a time"**

This occurs when `PickFile()` is called multiple times and the task being awaited didn't return or
throws an exception that wasn't caught. Be sure to catch any exceptions and handle them
appropriately. See the example code above.

### Android

**Exception "This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation."**

This occurs when you are using the old-style NuGet references (not the PackageReference mechanism)
and you forgot to add the NuGet package to the Android package. When using PackageReference this
is not necessary anymore because the bait-and-switch assemblies of FilePicker are correctly
resolved.

### iOS

**Picked file path is invalid, file doesn't exist**

On iOS the plugin uses UIDocumentPickerViewController and specifies the mode
UIDocumentPickerMode.Import. After picking is done, iOS copies the picked file
to the app's temporary "Inbox" folder where it can be accessed. iOS also cleans up the
temporary inbox folder regularly. After picking the file you have to either
copy the file to another folder, access the data by getting the property
DataArray or opening a stream to the file by calling GetStream().

## Contributors
* [jfversluis](https://github.com/jfversluis)
* [rafaelrmou](https://github.com/rafaelrmou) (original author)
* [vividos](https://github.com/vividos)
 
Thanks!

## License
[MIT Licence](LICENSE)
