// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ServerRemoteHostRawUserInterface
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;

namespace System.Management.Automation.Remoting
{
  internal class ServerRemoteHostRawUserInterface : PSHostRawUserInterface
  {
    private ServerRemoteHostUserInterface _remoteHostUserInterface;
    private ServerMethodExecutor _serverMethodExecutor;

    private HostDefaultData HostDefaultData => this._remoteHostUserInterface.ServerRemoteHost.HostInfo.HostDefaultData;

    internal ServerRemoteHostRawUserInterface(
      ServerRemoteHostUserInterface remoteHostUserInterface)
    {
      this._remoteHostUserInterface = remoteHostUserInterface;
      this._serverMethodExecutor = remoteHostUserInterface.ServerRemoteHost.ServerMethodExecutor;
    }

    public override ConsoleColor ForegroundColor
    {
      get => this.HostDefaultData.HasValue(HostDefaultDataId.ForegroundColor) ? (ConsoleColor) this.HostDefaultData.GetValue(HostDefaultDataId.ForegroundColor) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetForegroundColor);
      set
      {
        this.HostDefaultData.SetValue(HostDefaultDataId.ForegroundColor, (object) value);
        this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetForegroundColor, new object[1]
        {
          (object) value
        });
      }
    }

    public override ConsoleColor BackgroundColor
    {
      get => this.HostDefaultData.HasValue(HostDefaultDataId.BackgroundColor) ? (ConsoleColor) this.HostDefaultData.GetValue(HostDefaultDataId.BackgroundColor) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetBackgroundColor);
      set
      {
        this.HostDefaultData.SetValue(HostDefaultDataId.BackgroundColor, (object) value);
        this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBackgroundColor, new object[1]
        {
          (object) value
        });
      }
    }

    public override Coordinates CursorPosition
    {
      get => this.HostDefaultData.HasValue(HostDefaultDataId.CursorPosition) ? (Coordinates) this.HostDefaultData.GetValue(HostDefaultDataId.CursorPosition) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetCursorPosition);
      set
      {
        this.HostDefaultData.SetValue(HostDefaultDataId.CursorPosition, (object) value);
        this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetCursorPosition, new object[1]
        {
          (object) value
        });
      }
    }

    public override Coordinates WindowPosition
    {
      get => this.HostDefaultData.HasValue(HostDefaultDataId.WindowPosition) ? (Coordinates) this.HostDefaultData.GetValue(HostDefaultDataId.WindowPosition) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetWindowPosition);
      set
      {
        this.HostDefaultData.SetValue(HostDefaultDataId.WindowPosition, (object) value);
        this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetWindowPosition, new object[1]
        {
          (object) value
        });
      }
    }

    public override int CursorSize
    {
      get => this.HostDefaultData.HasValue(HostDefaultDataId.CursorSize) ? (int) this.HostDefaultData.GetValue(HostDefaultDataId.CursorSize) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetCursorSize);
      set
      {
        this.HostDefaultData.SetValue(HostDefaultDataId.CursorSize, (object) value);
        this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetCursorSize, new object[1]
        {
          (object) value
        });
      }
    }

    public override Size BufferSize
    {
      get => this.HostDefaultData.HasValue(HostDefaultDataId.BufferSize) ? (Size) this.HostDefaultData.GetValue(HostDefaultDataId.BufferSize) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetBufferSize);
      set
      {
        this.HostDefaultData.SetValue(HostDefaultDataId.BufferSize, (object) value);
        this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBufferSize, new object[1]
        {
          (object) value
        });
      }
    }

    public override Size WindowSize
    {
      get => this.HostDefaultData.HasValue(HostDefaultDataId.WindowSize) ? (Size) this.HostDefaultData.GetValue(HostDefaultDataId.WindowSize) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetWindowSize);
      set
      {
        this.HostDefaultData.SetValue(HostDefaultDataId.WindowSize, (object) value);
        this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetWindowSize, new object[1]
        {
          (object) value
        });
      }
    }

    public override string WindowTitle
    {
      get => this.HostDefaultData.HasValue(HostDefaultDataId.WindowTitle) ? (string) this.HostDefaultData.GetValue(HostDefaultDataId.WindowTitle) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetWindowTitle);
      set
      {
        this.HostDefaultData.SetValue(HostDefaultDataId.WindowTitle, (object) value);
        this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetWindowTitle, new object[1]
        {
          (object) value
        });
      }
    }

    public override Size MaxWindowSize => this.HostDefaultData.HasValue(HostDefaultDataId.MaxWindowSize) ? (Size) this.HostDefaultData.GetValue(HostDefaultDataId.MaxWindowSize) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetMaxWindowSize);

    public override Size MaxPhysicalWindowSize => this.HostDefaultData.HasValue(HostDefaultDataId.MaxPhysicalWindowSize) ? (Size) this.HostDefaultData.GetValue(HostDefaultDataId.MaxPhysicalWindowSize) : throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetMaxPhysicalWindowSize);

    public override bool KeyAvailable => throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetKeyAvailable);

    public override KeyInfo ReadKey(ReadKeyOptions options) => this._serverMethodExecutor.ExecuteMethod<KeyInfo>(RemoteHostMethodId.ReadKey, new object[1]
    {
      (object) options
    });

    public override void FlushInputBuffer() => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.FlushInputBuffer);

    public override void ScrollBufferContents(
      Rectangle source,
      Coordinates destination,
      Rectangle clip,
      BufferCell fill)
    {
      this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.ScrollBufferContents, new object[4]
      {
        (object) source,
        (object) destination,
        (object) clip,
        (object) fill
      });
    }

    public override void SetBufferContents(Rectangle rectangle, BufferCell fill) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBufferContents1, new object[2]
    {
      (object) rectangle,
      (object) fill
    });

    public override void SetBufferContents(Coordinates origin, BufferCell[,] contents) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBufferContents2, new object[2]
    {
      (object) origin,
      (object) contents
    });

    public override BufferCell[,] GetBufferContents(Rectangle rectangle) => throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetBufferContents);

    public override int LengthInBufferCells(string source) => source != null ? source.Length : throw new ArgumentNullException(nameof (source));

    public override int LengthInBufferCells(string source, int offset)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      return source.Length - offset;
    }
  }
}
