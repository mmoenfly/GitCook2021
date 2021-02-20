// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.RemoveModuleCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Remove", "Module", SupportsShouldProcess = true)]
  public sealed class RemoveModuleCommand : ModuleCmdletBase
  {
    [TraceSource("Parser", "Parser")]
    internal new static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");
    private string[] _name = new string[0];
    private PSModuleInfo[] _moduleInfo = new PSModuleInfo[0];
    private SwitchParameter _force;
    private int _numberRemoved;

    [Parameter(Mandatory = true, ParameterSetName = "name", Position = 0, ValueFromPipeline = true)]
    public string[] Name
    {
      set => this._name = value;
      get => this._name;
    }

    [Parameter(Mandatory = true, ParameterSetName = "ModuleInfo", Position = 0, ValueFromPipeline = true)]
    public PSModuleInfo[] ModuleInfo
    {
      set => this._moduleInfo = value;
      get => this._moduleInfo;
    }

    [Parameter]
    public SwitchParameter Force
    {
      get => this._force;
      set => this._force = value;
    }

    protected override void ProcessRecord()
    {
      List<PSModuleInfo> modules = this.Context.Modules.GetModules(this._name, false);
      modules.AddRange((IEnumerable<PSModuleInfo>) this._moduleInfo);
      List<PSModuleInfo> psModuleInfoList1 = new List<PSModuleInfo>();
      foreach (PSModuleInfo module in modules)
      {
        if (module.NestedModules != null && module.NestedModules.Count > 0)
          psModuleInfoList1.AddRange((IEnumerable<PSModuleInfo>) this.GetAllNestedModules(module));
      }
      if (psModuleInfoList1.Count > 0)
        modules.AddRange((IEnumerable<PSModuleInfo>) psModuleInfoList1);
      Dictionary<PSModuleInfo, PSModuleInfo> dictionary = new Dictionary<PSModuleInfo, PSModuleInfo>();
      foreach (PSModuleInfo key in modules)
      {
        if (key.AccessMode == ModuleAccessMode.Constant)
          this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "ModuleIsConstant", (object) key.Name)), "Modules_ModuleIsConstant", ErrorCategory.PermissionDenied, (object) key));
        else if (key.AccessMode == ModuleAccessMode.ReadOnly && !(bool) this._force)
          this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "ModuleIsReadOnly", (object) key.Name)), "Modules_ModuleIsReadOnly", ErrorCategory.PermissionDenied, (object) key));
        else if (this.ShouldProcess(ResourceManagerCache.FormatResourceString("Modules", "ConfirmRemoveModule", (object) key.Name, (object) key.Path)))
          dictionary[key] = key;
      }
      Dictionary<PSModuleInfo, List<PSModuleInfo>> requiredDependencies = this.GetRequiredDependencies();
      foreach (PSModuleInfo key in dictionary.Keys)
      {
        if (!(bool) this._force)
        {
          List<PSModuleInfo> psModuleInfoList2 = (List<PSModuleInfo>) null;
          if (requiredDependencies.TryGetValue(key, out psModuleInfoList2))
          {
            for (int index = psModuleInfoList2.Count - 1; index >= 0; --index)
            {
              if (dictionary.ContainsKey(psModuleInfoList2[index]))
                psModuleInfoList2.RemoveAt(index);
            }
            if (psModuleInfoList2.Count > 0)
            {
              this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "ModuleIsRequired", (object) key.Name, (object) psModuleInfoList2[0].Name)), "Modules_ModuleIsRequired", ErrorCategory.PermissionDenied, (object) key));
              continue;
            }
          }
        }
        ++this._numberRemoved;
        this.RemoveModule(key);
      }
    }

    private List<PSModuleInfo> GetAllNestedModules(PSModuleInfo module)
    {
      List<PSModuleInfo> psModuleInfoList = new List<PSModuleInfo>();
      if (module.NestedModules != null && module.NestedModules.Count > 0)
      {
        psModuleInfoList.AddRange((IEnumerable<PSModuleInfo>) module.NestedModules);
        foreach (PSModuleInfo nestedModule in module.NestedModules)
          psModuleInfoList.AddRange((IEnumerable<PSModuleInfo>) this.GetAllNestedModules(nestedModule));
      }
      return psModuleInfoList;
    }

    private Dictionary<PSModuleInfo, List<PSModuleInfo>> GetRequiredDependencies()
    {
      Dictionary<PSModuleInfo, List<PSModuleInfo>> dictionary = new Dictionary<PSModuleInfo, List<PSModuleInfo>>();
      ModuleIntrinsics modules = this.Context.Modules;
      string[] patterns = new string[1]{ "*" };
      foreach (PSModuleInfo module in modules.GetModules(patterns, false))
      {
        foreach (PSModuleInfo requiredModule in module.RequiredModules)
        {
          List<PSModuleInfo> psModuleInfoList = (List<PSModuleInfo>) null;
          if (!dictionary.TryGetValue(requiredModule, out psModuleInfoList))
            dictionary.Add(requiredModule, psModuleInfoList = new List<PSModuleInfo>());
          psModuleInfoList.Add(module);
        }
      }
      return dictionary;
    }

    protected override void EndProcessing()
    {
      if (this._numberRemoved != 0 || this.MyInvocation.BoundParameters.ContainsKey("WhatIf"))
        return;
      bool flag = true;
      foreach (string pattern in this._name)
      {
        if (!WildcardPattern.ContainsWildcardCharacters(pattern))
          flag = false;
      }
      if (flag && this._moduleInfo.Length == 0)
        return;
      this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "NoModulesRemoved")), "Modules_NoModulesRemoved", ErrorCategory.ResourceUnavailable, (object) null));
    }
  }
}
