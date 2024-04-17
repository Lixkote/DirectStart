using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace B8TAM
{
    class ControlHelper
    {
        public static readonly DependencyProperty MouseOverBorderBrushProperty
            = DependencyProperty.RegisterAttached(
                "MouseOverBorderBrush",
                typeof(Brush),
                typeof(ControlHelper),
                new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
        public static void SetMouseOverBorderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(MouseOverBorderBrushProperty, value);
        }
    }
}
