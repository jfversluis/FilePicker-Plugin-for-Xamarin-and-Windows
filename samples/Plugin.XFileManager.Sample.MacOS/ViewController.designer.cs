// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Plugin.XFilePicker.Sample.MacOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSImageView FileImagePreview { get; set; }

		[Outlet]
		AppKit.NSTextField FileNameLabel { get; set; }

		[Outlet]
		AppKit.NSTextField FilePathLabel { get; set; }

		[Outlet]
		AppKit.NSButton FilePickerButton { get; set; }

		[Action ("FilePickerButtonClicked:")]
		partial void FilePickerButtonClicked (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (FilePickerButton != null) {
				FilePickerButton.Dispose ();
				FilePickerButton = null;
			}

			if (FileNameLabel != null) {
				FileNameLabel.Dispose ();
				FileNameLabel = null;
			}

			if (FilePathLabel != null) {
				FilePathLabel.Dispose ();
				FilePathLabel = null;
			}

			if (FileImagePreview != null) {
				FileImagePreview.Dispose ();
				FileImagePreview = null;
			}
		}
	}
}
