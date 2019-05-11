#region Usings

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using AxialisIconGeneratorHelper.Controls;
using AxialisIconGeneratorHelper.Controls.Notification;
using AxialisIconGeneratorHelper.Extensions;
using AxialisIconGeneratorHelper.Properties;
using AxialisIconGeneratorHelper.Services;
using AxialisIconGeneratorHelper.Utils;
using AxialisIconGeneratorHelper.View;
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
        private bool quitInProgress;

        #endregion

        #region Public Properties

        public RelayCommand CopySvgCommand { get; }

        public RelayCommand CopyXamlCommand { get; }

        public RelayCommand QuitCommand { get; }

        public RelayCommand SaveCommand { get; }

        public ICommand TutorialCommand { get; }

        #endregion

        #region Not Static Constructors

        public MainViewModel()
        {
            this.notificationService = new NotificationService();
            this.SaveCommand = new RelayCommand(this.SaveExecute, CanCopy);
            this.CopySvgCommand = new RelayCommand(this.CopySvgExecute, CanCopy);
            this.CopyXamlCommand = new RelayCommand(this.CopyXamlExecute, CanCopy);
            this.QuitCommand = new RelayCommand(this.QuitExecute);
            this.TutorialCommand = new RelayCommand(TutorialExecute);
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
            HotKey.Register(Key.T, KeyModifier.Ctrl | KeyModifier.Shift | KeyModifier.NoRepeat, this.TutorialCommand);

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

                if (ClipboardHelper.SetText(content))
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

                if (ClipboardHelper.SetText(SvgUtils.ConvertToXaml(content)))
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
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            var textBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 0, 0, 5)
            };
            panel.Children.Add(textBlock);

            var gridCellControl = new GridCellControl
            {
                Background = new SolidColorBrush(ColorParser.ParseHexColor("#fcfbfa")),
                GridLineBrush = new SolidColorBrush(ColorParser.ParseHexColor("#dcdcdc")),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new Image
                {
                    Source = new DrawingImage(drawingGroup),
                    Stretch = Stretch.Uniform
                }
            };

            var binding = new Binding
            {
                Path = new PropertyPath("ActualWidth"),
                RelativeSource = RelativeSource.Self
            };
            gridCellControl.SetBinding(FrameworkElement.HeightProperty, binding);
            panel.Children.Add(gridCellControl);

            return panel;
        }

        private static void OnIsRunningTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Process.GetProcessesByName(@"IconGenerator").Any()) Environment.Exit(0);
        }

        private void QuitExecute()
        {
            if (this.quitInProgress) return;

            this.quitInProgress = true;
            this.notificationService.Show(new NotificationContent
            {
                Content = LocalizationManager.GetLocalizationString(@"Main.ClosingProgram"),
                Title = AppTitle
            }, expirationTime: TimeSpan.FromSeconds(2), onClose: () =>
            {
                foreach (var process in Process.GetProcessesByName(@"IconGenerator")) process.Kill();

                Environment.Exit(0);
            });
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
            var panel = new StackPanel {Orientation = Orientation.Vertical};

            var textBlock = new TextBlock();
            textBlock.Inlines.AddLine(LocalizationManager.GetLocalizationString(@"Help.CloseProgram"));
            textBlock.Inlines.AddLine(LocalizationManager.GetLocalizationString(@"Help.SaveSVG"));
            textBlock.Inlines.AddLine(LocalizationManager.GetLocalizationString(@"Help.CopySVG"));
            textBlock.Inlines.AddLine(LocalizationManager.GetLocalizationString(@"Help.CopyXAML"));
            textBlock.Inlines.Add(new Italic(new Run(LocalizationManager.GetLocalizationString(@"Help.ShowStartup"))));
            panel.Children.Add(textBlock);

            var separator = new Separator();
            panel.Children.Add(separator);

            var button = new Button
            {
                Content = "Tutorial",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.TutorialCommand
            };
            panel.Children.Add(button);

            this.notificationService.Show(new NotificationContent
            {
                Content = panel,
                Title = AppTitle
            }, expirationTime: TimeSpan.FromSeconds(10), onClick: () =>
            {
                Settings.Default.ShowWelcome = false;
                Settings.Default.Save();
            });
        }

        private void ShowInvalidSvgMessage()
        {
            this.notificationService.Show(new NotificationContent
            {
                Content = LocalizationManager.GetLocalizationString(@"Error.FailedParseSVG"),
                Title = AppTitle,
                Type = NotificationType.Error
            });
        }

        private static void TutorialExecute()
        {
            var tutorialWindow = new TutorialWindow();
            tutorialWindow.Show();
        }

        #endregion
    }
}