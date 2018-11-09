using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace WiredBrainCoffee.AdminApp.Service
{
  public interface IMessageDialogService
  {
    Task ShowInfoDialogAsync(string message, string title);
    Task<bool> ShowOkCancelDialogAsync(string message, string title);
  }
  public class MessageDialogService : IMessageDialogService
  {
    public async Task ShowInfoDialogAsync(string message, string title)
    {
      var dlg = new MessageDialog(message, title);
      await dlg.ShowAsync();
    }

    public async Task<bool> ShowOkCancelDialogAsync(string message, string title)
    {
      var dlg = new MessageDialog(message, title);

      var okCommand = new UICommand { Label = "OK" };
      var cancelCommand = new UICommand { Label = "Cancel" };

      dlg.Commands.Add(okCommand);
      dlg.Commands.Add(cancelCommand);

      var selectedCommand = await dlg.ShowAsync();
      return selectedCommand == okCommand;
    }
  }
}
