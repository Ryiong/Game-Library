using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Game_Library.Extensions
{
    public static class ButtonProps
    {
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.RegisterAttached("HoverBackground", typeof(Brush), typeof(ButtonProps), new PropertyMetadata(Brushes.Transparent));

        public static void SetHoverBackground(DependencyObject obj, Brush value) => obj.SetValue(HoverBackgroundProperty, value);
        public static Brush GetHoverBackground(DependencyObject obj) => (Brush)obj.GetValue(HoverBackgroundProperty);

        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.RegisterAttached("PressedBackground", typeof(Brush), typeof(ButtonProps), new PropertyMetadata(Brushes.Transparent));

        public static void SetPressedBackground(DependencyObject obj, Brush value) => obj.SetValue(PressedBackgroundProperty, value);
        public static Brush GetPressedBackground(DependencyObject obj) => (Brush)obj.GetValue(PressedBackgroundProperty);
    }
}
