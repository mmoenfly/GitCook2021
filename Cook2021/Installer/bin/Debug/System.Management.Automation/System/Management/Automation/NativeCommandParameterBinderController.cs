// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.NativeCommandParameterBinderController
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Text;

namespace System.Management.Automation
{
  internal class NativeCommandParameterBinderController : ParameterBinderController
  {
    [TraceSource("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).");

    internal NativeCommandParameterBinderController(NativeCommand command)
      : base(command.MyInvocation, command.Context, (ParameterBinderBase) new NativeCommandParameterBinder(command), (InternalCommand) command)
    {
    }

    internal string Arguments => ((NativeCommandParameterBinder) this.DefaultParameterBinder).Arguments;

    internal override bool BindParameter(
      CommandParameterInternal argument,
      ParameterBindingFlags flags)
    {
      this.DefaultParameterBinder.BindParameter(argument.Name, argument.Value2);
      return true;
    }

    internal override Collection<CommandParameterInternal> BindParameters(
      Collection<CommandParameterInternal> parameters)
    {
      ArrayList arrayList = new ArrayList();
      foreach (CommandParameterInternal parameter in parameters)
      {
        if (parameter.Name != null)
        {
          StringBuilder argumentBuilder = new StringBuilder();
          bool flag = true;
          if (parameter.Value1 is Token token)
          {
            argumentBuilder.Append(token.ToString());
            if (!token.FollowedBySpace)
              flag = false;
          }
          else
            argumentBuilder.Append(parameter.Name);
          if (parameter.Value2 != AutomationNull.Value && parameter.Value2 != UnboundParameter.Value)
          {
            if (flag)
            {
              arrayList.Add((object) argumentBuilder);
              argumentBuilder = new StringBuilder();
            }
            NativeCommandParameterBinder.appendOneNativeArgument(this.Context, argumentBuilder, false, parameter.Value2);
          }
          arrayList.Add((object) argumentBuilder);
        }
        else
          arrayList.Add(parameter.Value1);
      }
      this.DefaultParameterBinder.BindParameter((string) null, (object) arrayList);
      return new Collection<CommandParameterInternal>();
    }
  }
}
