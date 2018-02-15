using System;
using Xamarin.Forms;

namespace Plugin.FilePicker.Sample.Forms
{
    public partial class Plugin_FilePicker_Sample_FormsPage : ContentPage
    {
        private readonly Abstractions.IFilePicker _filePicker;

        public Plugin_FilePicker_Sample_FormsPage()
        {
            InitializeComponent();

            _filePicker = DependencyService.Get<Abstractions.IFilePicker>();
        }

        private async void Handle_Clicked(object sender, EventArgs args)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                await DisplayAlert("TODO", "Whoops, for iOS the sample is to be implemented", "OK");

                return;
            }
            
            var pickedFile = await _filePicker.PickFile();

            FileNameLabel.Text = pickedFile.FileName;
            FilePathLabel.Text = pickedFile.FilePath;

            if (pickedFile.FileName.EndsWith("jpg", StringComparison.Ordinal)
                || pickedFile.FileName.EndsWith("png", StringComparison.Ordinal))
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