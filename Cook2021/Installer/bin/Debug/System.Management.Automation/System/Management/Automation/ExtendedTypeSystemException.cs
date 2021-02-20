// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ExtendedTypeSystemException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class ExtendedTypeSystemException : RuntimeException
  {
    internal const string BaseName = "ExtendedTypeSystem";
    internal const string AccessMemberOutsidePSObjectMsg = "AccessMemberOutsidePSObject";
    internal const string ChangeStaticMemberMsg = "ChangeStaticMember";
    internal const string ReservedMemberNameMsg = "ReservedMemberName";
    internal const string CannotChangeReservedMemberMsg = "CannotChangeReservedMember";
    internal const string CycleInAliasMsg = "CycleInAlias";
    internal const string CodePropertySetterFormatMsg = "CodePropertySetterFormat";
    internal const string CodePropertyGetterFormatMsg = "CodePropertyGetterFormat";
    internal const string CodePropertyGetterAndSetterNullMsg = "CodePropertyGetterAndSetterNull";
    internal const string CodeMethodMethodFormatMsg = "CodeMethodMethodFormat";
    internal const string MemberAlreadyPresentMsg = "MemberAlreadyPresent";
    internal const string MemberAlreadyPresentFromTypesXmlMsg = "MemberAlreadyPresentFromTypesXml";
    internal const string MemberNotPresentMsg = "MemberNotPresent";
    internal const string EnumerationExceptionMsg = "EnumerationException";
    internal const string CannotAddPropertyOrMethodMsg = "CannotAddPropertyOrMethod";
    internal const string TypesXmlErrorMsg = "TypesXmlError";
    internal const string ToStringExceptionMsg = "ToStringException";
    internal const string NotAClsCompliantFieldPropertyMsg = "NotAClsCompliantFieldProperty";
    internal const string ExceptionRetrievingTypeNameHierarchyMsg = "ExceptionRetrievingTypeNameHierarchy";
    internal const string ExceptionGettingMemberMsg = "ExceptionGettingMember";
    internal const string ExceptionGettingMembersMsg = "ExceptionGettingMembers";
    internal const string ExceptionRetrievingPropertyReadStateMsg = "ExceptionRetrievingPropertyReadState";
    internal const string ExceptionRetrievingPropertyWriteStateMsg = "ExceptionRetrievingPropertyWriteState";
    internal const string ExceptionRetrievingPropertyTypeMsg = "ExceptionRetrievingPropertyType";
    internal const string ExceptionRetrievingPropertyStringMsg = "ExceptionRetrievingPropertyString";
    internal const string ExceptionRetrievingPropertyAttributesMsg = "ExceptionRetrievingPropertyAttributes";
    internal const string ExceptionRetrievingMethodDefinitionsMsg = "ExceptionRetrievingMethodDefinitions";
    internal const string ExceptionRetrievingMethodStringMsg = "ExceptionRetrievingMethodString";
    internal const string ExceptionRetrievingParameterizedPropertytypeMsg = "ExceptionRetrievingParameterizedPropertytype";
    internal const string ExceptionRetrievingParameterizedPropertyReadStateMsg = "ExceptionRetrievingParameterizedPropertyReadState";
    internal const string ExceptionRetrievingParameterizedPropertyWriteStateMsg = "ExceptionRetrievingParameterizedPropertyWriteState";
    internal const string ExceptionRetrievingParameterizedPropertyDefinitionsMsg = "ExceptionRetrievingParameterizedPropertyDefinitions";
    internal const string ExceptionRetrievingParameterizedPropertyStringMsg = "ExceptionRetrievingParameterizedPropertyString";
    internal const string CannotSetValueForMemberTypeMsg = "CannotSetValueForMemberType";
    internal const string PropertyNotFoundInTypeDescriptorMsg = "PropertyNotFoundInTypeDescriptor";
    internal const string NotTheSameTypeOrNotIcomparableMsg = "NotTheSameTypeOrNotIcomparable";
    internal const string PropertyIsSettableError = "PropertyIsSettableError";
    internal const string PropertyIsGettableError = "PropertyIsGettableError";
    internal const string PropertyGetError = "PropertyGetError";
    internal const string PropertySetError = "PropertySetError";
    internal const string PropertyTypeError = "PropertyTypeError";
    internal const string GetTypeNameHierarchyError = "GetTypeNameHierarchyError";
    internal const string GetProperties = "GetProperties";
    internal const string GetProperty = "GetProperty";
    internal const string NullReturnValueError = "NullReturnValueError";
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");

    public ExtendedTypeSystemException()
      : base(typeof (ExtendedTypeSystemException).FullName)
      => ExtendedTypeSystemException.tracer.TraceException((Exception) this);

    public ExtendedTypeSystemException(string message)
      : base(message)
      => ExtendedTypeSystemException.tracer.TraceException((Exception) this);

    public ExtendedTypeSystemException(string message, Exception innerException)
      : base(message, innerException)
      => ExtendedTypeSystemException.tracer.TraceException((Exception) this);

    internal ExtendedTypeSystemException(
      string errorId,
      Exception innerException,
      string baseName,
      string resourceId,
      params object[] arguments)
      : base(ResourceManagerCache.FormatResourceString(baseName, resourceId, arguments), innerException)
    {
      this.SetErrorId(errorId);
      ExtendedTypeSystemException.tracer.TraceException((Exception) this);
    }

    protected ExtendedTypeSystemException(SerializationInfo info, StreamingContext context)
      : base(info, context)
      => ExtendedTypeSystemException.tracer.TraceException((Exception) this);
  }
}
