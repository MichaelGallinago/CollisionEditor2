using CollisionEditor2.Views;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace CollisionEditor2.ViewModels;

public static class ViewModelFileUtilities
{
    public enum Filters { TileMap, AngleMap, WidthMap, HeightMap }

    private readonly static Dictionary<Filters, FileDialogFilter> filters = new()
    {
        [Filters.TileMap]   = new FileDialogFilter() { Name = "Image Files(*.png)",  Extensions = { "png" } },
        [Filters.AngleMap]  = new FileDialogFilter() { Name = "Binary Files(*.bin)", Extensions = { "bin" } },
        [Filters.WidthMap]  = new FileDialogFilter() { Name = "Binary Files(*.bin)", Extensions = { "bin" } },
        [Filters.HeightMap] = new FileDialogFilter() { Name = "Binary Files(*.bin)", Extensions = { "bin" } }
    };

    public static async Task<string> GetFileSavePath(MainWindow mainWindow, Filters filterID)
    {
        var fileDialog = new SaveFileDialog()
        {
            Filters = new List<FileDialogFilter>()
            { 
                filters[filterID]
            }
        };

        string? filePath = await fileDialog.ShowAsync(mainWindow);

        return filePath is null ? string.Empty : filePath;
    }

    public static async Task<string> GetFileOpenPath(MainWindow mainWindow,Filters filterID)
    {
        var fileDialog = new OpenFileDialog()
        {
            Filters = new List<FileDialogFilter>() 
            {
                filters[filterID], 
                new FileDialogFilter() 
                { 
                    Name = "All files(*.*)", 
                    Extensions = { "." } 
                } 
            }
        };

        string[]? filePath = await fileDialog.ShowAsync(mainWindow);

        return filePath is null ? string.Empty : string.Join("/", filePath);
    }
}
