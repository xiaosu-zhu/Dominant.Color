using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
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
        }

        private async void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;

            e.AcceptedOperation = DataPackageOperation.Copy;
            var p = await e.DataView.GetStorageItemsAsync();
            if (p.Count == 1)
            {
                if (p[0] is StorageFile f)
                {
                    if (f.ContentType.Contains("image"))
                    {
                        e.DragUIOverride.IsGlyphVisible = true;
                        e.DragUIOverride.Caption = "Drop to Build";
                        e.DragUIOverride.IsCaptionVisible = true;
                        e.DragUIOverride.IsContentVisible = true;
                        return;
                    }
                }
            }
            e.DragUIOverride.IsGlyphVisible = true;
            e.DragUIOverride.Caption = "Can't Drop";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = false;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.AcceptedOperation = DataPackageOperation.Copy;
            var p = await e.DataView.GetStorageItemsAsync();
            if (p.Count == 1)
            {
                if (p[0] is StorageFile f)
                {
                    if (f.ContentType.Contains("image"))
                    {
                        var guid = Guid.NewGuid();
                        var copied = await f.CopyAsync(ApplicationData.Current.LocalCacheFolder, guid.ToString() + f.FileType, NameCollisionOption.ReplaceExisting);
                        (Window.Current.Content as Frame).Navigate(typeof(ResultPage), new Uri(copied.Path));
                        return;
                    }
                }
            }
        }

        private static bool IsCtrlKeyPressed()
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        private async void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (IsCtrlKeyPressed())
            {
                if (e.Key == VirtualKey.V)
                {
                    DataPackageView dataPackageView = Clipboard.GetContent();
                    if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                    {
                        var p = await dataPackageView.GetStorageItemsAsync();
                        if (p.Count == 1)
                        {
                            if (p[0] is StorageFile f)
                            {
                                if (f.ContentType.Contains("image"))
                                {
                                    var guid = Guid.NewGuid();
                                    var copied = await f.CopyAsync(ApplicationData.Current.LocalCacheFolder, guid.ToString() + f.FileType, NameCollisionOption.ReplaceExisting);
                                    (Window.Current.Content as Frame).Navigate(typeof(ResultPage), new Uri(copied.Path));
                                    e.Handled = true;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Focus(FocusState.Programmatic);

            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                if (Uri.TryCreate(text, UriKind.Absolute, out var res)
                    && (res.Scheme == Uri.UriSchemeHttp || res.Scheme == Uri.UriSchemeHttps || res.Scheme == Uri.UriSchemeFile))
                {
                    TextBox.Text = text;
                    TextBox.Select(0, TextBox.Text.Length);
                }
            }
        }

        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (Uri.TryCreate(TextBox.Text, UriKind.Absolute, out var res)
                    && (res.Scheme == Uri.UriSchemeHttp || res.Scheme == Uri.UriSchemeHttps || res.Scheme == Uri.UriSchemeFile))
                {
                    try
                    {
                        (Window.Current.Content as Frame).Navigate(typeof(ResultPage), TextBox.Text);
                        e.Handled = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
