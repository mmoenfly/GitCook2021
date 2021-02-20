// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProxyCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Text;

namespace System.Management.Automation
{
  public sealed class ProxyCommand
  {
    [TraceSource("CommandMetadata", "The metadata associated with a cmdlet.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CommandMetadata", "The metadata associated with a cmdlet.");

    private ProxyCommand()
    {
    }

    public static string Create(CommandMetadata commandMetadata) => commandMetadata != null ? commandMetadata.GetProxyCommand("") : throw ProxyCommand.tracer.NewArgumentNullException("commandMetaData");

    public static string Create(CommandMetadata commandMetadata, string helpComment) => commandMetadata != null ? commandMetadata.GetProxyCommand(helpComment) : throw ProxyCommand.tracer.NewArgumentNullException("commandMetaData");

    public static string GetCmdletBindingAttribute(CommandMetadata commandMetadata) => commandMetadata != null ? commandMetadata.GetDecl() : throw ProxyCommand.tracer.NewArgumentNullException("commandMetaData");

    public static string GetParamBlock(CommandMetadata commandMetadata) => commandMetadata != null ? commandMetadata.GetParamBlock() : throw ProxyCommand.tracer.NewArgumentNullException("commandMetaData");

    public static string GetBegin(CommandMetadata commandMetadata) => commandMetadata != null ? commandMetadata.GetBeginBlock() : throw ProxyCommand.tracer.NewArgumentNullException("commandMetaData");

    public static string GetProcess(CommandMetadata commandMetadata) => commandMetadata != null ? commandMetadata.GetProcessBlock() : throw ProxyCommand.tracer.NewArgumentNullException("commandMetaData");

    public static string GetEnd(CommandMetadata commandMetadata) => commandMetadata != null ? commandMetadata.GetEndBlock() : throw ProxyCommand.tracer.NewArgumentNullException("commandMetaData");

    private static T GetProperty<T>(PSObject obj, string property) where T : class
    {
      T obj1 = default (T);
      if (obj != null && obj.Properties[property] != null)
        obj1 = obj.Properties[property].Value as T;
      return obj1;
    }

    private static string GetObjText(object obj)
    {
      string str = (string) null;
      if (obj is PSObject psObject)
        str = ProxyCommand.GetProperty<string>(psObject, "Text");
      if (str == null)
        str = obj.ToString();
      return str;
    }

    private static void AppendContent(StringBuilder sb, string section, object obj)
    {
      if (obj == null)
        return;
      string objText = ProxyCommand.GetObjText(obj);
      if (string.IsNullOrEmpty(objText))
        return;
      sb.Append("\n");
      sb.Append(section);
      sb.Append("\n\n");
      sb.Append(objText);
      sb.Append("\n");
    }

    private static void AppendContent(StringBuilder sb, string section, PSObject[] array)
    {
      if (array == null)
        return;
      bool flag = true;
      foreach (object obj in array)
      {
        string objText = ProxyCommand.GetObjText(obj);
        if (!string.IsNullOrEmpty(objText))
        {
          if (flag)
          {
            flag = false;
            sb.Append("\n\n");
            sb.Append(section);
            sb.Append("\n\n");
          }
          sb.Append(objText);
          sb.Append("\n");
        }
      }
      if (flag)
        return;
      sb.Append("\n");
    }

    private static void AppendType(StringBuilder sb, string section, PSObject parent)
    {
      PSObject property1 = ProxyCommand.GetProperty<PSObject>(parent, "type");
      PSObject property2 = ProxyCommand.GetProperty<PSObject>(property1, "name");
      if (property2 != null)
      {
        sb.AppendFormat("\n\n{0}\n\n", (object) section);
        sb.Append(ProxyCommand.GetObjText((object) property2));
        sb.Append("\n");
      }
      else
      {
        PSObject property3 = ProxyCommand.GetProperty<PSObject>(property1, "uri");
        if (property3 == null)
          return;
        sb.AppendFormat("\n\n{0}\n\n", (object) section);
        sb.Append(ProxyCommand.GetObjText((object) property3));
        sb.Append("\n");
      }
    }

    public static string GetHelpComments(PSObject help)
    {
      if (help == null)
        throw new ArgumentNullException(nameof (help));
      bool flag = false;
      foreach (string typeName in help.TypeNames)
      {
        if (typeName.Contains("HelpInfo"))
        {
          flag = true;
          break;
        }
      }
      if (!flag)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("ProxyCommandStrings", "HelpInfoObjectRequired"));
      StringBuilder sb = new StringBuilder();
      ProxyCommand.AppendContent(sb, ".SYNOPSIS", (object) ProxyCommand.GetProperty<string>(help, "Synopsis"));
      ProxyCommand.AppendContent(sb, ".DESCRIPTION", ProxyCommand.GetProperty<PSObject[]>(help, "Description"));
      PSObject[] property1 = ProxyCommand.GetProperty<PSObject[]>(ProxyCommand.GetProperty<PSObject>(help, "Parameters"), "Parameter");
      if (property1 != null)
      {
        foreach (PSObject psObject1 in property1)
        {
          PSObject property2 = ProxyCommand.GetProperty<PSObject>(psObject1, "Name");
          PSObject[] property3 = ProxyCommand.GetProperty<PSObject[]>(psObject1, "Description");
          sb.AppendFormat("\n.PARAMETER {0}\n\n", (object) property2);
          foreach (PSObject psObject2 in property3)
          {
            string str = ProxyCommand.GetProperty<string>(psObject2, "Text") ?? psObject2.ToString();
            if (!string.IsNullOrEmpty(str))
            {
              sb.Append(str);
              sb.Append("\n");
            }
          }
        }
      }
      PSObject[] property4 = ProxyCommand.GetProperty<PSObject[]>(ProxyCommand.GetProperty<PSObject>(help, "examples"), "example");
      if (property4 != null)
      {
        foreach (PSObject psObject1 in property4)
        {
          StringBuilder stringBuilder = new StringBuilder();
          PSObject[] property2 = ProxyCommand.GetProperty<PSObject[]>(psObject1, "introduction");
          if (property2 != null)
          {
            foreach (PSObject psObject2 in property2)
            {
              if (psObject2 != null)
                stringBuilder.Append(ProxyCommand.GetObjText((object) psObject2));
            }
          }
          PSObject property3 = ProxyCommand.GetProperty<PSObject>(psObject1, "code");
          if (property3 != null)
            stringBuilder.Append(property3.ToString());
          PSObject[] property5 = ProxyCommand.GetProperty<PSObject[]>(psObject1, "remarks");
          if (property5 != null)
          {
            stringBuilder.Append("\n");
            foreach (PSObject psObject2 in property5)
            {
              string property6 = ProxyCommand.GetProperty<string>(psObject2, "text");
              stringBuilder.Append(property6.ToString());
            }
          }
          if (stringBuilder.Length > 0)
          {
            sb.Append("\n\n.EXAMPLE\n\n");
            sb.Append(stringBuilder.ToString());
          }
        }
      }
      PSObject property7 = ProxyCommand.GetProperty<PSObject>(help, "alertSet");
      ProxyCommand.AppendContent(sb, ".NOTES", ProxyCommand.GetProperty<PSObject[]>(property7, "alert"));
      PSObject property8 = ProxyCommand.GetProperty<PSObject>(ProxyCommand.GetProperty<PSObject>(help, "inputTypes"), "inputType");
      ProxyCommand.AppendType(sb, ".INPUTS", property8);
      PSObject property9 = ProxyCommand.GetProperty<PSObject>(ProxyCommand.GetProperty<PSObject>(help, "returnValues"), "returnValue");
      ProxyCommand.AppendType(sb, ".OUTPUTS", property9);
      PSObject[] property10 = ProxyCommand.GetProperty<PSObject[]>(ProxyCommand.GetProperty<PSObject>(help, "relatedLinks"), "navigationLink");
      if (property10 != null)
      {
        foreach (PSObject psObject in property10)
        {
          ProxyCommand.AppendContent(sb, ".LINK", (object) ProxyCommand.GetProperty<PSObject>(psObject, "uri"));
          ProxyCommand.AppendContent(sb, ".LINK", (object) ProxyCommand.GetProperty<PSObject>(psObject, "linkText"));
        }
      }
      ProxyCommand.AppendContent(sb, ".COMPONENT", (object) ProxyCommand.GetProperty<PSObject>(help, "Component"));
      ProxyCommand.AppendContent(sb, ".ROLE", (object) ProxyCommand.GetProperty<PSObject>(help, "Role"));
      ProxyCommand.AppendContent(sb, ".FUNCTIONALITY", (object) ProxyCommand.GetProperty<PSObject>(help, "Functionality"));
      return sb.ToString();
    }
  }
}
