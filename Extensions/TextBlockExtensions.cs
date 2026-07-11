using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Game_Library.Extensions
{
    public static class TextBlockExtensions
    {
        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.RegisterAttached(
                "MaxLines", typeof(int), typeof(TextBlockExtensions),
                new PropertyMetadata(0, OnMaxLinesChanged));

        public static void SetMaxLines(UIElement element, int value) =>
            element.SetValue(MaxLinesProperty, value);

        public static int GetMaxLines(UIElement element) =>
            (int)element.GetValue(MaxLinesProperty);

        private static void OnMaxLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock tb && e.NewValue is int maxLines && maxLines > 0)
            {
                double lineHeight = tb.LineHeight;

                if (double.IsNaN(lineHeight) || lineHeight <= 0)
                {
                    lineHeight = tb.FontSize * 1.3;
                }

                tb.MaxHeight = lineHeight * maxLines;
            }
        }
    }
}
