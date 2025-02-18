using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;

namespace SystemMonitor
{
    public partial class SettingsWindow : Window
    {
        private MainWindow mainWindow;
        private Color selectedColor = Colors.Black;

        public SettingsWindow(MainWindow owner)
        {
            InitializeComponent();
            mainWindow = owner;
            Owner = owner;

            // 初始化当前设置
            var background = mainWindow.Background as SolidColorBrush;
            if (background != null)
            {
                selectedColor = background.Color;
                SelectedColorBrush.Color = selectedColor;
                OpacitySlider.Value = selectedColor.A;
            }

            // 初始化圆角设置
            var border = mainWindow.FindName("BackgroundBorder") as System.Windows.Controls.Border;
            if (border != null)
            {
                CornerRadiusSlider.Value = border.CornerRadius.TopLeft;
            }
        }

        private void BackgroundStyle_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var backgroundBorder = mainWindow.FindName("BackgroundBorder") as System.Windows.Controls.Border;
            if (backgroundBorder == null) return;

            if (NormalBackgroundRadio.IsChecked == true)
            {
                backgroundBorder.Effect = null;
                backgroundBorder.Background = new SolidColorBrush(selectedColor);
            }
            else if (FrostedGlassRadio.IsChecked == true)
            {
                var blurEffect = new BlurEffect { Radius = 10 };
                backgroundBorder.Effect = blurEffect;
                backgroundBorder.Background = new SolidColorBrush(selectedColor);
            }
            else if (TransparentRadio.IsChecked == true)
            {
                backgroundBorder.Effect = null;
                backgroundBorder.Background = new SolidColorBrush(Colors.Transparent);
            }
            else if (BlurredRadio.IsChecked == true)
            {
                var blurEffect = new BlurEffect { Radius = 20 };
                backgroundBorder.Effect = blurEffect;
                backgroundBorder.Background = new SolidColorBrush(selectedColor);
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;

            selectedColor.A = (byte)OpacitySlider.Value;
            SelectedColorBrush.Color = selectedColor;

            var backgroundBorder = mainWindow.FindName("BackgroundBorder") as System.Windows.Controls.Border;
            if (backgroundBorder?.Background is SolidColorBrush brush)
            {
                brush.Color = selectedColor;
            }
        }

        private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.ColorDialog
            {
                Color = System.Drawing.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B)
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedColor = Color.FromArgb(selectedColor.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
                SelectedColorBrush.Color = selectedColor;

                var backgroundBorder = mainWindow.FindName("BackgroundBorder") as System.Windows.Controls.Border;
                if (backgroundBorder?.Background is SolidColorBrush brush)
                {
                    brush.Color = selectedColor;
                }
            }
        }

        private void CornerRadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;

            var cornerRadius = new CornerRadius(CornerRadiusSlider.Value);
            var backgroundBorder = mainWindow.FindName("BackgroundBorder") as System.Windows.Controls.Border;
            var mainBorder = mainWindow.Content as System.Windows.Controls.Border;

            if (backgroundBorder != null)
            {
                backgroundBorder.CornerRadius = cornerRadius;
            }

            if (mainBorder != null)
            {
                mainBorder.CornerRadius = cornerRadius;
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}