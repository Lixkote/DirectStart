using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B8TAM
{
    // This calculates default tile colors for Win32 applications, or applications that do not have it specified correctly in their appxmanifest.
    internal class TileColor
    {
        public string LeftGradient { get; set; }
        public string RightGradient { get; set; }
        public string Border { get; set; }

        public enum DefaultTileColors
        {
            Blue,
            Teal,
            Pumpkin,
            LightGreen,
            PaleGreen,
            Gray,
            Purple,
            Pink,
            Red,
            Black,
        }

        public static TileColor GetTileColor(DefaultTileColors color)
        {
            switch (color)
            {
                case DefaultTileColors.Blue:
                    return new TileColor
                    {
                        LeftGradient = "#094AB2",
                        RightGradient = "#0A5BC4",
                        Border = "#236CCA"
                    };
                case DefaultTileColors.Teal:
                    return new TileColor
                    {
                        LeftGradient = "#008299",
                        RightGradient = "#00A0B1",
                        Border = "#1AAAB9"
                    };
                case DefaultTileColors.Pumpkin:
                    return new TileColor
                    {
                        LeftGradient = "#D24726",
                        RightGradient = "#DC572E",
                        Border = "#E06843"
                    };
                case DefaultTileColors.LightGreen:
                    return new TileColor
                    {
                        LeftGradient = "#008A00",
                        RightGradient = "#00A600",
                        Border = "#1AAF1A"
                    };
                case DefaultTileColors.PaleGreen:
                    return new TileColor
                    {
                        LeftGradient = "#128023",
                        RightGradient = "#159E2A",
                        Border = "#2A8D39"
                    };
                case DefaultTileColors.Gray:
                    return new TileColor
                    {
                        LeftGradient = "#595959",
                        RightGradient = "#6E6E6E",
                        Border = "#7D7D7D"
                    };
                case DefaultTileColors.Purple:
                    return new TileColor
                    {
                        LeftGradient = "#5133AB",
                        RightGradient = "#643EBF",
                        Border = "#7452C6"
                    };
                case DefaultTileColors.Pink:
                    return new TileColor
                    {
                        LeftGradient = "#8C0095",
                        RightGradient = "#A700AE",
                        Border = "#B01AB6"
                    };
                case DefaultTileColors.Red:
                    return new TileColor
                    {
                        LeftGradient = "#7B0000",
                        RightGradient = "#980000",
                        Border = "#A31A1A"
                    };
                case DefaultTileColors.Black:
                    return new TileColor
                    {
                        LeftGradient = "#323232",
                        RightGradient = "#373737",
                        Border = "#444444"
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }
        }
    }
}
