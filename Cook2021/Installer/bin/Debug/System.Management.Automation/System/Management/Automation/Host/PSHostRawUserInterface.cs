// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.PSHostRawUserInterface
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Host
{
  public abstract class PSHostRawUserInterface
  {
    private const string StringsBaseName = "MshHostRawUserInterfaceStrings";
    private const string NonPositiveNumberErrorTemplateResource = "NonPositiveNumberErrorTemplate";
    private const string BufferCellLengthErrorTemplateResource = "BufferCellLengthErrorTemplate";
    private const string AllNullOrEmptyStringsErrorTemplateResource = "AllNullOrEmptyStringsErrorTemplate";
    [TraceSource("PSHostRawUserInterface", "PSHost's Raw UI")]
    private static PSTraceSource _tracer = PSTraceSource.GetTracer(nameof (PSHostRawUserInterface), "PSHost's Raw UI");

    public abstract ConsoleColor ForegroundColor { get; set; }

    public abstract ConsoleColor BackgroundColor { get; set; }

    public abstract Coordinates CursorPosition { get; set; }

    public abstract Coordinates WindowPosition { get; set; }

    public abstract int CursorSize { get; set; }

    public abstract Size BufferSize { get; set; }

    public abstract Size WindowSize { get; set; }

    public abstract Size MaxWindowSize { get; }

    public abstract Size MaxPhysicalWindowSize { get; }

    public KeyInfo ReadKey() => this.ReadKey(ReadKeyOptions.IncludeKeyDown | ReadKeyOptions.IncludeKeyUp);

    public abstract KeyInfo ReadKey(ReadKeyOptions options);

    public abstract void FlushInputBuffer();

    public abstract bool KeyAvailable { get; }

    public abstract string WindowTitle { get; set; }

    public abstract void SetBufferContents(Coordinates origin, BufferCell[,] contents);

    public abstract void SetBufferContents(Rectangle rectangle, BufferCell fill);

    public abstract BufferCell[,] GetBufferContents(Rectangle rectangle);

    public abstract void ScrollBufferContents(
      Rectangle source,
      Coordinates destination,
      Rectangle clip,
      BufferCell fill);

    public virtual int LengthInBufferCells(string source, int offset) => source != null ? this.LengthInBufferCells(source.Substring(offset)) : throw PSHostRawUserInterface._tracer.NewArgumentNullException(nameof (source));

    public virtual int LengthInBufferCells(string source) => source != null ? source.Length : throw PSHostRawUserInterface._tracer.NewArgumentNullException(nameof (source));

    public virtual int LengthInBufferCells(char source) => 1;

    public BufferCell[,] NewBufferCellArray(
      string[] contents,
      ConsoleColor foregroundColor,
      ConsoleColor backgroundColor)
    {
      using (PSHostRawUserInterface._tracer.TraceMethod((object) contents))
      {
        byte[][] numArray = contents != null ? new byte[contents.Length][] : throw PSHostRawUserInterface._tracer.NewArgumentNullException(nameof (contents));
        int length = 0;
        for (int index1 = 0; index1 < contents.Length; ++index1)
        {
          if (!string.IsNullOrEmpty(contents[index1]))
          {
            int num = 0;
            numArray[index1] = new byte[contents[index1].Length];
            for (int index2 = 0; index2 < contents[index1].Length; ++index2)
            {
              numArray[index1][index2] = (byte) this.LengthInBufferCells(contents[index1][index2]);
              num += (int) numArray[index1][index2];
            }
            if (length < num)
              length = num;
          }
        }
        if (length <= 0)
          throw PSHostRawUserInterface._tracer.NewArgumentException(nameof (contents), "MshHostRawUserInterfaceStrings", "AllNullOrEmptyStringsErrorTemplate");
        BufferCell[,] bufferCellArray = new BufferCell[contents.Length, length];
        for (int index1 = 0; index1 < contents.Length; ++index1)
        {
          int index2 = 0;
          int index3 = 0;
          while (index3 < contents[index1].Length)
          {
            if (numArray[index1][index3] == (byte) 1)
              bufferCellArray[index1, index2] = new BufferCell(contents[index1][index3], foregroundColor, backgroundColor, BufferCellType.Complete);
            else if (numArray[index1][index3] == (byte) 2)
            {
              bufferCellArray[index1, index2] = new BufferCell(contents[index1][index3], foregroundColor, backgroundColor, BufferCellType.Leading);
              ++index2;
              bufferCellArray[index1, index2] = new BufferCell(char.MinValue, foregroundColor, backgroundColor, BufferCellType.Trailing);
            }
            ++index3;
            ++index2;
          }
          for (; index2 < length; ++index2)
            bufferCellArray[index1, index2] = new BufferCell(' ', foregroundColor, backgroundColor, BufferCellType.Complete);
        }
        return bufferCellArray;
      }
    }

    public BufferCell[,] NewBufferCellArray(int width, int height, BufferCell contents)
    {
      using (PSHostRawUserInterface._tracer.TraceMethod())
      {
        if (width <= 0)
          throw PSHostRawUserInterface._tracer.NewArgumentOutOfRangeException(nameof (width), (object) width, "MshHostRawUserInterfaceStrings", "NonPositiveNumberErrorTemplate", (object) nameof (width));
        BufferCell[,] bufferCellArray = height > 0 ? new BufferCell[height, width] : throw PSHostRawUserInterface._tracer.NewArgumentOutOfRangeException(nameof (height), (object) height, "MshHostRawUserInterfaceStrings", "NonPositiveNumberErrorTemplate", (object) nameof (height));
        switch (this.LengthInBufferCells(contents.Character))
        {
          case 1:
            for (int index1 = 0; index1 < bufferCellArray.GetLength(0); ++index1)
            {
              for (int index2 = 0; index2 < bufferCellArray.GetLength(1); ++index2)
              {
                bufferCellArray[index1, index2] = contents;
                bufferCellArray[index1, index2].BufferCellType = BufferCellType.Complete;
              }
            }
            break;
          case 2:
            int index3 = width % 2 == 0 ? width : width - 1;
            for (int index1 = 0; index1 < height; ++index1)
            {
              int index2;
              for (int index4 = 0; index4 < index3; index4 = index2 + 1)
              {
                bufferCellArray[index1, index4] = contents;
                bufferCellArray[index1, index4].BufferCellType = BufferCellType.Leading;
                index2 = index4 + 1;
                bufferCellArray[index1, index2] = new BufferCell(char.MinValue, contents.ForegroundColor, contents.BackgroundColor, BufferCellType.Trailing);
              }
              if (index3 < width)
              {
                bufferCellArray[index1, index3] = contents;
                bufferCellArray[index1, index3].BufferCellType = BufferCellType.Leading;
              }
            }
            break;
        }
        return bufferCellArray;
      }
    }

    public BufferCell[,] NewBufferCellArray(Size size, BufferCell contents)
    {
      using (PSHostRawUserInterface._tracer.TraceMethod())
        return this.NewBufferCellArray(size.Width, size.Height, contents);
    }
  }
}
