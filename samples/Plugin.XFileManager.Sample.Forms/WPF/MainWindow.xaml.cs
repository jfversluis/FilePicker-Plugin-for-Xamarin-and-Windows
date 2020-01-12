using Xamarin.Forms.Platform.WPF;

namespace Plugin.XFileManager.Sample.Forms.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
    {
        public MainWindow()
        {
            InitializeComponent();

            Xamarin.Forms.Forms.Init();
            LoadApplication(new Plugin.XFileManager.Sample.Forms.App());
        }
    }
}
