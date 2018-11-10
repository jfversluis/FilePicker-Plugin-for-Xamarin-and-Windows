using Plugin.FilePicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SampleFP
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            FileData fileData = await FilePicker.PickFile();
            var path = @"C:\Users\pedro\source\repos\pictos\FilePicker-Plugin-for-Xamarin-and-Windows\.gitignore";
            FilePicker.OpenFile(path);
        }
    }
}
