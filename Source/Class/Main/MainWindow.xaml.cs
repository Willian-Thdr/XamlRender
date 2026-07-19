using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Win32;

namespace XamlRender.Source;
public partial class MainWindow : Window
{
    private Window? windowPreview;
    private FileSystemWatcher watcher;
    
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

    private void LoadButton(object sender, EventArgs e)
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

    private CancellationTokenSource reload;

    private void Watch(string path)
    {
        watcher?.Dispose();
        reload?.Cancel();
        watcher = new FileSystemWatcher();

        watcher.Path = Path.GetDirectoryName(path);
        watcher.Filter = Path.GetFileName(path);

        watcher.NotifyFilter = NotifyFilters.LastWrite;

        watcher.Changed += (_, _) =>
        {
            reload?.Cancel();

            reload = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, reload.Token);

                    Dispatcher.Invoke(() =>
                    {
                        Load(path);
                    });
                } catch (TaskCanceledException)
                {
                }
            });
        };

        watcher.EnableRaisingEvents = true;

        Load(path);
    }

    private string ReadWhenSafe(string path)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                return File.ReadAllText(path);
            } 
            catch(IOException)
            {
                Thread.Sleep(100);
            }
        }

        throw new IOException("Não foi possível ler o arquivo");
    }

    private void Load(string way)
    {
        try
        {
            string xaml = ReadWhenSafe(way);

            xaml = Regex.Replace(
                xaml,
                @"x:Class=""[^""]*""",
                ""
            );

            Window newWindow = (Window)XamlReader.Parse(xaml);

            if (windowPreview != null)
            {
                newWindow.Left = windowPreview.Left;
                newWindow.Top = windowPreview.Top;
                newWindow.WindowStartupLocation = windowPreview.WindowStartupLocation;

                windowPreview.Close();
            }

            windowPreview = newWindow;
            windowPreview.Show();

        } catch (Exception e)
        {
            switch (e)
            {
                case NullReferenceException:
                    MessageBox.Show("O arquivo escolhido não contém nenhuma linha de código dentro de <Window>." + 
                    "\nPor favor, escolher um arquivo que contenha algo escrito.");
                break;

                default:
                    MessageBox.Show($"Erro: {e}");
                break;
            }
        }
    }
}