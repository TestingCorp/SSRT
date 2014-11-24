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

        public static readonly DependencyProperty VScrollBarCornerRadius =
                    DependencyProperty.RegisterAttached("VScrollBarCornerRadius",
                    typeof(CornerRadius),
                    typeof(ExtensionProperties),
                    new FrameworkPropertyMetadata(default(CornerRadius)));

        public static void SetVScrollBarCornerRadius(UIElement element, CornerRadius value)
        {
            element.SetValue(VScrollBarCornerRadius, value);
        }

        public static CornerRadius GetVScrollBarCornerRadius(UIElement element)
        {
            return (CornerRadius)element.GetValue(VScrollBarCornerRadius);
        }

        public static readonly DependencyProperty HScrollBarCornerRadius =
                    DependencyProperty.RegisterAttached("HScrollBarCornerRadius",
                    typeof(CornerRadius),
                    typeof(ExtensionProperties),
                    new FrameworkPropertyMetadata(default(CornerRadius)));

        public static void SetHScrollBarCornerRadius(UIElement element, CornerRadius value)
        {
            element.SetValue(HScrollBarCornerRadius, value);
        }

        public static CornerRadius GetHScrollBarCornerRadius(UIElement element)
        {
            return (CornerRadius)element.GetValue(HScrollBarCornerRadius);
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
            return (CornerRadius)element.GetValue(ScrollBarCornerRadius);
        }
    }
}
