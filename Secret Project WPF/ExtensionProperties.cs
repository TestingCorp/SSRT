using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Secret_Project_WPF
{
    public static class ExtensionProperties
    {
        public static readonly DependencyProperty ScrollBarBackgroundBrush =
            DependencyProperty.RegisterAttached("ScrollBarBackgroundBrush",
            typeof(Brush),
            typeof(ExtensionProperties),
            new FrameworkPropertyMetadata(default(Brush)));

        public static void SetScrollBarBackgroundBrush(UIElement element, Brush value)
        {
            element.SetValue(ScrollBarBackgroundBrush, value);
        }

        public static Brush GetScrollBarBackgroundBrush(UIElement element)
        {
            return (Brush)element.GetValue(ScrollBarBackgroundBrush);
        }

        public static readonly DependencyProperty ScrollBarThumbBrush =
            DependencyProperty.RegisterAttached("ScrollBarThumbBrush",
            typeof(Brush),
            typeof(ExtensionProperties),
            new FrameworkPropertyMetadata(default(Brush)));

        public static void SetScrollBarThumbBrush(UIElement element, Brush value)
        {
            element.SetValue(ScrollBarThumbBrush, value);
        }

        public static Brush GetScrollBarThumbBrush(UIElement element)
        {
            return (Brush)element.GetValue(ScrollBarThumbBrush);
        }

        public static readonly DependencyProperty ScrollBarCornerRadius =
                    DependencyProperty.RegisterAttached("ScrollBarCornerRadius",
                    typeof(CornerRadius),
                    typeof(ExtensionProperties),
                    new FrameworkPropertyMetadata(default(CornerRadius)));

        public static void SetScrollBarCornerRadius(UIElement element, CornerRadius value)
        {
            element.SetValue(ScrollBarCornerRadius, value);
        }

        public static CornerRadius GetScrollBarCornerRadius(UIElement element)
        {
            return (CornerRadius)element.GetValue(ScrollBarBackgroundBrush);
        }
    }
}
