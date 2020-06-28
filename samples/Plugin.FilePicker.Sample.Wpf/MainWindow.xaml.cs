using System;
using System.ComponentModel;
using System.Windows;

namespace Plugin.FilePicker.Sample.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string File { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            DataContext = this;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var data = await FilePicker.CrossFilePicker.Current.PickFile();
            File = data.FilePath;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("File"));
        }
    }
}
