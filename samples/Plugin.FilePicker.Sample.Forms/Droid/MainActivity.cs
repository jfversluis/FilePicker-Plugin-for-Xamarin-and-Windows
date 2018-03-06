#define CUSTOM_PICKER

using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.FilePicker.Abstractions;

namespace Plugin.FilePicker.Sample.Forms.Droid
{
    [Activity(Label = "Plugin.FilePicker.Sample.Forms.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            ConfigureDependencies();

            LoadApplication(new App());
        }

        private void ConfigureDependencies()
        {

#if CUSTOM_PICKER
            //Use custom file picker implementation
            Xamarin.Forms.DependencyService.Register<CustomFilePickerImplementation>();
#else
            //Use default file picker implementation
            Xamarin.Forms.DependencyService.Register<FilePickerImplementation>();
#endif

        }
    }
}
