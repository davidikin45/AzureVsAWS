using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace WiredBrainCoffee.AdminApp.Service
{
  public interface IFilePickerDialogService
  {
    Task<StorageFile> ShowMp4FileSaveDialogAsync(string suggestedFileName);
    Task<StorageFile> ShowMp4FileOpenDialogAsync();
  }
  public class FilePickerDialogService : IFilePickerDialogService
  {
    public async Task<StorageFile> ShowMp4FileOpenDialogAsync()
    {
      var picker = new FileOpenPicker
      {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary
      };
      picker.FileTypeFilter.Add(".mp4" );
      var storageFile = await picker.PickSingleFileAsync();

      return storageFile;
    }

    public async Task<StorageFile> ShowMp4FileSaveDialogAsync(string suggestedFileName)
    {
      var picker = new FileSavePicker
      {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        SuggestedFileName = suggestedFileName
      };

      picker.FileTypeChoices.Add("Video", new List<string>() { ".mp4" });

      var storageFile = await picker.PickSaveFileAsync();

      return storageFile;
    }
  }
}