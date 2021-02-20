﻿// Decompiled with JetBrains decompiler
// Type: Partial.WSPartials.RemovePartialFlagsService
// Assembly: PartialBeta, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9CF783A5-38FA-472F-8C51-9A2203433095
// Assembly location: E:\git\partialbits\PartialBeta.exe

using Partial.Properties;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace Partial.WSPartials
{
  [DesignerCategory("code")]
  [GeneratedCode("System.Web.Services", "2.0.50727.5420")]
  [WebServiceBinding(Name = "DominoSoapBinding", Namespace = "urn:DefaultNamespace")]
  [DebuggerStepThrough]
  public class RemovePartialFlagsService : SoapHttpClientProtocol
  {
    private SendOrPostCallback REMOVEPARTIALFLAGSOperationCompleted;
    private bool useDefaultCredentialsSetExplicitly;

    public RemovePartialFlagsService()
    {
      this.Url = Settings.Default.Partial_WSPartials_RemovePartialFlagsService;
      if (this.IsLocalFileSystemWebService(this.Url))
      {
        this.UseDefaultCredentials = true;
        this.useDefaultCredentialsSetExplicitly = false;
      }
      else
        this.useDefaultCredentialsSetExplicitly = true;
    }

    public new string Url
    {
      get => base.Url;
      set
      {
        if (this.IsLocalFileSystemWebService(base.Url) && !this.useDefaultCredentialsSetExplicitly && !this.IsLocalFileSystemWebService(value))
          base.UseDefaultCredentials = false;
        base.Url = value;
      }
    }

    public new bool UseDefaultCredentials
    {
      get => base.UseDefaultCredentials;
      set
      {
        base.UseDefaultCredentials = value;
        this.useDefaultCredentialsSetExplicitly = true;
      }
    }

    public event REMOVEPARTIALFLAGSCompletedEventHandler REMOVEPARTIALFLAGSCompleted;

    [SoapRpcMethod("", RequestNamespace = "urn:DefaultNamespace", ResponseNamespace = "urn:DefaultNamespace")]
    [return: SoapElement("REMOVEPARTIALFLAGSReturn")]
    public string REMOVEPARTIALFLAGS(string DBNAME, string VIEWNAME, string MYUNID) => (string) this.Invoke(nameof (REMOVEPARTIALFLAGS), new object[3]
    {
      (object) DBNAME,
      (object) VIEWNAME,
      (object) MYUNID
    })[0];

    public void REMOVEPARTIALFLAGSAsync(string DBNAME, string VIEWNAME, string MYUNID) => this.REMOVEPARTIALFLAGSAsync(DBNAME, VIEWNAME, MYUNID, (object) null);

    public void REMOVEPARTIALFLAGSAsync(
      string DBNAME,
      string VIEWNAME,
      string MYUNID,
      object userState)
    {
      if (this.REMOVEPARTIALFLAGSOperationCompleted == null)
        this.REMOVEPARTIALFLAGSOperationCompleted = new SendOrPostCallback(this.OnREMOVEPARTIALFLAGSOperationCompleted);
      this.InvokeAsync("REMOVEPARTIALFLAGS", new object[3]
      {
        (object) DBNAME,
        (object) VIEWNAME,
        (object) MYUNID
      }, this.REMOVEPARTIALFLAGSOperationCompleted, userState);
    }

    private void OnREMOVEPARTIALFLAGSOperationCompleted(object arg)
    {
      if (this.REMOVEPARTIALFLAGSCompleted == null)
        return;
      InvokeCompletedEventArgs completedEventArgs = (InvokeCompletedEventArgs) arg;
      this.REMOVEPARTIALFLAGSCompleted((object) this, new REMOVEPARTIALFLAGSCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error, completedEventArgs.Cancelled, completedEventArgs.UserState));
    }

    public new void CancelAsync(object userState) => base.CancelAsync(userState);

    private bool IsLocalFileSystemWebService(string url)
    {
      if (url == null || url == string.Empty)
        return false;
      Uri uri = new Uri(url);
      return uri.Port >= 1024 && string.Compare(uri.Host, "localHost", StringComparison.OrdinalIgnoreCase) == 0;
    }
  }
}
