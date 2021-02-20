// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.Host.InternalHostRawUserInterface
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Internal.Host
{
  internal class InternalHostRawUserInterface : PSHostRawUserInterface
  {
    private PSHostRawUserInterface externalRawUI;
    private InternalHost parentHost;
    [TraceSource("InternalHostRawUserInterface", "S.M.A.InternalHostRawUserInterface")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (InternalHostRawUserInterface), "S.M.A.InternalHostRawUserInterface");

    internal InternalHostRawUserInterface(
      PSHostRawUserInterface externalRawUI,
      InternalHost parentHost)
    {
      using (InternalHostRawUserInterface.tracer.TraceConstructor((object) this))
      {
        InternalHostRawUserInterface.tracer.WriteLine("externalRawUI {0} null", externalRawUI == null ? (object) "is" : (object) "is not");
        this.externalRawUI = externalRawUI;
        this.parentHost = parentHost;
      }
    }

    internal void ThrowNotInteractive() => throw new HostException(ResourceManagerCache.GetResourceString("HostInterfaceExceptionsStrings", "HostFunctionNotImplemented"), (Exception) null, "HostFunctionNotImplemented", ErrorCategory.NotImplemented);

    public override ConsoleColor ForegroundColor
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          ConsoleColor foregroundColor = this.externalRawUI.ForegroundColor;
          InternalHostRawUserInterface.tracer.WriteLine((object) foregroundColor);
          return foregroundColor;
        }
      }
      set
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty((object) value))
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          this.externalRawUI.ForegroundColor = value;
        }
      }
    }

    public override ConsoleColor BackgroundColor
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          ConsoleColor backgroundColor = this.externalRawUI.BackgroundColor;
          InternalHostRawUserInterface.tracer.WriteLine((object) backgroundColor);
          return backgroundColor;
        }
      }
      set
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty((object) value))
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          this.externalRawUI.BackgroundColor = value;
        }
      }
    }

    public override Coordinates CursorPosition
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          Coordinates cursorPosition = this.externalRawUI.CursorPosition;
          InternalHostRawUserInterface.tracer.WriteLine((object) cursorPosition);
          return cursorPosition;
        }
      }
      set
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty((object) value))
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          this.externalRawUI.CursorPosition = value;
        }
      }
    }

    public override Coordinates WindowPosition
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          Coordinates windowPosition = this.externalRawUI.WindowPosition;
          InternalHostRawUserInterface.tracer.WriteLine((object) windowPosition);
          return windowPosition;
        }
      }
      set
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty((object) value))
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          this.externalRawUI.WindowPosition = value;
        }
      }
    }

    public override int CursorSize
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          int cursorSize = this.externalRawUI.CursorSize;
          InternalHostRawUserInterface.tracer.WriteLine((object) cursorSize);
          return cursorSize;
        }
      }
      set
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty((object) value))
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          this.externalRawUI.CursorSize = value;
        }
      }
    }

    public override Size BufferSize
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          Size bufferSize = this.externalRawUI.BufferSize;
          InternalHostRawUserInterface.tracer.WriteLine((object) bufferSize);
          return bufferSize;
        }
      }
      set
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty((object) value))
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          this.externalRawUI.BufferSize = value;
        }
      }
    }

    public override Size WindowSize
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          Size windowSize = this.externalRawUI.WindowSize;
          InternalHostRawUserInterface.tracer.WriteLine((object) windowSize);
          return windowSize;
        }
      }
      set
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty((object) value))
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          this.externalRawUI.WindowSize = value;
        }
      }
    }

    public override Size MaxWindowSize
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          Size maxWindowSize = this.externalRawUI.MaxWindowSize;
          InternalHostRawUserInterface.tracer.WriteLine((object) maxWindowSize);
          return maxWindowSize;
        }
      }
    }

    public override Size MaxPhysicalWindowSize
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          Size physicalWindowSize = this.externalRawUI.MaxPhysicalWindowSize;
          InternalHostRawUserInterface.tracer.WriteLine((object) physicalWindowSize);
          return physicalWindowSize;
        }
      }
    }

    public override KeyInfo ReadKey(ReadKeyOptions options)
    {
      using (InternalHostRawUserInterface.tracer.TraceMethod())
      {
        if (this.externalRawUI == null)
          this.ThrowNotInteractive();
        KeyInfo keyInfo = new KeyInfo();
        try
        {
          keyInfo = this.externalRawUI.ReadKey(options);
        }
        catch (PipelineStoppedException ex)
        {
          LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.parentHost.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
          if (currentlyRunningPipeline == null)
            throw;
          else
            currentlyRunningPipeline.Stopper.Stop();
        }
        InternalHostRawUserInterface.tracer.WriteLine((object) keyInfo);
        return keyInfo;
      }
    }

    public override void FlushInputBuffer()
    {
      using (InternalHostRawUserInterface.tracer.TraceMethod())
      {
        if (this.externalRawUI == null)
          this.ThrowNotInteractive();
        this.externalRawUI.FlushInputBuffer();
      }
    }

    public override bool KeyAvailable
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          bool keyAvailable = this.externalRawUI.KeyAvailable;
          InternalHostRawUserInterface.tracer.WriteLine((object) keyAvailable);
          return keyAvailable;
        }
      }
    }

    public override string WindowTitle
    {
      get
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty())
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          string windowTitle = this.externalRawUI.WindowTitle;
          InternalHostRawUserInterface.tracer.WriteLine(windowTitle, new object[0]);
          return windowTitle;
        }
      }
      set
      {
        using (InternalHostRawUserInterface.tracer.TraceProperty(value, new object[0]))
        {
          if (this.externalRawUI == null)
            this.ThrowNotInteractive();
          this.externalRawUI.WindowTitle = value;
        }
      }
    }

    public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
    {
      using (InternalHostRawUserInterface.tracer.TraceMethod())
      {
        if (this.externalRawUI == null)
          this.ThrowNotInteractive();
        this.externalRawUI.SetBufferContents(origin, contents);
      }
    }

    public override void SetBufferContents(Rectangle r, BufferCell fill)
    {
      using (InternalHostRawUserInterface.tracer.TraceMethod())
      {
        if (this.externalRawUI == null)
          this.ThrowNotInteractive();
        this.externalRawUI.SetBufferContents(r, fill);
      }
    }

    public override BufferCell[,] GetBufferContents(Rectangle r)
    {
      using (InternalHostRawUserInterface.tracer.TraceMethod())
      {
        if (this.externalRawUI == null)
          this.ThrowNotInteractive();
        return this.externalRawUI.GetBufferContents(r);
      }
    }

    public override void ScrollBufferContents(
      Rectangle source,
      Coordinates destination,
      Rectangle clip,
      BufferCell fill)
    {
      using (InternalHostRawUserInterface.tracer.TraceMethod())
      {
        if (this.externalRawUI == null)
          this.ThrowNotInteractive();
        this.externalRawUI.ScrollBufferContents(source, destination, clip, fill);
      }
    }

    public override int LengthInBufferCells(string str)
    {
      if (this.externalRawUI == null)
        this.ThrowNotInteractive();
      return this.externalRawUI.LengthInBufferCells(str);
    }

    public override int LengthInBufferCells(string str, int offset)
    {
      if (this.externalRawUI == null)
        this.ThrowNotInteractive();
      return this.externalRawUI.LengthInBufferCells(str, offset);
    }

    public override int LengthInBufferCells(char character)
    {
      if (this.externalRawUI == null)
        this.ThrowNotInteractive();
      return this.externalRawUI.LengthInBufferCells(character);
    }
  }
}
