// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.PSHostUserInterface
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;

namespace System.Management.Automation.Host
{
  public abstract class PSHostUserInterface
  {
    public abstract PSHostRawUserInterface RawUI { get; }

    public abstract string ReadLine();

    public abstract SecureString ReadLineAsSecureString();

    public abstract void Write(string value);

    public abstract void Write(
      ConsoleColor foregroundColor,
      ConsoleColor backgroundColor,
      string value);

    public virtual void WriteLine() => this.WriteLine("");

    public abstract void WriteLine(string value);

    public virtual void WriteLine(
      ConsoleColor foregroundColor,
      ConsoleColor backgroundColor,
      string value)
    {
      switch (value)
      {
        case "":
        case null:
          this.Write("\n");
          break;
        default:
          this.Write(foregroundColor, backgroundColor, value);
          goto case "";
      }
    }

    public abstract void WriteErrorLine(string value);

    public abstract void WriteDebugLine(string message);

    public abstract void WriteProgress(long sourceId, ProgressRecord record);

    public abstract void WriteVerboseLine(string message);

    public abstract void WriteWarningLine(string message);

    public abstract Dictionary<string, PSObject> Prompt(
      string caption,
      string message,
      Collection<FieldDescription> descriptions);

    public abstract PSCredential PromptForCredential(
      string caption,
      string message,
      string userName,
      string targetName);

    public abstract PSCredential PromptForCredential(
      string caption,
      string message,
      string userName,
      string targetName,
      PSCredentialTypes allowedCredentialTypes,
      PSCredentialUIOptions options);

    public abstract int PromptForChoice(
      string caption,
      string message,
      Collection<ChoiceDescription> choices,
      int defaultChoice);
  }
}
