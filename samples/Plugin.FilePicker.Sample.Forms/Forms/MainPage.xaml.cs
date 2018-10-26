using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Plugin.FilePicker.Sample.Forms
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

            await PickAndShowFile(fileTypes);
        }

        private async Task PickAndShowFile(string[] fileTypes)
        {
            try
            {
                if (Device.RuntimePlatform == Device.Android &&
                    !await this.CheckPermissionsAsync())
                {
                    return;
                }

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

        private async Task<bool> CheckPermissionsAsync()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                    {
                        await DisplayAlert("Xamarin.Forms Sample", "Storage permission is needed for file picking", "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);

                    if (results.ContainsKey(Permission.Storage))
                    {
                        status = results[Permission.Storage];
                    }
                }

                if (status == PermissionStatus.Granted)
                {
                    return true;
                }
                else if (status != PermissionStatus.Unknown)
                {
                    await DisplayAlert("Xamarin.Forms Sample", "Storage permission was denied.", "OK");
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}