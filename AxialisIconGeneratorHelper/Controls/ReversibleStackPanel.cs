#region Usings

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace AxialisIconGeneratorHelper.Controls
{
    public class ReversibleStackPanel : StackPanel
    {
        #region Public Fields

        public static readonly DependencyProperty ReverseOrderProperty =
            DependencyProperty.Register("ReverseOrder", typeof(bool), typeof(ReversibleStackPanel), new PropertyMetadata(false));

        #endregion

        #region Public Properties

        public bool ReverseOrder
        {
            get => (bool) this.GetValue(ReverseOrderProperty);
            set => this.SetValue(ReverseOrderProperty, value);
        }

        #endregion

        #region Protected Methods

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double x = 0;
            double y = 0;

            var children = this.ReverseOrder ? this.InternalChildren.Cast<UIElement>().Reverse() : this.InternalChildren.Cast<UIElement>();
            foreach (var child in children)
            {
                Size size;

                if (this.Orientation == Orientation.Horizontal)
                {
                    size = new Size(child.DesiredSize.Width, Math.Max(arrangeSize.Height, child.DesiredSize.Height));
                    child.Arrange(new Rect(new Point(x, y), size));
                    x += size.Width;
                }
                else
                {
                    size = new Size(Math.Max(arrangeSize.Width, child.DesiredSize.Width), child.DesiredSize.Height);
                    child.Arrange(new Rect(new Point(x, y), size));
                    y += size.Height;
                }
            }

            return this.Orientation == Orientation.Horizontal ? new Size(x, arrangeSize.Height) : new Size(arrangeSize.Width, y);
        }

        #endregion
    }
}