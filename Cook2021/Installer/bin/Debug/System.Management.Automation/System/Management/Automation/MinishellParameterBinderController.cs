// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MinishellParameterBinderController
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation.Internal;
using System.Xml;

namespace System.Management.Automation
{
  internal class MinishellParameterBinderController : NativeCommandParameterBinderController
  {
    internal const string CommandParameter = "-command";
    internal const string EncodedCommandParameter = "-encodedCommand";
    internal const string ArgsParameter = "-args";
    internal const string EncodedArgsParameter = "-encodedarguments";
    internal const string InputFormatParameter = "-inputFormat";
    internal const string OutputFormatParameter = "-outputFormat";
    internal const string XmlFormatValue = "xml";
    internal const string TextFormatValue = "text";
    internal const string NonInteractiveParameter = "-noninteractive";
    [TraceSource("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).");
    private NativeCommandIOFormat inputFormatValue = NativeCommandIOFormat.Xml;
    private NativeCommandIOFormat outputFormatValue;
    private bool nonInteractive;

    internal MinishellParameterBinderController(NativeCommand command)
      : base(command)
    {
      using (MinishellParameterBinderController.tracer.TraceConstructor((object) this))
        ;
    }

    internal override Collection<CommandParameterInternal> BindParameters(
      Collection<CommandParameterInternal> parameters)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
        return (Collection<CommandParameterInternal>) null;
    }

    internal NativeCommandIOFormat InputFormat
    {
      get
      {
        using (MinishellParameterBinderController.tracer.TraceProperty((object) this.inputFormatValue))
          return this.inputFormatValue;
      }
    }

    internal NativeCommandIOFormat OutputFormat
    {
      get
      {
        using (MinishellParameterBinderController.tracer.TraceProperty((object) this.outputFormatValue))
          return this.outputFormatValue;
      }
    }

    internal bool NonInteractive
    {
      get
      {
        using (MinishellParameterBinderController.tracer.TraceProperty((object) this.nonInteractive))
          return this.nonInteractive;
      }
    }

    internal Collection<CommandParameterInternal> BindParameters(
      Collection<CommandParameterInternal> parameters,
      bool outputRedirected,
      string hostName)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
      {
        ArrayList args = new ArrayList();
        foreach (CommandParameterInternal parameter in parameters)
        {
          if (parameter.Name != null)
          {
            string name = parameter.Name;
            if (parameter.Value1 is Token)
              name = ((Token) parameter.Value1).ToString();
            args.Add((object) name);
            if (parameter.Value2 != AutomationNull.Value && parameter.Value2 != UnboundParameter.Value)
            {
              if (parameter.Value2 is Token)
                args.Add((object) ((Token) parameter.Value2).ToString());
              else
                args.Add(parameter.Value2);
            }
          }
          else
            args.Add(parameter.Value1);
        }
        this.DefaultParameterBinder.BindParameter((string) null, (object) this.ProcessMinishellParameters(args, outputRedirected, hostName).ToArray());
        return new Collection<CommandParameterInternal>();
      }
    }

    private ArrayList ProcessMinishellParameters(
      ArrayList args,
      bool outputRedirected,
      string hostName)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
      {
        ArrayList arrayList = new ArrayList();
        string lhs1 = (string) null;
        string lhs2 = (string) null;
        MinishellParameterBinderController.MinishellParameters seen = (MinishellParameterBinderController.MinishellParameters) 0;
        for (int index = 0; index < args.Count; ++index)
        {
          object obj = args[index];
          if (MinishellParameterBinderController.StartsWith("-command", obj))
          {
            this.HandleSeenParameter(ref seen, MinishellParameterBinderController.MinishellParameters.Command, "-command");
            arrayList.Add((object) "-encodedCommand");
            if (index + 1 >= args.Count)
              throw this.NewParameterBindingException((Exception) null, ErrorCategory.InvalidArgument, "-command", typeof (ScriptBlock), (Type) null, "NoValueForCommandParameter");
            if (!(args[index + 1] is ScriptBlock scriptBlock))
              throw this.NewParameterBindingException((Exception) null, ErrorCategory.InvalidArgument, "-command", typeof (ScriptBlock), args[index + 1].GetType(), "IncorrectValueForCommandParameter");
            string base64String = StringToBase64Converter.StringToBase64String(scriptBlock.ToString());
            arrayList.Add((object) base64String);
            ++index;
          }
          else if (obj is ScriptBlock)
          {
            this.HandleSeenParameter(ref seen, MinishellParameterBinderController.MinishellParameters.Command, "-command");
            arrayList.Add((object) "-encodedCommand");
            string base64String = StringToBase64Converter.StringToBase64String(obj.ToString());
            arrayList.Add((object) base64String);
          }
          else if (MinishellParameterBinderController.StartsWith("-inputFormat", obj))
          {
            this.HandleSeenParameter(ref seen, MinishellParameterBinderController.MinishellParameters.InputFormat, "-inputFormat");
            arrayList.Add((object) "-inputFormat");
            if (index + 1 >= args.Count)
              throw this.NewParameterBindingException((Exception) null, ErrorCategory.InvalidArgument, "-inputFormat", typeof (string), (Type) null, "NoValueForInputFormatParameter");
            lhs1 = this.ProcessFormatParameterValue("-inputFormat", args[index + 1]);
            ++index;
            arrayList.Add((object) lhs1);
          }
          else if (MinishellParameterBinderController.StartsWith("-outputFormat", obj))
          {
            this.HandleSeenParameter(ref seen, MinishellParameterBinderController.MinishellParameters.OutputFormat, "-outputFormat");
            arrayList.Add((object) "-outputFormat");
            if (index + 1 >= args.Count)
              throw this.NewParameterBindingException((Exception) null, ErrorCategory.InvalidArgument, "-outputFormat", typeof (string), (Type) null, "NoValueForOutputFormatParameter");
            lhs2 = this.ProcessFormatParameterValue("-outputFormat", args[index + 1]);
            ++index;
            arrayList.Add((object) lhs2);
          }
          else if (MinishellParameterBinderController.StartsWith("-args", obj))
          {
            this.HandleSeenParameter(ref seen, MinishellParameterBinderController.MinishellParameters.Arguments, "-args");
            arrayList.Add((object) "-encodedarguments");
            if (index + 1 >= args.Count)
              throw this.NewParameterBindingException((Exception) null, ErrorCategory.InvalidArgument, "-args", typeof (string), (Type) null, "NoValuesSpecifiedForArgs");
            string encodedString = MinishellParameterBinderController.ConvertArgsValueToEncodedString(args[index + 1]);
            ++index;
            arrayList.Add((object) encodedString);
          }
          else
            arrayList.Add(obj);
        }
        if (lhs1 == null)
        {
          arrayList.Add((object) "-inputFormat");
          arrayList.Add((object) "xml");
          lhs1 = "xml";
        }
        if (lhs2 == null)
        {
          arrayList.Add((object) "-outputFormat");
          if (outputRedirected)
          {
            arrayList.Add((object) "xml");
            lhs2 = "xml";
          }
          else
          {
            arrayList.Add((object) "text");
            lhs2 = "text";
          }
        }
        this.inputFormatValue = !MinishellParameterBinderController.StartsWith(lhs1, (object) "xml") ? NativeCommandIOFormat.Text : NativeCommandIOFormat.Xml;
        this.outputFormatValue = !MinishellParameterBinderController.StartsWith(lhs2, (object) "xml") ? NativeCommandIOFormat.Text : NativeCommandIOFormat.Xml;
        if (string.IsNullOrEmpty(hostName) || !hostName.Equals("ConsoleHost", StringComparison.OrdinalIgnoreCase))
        {
          this.nonInteractive = true;
          arrayList.Insert(0, (object) "-noninteractive");
        }
        return arrayList;
      }
    }

    private void HandleSeenParameter(
      ref MinishellParameterBinderController.MinishellParameters seen,
      MinishellParameterBinderController.MinishellParameters parameter,
      string parameterName)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
      {
        if ((seen & parameter) == parameter)
          throw this.NewParameterBindingException((Exception) null, ErrorCategory.InvalidArgument, parameterName, (Type) null, (Type) null, "ParameterSpecifiedAlready", (object) parameterName);
        seen |= parameter;
      }
    }

    private string ProcessFormatParameterValue(string parameterName, object value)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
      {
        string str1;
        try
        {
          str1 = (string) LanguagePrimitives.ConvertTo(value, typeof (string), (IFormatProvider) CultureInfo.InvariantCulture);
        }
        catch (PSInvalidCastException ex)
        {
          MinishellParameterBinderController.tracer.TraceException((Exception) ex);
          throw this.NewParameterBindingException((Exception) ex, ErrorCategory.InvalidArgument, parameterName, typeof (string), value.GetType(), "StringValueExpectedForFormatParameter", (object) parameterName);
        }
        string str2;
        if (MinishellParameterBinderController.StartsWith("xml", (object) str1))
          str2 = "xml";
        else if (MinishellParameterBinderController.StartsWith("text", (object) str1))
          str2 = "text";
        else
          throw this.NewParameterBindingException((Exception) null, ErrorCategory.InvalidArgument, parameterName, typeof (string), value.GetType(), "IncorrectValueForFormatParameter", (object) str1, (object) parameterName);
        return str2;
      }
    }

    private static string ConvertArgsValueToEncodedString(object value)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
      {
        ArrayList arrayList = MinishellParameterBinderController.ConvertArgsValueToArrayList(value);
        StringWriter stringWriter = new StringWriter((IFormatProvider) CultureInfo.InvariantCulture);
        XmlTextWriter xmlTextWriter = new XmlTextWriter((TextWriter) stringWriter);
        Serializer serializer = new Serializer((XmlWriter) xmlTextWriter);
        serializer.Serialize((object) arrayList);
        serializer.Done();
        xmlTextWriter.Flush();
        string input = stringWriter.ToString();
        MinishellParameterBinderController.tracer.WriteLine("serialized args: {0}", (object) input);
        return StringToBase64Converter.StringToBase64String(input);
      }
    }

    private static ArrayList ConvertArgsValueToArrayList(object value)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
      {
        ArrayList arrayList = new ArrayList();
        IEnumerator enumerator = LanguagePrimitives.GetEnumerator(value);
        if (enumerator == null)
        {
          arrayList.Add(MinishellParameterBinderController.DecodeArgValue(value));
        }
        else
        {
          while (enumerator.MoveNext())
            arrayList.Add(MinishellParameterBinderController.DecodeArgValue(enumerator.Current));
        }
        return arrayList;
      }
    }

    private static object DecodeArgValue(object value)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
      {
        if (!(value is Token token))
          return value;
        return token.Is(TokenId.NumberToken) ? token.Data : (object) token.TokenText;
      }
    }

    private static bool StartsWith(string lhs, object value)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
        return value is string str && lhs.StartsWith(str, StringComparison.OrdinalIgnoreCase);
    }

    private ParameterBindingException NewParameterBindingException(
      Exception innerException,
      ErrorCategory errorCategory,
      string parameterName,
      Type parameterType,
      Type typeSpecified,
      string errorIdAndResourceId,
      params object[] args)
    {
      using (MinishellParameterBinderController.tracer.TraceMethod())
        return new ParameterBindingException(innerException, errorCategory, this.InvocationInfo, (Token) null, parameterName, parameterType, typeSpecified, "NativeCP", errorIdAndResourceId, args);
    }

    [System.Flags]
    private enum MinishellParameters
    {
      Command = 1,
      Arguments = 2,
      InputFormat = 4,
      OutputFormat = 8,
    }
  }
}
