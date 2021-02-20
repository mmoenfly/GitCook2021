// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.Rectangle
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation.Host
{
  public struct Rectangle
  {
    private const string StringsBaseName = "MshHostRawUserInterfaceStrings";
    private const string LessThanErrorTemplateResource = "LessThanErrorTemplate";
    private int left;
    private int top;
    private int right;
    private int bottom;
    [TraceSource("BufferCell", "S.M.A.Host.BufferCell")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("BufferCell", "S.M.A.Host.BufferCell");

    public int Left
    {
      get => this.left;
      set => this.left = value;
    }

    public int Top
    {
      get => this.top;
      set => this.top = value;
    }

    public int Right
    {
      get => this.right;
      set => this.right = value;
    }

    public int Bottom
    {
      get => this.bottom;
      set => this.bottom = value;
    }

    public Rectangle(int left, int top, int right, int bottom)
    {
      if (right < left)
        throw Rectangle.tracer.NewArgumentException(nameof (right), "MshHostRawUserInterfaceStrings", "LessThanErrorTemplate", (object) nameof (right), (object) nameof (left));
      if (bottom < top)
        throw Rectangle.tracer.NewArgumentException(nameof (bottom), "MshHostRawUserInterfaceStrings", "LessThanErrorTemplate", (object) nameof (bottom), (object) nameof (top));
      this.left = left;
      this.top = top;
      this.right = right;
      this.bottom = bottom;
    }

    public Rectangle(Coordinates upperLeft, Coordinates lowerRight)
      : this(upperLeft.X, upperLeft.Y, lowerRight.X, lowerRight.Y)
    {
      using (Rectangle.tracer.TraceConstructor((object) this))
        ;
    }

    public override string ToString()
    {
      using (Rectangle.tracer.TraceMethod())
        return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0},{1} ; {2},{3}", (object) this.Left, (object) this.Top, (object) this.Right, (object) this.Bottom);
    }

    public override bool Equals(object obj)
    {
      using (Rectangle.tracer.TraceMethod(obj))
      {
        bool flag = false;
        if (obj is Rectangle rectangle)
          flag = this == rectangle;
        return flag;
      }
    }

    public override int GetHashCode()
    {
      using (Rectangle.tracer.TraceMethod())
      {
        int num1 = this.Top ^ this.Bottom;
        ulong num2 = (num1 >= 0 ? (ulong) num1 : (num1 != int.MinValue ? (ulong) -num1 : (ulong) (-1 * (num1 + 1)))) * 4294967296UL;
        int num3 = this.Left ^ this.Right;
        return (num3 >= 0 ? num2 + (ulong) num3 : (num3 != int.MinValue ? num2 + (ulong) -num1 : num2 + (ulong) (-1 * (num3 + 1)))).GetHashCode();
      }
    }

    public static bool operator ==(Rectangle first, Rectangle second)
    {
      using (Rectangle.tracer.TraceMethod())
        return first.Top == second.Top && first.Left == second.Left && first.Bottom == second.Bottom && first.Right == second.Right;
    }

    public static bool operator !=(Rectangle first, Rectangle second)
    {
      using (Rectangle.tracer.TraceMethod())
        return !(first == second);
    }
  }
}
