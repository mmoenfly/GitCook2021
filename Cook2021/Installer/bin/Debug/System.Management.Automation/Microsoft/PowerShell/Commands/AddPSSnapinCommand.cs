// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.AddPSSnapinCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Add", "PSSnapin")]
  public sealed class AddPSSnapinCommand : PSSnapInCommandBase
  {
    private string[] _pssnapins;
    private bool _passThru;

    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string[] Name
    {
      get => this._pssnapins;
      set => this._pssnapins = value;
    }

    [Parameter]
    public SwitchParameter PassThru
    {
      get => (SwitchParameter) this._passThru;
      set => this._passThru = (bool) value;
    }

    protected override void ProcessRecord()
    {
      Collection<PSSnapInInfo> searchList = (Collection<PSSnapInInfo>) null;
      foreach (string pssnapin in this._pssnapins)
      {
        Exception innerException = (Exception) null;
        Collection<string> snapInList = new Collection<string>();
        try
        {
          if (WildcardPattern.ContainsWildcardCharacters(pssnapin))
          {
            if (searchList == null)
              searchList = PSSnapInReader.ReadAll(PSVersionInfo.RegistryVersionKey);
            snapInList = this.SearchListForPattern(searchList, pssnapin);
            if (snapInList.Count == 0)
            {
              if (this._passThru)
              {
                this.WriteNonTerminatingError((object) pssnapin, "NoPSSnapInsFound", (Exception) PSSnapInCommandBase.tracer.NewArgumentException(pssnapin, "MshSnapInCmdletResources", "NoPSSnapInsFound", (object) pssnapin), ErrorCategory.InvalidArgument);
                continue;
              }
              continue;
            }
          }
          else
            snapInList.Add(pssnapin);
          this.AddPSSnapIns(snapInList);
        }
        catch (PSArgumentException ex)
        {
          innerException = (Exception) ex;
        }
        catch (SecurityException ex)
        {
          innerException = (Exception) ex;
        }
        if (innerException != null)
          this.WriteNonTerminatingError((object) pssnapin, "AddPSSnapInRead", innerException, ErrorCategory.InvalidArgument);
      }
    }

    private void AddPSSnapIns(Collection<string> snapInList)
    {
      if (snapInList == null)
        return;
      if (this.Context.RunspaceConfiguration == null)
      {
        Collection<PSSnapInInfo> snapIns = this.GetSnapIns((string) null);
        InitialSessionState initialSessionState = InitialSessionState.Create();
        bool flag = false;
        foreach (string snapIn in snapInList)
        {
          try
          {
            PSSnapInInfo psSnapInInfo1 = PSSnapInReader.Read(Utils.GetCurrentMajorVersion(), snapIn);
            PSSnapInInfo psSnapInInfo2 = PSSnapInCommandBase.IsSnapInLoaded(snapIns, psSnapInInfo1);
            if (psSnapInInfo2 == null)
            {
              psSnapInInfo2 = initialSessionState.ImportPSSnapIn(snapIn, out PSSnapInException _);
              flag = true;
            }
            if (this._passThru)
            {
              psSnapInInfo2.LoadIndirectResources(this.ResourceReader);
              this.WriteObject((object) psSnapInInfo2);
            }
          }
          catch (PSSnapInException ex)
          {
            this.WriteNonTerminatingError((object) snapIn, "AddPSSnapInRead", (Exception) ex, ErrorCategory.InvalidData);
          }
        }
        if (!flag)
          return;
        initialSessionState.Bind(this.Context, true);
      }
      else
      {
        foreach (string snapIn in snapInList)
        {
          Exception innerException = (Exception) null;
          try
          {
            PSSnapInException warning = (PSSnapInException) null;
            PSSnapInInfo psSnapInInfo = this.Runspace.AddPSSnapIn(snapIn, out warning);
            if (warning != null)
              this.WriteNonTerminatingError((object) snapIn, "AddPSSnapInRead", (Exception) warning, ErrorCategory.InvalidData);
            if (this._passThru)
            {
              psSnapInInfo.LoadIndirectResources(this.ResourceReader);
              this.WriteObject((object) psSnapInInfo);
            }
          }
          catch (PSArgumentException ex)
          {
            innerException = (Exception) ex;
          }
          catch (PSSnapInException ex)
          {
            innerException = (Exception) ex;
          }
          catch (SecurityException ex)
          {
            innerException = (Exception) ex;
          }
          if (innerException != null)
            this.WriteNonTerminatingError((object) snapIn, "AddPSSnapInRead", innerException, ErrorCategory.InvalidArgument);
        }
      }
    }
  }
}
