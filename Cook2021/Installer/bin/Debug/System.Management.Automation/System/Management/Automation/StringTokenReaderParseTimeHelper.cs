// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.StringTokenReaderParseTimeHelper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class StringTokenReaderParseTimeHelper : 
    IStringTokenReaderHelper2,
    IStringTokenReaderHelper
  {
    private Parser _parser;
    private int currentLineNumber;

    public StringTokenReaderParseTimeHelper(Parser parser, int startLineNumber)
    {
      this._parser = parser;
      this.currentLineNumber = startLineNumber;
    }

    void IStringTokenReaderHelper.AddLineStart(int offset) => ++this.currentLineNumber;

    bool IStringTokenReaderHelper.InteractiveInput => this._parser.InteractiveInput;

    Parser IStringTokenReaderHelper2.ParentParser => this._parser;

    int IStringTokenReaderHelper2.CurrentLineNumber => this.currentLineNumber;

    void IStringTokenReaderHelper.SetToEndOfInput()
    {
    }

    void IStringTokenReaderHelper.ReportMissingTerminator(
      string str,
      int startOffset,
      int errorOffset,
      string errorId,
      Token token,
      string terminator)
    {
      StringTokenReader.ReportMissingTerminator(this._parser, str, startOffset, errorOffset, errorId, terminator);
    }

    void IStringTokenReaderHelper.ReportError(
      string str,
      int offset,
      Token token,
      string errorId)
    {
      StringTokenReader.ReportError(this._parser, str, offset, errorId);
    }
  }
}
