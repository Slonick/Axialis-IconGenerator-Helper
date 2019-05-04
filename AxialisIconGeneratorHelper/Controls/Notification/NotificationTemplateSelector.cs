#region Usings

using System.Windows;
using System.Windows.Controls;

#endregion

namespace AxialisIconGeneratorHelper.Controls.Notification
{
    public class NotificationTemplateSelector : DataTemplateSelector
    {
        #region Private Fields

        private DataTemplate _defaultNotificationTemplate;
        private DataTemplate _defaultStringTemplate;

        #endregion

        #region Public Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (this._defaultStringTemplate == null && this._defaultNotificationTemplate == null) this.GetTemplatesFromResources((FrameworkElement) container);

            switch (item)
            {
                case string _:
                    return this._defaultStringTemplate;
                case NotificationContent _:
                    return this._defaultNotificationTemplate;
                default:
                    return base.SelectTemplate(item, container);
            }
        }

        #endregion

        #region Private Methods

        private void GetTemplatesFromResources(FrameworkElement container)
        {
            this._defaultStringTemplate = container?.FindResource("DefaultStringTemplate") as DataTemplate;
            this._defaultNotificationTemplate = container?.FindResource("DefaultNotificationTemplate") as DataTemplate;
        }

        #endregion
    }
}