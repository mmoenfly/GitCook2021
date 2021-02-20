// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.IStringTokenReaderHelper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal interface IStringTokenReaderHelper
  {
    void AddLineStart(int offset);

    void SetToEndOfInput();

    bool InteractiveInput { get; }

    void ReportMissingTerminator(
      string str,
      int startOffset,
      int errorOffset,
      string errorId,
      Token token,
      string terminator);

    void ReportError(string str, int offset, Token token, string errorId);
  }
}
