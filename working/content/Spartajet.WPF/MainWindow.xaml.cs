using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.Logging;
using Spartajet.WPF.ViewModel;

namespace Spartajet.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> logger;
    private readonly MainWindowViewModel model;
    public MainWindow(ILogger<MainWindow> logger, MainWindowViewModel model)
    {
        this.logger = logger;
        this.model = model;
        this.InitializeComponent();
        this.Loaded+= this.MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        this.DataContext= this.model;
        this.logger.LogInformation("MainWindow Loaded");
    }

}