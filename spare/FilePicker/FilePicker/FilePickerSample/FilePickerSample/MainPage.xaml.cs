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

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            var file = await CrossFilePicker.Current.PickFile();

            //hack: Android hurls at the thought of opening a new dialog while returning. Don't know about other platforms
            //todo: find a more appropriate work-a-round
            await Task.Delay(300);

            if (file == null)
            {
                await DisplayAlert("Cancelled", "No File Selected", "OK");
                return;
            }

            try
            {
                using (var stream = new MemoryStream(file.DataArray))
                {
                    stream.Flush();  
                }

                await DisplayAlert("Success", $"{file.FileName}: {file.DataArray.Length} bytes", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Exception: " + ex, "OK");
            }
        }
    }
}
