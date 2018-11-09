using System;
using Autofac;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WiredBrainCoffee.AdminApp.Startup;
using WiredBrainCoffee.AdminApp.ViewModel;

namespace WiredBrainCoffee.AdminApp
{
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += MainPage_Loaded;
      ViewModel = App.Current.Container.Resolve<MainViewModel>();

      ApplicationView.PreferredLaunchViewSize = new Size(800, 620);
      ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
    }

    public MainViewModel ViewModel { get; }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
      await ViewModel.LoadCoffeeVideosAsync();
    }
  }
}
