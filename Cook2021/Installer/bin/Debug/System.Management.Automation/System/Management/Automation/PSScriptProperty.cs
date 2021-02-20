// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSScriptProperty
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Text;

namespace System.Management.Automation
{
  public class PSScriptProperty : PSPropertyInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private ScriptBlock getterScript;
    private ScriptBlock setterScript;
    private bool shouldCloneOnAccess;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.TypeNameOfValue);
      stringBuilder.Append(" ");
      stringBuilder.Append(this.Name);
      stringBuilder.Append(" {");
      if (this.IsGettable)
      {
        stringBuilder.Append("get=");
        stringBuilder.Append(this.getterScript.ToString());
        stringBuilder.Append(";");
      }
      if (this.IsSettable)
      {
        stringBuilder.Append("set=");
        stringBuilder.Append(this.setterScript.ToString());
        stringBuilder.Append(";");
      }
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    public ScriptBlock GetterScript => this.shouldCloneOnAccess ? this.getterScript.Clone() : this.getterScript;

    public ScriptBlock SetterScript => this.shouldCloneOnAccess ? this.setterScript.Clone() : this.setterScript;

    public PSScriptProperty(string name, ScriptBlock getterScript)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSScriptProperty.tracer.NewArgumentException(nameof (name));
      this.getterScript = getterScript != null ? getterScript : throw PSScriptProperty.tracer.NewArgumentNullException(nameof (getterScript));
    }

    public PSScriptProperty(string name, ScriptBlock getterScript, ScriptBlock setterScript)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSScriptProperty.tracer.NewArgumentException(nameof (name));
      this.getterScript = getterScript != null || setterScript != null ? getterScript : throw PSScriptProperty.tracer.NewArgumentException("getterScript setterScript");
      this.setterScript = setterScript;
    }

    internal PSScriptProperty(
      string name,
      ScriptBlock getterScript,
      ScriptBlock setterScript,
      bool shouldCloneOnAccess)
      : this(name, getterScript, setterScript)
    {
      this.shouldCloneOnAccess = shouldCloneOnAccess;
    }

    public override PSMemberInfo Copy()
    {
      PSScriptProperty psScriptProperty = new PSScriptProperty(this.name, this.getterScript, this.setterScript);
      psScriptProperty.shouldCloneOnAccess = this.shouldCloneOnAccess;
      this.CloneBaseProperties((PSMemberInfo) psScriptProperty);
      return (PSMemberInfo) psScriptProperty;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.ScriptProperty;

    public override bool IsSettable => this.setterScript != null;

    public override bool IsGettable => this.getterScript != null;

    public override object Value
    {
      get
      {
        if (this.getterScript == null)
          throw new GetValueException("GetWithoutGetterFromScriptPropertyValue", (Exception) null, "ExtendedTypeSystem", "GetWithoutGetterException", new object[1]
          {
            (object) this.Name
          });
        try
        {
          return this.GetterScript.InvokeUsingCmdlet((Cmdlet) null, true, false, (object) AutomationNull.Value, (object) AutomationNull.Value, (object) this.instance);
        }
        catch (SessionStateOverflowException ex)
        {
          throw this.NewGetValueException((Exception) ex, "ScriptGetValueSessionStateOverflowException");
        }
        catch (RuntimeException ex)
        {
          throw this.NewGetValueException((Exception) ex, "ScriptGetValueRuntimeException");
        }
        catch (TerminateException ex)
        {
          throw;
        }
        catch (FlowControlException ex)
        {
          throw this.NewGetValueException((Exception) ex, "ScriptGetValueFlowControlException");
        }
        catch (PSInvalidOperationException ex)
        {
          throw this.NewGetValueException((Exception) ex, "ScriptgetValueInvalidOperationException");
        }
      }
      set
      {
        if (this.setterScript == null)
          throw new SetValueException("SetWithoutSetterFromScriptProperty", (Exception) null, "ExtendedTypeSystem", "SetWithoutSetterException", new object[1]
          {
            (object) this.Name
          });
        try
        {
          this.SetterScript.InvokeUsingCmdlet((Cmdlet) null, true, false, (object) AutomationNull.Value, (object) AutomationNull.Value, (object) this.instance, value);
        }
        catch (SessionStateOverflowException ex)
        {
          throw this.NewSetValueException((Exception) ex, "ScriptSetValueSessionStateOverflowException");
        }
        catch (RuntimeException ex)
        {
          throw this.NewSetValueException((Exception) ex, "ScriptSetValueRuntimeException");
        }
        catch (TerminateException ex)
        {
          throw;
        }
        catch (FlowControlException ex)
        {
          throw this.NewSetValueException((Exception) ex, "ScriptSetValueFlowControlException");
        }
        catch (PSInvalidOperationException ex)
        {
          throw this.NewSetValueException((Exception) ex, "ScriptSetValueInvalidOperationException");
        }
      }
    }

    public override string TypeNameOfValue => typeof (object).FullName;
  }
}
