// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptInvocationContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class ScriptInvocationContext
  {
    private static readonly object[] _emptyArray = new object[0];
    private bool _createScope;
    private object _dollarThis;
    private object _dollarBar;
    private object _dollarInput;
    private object[] _args;
    private Dictionary<string, PSVariable> _backupVariables = new Dictionary<string, PSVariable>();
    private CommandLineParameters _boundParameters = new CommandLineParameters();

    internal ScriptInvocationContext(
      bool createScope,
      object dollarThis,
      object dollarBar,
      object dollarInput,
      object[] args)
    {
      this._createScope = createScope;
      this._dollarThis = dollarThis;
      this._dollarBar = dollarBar;
      this._dollarInput = dollarInput != null ? dollarInput : (object) ScriptInvocationContext._emptyArray;
      this._args = args != null ? args : ScriptInvocationContext._emptyArray;
    }

    internal bool CreateScope => this._createScope;

    internal object DollarThis => this._dollarThis;

    internal object DollarBar => this._dollarBar;

    internal object DollarInput => this._dollarInput;

    internal object[] Args => this._args;

    internal Dictionary<string, PSVariable> BackupVariables => this._backupVariables;

    internal CommandLineParameters BoundParameters => this._boundParameters;
  }
}
