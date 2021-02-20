// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSParser
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Text;

namespace System.Management.Automation
{
  public sealed class PSParser
  {
    private Parser _parser;
    [TraceSource("PSParser", "PSParser")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PSParser), nameof (PSParser));

    private PSParser()
    {
      this._parser = new Parser();
      this._parser.AccumulateErrors = true;
    }

    private void Parse(string script)
    {
      try
      {
        this._parser.ParseScriptBlock(script, false);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
    }

    private Collection<PSToken> Tokens
    {
      get
      {
        Collection<PSToken> collection = new Collection<PSToken>();
        foreach (Token token in this._parser.Tokenizer.Tokens)
          collection.Add(new PSToken(token));
        return collection;
      }
    }

    private Collection<PSParseError> Errors
    {
      get
      {
        Collection<PSParseError> collection = new Collection<PSParseError>();
        foreach (RuntimeException error in this._parser.Errors)
          collection.Add(new PSParseError(error));
        return collection;
      }
    }

    public static Collection<PSToken> Tokenize(
      string script,
      out Collection<PSParseError> errors)
    {
      if (script == null)
        throw PSParser.tracer.NewArgumentNullException(nameof (script));
      PSParser psParser = new PSParser();
      psParser.Parse(script);
      errors = psParser.Errors;
      return psParser.Tokens;
    }

    public static Collection<PSToken> Tokenize(
      object[] script,
      out Collection<PSParseError> errors)
    {
      if (script == null)
        throw PSParser.tracer.NewArgumentNullException(nameof (script));
      StringBuilder stringBuilder = new StringBuilder();
      foreach (object obj in script)
      {
        if (obj != null)
          stringBuilder.AppendLine(obj.ToString());
      }
      return PSParser.Tokenize(stringBuilder.ToString(), out errors);
    }
  }
}
