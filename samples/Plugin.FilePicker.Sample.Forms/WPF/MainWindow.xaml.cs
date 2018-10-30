using Xamarin.Forms.Platform.WPF;

namespace Plugin.FilePicker.Sample.Forms.WPF
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
            LoadApplication(new Plugin.FilePicker.Sample.Forms.App());
        }
    }
}
