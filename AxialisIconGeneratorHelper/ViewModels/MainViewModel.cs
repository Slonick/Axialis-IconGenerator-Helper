#region Usings

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AxialisIconGeneratorHelper.Controls;
using AxialisIconGeneratorHelper.Controls.Notification;
using AxialisIconGeneratorHelper.Services;
using AxialisIconGeneratorHelper.Utils;
using Microsoft.Win32;

#endregion

namespace AxialisIconGeneratorHelper.ViewModels
{
    public class MainViewModel
    {
        #region Private Fields

        private Timer isRunningTimer;
        private readonly NotificationService notificationService;

        #endregion

        #region Not Static Constructors

        public MainViewModel()
        {
            this.notificationService = new NotificationService();
        }

        #endregion

        #region Public Methods

        public void Init()
        {
            HotKey.Register(Key.S, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.OnSaveHotKeyHandler);
            HotKey.Register(Key.C, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.OnCopySvgHotKeyHandler);
            HotKey.Register(Key.X, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.OnCopyXamlHotKeyHandler);
            HotKey.Register(Key.Q, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.OnQuitHotKeyHandler);

            var processes = Process.GetProcessesByName(@"IconGenerator");
            if (!processes.Any())
            {
                const string iconGeneratorPath = @"C:\Program Files (x86)\Axialis\IconGenerator\IconGenerator.exe";
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(iconGeneratorPath);
                    process.Start();
                }
            }

            this.isRunningTimer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 300
            };

            this.isRunningTimer.Elapsed += OnIsRunningTimerElapsed;
            this.ShowHelpMessage();
        }

        #endregion

        #region Private Methods

        private static UIElement GetNotificationContent(string text, Drawing drawingGroup)
        {
            const int cellSize = 12;

            return new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    new TextBlock
                    {
                        Text = text,
                        Foreground = new SolidColorBrush(ColorParser.ParseHexColor("#606060")),
                        Margin = new Thickness(0, 0, 0, 5)
                    },
                    new GridCellControl
                    {
                        Background = new SolidColorBrush(ColorParser.ParseHexColor("#fcfbfa")),
                        BorderBrush = new SolidColorBrush(ColorParser.ParseHexColor("#dcdcdc")),
                        BorderThickness = new Thickness(1),
                        CellSize = new Size(cellSize, cellSize),
                        Padding = new Thickness(cellSize),
                        Width = cellSize * 24,
                        Height = cellSize * 24,
                        Child = new Image
                        {
                            Source = new DrawingImage(drawingGroup),
                            Stretch = Stretch.Uniform
                        }
                    }
                }
            };
        }

        private void OnCopySvgHotKeyHandler()
        {
            var handle = InputUtils.FocusedControlInActiveWindow();
            var content = InputUtils.GetText(handle);
            var drawingGroup = SvgUtils.ConvertToDrawingGroup(content);
            if (drawingGroup.Children.Count < 1)
            {
                this.ShowInvalidSvgMessage();
                return;
            }

            Clipboard.SetDataObject(content);
            this.notificationService.Show(new NotificationContent
            {
                Content = GetNotificationContent(@"SVG скопирован", drawingGroup),
                Title = @"Axialis IconGenerator Helper",
                Type = NotificationType.Success
            });
        }

        private void OnCopyXamlHotKeyHandler()
        {
            var handle = InputUtils.FocusedControlInActiveWindow();
            var content = InputUtils.GetText(handle);
            var drawingGroup = SvgUtils.ConvertToDrawingGroup(content);
            if (drawingGroup.Children.Count < 1)
            {
                this.ShowInvalidSvgMessage();
                return;
            }

            Clipboard.SetDataObject(SvgUtils.ConvertToXaml(content));
            this.notificationService.Show(new NotificationContent
            {
                Content = GetNotificationContent(@"XAML скопирован", drawingGroup),
                Title = @"Axialis IconGenerator Helper",
                Type = NotificationType.Success
            });
        }

        private static void OnIsRunningTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Process.GetProcessesByName(@"IconGenerator").Any()) Environment.Exit(0);
        }

        private async void OnQuitHotKeyHandler()
        {
            this.notificationService.Show(new NotificationContent
            {
                Content = "Закрытие программы...",
                Title = @"Axialis IconGenerator Helper"
            });

            await Task.Delay(1000).ConfigureAwait(true);
            Environment.Exit(0);
        }

        private void OnSaveHotKeyHandler()
        {
            var handle = InputUtils.FocusedControlInActiveWindow();
            var content = InputUtils.GetText(handle);
            var drawingGroup = SvgUtils.ConvertToDrawingGroup(content);
            if (drawingGroup.Children.Count < 1)
            {
                this.ShowInvalidSvgMessage();
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "SVG|*.svg"
            };

            if (dialog.ShowDialog(Application.Current.MainWindow).Value)
            {
                File.WriteAllText(dialog.FileName, content);
                this.notificationService.Show(new NotificationContent
                {
                    Content = GetNotificationContent(@"SVG сохранён", drawingGroup),
                    Title = @"Axialis IconGenerator Helper",
                    Type = NotificationType.Success
                });
            }
        }

        private void ShowHelpMessage()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Ctrl+Shift+Q - Закрыть программу");
            builder.AppendLine("Ctrl+Shift+S - Сохранить SVG");
            builder.AppendLine("Ctrl+Shift+C - Копировать SVG");
            builder.AppendLine("Ctrl+Shift+X - Копировать XAML");

            this.notificationService.Show(new NotificationContent
            {
                Content = builder.ToString().Trim(),
                Title = @"Axialis IconGenerator Helper"
            });
        }

        private void ShowInvalidSvgMessage()
        {
            this.notificationService.Show(new NotificationContent
            {
                Content = "Invalid SVG",
                Title = @"Axialis IconGenerator Helper",
                Type = NotificationType.Error
            }, expirationTime: TimeSpan.MaxValue);
        }

        #endregion
    }
}