// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.BufferCell
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation.Host
{
  public struct BufferCell
  {
    private const string StringsBaseName = "MshHostRawUserInterfaceStrings";
    private char character;
    private ConsoleColor foregroundColor;
    private ConsoleColor backgroundColor;
    private BufferCellType bufferCellType;
    [TraceSource("BufferCell", "S.M.A.Host.BufferCell")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (BufferCell), "S.M.A.Host.BufferCell");

    public char Character
    {
      get => this.character;
      set => this.character = value;
    }

    public ConsoleColor ForegroundColor
    {
      get => this.foregroundColor;
      set => this.foregroundColor = value;
    }

    public ConsoleColor BackgroundColor
    {
      get => this.backgroundColor;
      set => this.backgroundColor = value;
    }

    public BufferCellType BufferCellType
    {
      get => this.bufferCellType;
      set => this.bufferCellType = value;
    }

    public BufferCell(
      char character,
      ConsoleColor foreground,
      ConsoleColor background,
      BufferCellType bufferCellType)
    {
      this.character = character;
      this.foregroundColor = foreground;
      this.backgroundColor = background;
      this.bufferCellType = bufferCellType;
    }

    public override string ToString()
    {
      using (BufferCell.tracer.TraceMethod())
        return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "'{0}' {1} {2} {3}", (object) this.Character, (object) this.ForegroundColor, (object) this.BackgroundColor, (object) this.BufferCellType);
    }

    public override bool Equals(object obj)
    {
      using (BufferCell.tracer.TraceMethod(obj))
      {
        bool flag = false;
        if (obj is BufferCell bufferCell)
          flag = this == bufferCell;
        return flag;
      }
    }

    public override int GetHashCode()
    {
      using (BufferCell.tracer.TraceMethod())
        return ((uint) (this.ForegroundColor ^ this.BackgroundColor) << 16 | (uint) this.Character).GetHashCode();
    }

    public static bool operator ==(BufferCell first, BufferCell second)
    {
      using (BufferCell.tracer.TraceMethod())
        return (int) first.Character == (int) second.Character && first.BackgroundColor == second.BackgroundColor && first.ForegroundColor == second.ForegroundColor && first.BufferCellType == second.BufferCellType;
    }

    public static bool operator !=(BufferCell first, BufferCell second)
    {
      using (BufferCell.tracer.TraceMethod())
        return !(first == second);
    }
  }
}
