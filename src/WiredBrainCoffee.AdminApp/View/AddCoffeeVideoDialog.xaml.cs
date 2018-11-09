using Windows.UI.Xaml.Controls;
using WiredBrainCoffee.AdminApp.ViewModel;

namespace WiredBrainCoffee.AdminApp.View
{
  public sealed partial class AddCoffeeVideoDialog : ContentDialog
  {
    public AddCoffeeVideoDialog(AddCoffeeVideoDialogViewModel viewModel)
    {
      this.InitializeComponent();
      ViewModel = viewModel;
    }

    public AddCoffeeVideoDialogViewModel ViewModel { get; }
  }
}