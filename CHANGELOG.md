# Changelog

## Version 2.1.xx

No code changes from the version 2.1.12-beta.

## Version 2.1.12-beta

Android: Fixed warning when compiling for TargetFrameworkVersion below 9.0 and using file picker
from NuGet package (#125).

WPF: Fixed using NuGet package under WPF by actually compiling the FilePicker plugin for .NET 4.5
and higher (#122) (#124).

Marked IFilePicker.OpenFile() and IFilePicker.SaveFile() obsolete; the plugin focuses on picking
files. Will be removed in 2.2.x.

## Version 2.1.11-beta

The plugin now uses multi-targeting; no visible changes to the API, but the NuGet package
explicitly lists on which platforms the plugin can be installed.

The plugin now uses SourceLink; when the Visual Studio option "Just my code" is deactivated,
the user can directly debug into the plugin's code. See https://github.com/dotnet/sourcelink/

## Version 2.0.135

Android: When the app doesn't have Permission.ReadExternalStorage granted yet, the app will now
ask the user to grant the permission (on API level >=23). The caller of PickFile() doesn't need to
request permissions now. The permission is automatically added to the app when referencing this
NuGet package.

## Version 2.0.121

WPF: Implemented FilePicker for WPF (#64).

Android: Fixed OpenFile() call with relative filename that uses wrong parent directory (#71).

Android: Fixed stack overflow when calling OpenFile() with FileData argument.

Marked FileData.ReadFully() obsolete; it shouldn't be used outside of the plugin. Will be removed
in 2.1.x.

## Version 2.0.114-beta

Android: Fixed getting path from OneDrive content provider (#79).

Android/iOS: File bytes are not read directly after picking, but only when FileData.DataArray is
accessed or FileData.GetStream() is called; this fixes accessing large files that wouldn't fit in
the device's memory, e.g. videos (#38).

## Version 2.0.106-beta

Android/iOS: PickFile() now throws more exceptions when an error while picking occured; before
that some error states were silently discarded.

Android: Added check if Android permission was granted; caller of PickFile() has to request
permission, though.

Android: Fixed exception when trying to pick a downloaded file; the Android DownloadManager won't
return an actual file path; downloaded files can only be resolved using ContentResolver (#86).

Android: fix android mime types implementation (#85)

iOS: Fixed getting pathname from FileUrl (fix of PR #54 that was reverted in PR #65).

## Version 2.0.81-beta

Added support for file types across platforms (#26).

Revert pull request "Fixed Path" (#65).

## Version 2.0.64-beta

Android: Fixed Path (#54); was returning File Url which cannot be used for file operations from
PCL / Shared Project, instead returning path.

## Version 2.0.58-beta

Fixed more NuGet package issues (#19, #23, #29).

UWP: Make UWP streamGetter use StorageFile's OpenStreamForRead method, instead of trying to use
the file path and File.IO (#33).

Android: Support for content:// picked files (#22).

Android: Fixed picking files from OneDrive; the ContentProvider returns no _data column (#44).

Fixes crash in Xamarin.Forms sample when user doesn't select file; also added example code in README file (#50)

Android: Fixed picking files from the download folder; on newer devices the document ID may not be
a number, but the real filename prefixed with "raw:" (#49)

## Version 2.0.8-beta

Fixed NuGet package issue (#13).

## Version 2.0.2-beta

The plugin was converted from using PCL to using .NET Standard, with platform support for Android,
iOS and UWP platforms.

Removed Windows Phone 8 and Windows 8/8.1 support.
