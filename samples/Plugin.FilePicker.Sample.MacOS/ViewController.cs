using System;
using AppKit;
using Foundation;

namespace Plugin.FilePicker.Sample.MacOS
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
        }

        async partial void FilePickerButtonClicked(Foundation.NSObject sender)
        {
            var pickedFile = await CrossFilePicker.Current.PickFile();

            if (pickedFile != null)
            {
                FileNameLabel.StringValue = pickedFile.FileName;
                FilePathLabel.StringValue = pickedFile.FilePath;

                if (pickedFile.FileName.EndsWith("png") || pickedFile.FileName.EndsWith("jpg"))
                    FileImagePreview.Image = NSImage.FromStream(pickedFile.GetStream());
            }
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}