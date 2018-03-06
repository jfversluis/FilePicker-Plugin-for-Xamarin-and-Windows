using System;
using Plugin.FilePicker.Abstractions;

namespace Plugin.FilePicker.Sample.Forms.Droid
{
    class CustomFilePickerImplementation : FilePickerImplementation, IFilePicker
    {
        #region Overrides

        protected override string[] GetAllowedMIMETypes()
        {

            //Try with just one MIME type
            //return new String[] {
            //            "image/*"
            //       };

            //Try with multiple MIIME types

            return new String[] {
               "application/pdf",
                "image/jpeg",
                "image/bmp",
                "image/png",
                //Microsoft Word
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                //Microsoft Excel
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                //Microsoft PowerPoint
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                //Microsoft Publisher
                "application/x-mspublisher",
                //Opendocument Text
                "application/vnd.oasis.opendocument.text",
                //Opendocument Presentation
                "application/vnd.oasis.opendocument.presentation",
                //Opendocument Spreadsheet
                "application/vnd.oasis.opendocument.spreadsheet"
            };
        }

        #endregion
    }
}