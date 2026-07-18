using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Win32;

namespace XamlRender.Source;
public partial class MainWindow : Window
{
    private FileSystemWatcher watcher;
    private Window windowPreview;
    
    public MainWindow()
    {
        InitializeComponent();

        OpenFileDialog dialog = new OpenFileDialog();

        dialog.Title = "Selecione um arquivo";
        dialog.Filter = "Xaml File (*.xaml)|*.xaml|All Files (*.*)|*.*";

        bool? result = dialog.ShowDialog();

        if(result == true)
        {
            Watch(dialog.FileName);
        }
    }

    private void LoadButton(Object sender, EventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog();

        dialog.Title = "Selecione um arquivo";        
        dialog.Filter = "Xaml File (*.xaml)|*.xaml|All Files (*.*)|*.*";

        bool? result = dialog.ShowDialog();

        if(result == true)
        {
            Watch(dialog.FileName);
        }
    }

    private void Watch(String path)
    {
        watcher = new FileSystemWatcher();

        watcher.Path = Path.GetDirectoryName(path);
        watcher.Filter = Path.GetFileName(path);

        watcher.NotifyFilter = NotifyFilters.LastWrite;

        watcher.Changed += Reload;

        watcher.EnableRaisingEvents = true;

        Load(path);
    }

    private void Reload(Object sender, FileSystemEventArgs e)
    {
        Thread.Sleep(100);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Load(e.FullPath);
        });
    }

    private void Load(string way)
    {
        try
        {
            string xaml = File.ReadAllText(way);
    
            xaml = Regex.Replace(
                xaml,
                @"x:Class=""[^""]*""",
                ""
            );
    
            windowPreview?.Close();
            windowPreview = (Window)XamlReader.Parse(xaml);
            windowPreview.Show();
            
        } catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }
}