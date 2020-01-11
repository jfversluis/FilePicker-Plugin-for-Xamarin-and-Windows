using Foundation;
using QuickLook;
using System;
using System.IO;

	public class PreviewControllerDS : QLPreviewControllerDataSource
	{
        //Document cache
		private QLPreviewItem item { get; set; }

        //Setting the document
		public PreviewControllerDS(QLPreviewItem item)
		{
            this.item = item;
		}

        //Setting document count to 1
		public override nint PreviewItemCount (QLPreviewController controller)
		{
			return 1;
		}

        //Return the document
		public override IQLPreviewItem GetPreviewItem (QLPreviewController controller, nint index)
		{
			return item;
		}
	}

    public class QLPreviewItemFileSystem : QLPreviewItem
    {

        string _fileName, _filePath;

        //Setting file name and path
        public QLPreviewItemFileSystem(string fileName, string filePath)
        {
            _fileName = fileName;
            _filePath = filePath;
        }

        //Return file name
        public override string ItemTitle
        {
            get
            {
                return _fileName;
            }
        }

        //Retun file path as NSUrl
        public override NSUrl ItemUrl
        {
            get
            {
                return NSUrl.FromFilename(_filePath);
            }
        }
    }

    public class QLPreviewItemBundle : QLPreviewItem
    {
        string _fileName, _filePath;

        //Setting file name and path
        public QLPreviewItemBundle(string fileName, string filePath)
        {
            _fileName = fileName;
            _filePath = filePath;
        }

        //Return file name
        public override string ItemTitle
        {
            get
            {
                return _fileName;
            }
        }

        //Retun file path as NSUrl
        public override NSUrl ItemUrl
        {
            get
            {
                var documents = NSBundle.MainBundle.BundlePath;
                var lib = Path.Combine(documents, _filePath);
                var url = NSUrl.FromFilename(lib);
                return url;
            }
        }
    }


