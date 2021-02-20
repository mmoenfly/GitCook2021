// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSScriptMethod
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Text;

namespace System.Management.Automation
{
  public class PSScriptMethod : PSMethodInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private ScriptBlock script;
    private bool shouldCloneOnAccess;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.TypeNameOfValue);
      stringBuilder.Append(" ");
      stringBuilder.Append(this.Name);
      stringBuilder.Append("();");
      return stringBuilder.ToString();
    }

    public ScriptBlock Script => this.shouldCloneOnAccess ? this.script.Clone() : this.script;

    public PSScriptMethod(string name, ScriptBlock script)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSScriptMethod.tracer.NewArgumentException(nameof (name));
      this.script = script != null ? script : throw PSScriptMethod.tracer.NewArgumentNullException(nameof (script));
    }

    internal PSScriptMethod(string name, ScriptBlock script, bool shouldCloneOnAccess)
      : this(name, script)
      => this.shouldCloneOnAccess = shouldCloneOnAccess;

    public override object Invoke(params object[] arguments)
    {
      if (arguments == null)
        throw PSScriptMethod.tracer.NewArgumentNullException(nameof (arguments));
      try
      {
        return this.Script.InvokeUsingCmdlet((Cmdlet) null, true, false, (object) AutomationNull.Value, (object) AutomationNull.Value, (object) this.instance, arguments);
      }
      catch (SessionStateOverflowException ex)
      {
        throw new MethodInvocationException("ScriptMethodSessionStateOverflowException", (Exception) ex, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
        {
          (object) this.Name,
          (object) arguments.Length,
          (object) ex.Message
        });
      }
      catch (RuntimeException ex)
      {
        throw new MethodInvocationException("ScriptMethodRuntimeException", (Exception) ex, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
        {
          (object) this.Name,
          (object) arguments.Length,
          (object) ex.Message
        });
      }
      catch (TerminateException ex)
      {
        throw;
      }
      catch (FlowControlException ex)
      {
        throw new MethodInvocationException("ScriptMethodFlowControlException", (Exception) ex, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
        {
          (object) this.Name,
          (object) arguments.Length,
          (object) ex.Message
        });
      }
      catch (PSInvalidOperationException ex)
      {
        throw new MethodInvocationException("ScriptMethodInvalidOperationException", (Exception) ex, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
        {
          (object) this.Name,
          (object) arguments.Length,
          (object) ex.Message
        });
      }
    }

    public override Collection<string> OverloadDefinitions => new Collection<string>()
    {
      this.ToString()
    };

    public override PSMemberInfo Copy()
    {
      PSScriptMethod psScriptMethod = new PSScriptMethod(this.name, this.script);
      psScriptMethod.shouldCloneOnAccess = this.shouldCloneOnAccess;
      this.CloneBaseProperties((PSMemberInfo) psScriptMethod);
      return (PSMemberInfo) psScriptMethod;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.ScriptMethod;

    public override string TypeNameOfValue => typeof (object).FullName;
  }
}
