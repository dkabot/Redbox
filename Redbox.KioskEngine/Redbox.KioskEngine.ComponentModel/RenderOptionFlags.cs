using System;

namespace Redbox.KioskEngine.ComponentModel
{
  [Flags]
  public enum RenderOptionFlags
  {
    None = 0,
    Background = 1,
    Text = 2,
    Image = 4,
    TextShadow = 8,
    DisableAntiAlias = 16, // 0x00000010
    Tile = 32, // 0x00000020
    ElideText = 64, // 0x00000040
    DrawBorder = 128, // 0x00000080
    DrawRoundedCorners = 256, // 0x00000100
    BackgroundGradient = 512, // 0x00000200
    HtmlToImage = 1024, // 0x00000400
  }
}
