// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.Size
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation.Host
{
  public struct Size
  {
    private int width;
    private int height;

    public int Width
    {
      get => this.width;
      set => this.width = value;
    }

    public int Height
    {
      get => this.height;
      set => this.height = value;
    }

    public Size(int width, int height)
    {
      this.width = width;
      this.height = height;
    }

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0},{1}", (object) this.Width, (object) this.Height);

    public override bool Equals(object obj)
    {
      bool flag = false;
      if (obj is Size size)
        flag = this == size;
      return flag;
    }

    public override int GetHashCode()
    {
      ulong num = (this.Width >= 0 ? (ulong) this.Width : (this.Width != int.MinValue ? (ulong) -this.Width : (ulong) (-1 * (this.Width + 1)))) * 4294967296UL;
      return (this.Height >= 0 ? num + (ulong) this.Height : (this.Height != int.MinValue ? num + (ulong) -this.Height : num + (ulong) (-1 * (this.Height + 1)))).GetHashCode();
    }

    public static bool operator ==(Size first, Size second) => first.Width == second.Width && first.Height == second.Height;

    public static bool operator !=(Size first, Size second) => !(first == second);
  }
}
