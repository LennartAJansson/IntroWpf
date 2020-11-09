# WPF with NET Core, Microsoft Hosting and MVVM



## Creating the application

Start by adding a WPF application for NET Core, I have used the projectname WpfSample in this documentation  
Add a reference to following packages:  

* Microsoft.Extensions.Hosting
* MvvmLightLibsStd10  



## Structuring the application

Add following folders inside the project:

* Models
* Services
* ViewModels
* Views  



## Cleaning up initially

Move MainWindow.xaml and MainWindow.xaml.cs into the folder Views and change the namespace in both files to be WpfSample.Views:  

In MainWindow.xaml.cs:
```
namespace WpfSample.Views
```

In MainWindow.xaml:
```
<Window x:Class="WpfSample.Views.MainWindow"
...
        xmlns:local="clr-namespace:WpfSample.Views"
```



## Adding the viewmodel and its locator

In the folder ViewModels add following two classes:

```
public class ViewModelLocator
{
    public MainViewModel MainViewModel
    {
        get
        {
            return GetMainViewModel().Result;
        }
    }

    public static async Task<MainViewModel> GetMainViewModel()
    {
        MainViewModel vm = App.ServiceProvider.GetRequiredService<MainViewModel>();
        await vm.GetAllAsync();
        return vm;
    }
}

public class MainViewModel : ViewModelBase
{
    public Task GetAllAsync()
    {
        return Task.CompletedTask;
    }
}
```

The ViewModelLocator is a factory that we will add to the application as a resource so that it will be easy for the xaml-views to get a MainViewModel bound to itself. The Locator is easy to extend to handle multiple viewmodels. In this sample I have assumed that we have an async method in MainViewModel named GetAllAsync that will be called immediately when the MainViewModel is instantiated, the reason could be to populate some data structures.



## Setting up the App class



### In App.xaml:

Remove the attribute ```StartupUri="MainWindow.xaml"``` this will be handled in the OnStartup override

Add a reference to the ViewModels namespace ```xmlns:vm="clr-namespace:WpfSample.ViewModels"```

Add a reference to ```xmlns:d="http://schemas.microsoft.com/expression/blend/2008"```

Add a reference to ```xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"```

Add an attribute ```mc:Ignorable="d"```

In Application.Resources add following ResourceDictionary:

```
<ResourceDictionary>
    <vm:ViewModelLocator x:Key="Locator" d:IsDataSource="true" />
    <Style TargetType="Window">
        <Setter Property="FontSize" Value="16" />
    </Style>
</ResourceDictionary>
```

Interesting bug: If you remove the Style resource and only have the ViewModelLocator in the dictionary it will fail, any Style resource will do because it will just serve as a kind of activator for the dictionary itself!



 ### In App.xaml.cs:

```
public partial class App : Application
{
    private readonly IHost host;
    public static IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => ConfigureAppServices(context, services))
            .Build();
        ServiceProvider = host.Services;
    }

    private void ConfigureAppServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddTransient<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await host.StartAsync();
        MainWindow mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (host)
        {
            await host.StopAsync();
        }
        base.OnExit(e);
    }
}
```

These two files are responsible for starting the application, we implement the IHost functionality by using Host.CreateDefaultBuilder, configure our services in ConfigureServices (the viewmodel and the initial window) and then when we build that IHostBuilder it will give us a complete IHost with support for configuration, logging, dependency injection and everything.

When we start the application it will start the IHost and then ask the ServiceProvider for an instance of MainWindow and show it.



## Bind the ViewModel to MainWindow

To bind the MainViewModel against MainWindow as a DataContext we and one single attribute to the Window element:

```DataContext="{Binding Source={StaticResource Locator}, Path=MainViewModel}"```



## First testrun

If we compile and run at this stage it should start up without any issues and we have finished setting up a NET Core based WPF application using MVVM and Microsoft.Extensions hosting framework.



## Add a service

Next step we will do is to add a sample service, we will create a service that simply will list the subdirectories in a folder.

In the folder Services create following interface and class:

```
public interface IFolderService
{
    Task<IEnumerable<string>> GetAllFoldersAsync(string path);
}

public class FolderService : IFolderService
{
    public Task<IEnumerable<string>> GetAllFoldersAsync(string path)
    {
        return Task.FromResult(Directory.EnumerateDirectories(path));
    }
}
```



## Register the service 

In App.xaml.cs, change the method ConfigureAppServices so it looks like this(one line added for the service):

```
private void ConfigureAppServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddSingleton<MainViewModel>();
    services.AddTransient<MainWindow>();
    services.AddScoped<IFolderService, FolderService>();
}
```



## Inject the service into MainViewModel

To inject the service into MainViewModel simply add a constructor that takes the IFolderService as a parameter and store it inside the class as a private field:

```
private readonly IFolderService folderService;

public MainViewModel(IFolderService folderService)
{
    this.folderService = folderService;
}
```



## Adding properties to MainViewModel for the view

Add a full property that holds an IEnumerable of strings for all folders and another full property that holds the selected folder. Note the usage of the Set method that is inherited from ViewModelBase:

```
public IEnumerable<string> Folders
{
    get { return folders; }
    set { Set("Folders", ref folders, value, true); }
}
private IEnumerable<string> folders;

public string SelectedFolder
{
    get { return selectedFolder; }
    set { Set("SelectedFolder", ref selectedFolder, value, true); }
}
private string selectedFolder;

```



## Initializing the properties

Initialize the property Folders in the method GetAllAsync:

```
public async Task GetAllAsync()
{
    Folders = await folderService.GetAllFoldersAsync(@"Change this to some valid folder path");
}
```



## Start using the ViewModel from the view

Remove the Grid element in MainWindow.xaml and replace with following xaml:

```
<StackPanel Orientation="Vertical">
    <ListBox x:Name="listBox" ItemsSource="{Binding Folders}" SelectedItem="{Binding SelectedFolder}" />
    <TextBox Width="120" Height="32" Text="{Binding SelectedFolder}" />
</StackPanel>
```

We have a Listbox that will be populated (ItemsSource) by its binding to Folders and it will set the selected folder by its binding of SelectedItem to SelectedFolder. 

### 

