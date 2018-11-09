using Windows.UI.Xaml.Controls;
using WiredBrainCoffee.AdminApp.ViewModel;

namespace WiredBrainCoffee.AdminApp.View
{
  public sealed partial class CoffeeVideoView : UserControl
  {
    public CoffeeVideoView()
    {
      this.InitializeComponent();
    }

    private CoffeeVideoViewModel _viewModel;

    public CoffeeVideoViewModel ViewModel
    {
      get { return _viewModel; }
      set
      {
        _viewModel = value;
        this.Bindings.Update();
      }
    }

    // Span the TextBox with the Blobname across 2 columns if the Blob is not a Snapshot
    public int ColumnSpanForBlobNameTextBox => (ViewModel?.IsSnapshot ?? false) ? 1 : 2;
  }
}
