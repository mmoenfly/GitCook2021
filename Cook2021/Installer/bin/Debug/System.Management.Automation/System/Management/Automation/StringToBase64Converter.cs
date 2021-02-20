// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.StringToBase64Converter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Management.Automation
{
  internal static class StringToBase64Converter
  {
    [TraceSource("StringToBase64StringConverter", "StringToBase64StringConverter")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("StringToBase64StringConverter", "StringToBase64StringConverter");

    internal static string StringToBase64String(string input)
    {
      string format = input != null ? Convert.ToBase64String(Encoding.Unicode.GetBytes(input.ToCharArray())) : throw StringToBase64Converter.tracer.NewArgumentNullException(nameof (input));
      StringToBase64Converter.tracer.WriteLine(format, new object[0]);
      return format;
    }

    internal static string Base64ToString(string base64)
    {
      string format = !string.IsNullOrEmpty(base64) ? new string(Encoding.Unicode.GetChars(Convert.FromBase64String(base64))) : throw StringToBase64Converter.tracer.NewArgumentNullException(nameof (base64));
      StringToBase64Converter.tracer.WriteLine(format, new object[0]);
      return format;
    }

    internal static object[] Base64ToArgsConverter(string base64)
    {
      string str = !string.IsNullOrEmpty(base64) ? new string(Encoding.Unicode.GetChars(Convert.FromBase64String(base64))) : throw StringToBase64Converter.tracer.NewArgumentNullException(nameof (base64));
      StringToBase64Converter.tracer.WriteLine(str, new object[0]);
      Deserializer deserializer = new Deserializer((XmlReader) new XmlTextReader((TextReader) new StringReader(str)));
      object obj = deserializer.Deserialize();
      if (!deserializer.Done())
        throw StringToBase64Converter.tracer.NewArgumentException("-args");
      if (!(obj is PSObject psObject))
        throw StringToBase64Converter.tracer.NewArgumentException("-args");
      if (!(psObject.BaseObject is ArrayList baseObject))
        throw StringToBase64Converter.tracer.NewArgumentException("-args");
      return baseObject.ToArray();
    }
  }
}
