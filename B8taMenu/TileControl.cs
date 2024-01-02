// Based on MahApps Metro's tile control
// Lixkote 2023

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace B8TAM
{
    public class TileControl : Button
    {
        /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register(nameof(Title),
                                          typeof(string),
                                          typeof(TileControl),
                                          new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the title of the <see cref="Tile"/>.
        /// </summary>
        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(nameof(Icon),
                                      typeof(ImageSource),
                                      typeof(TileControl),
                                      new PropertyMetadata(default(ImageSource)));

        /// <summary>
        /// Gets or sets the icon of the <see cref="Tile"/>.
        /// </summary>
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>Identifies the <see cref="HorizontalTitleAlignment"/> dependency property.</summary>
        public static readonly DependencyProperty HorizontalTitleAlignmentProperty =
            DependencyProperty.Register(nameof(HorizontalTitleAlignment),
                                        typeof(HorizontalAlignment),
                                        typeof(TileControl),
                                        new FrameworkPropertyMetadata(HorizontalAlignment.Left));

        /// <summary> 
        /// Gets or sets the horizontal alignment of the <see cref="Title"/>.
        /// </summary> 
        [Bindable(true), Category("Layout")]
        public HorizontalAlignment HorizontalTitleAlignment
        {
            get => (HorizontalAlignment)this.GetValue(HorizontalTitleAlignmentProperty);
            set => this.SetValue(HorizontalTitleAlignmentProperty, value);
        }

        /// <summary>Identifies the <see cref="VerticalTitleAlignment"/> dependency property.</summary> 
        public static readonly DependencyProperty VerticalTitleAlignmentProperty =
            DependencyProperty.Register(nameof(VerticalTitleAlignment),
                                        typeof(VerticalAlignment),
                                        typeof(TileControl),
                                        new FrameworkPropertyMetadata(VerticalAlignment.Bottom));

        /// <summary>
        /// Gets or sets the vertical alignment of the <see cref="Title"/>.
        /// </summary>
        [Bindable(true), Category("Layout")]
        public VerticalAlignment VerticalTitleAlignment
        {
            get => (VerticalAlignment)this.GetValue(VerticalTitleAlignmentProperty);
            set => this.SetValue(VerticalTitleAlignmentProperty, value);
        }

        /// <summary>Identifies the <see cref="Count"/> dependency property.</summary>
        public static readonly DependencyProperty CountProperty
            = DependencyProperty.Register(nameof(Count),
                                          typeof(string),
                                          typeof(TileControl),
                                          new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets a Count text.
        /// </summary>
        public string Count
        {
            get => (string)this.GetValue(CountProperty);
            set => this.SetValue(CountProperty, value);
        }

        /// <summary>Identifies the <see cref="TitleFontSize"/> dependency property.</summary>
        public static readonly DependencyProperty TitleFontSizeProperty
            = DependencyProperty.Register(nameof(TitleFontSize),
                                          typeof(double),
                                          typeof(TileControl),
                                          new PropertyMetadata(16d));

        /// <summary>
        /// Gets or sets the font size of the <see cref="Title"/>.
        /// </summary>
        public double TitleFontSize
        {
            get => (double)this.GetValue(TitleFontSizeProperty);
            set => this.SetValue(TitleFontSizeProperty, value);
        }

        /// <summary>Identifies the <see cref="CountFontSize"/> dependency property.</summary>
        public static readonly DependencyProperty CountFontSizeProperty
            = DependencyProperty.Register(nameof(CountFontSize),
                                          typeof(double),
                                          typeof(TileControl),
                                          new PropertyMetadata(17d));

        /// Gets or sets the font size of the <see cref="Count"/>.
        public double CountFontSize
        {
            get => (double)this.GetValue(CountFontSizeProperty);
            set => this.SetValue(CountFontSizeProperty, value);
        }

        static TileControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TileControl), new FrameworkPropertyMetadata(typeof(TileControl)));
        }
    }
}