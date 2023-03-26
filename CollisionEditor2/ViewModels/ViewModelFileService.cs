using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using CollisionEditor2.Views;

namespace CollisionEditor2.ViewModels;

public static class ViewModelFileService
{

    public enum Filters { TileMap, AngleMap, WidthMap, HeightMap }

    private static Dictionary<Filters, FileDialogFilter> filters = new Dictionary<Filters, FileDialogFilter>()
    {
        [Filters.TileMap]   = new FileDialogFilter() { Name = "Image Files(*.png)",  Extensions = { "png" } },
        [Filters.AngleMap]  = new FileDialogFilter() { Name = "Binary Files(*.bin)", Extensions = { "bin" } },
        [Filters.WidthMap]  = new FileDialogFilter() { Name = "Binary Files(*.bin)", Extensions = { "bin" } },
        [Filters.HeightMap] = new FileDialogFilter() { Name = "Binary Files(*.bin)", Extensions = { "bin" } }
    };

    public static string GetFileSavePath(MainWindow mainWindow, Filters filterID)
    {
        var fileDialog = new SaveFileDialog()
        {
            Filters =new List<FileDialogFilter>(){ filters[filterID]}
        };
        fileDialog.ShowAsync(mainWindow);

        if (fileDialog.InitialFileName == null)
        {
            return string.Empty;
        }
        else
        {
            return fileDialog.InitialFileName;
        }
    }

    public static string GetFileOpenPath(MainWindow mainWindow,Filters filterID)
    {
        var fileDialog = new OpenFileDialog()
        {
            Filters = new List<FileDialogFilter>() 
            {
                filters[filterID], new FileDialogFilter() { Name = "All files(*.*)", Extensions = { "." } } 
            }
        };

        string[] filePath = fileDialog.ShowAsync(mainWindow).Result;
        if (filePath == null)
        {
            return string.Empty;
        }
        else
        {
            return string.Join("/", filePath);
        }
    }
}
