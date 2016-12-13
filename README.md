## FilePicker Plugin for Xamarin.Forms

Simple cross-platform plug-in that allows you to pick files from the filesystem (iCloud drive in case of iOS) and work with them.

Since the [original project](https://github.com/Studyxnet/FilePicker-Plugin-for-Xamarin-and-Windows) seems abondoned I've forked my own so I could update it. PRs are very welcome!

### Setup
[![Build status](https://ci.appveyor.com/api/projects/status/4jbyfpc5hsjak3hx?svg=true)](https://ci.appveyor.com/project/jfversluis/filepicker-plugin-for-xamarin-and-windows)

* Available on NuGet: [FilePicker Nuget](https://www.nuget.org/packages/Xamarin.Plugin.FilePicker)
* Install into your PCL project and Client projects.

**Platform Support**

|Platform|Supported|Version|
| ------------------- | :-----------: | :------------------: |
|Xamarin.iOS|Yes|iOS 6+|
|Xamarin.iOS Unified|Yes|iOS 6+|
|Xamarin.Android|Yes|API 10+|
|Windows Phone Silverlight|No||
|Windows Phone RT|Yes|8.1+|
|Windows Store RT|Yes|8.1+|
|Windows 10 UWP|Yes|10+|
|Xamarin.Mac|No||

### API Usage

Call **CrossFilePicker.Current** from any project or PCL to gain access to APIs.

### **IMPORTANT**
**Android:**
The `WRITE_EXTERNAL_STORAGE` & `READ_EXTERNAL_STORAGE` permissions are required.

**iOS:** 
Need [Configure iCloud Driver for your app](https://developer.xamarin.com/guides/ios/platform_features/intro_to_cloudkit)

#### Contributors
* [jfversluis](https://github.com/jfversluis)

Original version by:

* [rafaelrmou](https://github.com/rafaelrmou)
 
Thanks!

#### License
Licensed under main repo license
