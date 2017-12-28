using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Palette
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ResultPage : Page, INotifyPropertyChanged
    {
        ObservableCollection<ColorBrush> PaletteResult = new ObservableCollection<ColorBrush>();
        public ResultPage()
        {
            this.InitializeComponent();
            TitleForeground = (SolidColorBrush)(Resources["SystemControlForegroundBaseHighBrush"]);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var p = e.Parameter;
            if (p is Uri s)
            {
                ImageUri = s;
                var palette = await Palette.GeneratePalette(s);

                if (palette == null)
                {
                    Header.Content = "Build failed, file may be invalid.";
                    return;
                }

                DominantColor = new SolidColorBrush(palette[0].Color.ColorThiefToColor());
                PaletteResult.Clear();
                DominantColorValue = DominantColor.Color.GetHexString();

                if (Palette.IsDarkColor(DominantColor.Color))
                {
                    TitleForeground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    TitleForeground = new SolidColorBrush(Colors.Black);
                }

                palette.RemoveAt(0);
                foreach (var item in palette)
                {
                    PaletteResult.Add(new ColorBrush()
                    {
                        Background = new SolidColorBrush(item.Color.ColorThiefToColor()),
                        Value = item.Color.ToHexString(),
                        Foreground = TitleForeground
                    });
                }
            }
        }

        private string dominantColorValue = "Loading";
        public string DominantColorValue
        {
            get { return dominantColorValue; }
            set { SetProperty(ref dominantColorValue, value); }
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

        private Uri imageUri;
        public Uri ImageUri
        {
            get { return imageUri; }
            set { SetProperty(ref imageUri, value); }
        }

        private SolidColorBrush domColor;
        public SolidColorBrush DominantColor
        {
            get { return domColor; }
            set { SetProperty(ref domColor, value); }
        }

        private SolidColorBrush titleForeground;
        public SolidColorBrush TitleForeground
        {
            get { return titleForeground; }
            set { SetProperty(ref titleForeground, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        bool SetProperty<T>(ref T backingField, T Value, [CallerMemberName] string propertyName = null)
        {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, Value);
            if (changed)
            {
                backingField = Value;
                this.RaisePropertyChanged(propertyName);
            }
            return changed;
        }

        public Visibility BooltoVisibility(bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility BoolNottoVisibility(bool b)
        {
            return !b ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility NullableBooltoVisibility(bool? b)
        {
            if (b is bool a)
            {
                return a ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }
        public Visibility NullableBoolNottoVisibility(bool? b)
        {
            if (b is bool a)
            {
                return !a ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
        public Visibility CollapseIfEmpty(string s)
        {
            return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotEmpty(string s)
        {
            return !string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNull(object s)
        {
            return s == null ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotNull(object s)
        {
            return s != null ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfZero(int count)
        {
            return count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfZero(uint count)
        {
            return count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotZero(int count)
        {
            return count != 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotZero(uint count)
        {
            return count != 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            DataPackage dataPackage = new DataPackage
            {
                // copy 
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetText((e.ClickedItem as ColorBrush).Value);
            Clipboard.SetContent(dataPackage);
            ToastPop.Begin();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage
            {
                // copy 
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetText(DominantColorValue);
            Clipboard.SetContent(dataPackage);
            ToastPop.Begin();
        }
    }

    public class ColorBrush
    {
        public SolidColorBrush Background { get; set; }
        public string Value { get; set; }
        public SolidColorBrush Foreground { get; set; }
    }
}
