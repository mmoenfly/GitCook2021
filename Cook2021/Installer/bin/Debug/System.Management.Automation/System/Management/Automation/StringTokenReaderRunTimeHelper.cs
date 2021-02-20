// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.StringTokenReaderRunTimeHelper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class StringTokenReaderRunTimeHelper : IStringTokenReaderHelper2, IStringTokenReaderHelper
  {
    private Parser _parser;
    private int currentLineNumber = 1;

    public StringTokenReaderRunTimeHelper(Parser parser) => this._parser = parser;

    void IStringTokenReaderHelper.AddLineStart(int offset) => ++this.currentLineNumber;

    bool IStringTokenReaderHelper.InteractiveInput => false;

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
      throw InterpreterError.NewInterpreterException((object) str, typeof (RuntimeException), token, errorId, (object) StringTokenReaderRunTimeHelper.OffsetPositionMessage(startOffset), (object) terminator);
    }

    void IStringTokenReaderHelper.ReportError(
      string str,
      int offset,
      Token token,
      string errorId)
    {
      throw InterpreterError.NewInterpreterException((object) str, typeof (RuntimeException), token, errorId);
    }

    internal static string OffsetPositionMessage(int offset) => StringUtil.Format(ResourceManagerCache.GetResourceString("Parser", "TextForCharPositionMessage"), (object) offset);
  }
}
