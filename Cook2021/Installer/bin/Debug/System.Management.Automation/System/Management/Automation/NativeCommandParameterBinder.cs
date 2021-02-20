// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.NativeCommandParameterBinder
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;
using System.Text;

namespace System.Management.Automation
{
  internal class NativeCommandParameterBinder : ParameterBinderBase
  {
    [TraceSource("NativeCommandParameterBinder", "The parameter binder for native commands")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (NativeCommandParameterBinder), "The parameter binder for native commands");
    private StringBuilder arguments = new StringBuilder();
    private static readonly char[] whiteSpace = new char[2]
    {
      ' ',
      '\t'
    };
    private NativeCommand nativeCommand;

    internal NativeCommandParameterBinder(NativeCommand command)
      : base(command.MyInvocation, command.Context, (InternalCommand) command)
      => this.nativeCommand = command;

    internal override void BindParameter(string name, object value)
    {
      if (name != null)
        NativeCommandParameterBinder.appendNativeArguments(this.Context, this.arguments, (object) name);
      if (value == AutomationNull.Value || value == UnboundParameter.Value)
        return;
      NativeCommandParameterBinder.appendNativeArguments(this.Context, this.arguments, value);
    }

    internal override object GetDefaultParameterValue(string name) => (object) null;

    internal string Arguments
    {
      get
      {
        NativeCommandParameterBinder.tracer.WriteLine("Raw argument string: " + this.arguments.ToString(), new object[0]);
        string[] commandLine = CommandLineParameterBinderNativeMethods.PreParseCommandLine(this.arguments.ToString());
        for (int index = 0; index < commandLine.Length; ++index)
          NativeCommandParameterBinder.tracer.WriteLine("Argument {0}: {1}", (object) index, (object) commandLine[index]);
        return this.arguments.ToString();
      }
    }

    internal static void appendOneNativeArgument(
      ExecutionContext context,
      StringBuilder argumentBuilder,
      bool needInitialSpace,
      object obj)
    {
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(obj);
      bool flag = needInitialSpace;
      do
      {
        string stringParser;
        if (enumerator == null)
          stringParser = PSObject.ToStringParser(context, obj);
        else if (ParserOps.MoveNext(context, (Token) null, enumerator))
          stringParser = PSObject.ToStringParser(context, ParserOps.Current((Token) null, enumerator));
        else
          goto label_13;
        if (!string.IsNullOrEmpty(stringParser))
        {
          if (flag)
            argumentBuilder.Append(' ');
          else
            flag = true;
          if (stringParser.IndexOfAny(NativeCommandParameterBinder.whiteSpace) >= 0 && stringParser.Length > 1 && stringParser[0] != '"')
          {
            argumentBuilder.Append('"');
            argumentBuilder.Append(stringParser);
            argumentBuilder.Append('"');
          }
          else
            argumentBuilder.Append(stringParser);
        }
      }
      while (enumerator != null);
      goto label_14;
label_13:
      return;
label_14:;
    }

    private static void appendNativeArguments(
      ExecutionContext context,
      StringBuilder argumentBuilder,
      object arg)
    {
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(arg);
      if (enumerator == null)
      {
        NativeCommandParameterBinder.appendOneNativeArgument(context, argumentBuilder, true, arg);
      }
      else
      {
        while (enumerator.MoveNext())
        {
          object current = enumerator.Current;
          NativeCommandParameterBinder.appendOneNativeArgument(context, argumentBuilder, true, current);
        }
      }
    }
  }
}
