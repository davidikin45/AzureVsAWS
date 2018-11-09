using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using WiredBrainCoffee.AdminApp.Service;
using WiredBrainCoffee.Storage;

namespace WiredBrainCoffee.AdminApp.ViewModel
{
  public interface IAddCoffeeVideoDialogViewModel
  {
    bool DialogResultIsOk { get; }
    byte[] BlobByteArray { get; }
    string BlobName { get; }
    string BlobTitle { get; }
    string BlobDescription { get; }
  }
  public class AddCoffeeVideoDialogViewModel : ViewModelBase, IAddCoffeeVideoDialogViewModel
  {
    private string _blobNameWithoutExtension;
    private ICoffeeVideoStorage _coffeeVideoStorage;
    private IFilePickerDialogService _filePickerDialogService;
    private readonly IMessageDialogService _messageDialogService;

    public AddCoffeeVideoDialogViewModel(ICoffeeVideoStorage coffeeVideoStorage,
      IFilePickerDialogService filePickerDialogService,
      IMessageDialogService messageDialogService)
    {
      _coffeeVideoStorage = coffeeVideoStorage;
      _filePickerDialogService = filePickerDialogService;
      _messageDialogService = messageDialogService;
    }

    public byte[] BlobByteArray { get; private set; }

    public string BlobNameWithoutExtension
    {
      get => _blobNameWithoutExtension;
      set
      {
        _blobNameWithoutExtension = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(IsPrimaryButtonEnabled));
      }
    }

    public bool IsPrimaryButtonEnabled => BlobByteArray != null && !string.IsNullOrWhiteSpace(BlobNameWithoutExtension);

    public string BlobName => BlobNameWithoutExtension + ".mp4";

    public string BlobTitle { get; set; }

    public string BlobDescription { get; set; }

    public bool DialogResultIsOk { get; set; }

    public async Task SelectVideoAsync()
    {
      var storageFile = await _filePickerDialogService.ShowMp4FileOpenDialogAsync();

      if (storageFile != null)
      {
        BlobNameWithoutExtension = Path.GetFileNameWithoutExtension(storageFile.Name);

        var randomAccessStream = await storageFile.OpenReadAsync();
        BlobByteArray = new byte[randomAccessStream.Size];
        using (var dataReader = new DataReader(randomAccessStream))
        {
          await dataReader.LoadAsync((uint)randomAccessStream.Size);
          dataReader.ReadBytes(BlobByteArray);
        }

        OnPropertyChanged(nameof(IsPrimaryButtonEnabled));
      }
    }

    public async Task PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
      // When you set args.Cancel after you await asynchronous code,
      // args.Cancel has only an effect if you're using a deferral like in this method.
      // In the finally block the deferral is completed, which says you're done with the async code
      var deferral = args.GetDeferral();
      try
      {
        var blobExists = await _coffeeVideoStorage.CheckIfBlobExistsAsync(BlobName);
        if (blobExists)
        {
          args.Cancel = true;

          await _messageDialogService.ShowInfoDialogAsync(
            $"A blob with the name \"{BlobName}\" exists already. " +
            $"Please select another name, thanks! :)", "Info");
        }
        else
        {
          DialogResultIsOk = true;
        }
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
      finally
      {
        deferral.Complete();
      }
    }
  }
}
