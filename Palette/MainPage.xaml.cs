using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Palette
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var t = ThreadPool.RunAsync(async x =>
            {
                await Init();
            });
        }

        public async Task Init()
        {
            var folder = ApplicationData.Current.LocalCacheFolder;
            var p = await folder.GetBasicPropertiesAsync();
            if (p.Size > Math.Pow(2, 20) * 10)
            {
                var items = await folder.GetItemsAsync();
                foreach (var item in items)
                {
                    await item.DeleteAsync();
                }
            }
        }

        private async void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;

            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            var p = await e.DataView.GetStorageItemsAsync();
            if (p.Count > 0)
            {
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.Caption = "Drop to Build";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = true;
            }
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            var p = await e.DataView.GetStorageItemsAsync();
            if (p.Count > 0)
            {
                if (p[0] is StorageFile f)
                {
                    var guid = new Guid();
                    var copied = await f.CopyAsync(ApplicationData.Current.LocalCacheFolder, guid.ToString() + f.FileType, NameCollisionOption.ReplaceExisting);
                    var palette = await Palette.GeneratePalette(new Uri(copied.Path));
                }
            }
        }
    }
}
