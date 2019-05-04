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
using System.Xml;
using AxialisIconGeneratorHelper.Controls;
using AxialisIconGeneratorHelper.Controls.Notification;
using AxialisIconGeneratorHelper.Services;
using AxialisIconGeneratorHelper.Utils;
using AxialisIconGeneratorHelper.ViewModels.Base;
using Microsoft.Win32;

#endregion

namespace AxialisIconGeneratorHelper.ViewModels
{
    public class MainViewModel
    {
        #region Private Constants

        private const string IconGeneratorPath = @"C:\Program Files (x86)\Axialis\IconGenerator\IconGenerator.exe";
        private const string IconGeneratorProcessName = @"IconGenerator";
        private const string AppTitle = @"Axialis IconGenerator Helper";

        #endregion

        #region Private Fields

        private Timer isRunningTimer;
        private readonly NotificationService notificationService;

        #endregion

        #region Public Properties

        public RelayCommand CopySvgCommand { get; }

        public RelayCommand CopyXamlCommand { get; }

        public RelayCommand QuitCommand { get; }

        public RelayCommand SaveCommand { get; }

        #endregion

        #region Not Static Constructors

        public MainViewModel()
        {
            this.notificationService = new NotificationService();
            this.SaveCommand = new RelayCommand(this.SaveExecute, CanCopy);
            this.CopySvgCommand = new RelayCommand(this.CopySvgExecute, CanCopy);
            this.CopyXamlCommand = new RelayCommand(this.CopyXamlExecute, CanCopy);
            this.QuitCommand = new RelayCommand(this.QuitExecute);
        }

        #endregion

        #region Public Methods

        public void Init()
        {
            HotKey.Register(Key.S, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.SaveCommand);
            HotKey.Register(Key.C, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.CopySvgCommand);
            HotKey.Register(Key.X, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.CopyXamlCommand);
            HotKey.Register(Key.Q, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.QuitCommand);

            var processes = Process.GetProcessesByName(IconGeneratorProcessName);
            if (!processes.Any())
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(IconGeneratorPath);
                    process.Start();
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

        private static bool CanCopy()
        {
            var handle = InputUtils.FocusedControlInActiveWindow();
            if (!FocusInIconGenerator(handle)) return false;

            var content = InputUtils.GetText(handle);
            return !string.IsNullOrWhiteSpace(content);
        }

        private void CopySvgExecute()
        {
            var handle = InputUtils.FocusedControlInActiveWindow();
            var content = InputUtils.GetText(handle);
            
            try
            {
                var drawingGroup = SvgUtils.ConvertToDrawingGroup(content);
                if (drawingGroup.Children.Count < 1) throw new XmlException();

                Clipboard.SetDataObject(content);
                this.notificationService.Show(new NotificationContent
                {
                    Content = GetNotificationContent(@"SVG скопирован", drawingGroup),
                    Title = AppTitle,
                    Type = NotificationType.Success
                });
            }
            catch (XmlException)
            {
                this.ShowInvalidSvgMessage();
            }
        }

        private void CopyXamlExecute()
        {
            var handle = InputUtils.FocusedControlInActiveWindow();
            var content = InputUtils.GetText(handle);

            try
            {
                var drawingGroup = SvgUtils.ConvertToDrawingGroup(content);
                if (drawingGroup.Children.Count < 1) throw new XmlException();

                Clipboard.SetDataObject(SvgUtils.ConvertToXaml(content));
                this.notificationService.Show(new NotificationContent
                {
                    Content = GetNotificationContent(@"XAML скопирован", drawingGroup),
                    Title = AppTitle,
                    Type = NotificationType.Success
                });
            }
            catch (XmlException)
            {
                this.ShowInvalidSvgMessage();
            }
        }

        private static bool FocusInIconGenerator(IntPtr controlHandle)
            => ProcessUtil.GetProcessNameById(controlHandle) == IconGeneratorProcessName;

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

        private static void OnIsRunningTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Process.GetProcessesByName(@"IconGenerator").Any()) Environment.Exit(0);
        }

        private async void QuitExecute()
        {
            this.notificationService.Show(new NotificationContent
            {
                Content = "Закрытие программы...",
                Title = AppTitle
            });

            await Task.Delay(1000).ConfigureAwait(true);
            Environment.Exit(0);
        }

        private void SaveExecute()
        {
            var handle = InputUtils.FocusedControlInActiveWindow();
            var content = InputUtils.GetText(handle);
            try
            {
                var drawingGroup = SvgUtils.ConvertToDrawingGroup(content);
                if (drawingGroup.Children.Count < 1) throw new XmlException();

                var dialog = new SaveFileDialog { Filter = "SVG|*.svg" };
                if (!dialog.ShowDialog(Application.Current.MainWindow).Value) return;

                File.WriteAllText(dialog.FileName, content);
                this.notificationService.Show(new NotificationContent
                {
                    Content = GetNotificationContent(@"SVG сохранён", drawingGroup),
                    Title = AppTitle,
                    Type = NotificationType.Success
                });
            }
            catch (XmlException)
            {
                this.ShowInvalidSvgMessage();
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
                Title = AppTitle
            }, expirationTime: TimeSpan.FromSeconds(10));
        }

        private void ShowInvalidSvgMessage()
        {
            this.notificationService.Show(new NotificationContent
            {
                Content = @"Не удалось распарсить SVG",
                Title = AppTitle,
                Type = NotificationType.Error
            }, expirationTime: TimeSpan.MaxValue);
        }

        #endregion
    }
}