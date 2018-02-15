#define CUSTOM_PICKER

using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace Plugin.FilePicker.Sample.Forms.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            ConfigureDependencies();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
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
