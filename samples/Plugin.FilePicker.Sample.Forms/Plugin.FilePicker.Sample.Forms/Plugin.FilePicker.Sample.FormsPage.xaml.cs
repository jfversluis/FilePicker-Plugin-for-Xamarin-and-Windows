using System;
using Xamarin.Forms;

namespace Plugin.FilePicker.Sample.Forms
{
    public partial class Plugin_FilePicker_Sample_FormsPage : ContentPage
    {
        public Plugin_FilePicker_Sample_FormsPage()
        {
            InitializeComponent();
        }

        private async void Handle_Clicked(object sender, EventArgs args)
        {
            var pickedFile = await CrossFilePicker.Current.PickFile();

            if (pickedFile != null)
            {
                FileNameLabel.Text = pickedFile.FileName;
                FilePathLabel.Text = pickedFile.FilePath;

                if (pickedFile.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase)
                    || pickedFile.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    FileImagePreview.Source = ImageSource.FromStream(() => pickedFile.GetStream());
                    FileImagePreview.IsVisible = true;
                }
                else
                {
                    FileImagePreview.IsVisible = false;
                }
            }
        }
    }
}