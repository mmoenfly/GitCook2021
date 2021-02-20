// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.FieldDescription
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation.Host
{
  public class FieldDescription
  {
    private const string StringsBaseName = "DescriptionsStrings";
    private const string NullOrEmptyErrorTemplateResource = "NullOrEmptyErrorTemplate";
    private readonly string name;
    private string label = "";
    private string parameterTypeName;
    private string parameterTypeFullName;
    private string parameterAssemblyFullName;
    private string helpMessage = "";
    private bool isMandatory = true;
    private PSObject defaultValue;
    private Collection<Attribute> metadata = new Collection<Attribute>();
    private bool modifiedByRemotingProtocol;
    private bool isFromRemoteHost;
    [TraceSource("FieldDescription", "Describes a parameter to a host when prompting for input.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (FieldDescription), "Describes a parameter to a host when prompting for input.");

    public FieldDescription(string name)
    {
      using (FieldDescription.tracer.TraceConstructor((object) this))
        this.name = !string.IsNullOrEmpty(name) ? name : throw FieldDescription.tracer.NewArgumentException(nameof (name), "DescriptionsStrings", "NullOrEmptyErrorTemplate", (object) nameof (name));
    }

    public string Name
    {
      get
      {
        using (FieldDescription.tracer.TraceProperty(this.name, new object[0]))
          return this.name;
      }
    }

    public void SetParameterType(Type parameterType)
    {
      using (FieldDescription.tracer.TraceMethod((object) parameterType))
      {
        if (parameterType == null)
          throw FieldDescription.tracer.NewArgumentNullException(nameof (parameterType));
        this.SetParameterTypeName(parameterType.Name);
        this.SetParameterTypeFullName(parameterType.FullName);
        this.SetParameterAssemblyFullName(parameterType.AssemblyQualifiedName);
      }
    }

    public string ParameterTypeName
    {
      get
      {
        using (FieldDescription.tracer.TraceProperty(this.parameterTypeName, new object[0]))
        {
          if (string.IsNullOrEmpty(this.parameterTypeName))
            this.SetParameterType(Type.GetType("System.String"));
          return this.parameterTypeName;
        }
      }
    }

    public string ParameterTypeFullName
    {
      get
      {
        using (FieldDescription.tracer.TraceProperty(this.parameterTypeFullName, new object[0]))
        {
          if (string.IsNullOrEmpty(this.parameterTypeFullName))
            this.SetParameterType(Type.GetType("System.String"));
          return this.parameterTypeFullName;
        }
      }
    }

    public string ParameterAssemblyFullName
    {
      get
      {
        using (FieldDescription.tracer.TraceProperty(this.parameterAssemblyFullName, new object[0]))
        {
          if (string.IsNullOrEmpty(this.parameterAssemblyFullName))
            this.SetParameterType(Type.GetType("System.String"));
          return this.parameterAssemblyFullName;
        }
      }
    }

    public string Label
    {
      get
      {
        using (FieldDescription.tracer.TraceProperty(this.label, new object[0]))
          return this.label;
      }
      set
      {
        using (FieldDescription.tracer.TraceProperty(value, new object[0]))
          this.label = value != null ? value : throw FieldDescription.tracer.NewArgumentNullException(nameof (value));
      }
    }

    public string HelpMessage
    {
      get
      {
        using (FieldDescription.tracer.TraceProperty(this.helpMessage, new object[0]))
          return this.helpMessage;
      }
      set
      {
        using (FieldDescription.tracer.TraceProperty(value, new object[0]))
          this.helpMessage = value != null ? value : throw FieldDescription.tracer.NewArgumentNullException(nameof (value));
      }
    }

    public bool IsMandatory
    {
      get
      {
        using (FieldDescription.tracer.TraceProperty((object) this.isMandatory))
          return this.isMandatory;
      }
      set
      {
        using (FieldDescription.tracer.TraceProperty((object) value))
          this.isMandatory = value;
      }
    }

    public PSObject DefaultValue
    {
      get
      {
        using (FieldDescription.tracer.TraceProperty((object) this.defaultValue))
          return this.defaultValue;
      }
      set
      {
        using (FieldDescription.tracer.TraceProperty((object) value))
          this.defaultValue = value;
      }
    }

    public Collection<Attribute> Attributes
    {
      get
      {
        using (FieldDescription.tracer.TraceProperty((object) this.metadata))
        {
          if (this.metadata == null)
            this.metadata = new Collection<Attribute>();
          return this.metadata;
        }
      }
    }

    internal void SetParameterTypeName(string nameOfType)
    {
      using (FieldDescription.tracer.TraceMethod(nameOfType, new object[0]))
        this.parameterTypeName = !string.IsNullOrEmpty(nameOfType) ? nameOfType : throw FieldDescription.tracer.NewArgumentException(nameof (nameOfType), "DescriptionsStrings", "NullOrEmptyErrorTemplate", (object) nameof (nameOfType));
    }

    internal void SetParameterTypeFullName(string fullNameOfType)
    {
      using (FieldDescription.tracer.TraceMethod(fullNameOfType, new object[0]))
        this.parameterTypeFullName = !string.IsNullOrEmpty(fullNameOfType) ? fullNameOfType : throw FieldDescription.tracer.NewArgumentException(nameof (fullNameOfType), "DescriptionsStrings", "NullOrEmptyErrorTemplate", (object) nameof (fullNameOfType));
    }

    internal void SetParameterAssemblyFullName(string fullNameOfAssembly)
    {
      using (FieldDescription.tracer.TraceMethod(fullNameOfAssembly, new object[0]))
        this.parameterAssemblyFullName = !string.IsNullOrEmpty(fullNameOfAssembly) ? fullNameOfAssembly : throw FieldDescription.tracer.NewArgumentException(nameof (fullNameOfAssembly), "DescriptionsStrings", "NullOrEmptyErrorTemplate", (object) nameof (fullNameOfAssembly));
    }

    internal bool ModifiedByRemotingProtocol
    {
      get => this.modifiedByRemotingProtocol;
      set => this.modifiedByRemotingProtocol = value;
    }

    internal bool IsFromRemoteHost
    {
      get => this.isFromRemoteHost;
      set => this.isFromRemoteHost = value;
    }
  }
}
