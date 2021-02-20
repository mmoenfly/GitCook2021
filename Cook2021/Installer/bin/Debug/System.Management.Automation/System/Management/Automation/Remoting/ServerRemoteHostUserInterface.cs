// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ServerRemoteHostUserInterface
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Internal.Host;
using System.Security;

namespace System.Management.Automation.Remoting
{
  internal class ServerRemoteHostUserInterface : 
    PSHostUserInterface,
    IHostUISupportsMultipleChoiceSelection
  {
    private PSHostRawUserInterface _rawUI;
    private ServerRemoteHost _remoteHost;
    private ServerMethodExecutor _serverMethodExecutor;

    internal ServerRemoteHostUserInterface(ServerRemoteHost remoteHost)
    {
      this._remoteHost = remoteHost;
      this._serverMethodExecutor = remoteHost.ServerMethodExecutor;
      this._rawUI = remoteHost.HostInfo.IsHostRawUINull ? (PSHostRawUserInterface) null : (PSHostRawUserInterface) new ServerRemoteHostRawUserInterface(this);
    }

    public override PSHostRawUserInterface RawUI => this._rawUI;

    internal ServerRemoteHost ServerRemoteHost => this._remoteHost;

    public override string ReadLine() => this._serverMethodExecutor.ExecuteMethod<string>(RemoteHostMethodId.ReadLine);

    public override int PromptForChoice(
      string caption,
      string message,
      Collection<ChoiceDescription> choices,
      int defaultChoice)
    {
      return this._serverMethodExecutor.ExecuteMethod<int>(RemoteHostMethodId.PromptForChoice, new object[4]
      {
        (object) caption,
        (object) message,
        (object) choices,
        (object) defaultChoice
      });
    }

    public Collection<int> PromptForChoice(
      string caption,
      string message,
      Collection<ChoiceDescription> choices,
      IEnumerable<int> defaultChoices)
    {
      return this._serverMethodExecutor.ExecuteMethod<Collection<int>>(RemoteHostMethodId.PromptForChoiceMultipleSelection, new object[4]
      {
        (object) caption,
        (object) message,
        (object) choices,
        (object) defaultChoices
      });
    }

    public override Dictionary<string, PSObject> Prompt(
      string caption,
      string message,
      Collection<FieldDescription> descriptions)
    {
      Dictionary<string, PSObject> dictionary = this._serverMethodExecutor.ExecuteMethod<Dictionary<string, PSObject>>(RemoteHostMethodId.Prompt, new object[3]
      {
        (object) caption,
        (object) message,
        (object) descriptions
      });
      foreach (FieldDescription description in descriptions)
      {
        Type fieldType = InternalHostUserInterface.GetFieldType(description);
        PSObject psObject;
        object result;
        if (fieldType != null && dictionary.TryGetValue(description.Name, out psObject) && LanguagePrimitives.TryConvertTo((object) psObject, fieldType, (IFormatProvider) CultureInfo.InvariantCulture, out result))
          dictionary[description.Name] = result == null ? (PSObject) null : PSObject.AsPSObject(result);
      }
      return dictionary;
    }

    public override void Write(string message) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.Write1, new object[1]
    {
      (object) message
    });

    public override void Write(
      ConsoleColor foregroundColor,
      ConsoleColor backgroundColor,
      string message)
    {
      this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.Write2, new object[3]
      {
        (object) foregroundColor,
        (object) backgroundColor,
        (object) message
      });
    }

    public override void WriteLine() => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteLine1);

    public override void WriteLine(string message) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteLine2, new object[1]
    {
      (object) message
    });

    public override void WriteLine(
      ConsoleColor foregroundColor,
      ConsoleColor backgroundColor,
      string message)
    {
      this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteLine3, new object[3]
      {
        (object) foregroundColor,
        (object) backgroundColor,
        (object) message
      });
    }

    public override void WriteErrorLine(string message) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteErrorLine, new object[1]
    {
      (object) message
    });

    public override void WriteDebugLine(string message) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteDebugLine, new object[1]
    {
      (object) message
    });

    public override void WriteProgress(long sourceId, ProgressRecord record) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteProgress, new object[2]
    {
      (object) sourceId,
      (object) record
    });

    public override void WriteVerboseLine(string message) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteVerboseLine, new object[1]
    {
      (object) message
    });

    public override void WriteWarningLine(string message) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteWarningLine, new object[1]
    {
      (object) message
    });

    public override SecureString ReadLineAsSecureString() => this._serverMethodExecutor.ExecuteMethod<SecureString>(RemoteHostMethodId.ReadLineAsSecureString);

    public override PSCredential PromptForCredential(
      string caption,
      string message,
      string userName,
      string targetName)
    {
      return this._serverMethodExecutor.ExecuteMethod<PSCredential>(RemoteHostMethodId.PromptForCredential1, new object[4]
      {
        (object) caption,
        (object) message,
        (object) userName,
        (object) targetName
      });
    }

    public override PSCredential PromptForCredential(
      string caption,
      string message,
      string userName,
      string targetName,
      PSCredentialTypes allowedCredentialTypes,
      PSCredentialUIOptions options)
    {
      return this._serverMethodExecutor.ExecuteMethod<PSCredential>(RemoteHostMethodId.PromptForCredential2, new object[6]
      {
        (object) caption,
        (object) message,
        (object) userName,
        (object) targetName,
        (object) allowedCredentialTypes,
        (object) options
      });
    }
  }
}
