## FilePicker Plugin for Xamarin.Forms

Simple cross-platform plug-in that allows you to pick files and work with them.

The original project van be found here, but seems abandoned, this one was forked and further developed.

### Setup
[![Build status](https://ci.appveyor.com/api/projects/status/bbdou6ptk14tbak5?svg=true)](https://ci.appveyor.com/project/jfversluis/filepicker-plugin-for-xamarin-and-windows-5pvwc)
 [![NuGet version](https://badge.fury.io/nu/Xamarin.Plugin.FilePicker.svg)](https://badge.fury.io/nu/Xamarin.Plugin.FilePicker)

* Available on NuGet: [FilePicker Nuget](https://www.nuget.org/packages/Xamarin.Plugin.FilePicker/)
* Install into your PCL project **and** Client projects.

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
|Xamarin.Mac|Yes|* 10.12+|

\* The Xamarin.Mac implementaiton has only been tested on MacOS 10.12.

### API Usage

Call **CrossFilePicker.Current** from any project or PCL to gain access to APIs.

### **IMPORTANT**
**Android:**
The `WRITE_EXTERNAL_STORAGE` & `READ_EXTERNAL_STORAGE` permissions are required.

**iOS:** 
Need [Configure iCloud Driver for your app](https://developer.xamarin.com/guides/ios/platform_features/intro_to_cloudkit)

#### Contributors
* [jfversluis](https://github.com/jfversluis)
* [rafaelrmou](https://github.com/rafaelrmou) (original author)
 
Thanks!

#### License
Licensed under main repo license
