# FilePicker Plugin for Xamarin.Forms

Simple cross-platform plug-in that allows you to pick files and work with them.

The original project can be found [here](https://github.com/Studyxnet/FilePicker-Plugin-for-Xamarin-and-Windows/), but seems abandoned, this one was forked and further developed.

## Build status
### Stable [![Build status](https://ci.appveyor.com/api/projects/status/bbdou6ptk14tbak5?svg=true)](https://ci.appveyor.com/project/jfversluis/filepicker-plugin-for-xamarin-and-windows-5pvwc) [![NuGet version](https://badge.fury.io/nu/Xamarin.Plugin.FilePicker.svg)](https://badge.fury.io/nu/Xamarin.Plugin.FilePicker)
 
NuGet: [https://www.nuget.org/packages/Xamarin.Plugin.FilePicker/](https://www.nuget.org/packages/Xamarin.Plugin.FilePicker/)
 
### Development feed (possibly instable) [![Build status](https://ci.appveyor.com/api/projects/status/bbdou6ptk14tbak5/branch/develop?svg=true)](https://ci.appveyor.com/project/jfversluis/filepicker-plugin-for-xamarin-and-windows-5pvwc)

Add this as a source to your IDE to find the latest packages: [https://ci.appveyor.com/nuget/filepicker-plugin-for-xamarin](https://ci.appveyor.com/nuget/filepicker-plugin-for-xamarin)

## Setup

* Install into your Xamarin.Android, Xamarin.iOS, Xamarin.Forms, Xamarin.Mac project and Client projects.

**Platform Support**

|Platform|Supported|Version|Remarks|
| ------------------- | :-----------: | :------------------: | :------------------: |
|Xamarin.iOS|Yes|iOS 6+||
|Xamarin.iOS Unified|Yes|iOS 6+||
|Xamarin.Android|Yes|API 10+||
|Windows Phone Silverlight|No|||
|Windows Phone RT|Yes|8.1+|Up to package version 1.4.x|
|Windows Store RT|Yes|8.1+|Up to package version 1.4.x|
|Windows 10 UWP|Yes|10+||
|Xamarin.Mac|Yes|* 10.12+||

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
The `WRITE_EXTERNAL_STORAGE` & `READ_EXTERNAL_STORAGE` permissions are required.

Starting from Android 6.0 (Marshmallow) you have to request the permission from the user. This can
be done using `ActivityCompat.RequestPermission()` or you can use the
[Xamarin.Plugin.Permission](https://github.com/jamesmontemagno/PermissionsPlugin) plugin. See
also code in the sample project:
https://github.com/jfversluis/FilePicker-Plugin-for-Xamarin-and-Windows/blob/develop/samples/Plugin.FilePicker.Sample.Forms/Forms/MainPage.xaml.cs

**iOS:** 
Need [Configure iCloud Driver for your app](https://developer.xamarin.com/guides/ios/platform_features/intro_to_cloudkit)

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

**InvalidOperationException "Android permission READ_EXTERNAL_STORAGE is missing or was denied by user"**

Starting from Android 6.0 (Marshmallow) permissions must be added to the AndroidManifest.xml (as
before) but the permission must be requested and granted by the user at runtime as well. When
the user denied the permission, don't call PickFile(). Check out the sample project in the github
repository for an example how to check for permission.

### iOS

**Picked file path is invalid, file doesn't exist**

On iOS the plugin uses UIDocumentPickerViewController and specifies the mode
UIDocumentPickerMode.Import. After picking is done, iOS copies the picked file
to the app's "Inbox" folder where it can be accessed. iOS also cleans up the
temporary inbox folder regularly. After picking the file you have to either
copy the file to another folder or access the data by getting the property
DataBytes or opening a stream to the file by calling GetStream().

## Contributors
* [jfversluis](https://github.com/jfversluis)
* [rafaelrmou](https://github.com/rafaelrmou) (original author)
* [vividos](https://github.com/vividos)
 
Thanks!

## License
[MIT Licence](LICENSE)
