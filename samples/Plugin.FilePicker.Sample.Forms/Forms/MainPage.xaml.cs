using System;
using Xamarin.Forms;

namespace Plugin.FilePicker.Sample.Forms
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Handle_Clicked(object sender, EventArgs args)
        {
            try
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
            catch (Exception ex)
            {
                FileNameLabel.Text = ex.ToString();
                FilePathLabel.Text = string.Empty;
                FileImagePreview.IsVisible = false;
            }
        }
    }
}