using MobileCoreServices;
using Plugin.FilePicker.Abstractions;

namespace Plugin.FilePicker.Sample.Forms.iOS
{
    class CustomFilePickerImplementation : FilePickerImplementation, IFilePicker
    {

        #region Overrides

        protected override string[] GetAllowedUTIs()
        {
            return new string[] {
                    UTType.PDF,
                    UTType.Image
                };

        }

        #endregion

    }
}