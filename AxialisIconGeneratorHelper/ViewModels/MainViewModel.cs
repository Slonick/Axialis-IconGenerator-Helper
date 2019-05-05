#region Usings

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using AxialisIconGeneratorHelper.Controls;
using AxialisIconGeneratorHelper.Controls.Notification;
using AxialisIconGeneratorHelper.Properties;
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

        private const string AppTitle = @"Axialis IconGenerator Helper";
        private const string IconGeneratorExe = @"IconGenerator.exe";
        private const string IconGeneratorProcessName = @"IconGenerator";
        private const string IconGeneratorUrl = @"https://www.axialis.com/downloads/Axialis-IconGenerator.exe";

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
            var processes = Process.GetProcessesByName(IconGeneratorProcessName);
            if (!processes.Any())
            {
                if (!AxialisUtils.IconGeneratorIsInstalled())
                {
                    this.notificationService.Show(new NotificationContent
                    {
                        Content = LocalizationManager.GetLocalizationString("Error.NotInstalled"),
                        Title = AppTitle,
                        Type = NotificationType.Error
                    }, onClose: () => Environment.Exit(0), onClick: () => Process.Start(IconGeneratorUrl));

                    return;
                }

                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(Path.Combine(AxialisUtils.GetIconGeneratorPath(), IconGeneratorExe));
                    process.Start();
                }
            }

            HotKey.Register(Key.S, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.SaveCommand);
            HotKey.Register(Key.C, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.CopySvgCommand);
            HotKey.Register(Key.X, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.CopyXamlCommand);
            HotKey.Register(Key.Q, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.QuitCommand);

            this.isRunningTimer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 300
            };

            this.isRunningTimer.Elapsed += OnIsRunningTimerElapsed;
            if (Settings.Default.ShowWelcome) this.ShowHelpMessage();
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

                ClipboardHelper.ClipboardSetTextSafely(content);
                this.notificationService.Show(new NotificationContent
                {
                    Content = GetNotificationContent(LocalizationManager.GetLocalizationString(@"Main.SVGCopied"), drawingGroup),
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

                ClipboardHelper.ClipboardSetTextSafely(SvgUtils.ConvertToXaml(content));
                this.notificationService.Show(new NotificationContent
                {
                    Content = GetNotificationContent(LocalizationManager.GetLocalizationString(@"Main.XAMLCopied"), drawingGroup),
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

        private void QuitExecute()
        {
            this.notificationService.Show(new NotificationContent
            {
                Content = LocalizationManager.GetLocalizationString(@"Main.ClosingProgram"),
                Title = AppTitle
            }, onClose: () => Environment.Exit(0));
        }

        private void SaveExecute()
        {
            var handle = InputUtils.FocusedControlInActiveWindow();
            var content = InputUtils.GetText(handle);
            try
            {
                var drawingGroup = SvgUtils.ConvertToDrawingGroup(content);
                if (drawingGroup.Children.Count < 1) throw new XmlException();

                var dialog = new SaveFileDialog {Filter = "SVG|*.svg"};
                if (!dialog.ShowDialog(Application.Current.MainWindow).Value) return;

                File.WriteAllText(dialog.FileName, content);
                this.notificationService.Show(new NotificationContent
                {
                    Content = GetNotificationContent(LocalizationManager.GetLocalizationString(@"Main.SVGSaved"), drawingGroup),
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
            builder.AppendLine(LocalizationManager.GetLocalizationString(@"Help.CloseProgram"));
            builder.AppendLine(LocalizationManager.GetLocalizationString(@"Help.SaveSVG"));
            builder.AppendLine(LocalizationManager.GetLocalizationString(@"Help.CopySVG"));
            builder.AppendLine(LocalizationManager.GetLocalizationString(@"Help.CopyXAML"));

            this.notificationService.Show(new NotificationContent
            {
                Content = builder.ToString().Trim(),
                Title = AppTitle
            }, expirationTime: TimeSpan.FromSeconds(10));

            Settings.Default.ShowWelcome = false;
            Settings.Default.Save();
        }

        private void ShowInvalidSvgMessage()
        {
            this.notificationService.Show(new NotificationContent
            {
                Content = LocalizationManager.GetLocalizationString(@"Error.FailedParseSVG"),
                Title = AppTitle,
                Type = NotificationType.Error
            }, expirationTime: TimeSpan.MaxValue);
        }

        #endregion
    }
}