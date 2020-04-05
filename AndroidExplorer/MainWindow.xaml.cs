using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AndroidExplorer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private RootNode _root = null;

        public MainWindow()
        {
            InitializeComponent();

            _root = new RootNode("root");
            DataContext = _root.Children;
            RecoverWindowBounds();
            UpdateDevices();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveWindowBounds();
            base.OnClosing(e);
        }


        private void RecoverWindowBounds()
        {
            var settings = AndroidExplorer.Properties.Settings.Default;

            if (settings.WindowLeft >= 0 &&
                (settings.WindowLeft + settings.WindowWidth) < SystemParameters.VirtualScreenWidth)
            {
                Left = settings.WindowLeft;
            }

            if (settings.WindowTop >= 0 &&
                (settings.WindowTop + settings.WindowHeight) < SystemParameters.VirtualScreenHeight)
            {
                Top = settings.WindowTop;
            }

            if (settings.WindowWidth > 0 &&
                settings.WindowWidth <= SystemParameters.WorkArea.Width)
            {
                Width = settings.WindowWidth;
            }

            if (settings.WindowHeight > 0 &&
                settings.WindowHeight <= SystemParameters.WorkArea.Height)
            {
                Height = settings.WindowHeight;
            }

            if (settings.WindowMaximized)
            {
                Loaded += (o, e) => WindowState = WindowState.Maximized;
            }

            SplitGrid.ColumnDefinitions[0].Width = new GridLength(settings.GridLeftPartWidth, GridUnitType.Star);
            SplitGrid.ColumnDefinitions[2].Width = new GridLength(settings.GridRightPartWidth, GridUnitType.Star);

        }

        private void SaveWindowBounds()
        {
            WindowState = WindowState.Normal; // 最大化解除
            var settings = AndroidExplorer.Properties.Settings.Default;
            var widths = SplitGrid.ColumnDefinitions.Select(p => p.ActualWidth).ToArray();
            settings.GridLeftPartWidth = widths[0];
            settings.GridRightPartWidth = widths[2];
            settings.WindowMaximized = WindowState == WindowState.Maximized;
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
            settings.Save();
        }

        async private void UpdateDevices()
        {
            while (true)
            {
                await _root.Update(false);
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        async private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = ((TreeView)sender).SelectedItem as TreeNode;
            if (selectedItem != null)
            {
                DetailView.ItemsSource = selectedItem.DetailItems;
                //selectedItem.DetailItems.Clear();
                await selectedItem.Update(false);
            }
        }
    }
}
