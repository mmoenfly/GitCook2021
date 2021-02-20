// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.FormatErrorPolicy
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class FormatErrorPolicy
  {
    private bool? _showErrorsAsMessages;
    private bool? _showErrorsInFormattedOutput = new bool?();
    internal string errorStringInFormattedOutput = "#ERR";
    internal string formatErrorStringInFormattedOutput = "#FMTERR";

    internal bool ShowErrorsAsMessages
    {
      set
      {
        if (this._showErrorsAsMessages.HasValue)
          return;
        this._showErrorsAsMessages = new bool?(value);
      }
      get => this._showErrorsAsMessages.HasValue && this._showErrorsAsMessages.Value;
    }

    internal bool ShowErrorsInFormattedOutput
    {
      set
      {
        if (this._showErrorsInFormattedOutput.HasValue)
          return;
        this._showErrorsInFormattedOutput = new bool?(value);
      }
      get => this._showErrorsInFormattedOutput.HasValue && this._showErrorsInFormattedOutput.Value;
    }
  }
}
