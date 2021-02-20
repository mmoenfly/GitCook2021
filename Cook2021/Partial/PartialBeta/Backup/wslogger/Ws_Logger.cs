// Decompiled with JetBrains decompiler
// Type: Partial.wslogger.Ws_Logger
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
using System.Web.Services.Description;
using System.Web.Services.Protocols;

namespace Partial.wslogger
{
  [WebServiceBinding(Name = "Ws_LoggerSoap", Namespace = "http://installs.ccisupportsite.com/wslogger/")]
  [DebuggerStepThrough]
  [DesignerCategory("code")]
  [GeneratedCode("System.Web.Services", "2.0.50727.5420")]
  public class Ws_Logger : SoapHttpClientProtocol
  {
    private SendOrPostCallback LoggerOperationCompleted;
    private SendOrPostCallback ChkInstallOperationCompleted;
    private SendOrPostCallback UpdInstallOperationCompleted;
    private SendOrPostCallback FindNextIdOperationCompleted;
    private SendOrPostCallback TestdOperationCompleted;
    private SendOrPostCallback RndCustXmlOperationCompleted;
    private bool useDefaultCredentialsSetExplicitly;

    public Ws_Logger()
    {
      this.Url = Settings.Default.Partial_wslogger_WS_Logger;
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

    public event LoggerCompletedEventHandler LoggerCompleted;

    public event ChkInstallCompletedEventHandler ChkInstallCompleted;

    public event UpdInstallCompletedEventHandler UpdInstallCompleted;

    public event FindNextIdCompletedEventHandler FindNextIdCompleted;

    public event TestdCompletedEventHandler TestdCompleted;

    public event RndCustXmlCompletedEventHandler RndCustXmlCompleted;

    [SoapDocumentMethod("http://installs.ccisupportsite.com/wslogger/Logger", ParameterStyle = SoapParameterStyle.Wrapped, RequestNamespace = "http://installs.ccisupportsite.com/wslogger/", ResponseNamespace = "http://installs.ccisupportsite.com/wslogger/", Use = SoapBindingUse.Literal)]
    public string Logger(string sUnid, string sMessage, int iStatus, int iBatchid) => (string) this.Invoke(nameof (Logger), new object[4]
    {
      (object) sUnid,
      (object) sMessage,
      (object) iStatus,
      (object) iBatchid
    })[0];

    public void LoggerAsync(string sUnid, string sMessage, int iStatus, int iBatchid) => this.LoggerAsync(sUnid, sMessage, iStatus, iBatchid, (object) null);

    public void LoggerAsync(
      string sUnid,
      string sMessage,
      int iStatus,
      int iBatchid,
      object userState)
    {
      if (this.LoggerOperationCompleted == null)
        this.LoggerOperationCompleted = new SendOrPostCallback(this.OnLoggerOperationCompleted);
      this.InvokeAsync("Logger", new object[4]
      {
        (object) sUnid,
        (object) sMessage,
        (object) iStatus,
        (object) iBatchid
      }, this.LoggerOperationCompleted, userState);
    }

    private void OnLoggerOperationCompleted(object arg)
    {
      if (this.LoggerCompleted == null)
        return;
      InvokeCompletedEventArgs completedEventArgs = (InvokeCompletedEventArgs) arg;
      this.LoggerCompleted((object) this, new LoggerCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error, completedEventArgs.Cancelled, completedEventArgs.UserState));
    }

    [SoapDocumentMethod("http://installs.ccisupportsite.com/wslogger/ChkInstall", ParameterStyle = SoapParameterStyle.Wrapped, RequestNamespace = "http://installs.ccisupportsite.com/wslogger/", ResponseNamespace = "http://installs.ccisupportsite.com/wslogger/", Use = SoapBindingUse.Literal)]
    public string ChkInstall(string sUnid) => (string) this.Invoke(nameof (ChkInstall), new object[1]
    {
      (object) sUnid
    })[0];

    public void ChkInstallAsync(string sUnid) => this.ChkInstallAsync(sUnid, (object) null);

    public void ChkInstallAsync(string sUnid, object userState)
    {
      if (this.ChkInstallOperationCompleted == null)
        this.ChkInstallOperationCompleted = new SendOrPostCallback(this.OnChkInstallOperationCompleted);
      this.InvokeAsync("ChkInstall", new object[1]
      {
        (object) sUnid
      }, this.ChkInstallOperationCompleted, userState);
    }

    private void OnChkInstallOperationCompleted(object arg)
    {
      if (this.ChkInstallCompleted == null)
        return;
      InvokeCompletedEventArgs completedEventArgs = (InvokeCompletedEventArgs) arg;
      this.ChkInstallCompleted((object) this, new ChkInstallCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error, completedEventArgs.Cancelled, completedEventArgs.UserState));
    }

    [SoapDocumentMethod("http://installs.ccisupportsite.com/wslogger/UpdInstall", ParameterStyle = SoapParameterStyle.Wrapped, RequestNamespace = "http://installs.ccisupportsite.com/wslogger/", ResponseNamespace = "http://installs.ccisupportsite.com/wslogger/", Use = SoapBindingUse.Literal)]
    public void UpdInstall(string sUnid, string val) => this.Invoke(nameof (UpdInstall), new object[2]
    {
      (object) sUnid,
      (object) val
    });

    public void UpdInstallAsync(string sUnid, string val) => this.UpdInstallAsync(sUnid, val, (object) null);

    public void UpdInstallAsync(string sUnid, string val, object userState)
    {
      if (this.UpdInstallOperationCompleted == null)
        this.UpdInstallOperationCompleted = new SendOrPostCallback(this.OnUpdInstallOperationCompleted);
      this.InvokeAsync("UpdInstall", new object[2]
      {
        (object) sUnid,
        (object) val
      }, this.UpdInstallOperationCompleted, userState);
    }

    private void OnUpdInstallOperationCompleted(object arg)
    {
      if (this.UpdInstallCompleted == null)
        return;
      InvokeCompletedEventArgs completedEventArgs = (InvokeCompletedEventArgs) arg;
      this.UpdInstallCompleted((object) this, new AsyncCompletedEventArgs(completedEventArgs.Error, completedEventArgs.Cancelled, completedEventArgs.UserState));
    }

    [SoapDocumentMethod("http://installs.ccisupportsite.com/wslogger/FindNextId", ParameterStyle = SoapParameterStyle.Wrapped, RequestNamespace = "http://installs.ccisupportsite.com/wslogger/", ResponseNamespace = "http://installs.ccisupportsite.com/wslogger/", Use = SoapBindingUse.Literal)]
    public string FindNextId(string sUnid) => (string) this.Invoke(nameof (FindNextId), new object[1]
    {
      (object) sUnid
    })[0];

    public void FindNextIdAsync(string sUnid) => this.FindNextIdAsync(sUnid, (object) null);

    public void FindNextIdAsync(string sUnid, object userState)
    {
      if (this.FindNextIdOperationCompleted == null)
        this.FindNextIdOperationCompleted = new SendOrPostCallback(this.OnFindNextIdOperationCompleted);
      this.InvokeAsync("FindNextId", new object[1]
      {
        (object) sUnid
      }, this.FindNextIdOperationCompleted, userState);
    }

    private void OnFindNextIdOperationCompleted(object arg)
    {
      if (this.FindNextIdCompleted == null)
        return;
      InvokeCompletedEventArgs completedEventArgs = (InvokeCompletedEventArgs) arg;
      this.FindNextIdCompleted((object) this, new FindNextIdCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error, completedEventArgs.Cancelled, completedEventArgs.UserState));
    }

    [SoapDocumentMethod("http://installs.ccisupportsite.com/wslogger/Testd", ParameterStyle = SoapParameterStyle.Wrapped, RequestNamespace = "http://installs.ccisupportsite.com/wslogger/", ResponseNamespace = "http://installs.ccisupportsite.com/wslogger/", Use = SoapBindingUse.Literal)]
    public MyTest Testd(string sName, string sList) => (MyTest) this.Invoke(nameof (Testd), new object[2]
    {
      (object) sName,
      (object) sList
    })[0];

    public void TestdAsync(string sName, string sList) => this.TestdAsync(sName, sList, (object) null);

    public void TestdAsync(string sName, string sList, object userState)
    {
      if (this.TestdOperationCompleted == null)
        this.TestdOperationCompleted = new SendOrPostCallback(this.OnTestdOperationCompleted);
      this.InvokeAsync("Testd", new object[2]
      {
        (object) sName,
        (object) sList
      }, this.TestdOperationCompleted, userState);
    }

    private void OnTestdOperationCompleted(object arg)
    {
      if (this.TestdCompleted == null)
        return;
      InvokeCompletedEventArgs completedEventArgs = (InvokeCompletedEventArgs) arg;
      this.TestdCompleted((object) this, new TestdCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error, completedEventArgs.Cancelled, completedEventArgs.UserState));
    }

    [SoapDocumentMethod("http://installs.ccisupportsite.com/wslogger/RndCustXml", ParameterStyle = SoapParameterStyle.Wrapped, RequestNamespace = "http://installs.ccisupportsite.com/wslogger/", ResponseNamespace = "http://installs.ccisupportsite.com/wslogger/", Use = SoapBindingUse.Literal)]
    public string RndCustXml(string sSourceXml, string sUnid) => (string) this.Invoke(nameof (RndCustXml), new object[2]
    {
      (object) sSourceXml,
      (object) sUnid
    })[0];

    public void RndCustXmlAsync(string sSourceXml, string sUnid) => this.RndCustXmlAsync(sSourceXml, sUnid, (object) null);

    public void RndCustXmlAsync(string sSourceXml, string sUnid, object userState)
    {
      if (this.RndCustXmlOperationCompleted == null)
        this.RndCustXmlOperationCompleted = new SendOrPostCallback(this.OnRndCustXmlOperationCompleted);
      this.InvokeAsync("RndCustXml", new object[2]
      {
        (object) sSourceXml,
        (object) sUnid
      }, this.RndCustXmlOperationCompleted, userState);
    }

    private void OnRndCustXmlOperationCompleted(object arg)
    {
      if (this.RndCustXmlCompleted == null)
        return;
      InvokeCompletedEventArgs completedEventArgs = (InvokeCompletedEventArgs) arg;
      this.RndCustXmlCompleted((object) this, new RndCustXmlCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error, completedEventArgs.Cancelled, completedEventArgs.UserState));
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
