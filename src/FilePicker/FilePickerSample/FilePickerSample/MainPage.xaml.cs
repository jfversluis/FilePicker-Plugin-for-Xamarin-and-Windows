using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.FilePicker;
using Xamarin.Forms;

namespace FilePickerSample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void PickFile_Clicked(object sender, EventArgs args)
        {
            await PickAndShowFile(null);
        }

        private async void PickImage_Clicked(object sender, EventArgs args)
        {
            string[] fileTypes = null;

            if (Device.RuntimePlatform == Device.Android)
            {
                fileTypes = new string[] { "image/png", "image/jpeg" };
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                fileTypes = new string[] { "public.image" }; // same as iOS constant UTType.Image
            }

            if (Device.RuntimePlatform == Device.UWP)
            {
                fileTypes = new string[] { ".jpg", ".png" };
            }

            if (Device.RuntimePlatform == Device.WPF)
            {
                fileTypes = new string[] { "JPEG files (*.jpg)|*.jpg", "PNG files (*.png)|*.png" };
            }

            await PickAndShowFile(fileTypes);
        }

        private async void SaveSomeText_Clicked(object sender, EventArgs e)
        {
            string[] fileTypes = null;

            if (Device.RuntimePlatform == Device.Android)
            {
                fileTypes = new string[] { "text/plain" };
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                fileTypes = new string[] { "public.text" }; // same as iOS constant UTType.Text
            }

            if (Device.RuntimePlatform == Device.UWP)
            {
                fileTypes = new string[] { "Text Files", ".txt" };
            }

            if (Device.RuntimePlatform == Device.WPF)
            {
                fileTypes = new string[] { "Text files (*.txt)|*.txt" };
            }

            await CreateOrSaveText("hello world", fileTypes);
        }

        private async Task PickAndShowFile(string[] fileTypes)
        {
            try
            {
                var pickedFile = await CrossFilePicker.Current.PickFile(fileTypes);

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

        private async Task CreateOrSaveText(string textToSave, string[] fileTypes)
        {
            try
            {
                var saveFile = await CrossFilePicker.Current.CreateOrOverwriteFile(fileTypes);

                if (saveFile != null)
                {
                    FileNameLabel.Text = saveFile.FileName;
                    FilePathLabel.Text = saveFile.FilePath;

                    using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(textToSave)))
                    {
                        await saveFile.WriteToFile(stream);
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
