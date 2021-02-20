// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.LogContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class LogContext
  {
    private string _severity = "";
    private string _hostName = "";
    private string _hostVersion = "";
    private string _hostId = "";
    private string _engineVersion = "";
    private string _runspaceId = "";
    private string _pipelineId = "";
    private string _commandName = "";
    private string _commandType = "";
    private string _scriptName = "";
    private string _commandPath = "";
    private string _commandLine = "";
    private string _sequenceNumber = "";
    private string _user = "";
    private string _time = "";
    private string _shellId;

    internal string Severity
    {
      get => this._severity;
      set => this._severity = value;
    }

    internal string HostName
    {
      get => this._hostName;
      set => this._hostName = value;
    }

    internal string HostVersion
    {
      get => this._hostVersion;
      set => this._hostVersion = value;
    }

    internal string HostId
    {
      get => this._hostId;
      set => this._hostId = value;
    }

    internal string EngineVersion
    {
      get => this._engineVersion;
      set => this._engineVersion = value;
    }

    internal string RunspaceId
    {
      get => this._runspaceId;
      set => this._runspaceId = value;
    }

    internal string PipelineId
    {
      get => this._pipelineId;
      set => this._pipelineId = value;
    }

    internal string CommandName
    {
      get => this._commandName;
      set => this._commandName = value;
    }

    internal string CommandType
    {
      get => this._commandType;
      set => this._commandType = value;
    }

    internal string ScriptName
    {
      get => this._scriptName;
      set => this._scriptName = value;
    }

    internal string CommandPath
    {
      get => this._commandPath;
      set => this._commandPath = value;
    }

    internal string CommandLine
    {
      get => this._commandLine;
      set => this._commandLine = value;
    }

    internal string SequenceNumber
    {
      get => this._sequenceNumber;
      set => this._sequenceNumber = value;
    }

    internal string User
    {
      get => this._user;
      set => this._user = value;
    }

    internal string Time
    {
      get => this._time;
      set => this._time = value;
    }

    internal string ShellId
    {
      get => this._shellId;
      set => this._shellId = value;
    }
  }
}
