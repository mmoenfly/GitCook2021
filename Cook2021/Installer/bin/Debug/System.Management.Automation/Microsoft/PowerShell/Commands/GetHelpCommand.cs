// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.GetHelpCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Get", "Help", DefaultParameterSetName = "AllUsersView")]
  public sealed class GetHelpCommand : PSCmdlet
  {
    internal const string resBaseName = "HelpErrors";
    private string _name = "";
    private string _path;
    private string[] _category;
    private string[] _component;
    private string[] _functionality;
    private string[] _role;
    private string _provider = "";
    private string _parameter;
    private bool showOnlineHelp;
    private GetHelpCommand.HelpView _viewTokenToAdd;
    [TraceSource("GetHelpCommand ", "GetHelpCommand ")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("GetHelpCommand ", "GetHelpCommand ");

    public GetHelpCommand()
    {
      using (GetHelpCommand.tracer.TraceConstructor((object) this))
        ;
    }

    [System.Management.Automation.Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
    public string Name
    {
      get
      {
        using (GetHelpCommand.tracer.TraceProperty())
          return this._name;
      }
      set
      {
        using (GetHelpCommand.tracer.TraceProperty())
          this._name = value;
      }
    }

    [System.Management.Automation.Parameter]
    public string Path
    {
      get
      {
        using (GetHelpCommand.tracer.TraceProperty())
          return this._path;
      }
      set
      {
        using (GetHelpCommand.tracer.TraceProperty())
          this._path = value;
      }
    }

    [System.Management.Automation.Parameter]
    public string[] Category
    {
      get
      {
        using (GetHelpCommand.tracer.TraceProperty())
          return this._category;
      }
      set
      {
        using (GetHelpCommand.tracer.TraceProperty())
          this._category = value;
      }
    }

    [System.Management.Automation.Parameter]
    public string[] Component
    {
      get
      {
        using (GetHelpCommand.tracer.TraceProperty())
          return this._component;
      }
      set
      {
        using (GetHelpCommand.tracer.TraceProperty())
          this._component = value;
      }
    }

    [System.Management.Automation.Parameter]
    public string[] Functionality
    {
      get
      {
        using (GetHelpCommand.tracer.TraceProperty())
          return this._functionality;
      }
      set
      {
        using (GetHelpCommand.tracer.TraceProperty())
          this._functionality = value;
      }
    }

    [System.Management.Automation.Parameter]
    public string[] Role
    {
      get
      {
        using (GetHelpCommand.tracer.TraceProperty())
          return this._role;
      }
      set
      {
        using (GetHelpCommand.tracer.TraceProperty())
          this._role = value;
      }
    }

    [System.Management.Automation.Parameter(ParameterSetName = "DetailedView")]
    public SwitchParameter Detailed
    {
      set
      {
        using (GetHelpCommand.tracer.TraceProperty("set_Detailed", new object[0]))
        {
          if (!value.ToBool())
            return;
          this._viewTokenToAdd = GetHelpCommand.HelpView.DetailedView;
        }
      }
    }

    [System.Management.Automation.Parameter(ParameterSetName = "AllUsersView")]
    public SwitchParameter Full
    {
      set
      {
        using (GetHelpCommand.tracer.TraceProperty("set_Full", new object[0]))
        {
          if (!value.ToBool())
            return;
          this._viewTokenToAdd = GetHelpCommand.HelpView.FullView;
        }
      }
    }

    [System.Management.Automation.Parameter(ParameterSetName = "Examples")]
    public SwitchParameter Examples
    {
      set
      {
        using (GetHelpCommand.tracer.TraceProperty("set_Examples", new object[0]))
        {
          if (!value.ToBool())
            return;
          this._viewTokenToAdd = GetHelpCommand.HelpView.ExamplesView;
        }
      }
    }

    [System.Management.Automation.Parameter(ParameterSetName = "Parameters")]
    public string Parameter
    {
      set
      {
        using (GetHelpCommand.tracer.TraceProperty("set_Parameters", new object[0]))
          this._parameter = value;
      }
      get
      {
        using (GetHelpCommand.tracer.TraceProperty("get_Parameters", new object[0]))
          return this._parameter;
      }
    }

    [System.Management.Automation.Parameter]
    public SwitchParameter Online
    {
      set
      {
        using (GetHelpCommand.tracer.TraceProperty())
        {
          this.showOnlineHelp = (bool) value;
          if (!this.showOnlineHelp)
            return;
          GetHelpCommand.VerifyParameterForbiddenInRemoteRunspace((Cmdlet) this, nameof (Online));
        }
      }
      get
      {
        using (GetHelpCommand.tracer.TraceProperty())
          return (SwitchParameter) this.showOnlineHelp;
      }
    }

    protected override void ProcessRecord()
    {
      using (GetHelpCommand.tracer.TraceMethod())
      {
        bool failed = false;
        HelpCategory helpCategory = this.ToHelpCategory(this._category, ref failed);
        if (failed)
          return;
        this.ValidateAndThrowIfError(helpCategory);
        HelpRequest helpRequest = new HelpRequest(this.Name, helpCategory);
        helpRequest.Provider = this._provider;
        helpRequest.Component = this._component;
        helpRequest.Role = this._role;
        helpRequest.Functionality = this._functionality;
        helpRequest.ProviderContext = new ProviderContext(this.Path, this.Context.Engine.Context, this.SessionState.Path);
        helpRequest.CommandOrigin = this.MyInvocation.CommandOrigin;
        IEnumerable<HelpInfo> help = this.Context.HelpSystem.GetHelp(helpRequest);
        HelpInfo helpInfo1 = (HelpInfo) null;
        int num = 0;
        foreach (HelpInfo helpInfo2 in help)
        {
          if (this.IsStopping)
            return;
          if (num == 0)
          {
            helpInfo1 = helpInfo2;
          }
          else
          {
            if (helpInfo1 != null)
            {
              this.WriteObjectsOrShowOnlineHelp(helpInfo1, false);
              helpInfo1 = (HelpInfo) null;
            }
            this.WriteObjectsOrShowOnlineHelp(helpInfo2, false);
          }
          ++num;
        }
        if (1 == num)
          this.WriteObjectsOrShowOnlineHelp(helpInfo1, true);
        else if (this.showOnlineHelp && num > 1)
          throw GetHelpCommand.tracer.NewInvalidOperationException("HelpErrors", "MultipleOnlineTopicsNotSupported", (object) "Online");
        if ((num != 0 || WildcardPattern.ContainsWildcardCharacters(helpRequest.Target)) && !this.Context.HelpSystem.VerboseHelpErrors || this.Context.HelpSystem.LastErrors.Count <= 0)
          return;
        foreach (ErrorRecord lastError in this.Context.HelpSystem.LastErrors)
          this.WriteError(lastError);
      }
    }

    private HelpCategory ToHelpCategory(string[] category, ref bool failed)
    {
      using (GetHelpCommand.tracer.TraceMethod())
      {
        if (category == null || category.Length == 0)
          return HelpCategory.None;
        HelpCategory helpCategory1 = HelpCategory.None;
        failed = false;
        for (int index = 0; index < category.Length; ++index)
        {
          try
          {
            HelpCategory helpCategory2 = (HelpCategory) Enum.Parse(typeof (HelpCategory), category[index], true);
            helpCategory1 |= helpCategory2;
          }
          catch (ArgumentException ex)
          {
            this.WriteError(new ErrorRecord((Exception) new HelpCategoryInvalidException(category[index], (Exception) ex), "InvalidHelpCategory", ErrorCategory.InvalidArgument, (object) null));
            failed = true;
          }
        }
        return helpCategory1;
      }
    }

    private PSObject TransformView(PSObject originalHelpObject)
    {
      using (GetHelpCommand.tracer.TraceMethod(nameof (TransformView), new object[0]))
      {
        if (this._viewTokenToAdd == GetHelpCommand.HelpView.Default)
        {
          GetHelpCommand.tracer.WriteLine("Detailed, Full, Examples are not selected. Constructing default view.", new object[0]);
          return originalHelpObject;
        }
        string str1 = this._viewTokenToAdd.ToString();
        PSObject psObject = originalHelpObject.Copy();
        psObject.TypeNames.Clear();
        if (originalHelpObject.TypeNames.Count == 0)
        {
          string str2 = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "HelpInfo#{0}", (object) str1);
          psObject.TypeNames.Add(str2);
        }
        else
        {
          foreach (string typeName in originalHelpObject.TypeNames)
          {
            if (!typeName.ToLower(CultureInfo.InvariantCulture).Equals("system.string") && !typeName.ToLower(CultureInfo.InvariantCulture).Equals("system.object"))
            {
              string str2 = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}#{1}", (object) typeName, (object) str1);
              GetHelpCommand.tracer.WriteLine("Adding type {0}", (object) str2);
              psObject.TypeNames.Add(str2);
            }
          }
          foreach (string typeName in originalHelpObject.TypeNames)
          {
            GetHelpCommand.tracer.WriteLine("Adding type {0}", (object) typeName);
            psObject.TypeNames.Add(typeName);
          }
        }
        return psObject;
      }
    }

    private void GetAndWriteParameterInfo(HelpInfo helpInfo)
    {
      using (GetHelpCommand.tracer.TraceMethod(nameof (GetAndWriteParameterInfo), new object[0]))
      {
        GetHelpCommand.tracer.WriteLine("Searching parameters for {0}", (object) helpInfo.Name);
        PSObject[] parameter = helpInfo.GetParameter(this._parameter);
        if (parameter == null || parameter.Length == 0)
        {
          this.WriteError(new ErrorRecord((Exception) GetHelpCommand.tracer.NewArgumentException("Parameter", "HelpErrors", "NoParmsFound", (object) this._parameter), "NoParmsFound", ErrorCategory.InvalidArgument, (object) helpInfo));
        }
        else
        {
          foreach (object sendToPipeline in parameter)
            this.WriteObject(sendToPipeline);
        }
      }
    }

    private void ValidateAndThrowIfError(HelpCategory cat)
    {
      using (GetHelpCommand.tracer.TraceMethod(nameof (ValidateAndThrowIfError), new object[0]))
      {
        if (cat == HelpCategory.None)
          return;
        HelpCategory helpCategory = HelpCategory.Alias | HelpCategory.Cmdlet | HelpCategory.ScriptCommand | HelpCategory.Function | HelpCategory.Filter | HelpCategory.ExternalScript;
        if ((cat & helpCategory) != HelpCategory.None)
          return;
        if (!string.IsNullOrEmpty(this._parameter))
          throw GetHelpCommand.tracer.NewArgumentException("Parameter", "HelpErrors", "ParamNotSupported", (object) "-Parameter");
        if (this._component != null)
          throw GetHelpCommand.tracer.NewArgumentException("Component", "HelpErrors", "ParamNotSupported", (object) "-Component");
        if (this._role != null)
          throw GetHelpCommand.tracer.NewArgumentException("Role", "HelpErrors", "ParamNotSupported", (object) "-Role");
        if (this._functionality != null)
          throw GetHelpCommand.tracer.NewArgumentException("Functionality", "HelpErrors", "ParamNotSupported", (object) "-Functionality");
      }
    }

    private void WriteObjectsOrShowOnlineHelp(HelpInfo helpInfo, bool showFullHelp)
    {
      if (helpInfo == null)
        return;
      if (showFullHelp && this.showOnlineHelp)
      {
        bool flag = false;
        GetHelpCommand.tracer.WriteLine("Preparing to show help online.", new object[0]);
        Uri uriForOnlineHelp = helpInfo.GetUriForOnlineHelp();
        if ((Uri) null != uriForOnlineHelp)
          this.LaunchOnlineHelp(uriForOnlineHelp);
        else if (!flag)
          throw GetHelpCommand.tracer.NewInvalidOperationException("HelpErrors", "NoURIFound");
      }
      else if (showFullHelp)
      {
        if (!string.IsNullOrEmpty(this._parameter))
          this.GetAndWriteParameterInfo(helpInfo);
        else
          this.WriteObject((object) this.TransformView(helpInfo.FullHelp));
      }
      else
      {
        if (!string.IsNullOrEmpty(this._parameter))
        {
          PSObject[] parameter = helpInfo.GetParameter(this._parameter);
          if (parameter == null || parameter.Length == 0)
            return;
        }
        this.WriteObject((object) helpInfo.ShortHelp);
      }
    }

    private void LaunchOnlineHelp(Uri uriToLaunch)
    {
      if (!uriToLaunch.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && !uriToLaunch.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        throw GetHelpCommand.tracer.NewInvalidOperationException("HelpErrors", "ProtocolNotSupported", (object) uriToLaunch.ToString(), (object) "http", (object) "https");
      Exception innerException = (Exception) null;
      try
      {
        new Process()
        {
          StartInfo = {
            UseShellExecute = true,
            FileName = uriToLaunch.OriginalString
          }
        }.Start();
      }
      catch (InvalidOperationException ex)
      {
        innerException = (Exception) ex;
      }
      catch (Win32Exception ex)
      {
        innerException = (Exception) ex;
      }
      if (innerException != null)
        throw GetHelpCommand.tracer.NewInvalidOperationException(innerException, "HelpErrors", "CannotLaunchURI", (object) uriToLaunch.OriginalString);
    }

    internal static void VerifyParameterForbiddenInRemoteRunspace(
      Cmdlet cmdlet,
      string parameterName)
    {
      if (!NativeCommandProcessor.IsServerSide)
        return;
      ErrorRecord errorRecord = new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString(typeof (PSObject).Assembly, "CommandBaseStrings", "ParameterNotValidInRemoteRunspace", (object) cmdlet.MyInvocation.InvocationName, (object) parameterName)), "ParameterNotValidInRemoteRunspace", ErrorCategory.InvalidArgument, (object) null);
      cmdlet.ThrowTerminatingError(errorRecord);
    }

    internal enum HelpView
    {
      Default,
      DetailedView,
      FullView,
      ExamplesView,
    }
  }
}
