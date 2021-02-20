// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Breakpoint
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public class Breakpoint
  {
    private ScriptBlock _action;
    private ExecutionContext _context;
    private bool _enabled = true;
    private int _hitCount;
    private int _id;
    private string _script;
    private Breakpoint.BreakpointType _type;
    private static int _lastID;

    public ScriptBlock Action => this._action;

    internal ExecutionContext Context => this._context;

    public bool Enabled => this.EnabledInternal;

    internal bool EnabledInternal
    {
      get => this._enabled;
      set => this._enabled = value;
    }

    public int HitCount => this._hitCount;

    public int Id => this._id;

    internal bool IsScriptBreakpoint => this._script != null;

    public string Script => this._script;

    internal Breakpoint.BreakpointType Type => this._type;

    internal Breakpoint(
      ExecutionContext context,
      string script,
      ScriptBlock action,
      Breakpoint.BreakpointType type)
    {
      this._context = context;
      this._script = script;
      this._id = Breakpoint._lastID++;
      this._type = type;
      this._action = action;
      this._hitCount = 0;
    }

    internal Breakpoint.BreakpointAction Trigger()
    {
      ++this._hitCount;
      if (this.Action == null)
        return Breakpoint.BreakpointAction.Break;
      try
      {
        this.Action.Invoke();
      }
      catch (BreakException ex)
      {
        return Breakpoint.BreakpointAction.Break;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      return Breakpoint.BreakpointAction.Continue;
    }

    internal enum BreakpointType
    {
      Line,
      Variable,
      Command,
      Statement,
    }

    internal enum BreakpointAction
    {
      Continue,
      Break,
    }
  }
}
