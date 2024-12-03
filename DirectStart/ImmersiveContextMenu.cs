using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace B8TAM
{
	public class ImmesiveContextMenu : ContextMenu
	{
		public static readonly DependencyProperty OpenedWithTouchProperty = DependencyProperty.Register("OpenedWithTouch", typeof(bool), typeof(B8TAM.ImmesiveContextMenu), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool OpenedWithTouch
		{
			get
			{
				return (bool)GetValue(OpenedWithTouchProperty);
			}
			set
			{
				SetValue(OpenedWithTouchProperty, value);
			}
		}       

        public ImmesiveContextMenu()
		{
			base.Loaded += ImmesiveContextMenu_Loaded;
        }

        private static Color GetColor(IntPtr pElementName)
        {
            var colourset = DUIColorHelper.GetImmersiveUserColorSetPreference(false, false);
            uint type = DUIColorHelper.GetImmersiveColorTypeFromName(pElementName);
            Marshal.FreeCoTaskMem(pElementName);
            uint colourdword = DUIColorHelper.GetImmersiveColorFromColorSetEx((uint)colourset, type, false, 0);
            byte[] colourbytes = new byte[4];
            colourbytes[0] = (byte)((0xFF000000 & colourdword) >> 24); // A
            colourbytes[1] = (byte)((0x00FF0000 & colourdword) >> 16); // B
            colourbytes[2] = (byte)((0x0000FF00 & colourdword) >> 8); // G
            colourbytes[3] = (byte)(0x000000FF & colourdword); // R
            Color color = Color.FromArgb(colourbytes[0], colourbytes[3], colourbytes[2], colourbytes[1]);
            return color;
        }

        private void ImmesiveContextMenu_Loaded(object sender, RoutedEventArgs e)
		{
			UIElement placementTarget = ContextMenuService.GetPlacementTarget(this);
			UIElement placementTarget2 = base.PlacementTarget;
			if (placementTarget != null)
			{
				placementTarget.TouchDown += Source_TouchDown;
				placementTarget.MouseDown += Source_MouseDown;
			}
			else if (placementTarget2 != null)
			{
				placementTarget2.TouchDown += Source_TouchDown;
				placementTarget2.MouseDown += Source_MouseDown;
			}
		}

		private void Source_MouseDown(object sender, MouseButtonEventArgs e)
		{
			OpenedWithTouch = false;
		}

		private void Source_TouchDown(object sender, TouchEventArgs e)
		{
			Timer touchTimer = new Timer(1.0);
			touchTimer.Elapsed += delegate
			{
				base.Dispatcher.Invoke((Action)delegate
				{
					if (base.IsOpen)
					{
						OpenedWithTouch = true;
					}
					else if (!(e.OriginalSource as UIElement).AreAnyTouchesOver)
					{
						touchTimer.Stop();
					}
				}, new object[0]);
			};
			touchTimer.Start();
		}
	}
}
