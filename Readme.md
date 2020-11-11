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

Add a full property that holds an IEnumerable of strings for all folders and another full property that holds the selected folder. Note the usage of the Set method that is inherited from ViewModelBase, this will make it possible to send messages to the view to invalidate itself and redraw its content:

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

Initialize the property Folders in the method MainViewModel.GetAllAsync:

```
public async Task GetAllAsync()
{
    Folders = await folderService.GetAllFoldersAsync(@"Change this to some valid folder path");
}
```



## Start using the ViewModel from the view

The property DataContext is found on all components in WPF, we could simplify by setting MainViewModel as the datacontext for the main window and it will be inherited by all components found in this window. But, it's also possible to assign a separate datacontext/viewmodel to one single component or a subset of components(like fx a UserControl). The main reason to use separate viewmodels is simply to create a distinct separation between various parts of the view. 

Normally viewmodels aren't aware of each other, that's the isolation benefit we have from this kind of design. If, however, there's a need for some kind of communication between two viewmodels, then it could be solved by using registered events/delegates.  

### Implement value bindings

Remove the Grid element in MainWindow.xaml and replace with following xaml:

```
<StackPanel Orientation="Vertical">
    <ListBox x:Name="listBox" ItemsSource="{Binding Folders}" SelectedItem="{Binding SelectedFolder}" />
    <TextBox Width="120" Height="32" Text="{Binding SelectedFolder}" />
</StackPanel>
```

We have a Listbox that will be populated (ItemsSource) by its binding to Folders and it will set the selected folder by its binding of SelectedItem to SelectedFolder.  Things to explore further here is how bindings work, One-Way, Two-Way etc. There's a multitude of alternatives here. A hint is that it is possible to use bindings against visibility of a component and by that it's possible to get a more interactive view that changes behaviour based on the values in the viewmodel. I will add a sample of this kind of implementation later in this text.

### Add command bindings

When it comes to buttons and components that calls for actions we can bind the controls agains the RelayCommand class from MVVMLight. In the viewmodel add following readonly property:

```public RelayCommand GetCommand { get; }```

In the constructor, initialize it with following:

```GetCommand = new RelayCommand(async () => await GetAllAsync());```

This is the simplest form of RelayCommand, we assign the execute action to call the method GetAllAsync in the viewmodel. It's possible to add more parameters to RelayCommand constructor, one useful is the Func parameter where its possible to define if the control should be enabled or not (canExecute).

If you have a button the binding against a RelayCommand should look like this:

```<Button Command="{Binding GetCommand}" Content="Refresh" Width="120" Height="32" />``` 

## A more complex binding example

Let's say that we have a DataGrid with some content and we would like to be able to delete a row in the grid. One way is to add a button for each row where they all are bound to the same RelayCommand.

Declare a RelayCommand property with <int> as valuetype, this defines that the RelayCommand will carry a parameter of type int:

```public RelayCommand<int> DeleteCommand { get; }```

Initialize it as usual and send the parameter named id along to your method DeleteAsync.

```DeleteCommand = new RelayCommand<int>(async (int id) => await DeleteAsync(id));```

The called method should look something like this:

```
private async Task DeleteAsync(int id)
{
    if (id != 0)
    {
        //Delete the item with this specific id
    }
}
```

In the xaml-code you bind the ItemsSource and SelectedItem of the DataGrid, you also bind the columns normally to the properties in each row. When it comes to the button column you have to use a template column for it and in the template you define a button that you bind with RelativeSource to the DataGrid (and by that to the DataContext of the DataGrid) and then define the Path to the RelayCommand in the viewmodel and then finally you bind the CommandParameter to the Id property of the item for that row. The CommandParameter will be sent automatically as a parameter to the RelayCommand.

```
<DataGrid Grid.Row="0" ItemsSource="{Binding Workloads}" SelectedItem="{Binding SelectedWorkload}"
    ScrollViewer.VerticalScrollBarVisibility="Visible" AutoGenerateColumns="False" IsReadOnly="True">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Id"  Binding="{Binding Id}"/>
        <DataGridTextColumn Header="Person"  Binding="{Binding Person}"/>
        <DataGridTemplateColumn Header="Delete" Width="auto" MinWidth="70">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <Button Width="60" Height="24" 
                        Command="{Binding RelativeSource={RelativeSource AncestorType={x:Type DataGrid}},
                        Path=DataContext.DeleteCommand}" CommandParameter="{Binding Id}"
                        Content="Delete" />
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

