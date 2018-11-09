using Autofac;
using WiredBrainCoffee.AdminApp.Service;
using WiredBrainCoffee.AdminApp.Settings;
using WiredBrainCoffee.AdminApp.View;
using WiredBrainCoffee.AdminApp.ViewModel;
using WiredBrainCoffee.Storage;

namespace WiredBrainCoffee.AdminApp.Startup
{
  class Bootstrapper
  {
    public IContainer Bootstrap()
    {
      var builder = new ContainerBuilder();

      builder.RegisterType<MainViewModel>().As<IMainViewModel>().AsSelf().SingleInstance();
      builder.RegisterType<CoffeeVideoViewModel>().AsSelf();

      builder.RegisterType<CoffeeVideoStorage>().As<ICoffeeVideoStorage>()
        .WithParameter("connectionString",AppSettings.ConnectionString);

      builder.RegisterType<AddCoffeeVideoDialog>().AsSelf();
      builder.RegisterType<AddCoffeeVideoDialogViewModel>().As<IAddCoffeeVideoDialogViewModel>().AsSelf();

      builder.RegisterType<AddCoffeeVideoDialogService>().As<IAddCoffeeVideoDialogService>();
      builder.RegisterType<MessageDialogService>().As<IMessageDialogService>();
      builder.RegisterType<FilePickerDialogService>().As<IFilePickerDialogService>();

      return builder.Build();
    }
  }
}
