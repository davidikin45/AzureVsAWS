using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using WiredBrainCoffee.AdminApp.Service;
using WiredBrainCoffee.Storage;

namespace WiredBrainCoffee.AdminApp.ViewModel
{
  public class CoffeeVideoViewModel : ViewModelBase
  {
    private CloudBlockBlob _cloudBlockBlob;
    private readonly DispatcherTimer _leaseRenewTimer;
    private readonly IFilePickerDialogService _filePickerDialogService;
    private readonly IMessageDialogService _messageDialogService;
    private IMainViewModel _mainViewModel;
    private readonly ICoffeeVideoStorage _coffeeVideoStorage;
    private string _title;
    private string _description;
    private string _leaseId;

    public CoffeeVideoViewModel(CloudBlockBlob cloudBlockBlob,
     ICoffeeVideoStorage coffeeVideoStorage,
     IFilePickerDialogService filePickerDialogService,
     IMessageDialogService messageDialogService,
     IMainViewModel mainViewModel)
    {
      _cloudBlockBlob = cloudBlockBlob
        ?? throw new ArgumentNullException(nameof(cloudBlockBlob));

      _leaseRenewTimer = new DispatcherTimer
      {
        Interval = TimeSpan.FromSeconds(45)
      };

      _leaseRenewTimer.Tick += async (e, s) =>
      {
        await _coffeeVideoStorage.RenewLeaseAsync(cloudBlockBlob, LeaseId);
        Debug.WriteLine("Lease renewed");
      };

      _filePickerDialogService = filePickerDialogService;
      _messageDialogService = messageDialogService;
      _mainViewModel = mainViewModel;
      _coffeeVideoStorage = coffeeVideoStorage;

      UpdateViewModelPropertiesFromMetadata();
    }

    public string BlobName => _cloudBlockBlob.Name;

    public string BlobUri => _cloudBlockBlob.SnapshotQualifiedUri.ToString();

    public string BlobUriWithSasToken => _coffeeVideoStorage.GetBlobUriWithSasToken(_cloudBlockBlob);

    public bool IsSnapshot => _cloudBlockBlob.IsSnapshot;

    public string SnapshotTime => $"{_cloudBlockBlob.SnapshotTime:MM/dd/yyyy hh:mm:ss tt}";

    public string Title
    {
      get { return _title; }
      set
      {
        if (_title != value)
        {
          _title = value;
          OnPropertyChanged();
          OnPropertyChanged(nameof(IsMetadataChanged));
        }
      }
    }

    public string Description
    {
      get { return _description; }
      set
      {
        if (_description != value)
        {
          _description = value;
          OnPropertyChanged();
          OnPropertyChanged(nameof(IsMetadataChanged));
        }
      }
    }

    public bool IsMetadataChanged
    {
      get
      {
        var (title, description) = _coffeeVideoStorage.GetBlobMetadata(_cloudBlockBlob);
        return !string.Equals(title, Title) || !string.Equals(description, Description);
      }
    }

    public bool HasLease => LeaseId != null;

    public string LeaseId
    {
      get => _leaseId;
      private set
      {
        _leaseId = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(HasLease));
      }
    }

    public async Task DownloadVideoToFileAsync()
    {
      try
      {
        var storageFile = await _filePickerDialogService.ShowMp4FileSaveDialogAsync(BlobName);
        if (storageFile != null)
        {
          _mainViewModel.StartLoading($"Downloading your video {BlobName}");
          using (var streamToWrite = await storageFile.OpenStreamForWriteAsync())
          {
            await _coffeeVideoStorage.DownloadVideoAsync(_cloudBlockBlob, streamToWrite);
          }
        }
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
      finally
      {
        _mainViewModel.StopLoading();
      }
    }

    public async Task OverwriteVideoAsync()
    {
      try
      {
        var storageFile = await _filePickerDialogService.ShowMp4FileOpenDialogAsync();
        if (storageFile != null)
        {
          _mainViewModel.StartLoading($"Overwriting your video {BlobName}");
          var randomAccessStream = await storageFile.OpenReadAsync();
          var videoByteArray = new byte[randomAccessStream.Size];
          using (var dataReader = new DataReader(randomAccessStream))
          {
            await dataReader.LoadAsync((uint)randomAccessStream.Size);
            dataReader.ReadBytes(videoByteArray);
          };

          await _coffeeVideoStorage.OverwriteVideoAsync(_cloudBlockBlob, videoByteArray, LeaseId);
          OnPropertyChanged(nameof(BlobUri));
          OnPropertyChanged(nameof(BlobUriWithSasToken));
        }
      }
      catch (StorageException ex) when (
        ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed
        && ex.RequestInformation.ErrorCode == "ConditionNotMet")
      {
        await ShowVideoChangedMessageAndReloadAsync();
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
      finally
      {
        _mainViewModel.StopLoading();
      }
    }

    public async Task ArchiveVideoAsync()
    {
      try
      {
        _mainViewModel.StartLoading($"Archiving your video {BlobName}");
        await _coffeeVideoStorage.ArchiveVideoAsync(_cloudBlockBlob);
        await _messageDialogService.ShowInfoDialogAsync("Video archived", "Info");
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
      finally
      {
        _mainViewModel.StopLoading();
      }
    }

    public async Task DeleteVideoAsync()
    {
      try
      {
        var isOk = await _messageDialogService.ShowOkCancelDialogAsync($"Delete the video {_cloudBlockBlob.Name}?", "Question");
        if (isOk)
        {
          _mainViewModel.StartLoading($"Deleting your video {BlobName}");
          await _coffeeVideoStorage.DeleteVideoAsync(_cloudBlockBlob, LeaseId);
          _mainViewModel.RemoveCoffeeVideoViewModel(this);
          _mainViewModel.StopLoading();
          _mainViewModel = null;
        }
      }
      catch (StorageException ex) when (
        ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed
        && ex.RequestInformation.ErrorCode == "ConditionNotMet")
      {
        await ShowVideoChangedMessageAndReloadAsync();
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
      finally
      {
        if (_mainViewModel != null)
        {
          _mainViewModel.StopLoading();
        }
      }
    }

    public async Task UpdateMetadataAsync()
    {
      try
      {
        _mainViewModel.StartLoading($"Updating metadata");
        await _coffeeVideoStorage.UpdateMetadataAsync(_cloudBlockBlob, Title, Description, LeaseId);
        OnPropertyChanged(nameof(IsMetadataChanged));
      }
      catch (StorageException ex) when (
        ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed
        && ex.RequestInformation.ErrorCode == "ConditionNotMet")
      {
        await ShowVideoChangedMessageAndReloadAsync();
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
      finally
      {
        _mainViewModel.StopLoading();
      }
    }

    public async Task ReloadMetadataAsync()
    {
      try
      {
        _mainViewModel.StartLoading($"Reloading metadata");
        await _coffeeVideoStorage.ReloadMetadataAsync(_cloudBlockBlob);
        UpdateViewModelPropertiesFromMetadata();
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
      finally
      {
        _mainViewModel.StopLoading();
      }
    }

    public async Task AcquireLeaseAsync()
    {
      try
      {
        LeaseId = await _coffeeVideoStorage.AcquireOneMinuteLeaseAsync(_cloudBlockBlob);
        _leaseRenewTimer.Start();
        await _messageDialogService.ShowInfoDialogAsync($"Lease acquired. Lease Id: {LeaseId}", "Info");
      }
      catch (StorageException ex) when (
       ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed
       && ex.RequestInformation.ErrorCode == "ConditionNotMet")
      {
        await ShowVideoChangedMessageAndReloadAsync();
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
    }

    public async Task ReleaseLeaseAsync()
    {
      try
      {
        _leaseRenewTimer.Stop();
        await _coffeeVideoStorage.ReleaseLeaseAsync(_cloudBlockBlob, LeaseId);
        LeaseId = null;
        await _messageDialogService.ShowInfoDialogAsync($"Lease was released", "Info");
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
    }

    public async Task ShowLeaseInfoAsync()
    {
      try
      {
        var leaseInfo = await _coffeeVideoStorage.LoadLeaseInfoAsync(_cloudBlockBlob);
        await _messageDialogService.ShowInfoDialogAsync(leaseInfo, "Info");
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
    }

    public async Task CreateSnapshotAsync()
    {
      try
      {
        await _coffeeVideoStorage.CreateSnapshotAsync(_cloudBlockBlob);
        await _messageDialogService.ShowInfoDialogAsync("Snapshot created", "Info");
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
    }

    public async Task PromoteSnapshotAsync()
    {
      try
      {
        await _coffeeVideoStorage.PromoteSnapshotAsync(_cloudBlockBlob);
        await _mainViewModel.ReloadAfterSnapshotPromotionAsync(this);
        await _messageDialogService.ShowInfoDialogAsync("Snapshot promoted", "Info");
      }
      catch (Exception ex)
      {
        await _messageDialogService.ShowInfoDialogAsync(ex.Message, "Error");
      }
    }

    private void UpdateViewModelPropertiesFromMetadata()
    {
      var (title, description) = _coffeeVideoStorage.GetBlobMetadata(_cloudBlockBlob);
      Title = title;
      Description = description;
    }

    private async Task ShowVideoChangedMessageAndReloadAsync()
    {
      await _messageDialogService.ShowInfoDialogAsync("Someone else has changed this video, we reload it and then you can update", "Info");
      await ReloadMetadataAsync();
      OnPropertyChanged(nameof(BlobUriWithSasToken));
    }
  }
}
