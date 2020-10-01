using System;
using System.IO;
using Foundation;
using UIKit;

namespace Plugin.FilePicker
{
    public class GenericDocument : UIDocument
    {
        public GenericDocument(NSUrl url) : base(url)
        {
        }

        public override bool LoadFromContents(NSObject contents, string typeName, out NSError outError)
        {
            outError = null;
            return true;
        }

        public override NSObject ContentsForType(string typeName, out NSError outError)
        {
            outError = null;

            if (DataStream == null)
            {
                return null;
            }

            return NSData.FromStream(DataStream);
        }

        public Stream DataStream { private get; set; }
    }
}