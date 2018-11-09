using System;
using System.Threading.Tasks;
using WiredBrainCoffee.AdminApp.ViewModel;
using WiredBrainCoffee.AdminApp.View;

namespace WiredBrainCoffee.AdminApp.Service
{
  public interface IAddCoffeeVideoDialogService
  {
    Task<IAddCoffeeVideoDialogViewModel> ShowDialogAsync();
  }
  public class AddCoffeeVideoDialogService : IAddCoffeeVideoDialogService
  {
    private readonly Func<AddCoffeeVideoDialog> _dialogCreator;

    public AddCoffeeVideoDialogService(Func<AddCoffeeVideoDialog> dialogCreator)
    {
      _dialogCreator = dialogCreator;
    }
    public async Task<IAddCoffeeVideoDialogViewModel> ShowDialogAsync()
    {
      var dialog = _dialogCreator();
      await dialog.ShowAsync();
      return dialog.ViewModel;
    }
  }
}
