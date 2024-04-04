using System.Windows;
using ClassLibrary.Services;

namespace WPFApplication;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		// See https://github.com/dotnet/vscode-csharp/issues/5958
		InitializeComponent();
	}

	private void Window_Loaded( object sender, RoutedEventArgs e )
	{
		ForecastList.ItemsSource = new WeatherForecastServiceEx().Get();
	}
	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
    	Close();
	}
}