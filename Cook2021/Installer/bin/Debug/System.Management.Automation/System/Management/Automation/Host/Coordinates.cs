// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.Coordinates
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation.Host
{
  public struct Coordinates
  {
    private int x;
    private int y;

    public int X
    {
      get => this.x;
      set => this.x = value;
    }

    public int Y
    {
      get => this.y;
      set => this.y = value;
    }

    public Coordinates(int x, int y)
    {
      this.x = x;
      this.y = y;
    }

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0},{1}", (object) this.X, (object) this.Y);

    public override bool Equals(object obj)
    {
      bool flag = false;
      if (obj is Coordinates coordinates)
        flag = this == coordinates;
      return flag;
    }

    public override int GetHashCode()
    {
      ulong num = (this.X >= 0 ? (ulong) this.X : (this.X != int.MinValue ? (ulong) -this.X : (ulong) (-1 * (this.X + 1)))) * 4294967296UL;
      return (this.Y >= 0 ? num + (ulong) this.Y : (this.Y != int.MinValue ? num + (ulong) -this.Y : num + (ulong) (-1 * (this.Y + 1)))).GetHashCode();
    }

    public static bool operator ==(Coordinates first, Coordinates second) => first.X == second.X && first.Y == second.Y;

    public static bool operator !=(Coordinates first, Coordinates second) => !(first == second);
  }
}
