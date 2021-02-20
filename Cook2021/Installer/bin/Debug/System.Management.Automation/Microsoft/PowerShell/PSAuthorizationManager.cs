// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.PSAuthorizationManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;

namespace Microsoft.PowerShell
{
  public sealed class PSAuthorizationManager : AuthorizationManager
  {
    [TraceSource("PSAuthorizationManager", "tracer for PSAuthorizationManager")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PSAuthorizationManager), "tracer for PSAuthorizationManager");
    private ExecutionPolicy executionPolicy;
    private string shellId;

    public PSAuthorizationManager(string shellId)
      : base(shellId)
    {
      using (PSAuthorizationManager.tracer.TraceConstructor((object) this))
        this.shellId = !string.IsNullOrEmpty(shellId) ? shellId : throw PSAuthorizationManager.tracer.NewArgumentNullException(nameof (shellId));
    }

    private static bool IsSupportedExtension(string ext) => ext.Equals(".ps1", StringComparison.OrdinalIgnoreCase) || ext.Equals(".psm1", StringComparison.OrdinalIgnoreCase) || ext.Equals(".ps1xml", StringComparison.OrdinalIgnoreCase);

    private bool CheckPolicy(ExternalScriptInfo script, PSHost host, out Exception reason)
    {
      bool flag1 = false;
      reason = (Exception) null;
      string path = script.Path;
      if (path.IndexOf('\\') < 0)
        throw PSAuthorizationManager.tracer.NewArgumentException("path");
      FileInfo fileInfo = path.LastIndexOf('\\') != path.Length - 1 ? new FileInfo(path) : throw PSAuthorizationManager.tracer.NewArgumentException("path");
      if (!fileInfo.Exists)
        return false;
      if (!PSAuthorizationManager.IsSupportedExtension(fileInfo.Extension) || this.IsProductBinary(path))
        return true;
      this.executionPolicy = SecuritySupport.GetExecutionPolicy(this.shellId);
      if (this.executionPolicy == ExecutionPolicy.Bypass)
        return true;
      if (SecuritySupport.GetSaferPolicy(path) == SaferPolicy.Disallowed)
      {
        string message = ResourceManagerCache.FormatResourceString("Authenticode", "Reason_DisallowedBySafer", (object) path);
        reason = (Exception) new UnauthorizedAccessException(message);
        return false;
      }
      if (this.executionPolicy == ExecutionPolicy.Unrestricted)
      {
        if (!this.IsLocalFile(fileInfo.FullName))
        {
          if (string.IsNullOrEmpty(script.ScriptContents))
          {
            string message = ResourceManagerCache.FormatResourceString("Authenticode", "Reason_FileContentUnavailable", (object) path);
            reason = (Exception) new UnauthorizedAccessException(message);
            return false;
          }
          System.Management.Automation.Signature withEncodingRetry = this.GetSignatureWithEncodingRetry(path, script);
          if (withEncodingRetry.Status == SignatureStatus.Valid && this.IsTrustedPublisher(withEncodingRetry, path))
            flag1 = true;
          if (!flag1)
          {
            PSAuthorizationManager.RunPromptDecision runPromptDecision;
            do
            {
              runPromptDecision = this.RemoteFilePrompt(path, host);
              if (runPromptDecision == PSAuthorizationManager.RunPromptDecision.Suspend)
                host.EnterNestedPrompt();
            }
            while (runPromptDecision == PSAuthorizationManager.RunPromptDecision.Suspend);
            switch (runPromptDecision - 1)
            {
              case PSAuthorizationManager.RunPromptDecision.DoNotRun:
                flag1 = true;
                break;
              default:
                flag1 = false;
                string message = ResourceManagerCache.FormatResourceString("Authenticode", "Reason_DoNotRun", (object) path);
                reason = (Exception) new UnauthorizedAccessException(message);
                break;
            }
          }
        }
        else
          flag1 = true;
      }
      else if (this.IsLocalFile(fileInfo.FullName) && this.executionPolicy == ExecutionPolicy.RemoteSigned)
        flag1 = true;
      else if (this.executionPolicy == ExecutionPolicy.AllSigned || this.executionPolicy == ExecutionPolicy.RemoteSigned)
      {
        if (string.IsNullOrEmpty(script.ScriptContents))
        {
          string message = ResourceManagerCache.FormatResourceString("Authenticode", "Reason_FileContentUnavailable", (object) path);
          reason = (Exception) new UnauthorizedAccessException(message);
          return false;
        }
        System.Management.Automation.Signature withEncodingRetry = this.GetSignatureWithEncodingRetry(path, script);
        if (withEncodingRetry.Status == SignatureStatus.Valid)
        {
          flag1 = this.IsTrustedPublisher(withEncodingRetry, path) || this.SetPolicyFromAuthenticodePrompt(path, host, ref reason, withEncodingRetry);
        }
        else
        {
          flag1 = false;
          if (withEncodingRetry.Status == SignatureStatus.NotTrusted)
            reason = (Exception) new UnauthorizedAccessException(ResourceManagerCache.FormatResourceString("Authenticode", "Reason_NotTrusted", (object) path, (object) withEncodingRetry.SignerCertificate.SubjectName.Name));
          else
            reason = (Exception) new UnauthorizedAccessException(ResourceManagerCache.FormatResourceString("Authenticode", "Reason_Unknown", (object) path, (object) withEncodingRetry.StatusMessage));
        }
      }
      else
      {
        flag1 = false;
        bool flag2 = false;
        if (string.Equals(fileInfo.Extension, ".ps1xml", StringComparison.OrdinalIgnoreCase))
        {
          string[] strArray = new string[2]
          {
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
          };
          foreach (string str in strArray)
          {
            if (fileInfo.FullName.StartsWith(str, StringComparison.OrdinalIgnoreCase))
              flag1 = true;
          }
          if (!flag1)
          {
            System.Management.Automation.Signature withEncodingRetry = this.GetSignatureWithEncodingRetry(path, script);
            if (withEncodingRetry.Status == SignatureStatus.Valid)
            {
              if (this.IsTrustedPublisher(withEncodingRetry, path))
              {
                flag1 = true;
              }
              else
              {
                flag1 = this.SetPolicyFromAuthenticodePrompt(path, host, ref reason, withEncodingRetry);
                flag2 = true;
              }
            }
          }
        }
        if (!flag1 && !flag2)
          reason = (Exception) new UnauthorizedAccessException(ResourceManagerCache.FormatResourceString("Authenticode", "Reason_RestrictedMode", (object) path));
      }
      return flag1;
    }

    private bool SetPolicyFromAuthenticodePrompt(
      string path,
      PSHost host,
      ref Exception reason,
      System.Management.Automation.Signature signature)
    {
      bool flag = false;
      switch (this.AuthenticodePrompt(path, signature, host))
      {
        case PSAuthorizationManager.RunPromptDecision.NeverRun:
          this.UntrustPublisher(signature);
          string message1 = ResourceManagerCache.FormatResourceString("Authenticode", "Reason_NeverRun", (object) path);
          reason = (Exception) new UnauthorizedAccessException(message1);
          flag = false;
          break;
        case PSAuthorizationManager.RunPromptDecision.DoNotRun:
          flag = false;
          string message2 = ResourceManagerCache.FormatResourceString("Authenticode", "Reason_DoNotRun", (object) path);
          reason = (Exception) new UnauthorizedAccessException(message2);
          break;
        case PSAuthorizationManager.RunPromptDecision.RunOnce:
          flag = true;
          break;
        case PSAuthorizationManager.RunPromptDecision.AlwaysRun:
          this.TrustPublisher(signature);
          flag = true;
          break;
      }
      return flag;
    }

    private bool IsLocalFile(string filename)
    {
      Zone fromUrl = Zone.CreateFromUrl(filename);
      return fromUrl.SecurityZone == SecurityZone.MyComputer || fromUrl.SecurityZone == SecurityZone.Intranet || fromUrl.SecurityZone == SecurityZone.Trusted;
    }

    private bool IsProductBinary(string file)
    {
      if (!string.Equals(new FileInfo(file).Extension, ".ps1xml", StringComparison.OrdinalIgnoreCase))
        return false;
      string str;
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\PowerShell\\" + PSVersionInfo.RegistryVersionKey + "\\PowerShellEngine"))
        str = (string) registryKey.GetValue("ApplicationBase");
      return new FileInfo(file).FullName.ToUpper(CultureInfo.CurrentCulture).IndexOf(str.ToUpper(CultureInfo.CurrentCulture), StringComparison.CurrentCulture) == 0;
    }

    private bool IsTrustedPublisher(System.Management.Automation.Signature signature, string file)
    {
      string thumbprint = signature.SignerCertificate.Thumbprint;
      X509Store x509Store = new X509Store(StoreName.TrustedPublisher);
      x509Store.Open(OpenFlags.ReadOnly);
      foreach (X509Certificate2 certificate in x509Store.Certificates)
      {
        if (string.Equals(certificate.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase) && !this.IsUntrustedPublisher(signature, file))
          return true;
      }
      return false;
    }

    private bool IsUntrustedPublisher(System.Management.Automation.Signature signature, string file)
    {
      string thumbprint = signature.SignerCertificate.Thumbprint;
      X509Store x509Store = new X509Store(StoreName.Disallowed);
      x509Store.Open(OpenFlags.ReadOnly);
      foreach (X509Certificate2 certificate in x509Store.Certificates)
      {
        if (string.Equals(certificate.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    private void TrustPublisher(System.Management.Automation.Signature signature)
    {
      X509Certificate2 signerCertificate = signature.SignerCertificate;
      X509Store x509Store = new X509Store(StoreName.TrustedPublisher);
      try
      {
        x509Store.Open(OpenFlags.ReadWrite);
        x509Store.Add(signerCertificate);
      }
      finally
      {
        x509Store.Close();
      }
    }

    private void UntrustPublisher(System.Management.Automation.Signature signature)
    {
      X509Certificate2 signerCertificate = signature.SignerCertificate;
      X509Store x509Store1 = new X509Store(StoreName.Disallowed);
      X509Store x509Store2 = new X509Store(StoreName.TrustedPublisher);
      try
      {
        x509Store2.Open(OpenFlags.ReadWrite);
        x509Store2.Remove(signerCertificate);
      }
      finally
      {
        x509Store2.Close();
      }
      try
      {
        x509Store1.Open(OpenFlags.ReadWrite);
        x509Store1.Add(signerCertificate);
      }
      finally
      {
        x509Store1.Close();
      }
    }

    private System.Management.Automation.Signature GetSignatureWithEncodingRetry(
      string path,
      ExternalScriptInfo script)
    {
      string fileContent1 = Encoding.Unicode.GetString(script.OriginalEncoding.GetPreamble()) + script.ScriptContents;
      System.Management.Automation.Signature signature1 = SignatureHelper.GetSignature(path, fileContent1);
      if (signature1.Status != SignatureStatus.Valid && script.OriginalEncoding != Encoding.Unicode)
      {
        string fileContent2 = Encoding.Unicode.GetString(Encoding.Unicode.GetPreamble()) + script.ScriptContents;
        System.Management.Automation.Signature signature2 = SignatureHelper.GetSignature(path, fileContent2);
        if (signature2.Status == SignatureStatus.Valid)
          signature1 = signature2;
      }
      return signature1;
    }

    protected internal override bool ShouldRun(
      CommandInfo commandInfo,
      CommandOrigin origin,
      PSHost host,
      out Exception reason)
    {
      bool flag = false;
      reason = (Exception) null;
      Utils.CheckArgForNull(PSAuthorizationManager.tracer, (object) commandInfo, nameof (commandInfo));
      Utils.CheckArgForNullOrEmpty(PSAuthorizationManager.tracer, commandInfo.Name, "commandInfo.Name");
      switch (commandInfo.CommandType)
      {
        case CommandTypes.Alias:
          flag = true;
          break;
        case CommandTypes.Function:
        case CommandTypes.Filter:
          flag = true;
          break;
        case CommandTypes.Cmdlet:
          flag = true;
          break;
        case CommandTypes.ExternalScript:
          if (!(commandInfo is ExternalScriptInfo script))
          {
            reason = (Exception) PSAuthorizationManager.tracer.NewArgumentException("scriptInfo");
            break;
          }
          flag = this.CheckPolicy(script, host, out reason);
          break;
        case CommandTypes.Application:
          flag = true;
          break;
        case CommandTypes.Script:
          flag = true;
          break;
      }
      return flag;
    }

    private PSAuthorizationManager.RunPromptDecision AuthenticodePrompt(
      string path,
      System.Management.Automation.Signature signature,
      PSHost host)
    {
      if (host == null || host.UI == null)
        return PSAuthorizationManager.RunPromptDecision.DoNotRun;
      PSAuthorizationManager.RunPromptDecision runPromptDecision1 = PSAuthorizationManager.RunPromptDecision.DoNotRun;
      if (signature == null)
        return runPromptDecision1;
      PSAuthorizationManager.RunPromptDecision runPromptDecision2;
      switch (signature.Status)
      {
        case SignatureStatus.Valid:
          Collection<ChoiceDescription> authenticodePromptChoices = this.GetAuthenticodePromptChoices();
          string resourceString = ResourceManagerCache.GetResourceString("Authenticode", "AuthenticodePromptCaption");
          string message;
          if (signature.SignerCertificate == null)
            message = ResourceManagerCache.FormatResourceString("Authenticode", "AuthenticodePromptText_UnknownPublisher", (object) path);
          else
            message = ResourceManagerCache.FormatResourceString("Authenticode", "AuthenticodePromptText", (object) path, (object) signature.SignerCertificate.SubjectName.Name);
          runPromptDecision2 = (PSAuthorizationManager.RunPromptDecision) host.UI.PromptForChoice(resourceString, message, authenticodePromptChoices, 1);
          break;
        case SignatureStatus.UnknownError:
        case SignatureStatus.NotSigned:
        case SignatureStatus.HashMismatch:
        case SignatureStatus.NotSupportedFileFormat:
          runPromptDecision2 = PSAuthorizationManager.RunPromptDecision.DoNotRun;
          break;
        default:
          runPromptDecision2 = PSAuthorizationManager.RunPromptDecision.DoNotRun;
          break;
      }
      return runPromptDecision2;
    }

    private PSAuthorizationManager.RunPromptDecision RemoteFilePrompt(
      string path,
      PSHost host)
    {
      if (host == null || host.UI == null)
        return PSAuthorizationManager.RunPromptDecision.DoNotRun;
      Collection<ChoiceDescription> filePromptChoices = this.GetRemoteFilePromptChoices();
      string resourceString = ResourceManagerCache.GetResourceString("Authenticode", "RemoteFilePromptCaption");
      string message = ResourceManagerCache.FormatResourceString("Authenticode", "RemoteFilePromptText", (object) path);
      switch (host.UI.PromptForChoice(resourceString, message, filePromptChoices, 0))
      {
        case 0:
          return PSAuthorizationManager.RunPromptDecision.DoNotRun;
        case 1:
          return PSAuthorizationManager.RunPromptDecision.RunOnce;
        case 2:
          return PSAuthorizationManager.RunPromptDecision.Suspend;
        default:
          return PSAuthorizationManager.RunPromptDecision.DoNotRun;
      }
    }

    private Collection<ChoiceDescription> GetAuthenticodePromptChoices()
    {
      Collection<ChoiceDescription> collection = new Collection<ChoiceDescription>();
      string resourceString1 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_NeverRun");
      string resourceString2 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_NeverRun_Help");
      string resourceString3 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_DoNotRun");
      string resourceString4 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_DoNotRun_Help");
      string resourceString5 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_RunOnce");
      string resourceString6 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_RunOnce_Help");
      string resourceString7 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_AlwaysRun");
      string resourceString8 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_AlwaysRun_Help");
      collection.Add(new ChoiceDescription(resourceString1, resourceString2));
      collection.Add(new ChoiceDescription(resourceString3, resourceString4));
      collection.Add(new ChoiceDescription(resourceString5, resourceString6));
      collection.Add(new ChoiceDescription(resourceString7, resourceString8));
      return collection;
    }

    private Collection<ChoiceDescription> GetRemoteFilePromptChoices()
    {
      Collection<ChoiceDescription> collection = new Collection<ChoiceDescription>();
      string resourceString1 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_DoNotRun");
      string resourceString2 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_DoNotRun_Help");
      string resourceString3 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_RunOnce");
      string resourceString4 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_RunOnce_Help");
      string resourceString5 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_Suspend");
      string resourceString6 = ResourceManagerCache.GetResourceString("Authenticode", "Choice_Suspend_Help");
      collection.Add(new ChoiceDescription(resourceString1, resourceString2));
      collection.Add(new ChoiceDescription(resourceString3, resourceString4));
      collection.Add(new ChoiceDescription(resourceString5, resourceString6));
      return collection;
    }

    internal enum RunPromptDecision
    {
      NeverRun,
      DoNotRun,
      RunOnce,
      AlwaysRun,
      Suspend,
    }
  }
}
