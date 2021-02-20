// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSTraceSource
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Management.Automation
{
  public class PSTraceSource
  {
    private const string errorFormatter = "ERROR: ";
    private const string warningFormatter = "Warning: ";
    private const string verboseFormatter = "Verbose: ";
    private const string writeLineFormatter = "";
    private const string constructorOutputFormatter = "Enter Ctor {0}";
    private const string constructorLeavingFormatter = "Leave Ctor {0}";
    private const string disposeOutputFormatter = "Enter Disposer {0}";
    private const string disposeLeavingFormatter = "Leave Disposer {0}";
    private const string methodOutputFormatter = "Enter {0}:";
    private const string methodLeavingFormatter = "Leave {0}";
    private const string propertyOutputFormatter = "Enter property {0}:";
    private const string propertyLeavingFormatter = "Leave property {0}";
    private const string delegateHandlerOutputFormatter = "Enter delegate handler: {0}:";
    private const string delegateHandlerLeavingFormatter = "Leave delegate handler: {0}";
    private const string eventHandlerOutputFormatter = "Enter event handler: {0}:";
    private const string eventHandlerLeavingFormatter = "Leave event handler: {0}";
    private const string exceptionOutputFormatter = "{0}: {1}\n{2}";
    private const string innermostExceptionOutputFormatter = "Inner-most {0}: {1}\n{2}";
    private const string lockEnterFormatter = "Enter Lock: {0}";
    private const string lockLeavingFormatter = "Leave Lock: {0}";
    private const string lockAcquiringFormatter = "Acquiring Lock: {0}";
    private static bool globalTraceInitialized;
    private bool alreadyTracing;
    private static LocalDataStoreSlot threadIndentLevel = Thread.AllocateDataSlot();
    private PSTraceSourceOptions flags;
    private string description = string.Empty;
    private bool showHeaders = true;
    private string fullName = string.Empty;
    private string name;
    private TraceSource traceSource;
    private static Dictionary<string, PSTraceSource> traceCatalog = new Dictionary<string, PSTraceSource>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, PSTraceSource> preConfiguredTraceSource = new Dictionary<string, PSTraceSource>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);

    internal static PSTraceSource GetTracer(string name, string description) => PSTraceSource.GetTracer(name, description, true);

    internal static PSTraceSource GetTracer(
      string name,
      string description,
      bool traceHeaders)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException(nameof (name));
      PSTraceSource psTraceSource1 = (PSTraceSource) null;
      if (PSTraceSource.TraceCatalog.ContainsKey(name))
        psTraceSource1 = PSTraceSource.TraceCatalog[name];
      if (psTraceSource1 == null)
      {
        string str = name;
        if (!PSTraceSource.PreConfiguredTraceSource.ContainsKey(str))
        {
          if (str.Length > 16)
          {
            str = str.Substring(0, 16);
            if (!PSTraceSource.PreConfiguredTraceSource.ContainsKey(str))
              str = (string) null;
          }
          else
            str = (string) null;
        }
        if (str != null)
        {
          PSTraceSource psTraceSource2 = PSTraceSource.PreConfiguredTraceSource[str];
          psTraceSource1 = PSTraceSource.GetNewTraceSource(str, description, traceHeaders);
          psTraceSource1.Options = psTraceSource2.Options;
          psTraceSource1.Listeners.Clear();
          psTraceSource1.Listeners.AddRange(psTraceSource2.Listeners);
          PSTraceSource.TraceCatalog.Add(str, psTraceSource1);
          PSTraceSource.PreConfiguredTraceSource.Remove(str);
        }
      }
      if (psTraceSource1 == null)
      {
        psTraceSource1 = PSTraceSource.GetNewTraceSource(name, description, traceHeaders);
        PSTraceSource.TraceCatalog[psTraceSource1.FullName] = psTraceSource1;
      }
      if (psTraceSource1.Options != PSTraceSourceOptions.None && traceHeaders)
      {
        psTraceSource1.TraceGlobalAppDomainHeader();
        psTraceSource1.TracerObjectHeader(Assembly.GetCallingAssembly());
      }
      return psTraceSource1;
    }

    internal static PSTraceSource GetNewTraceSource(
      string name,
      string description,
      bool traceHeaders)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException(nameof (name));
      return new PSTraceSource(name, name, description, traceHeaders);
    }

    internal PSArgumentNullException NewArgumentNullException(
      string paramName)
    {
      string message = !string.IsNullOrEmpty(paramName) ? ResourceManagerCache.FormatResourceString(Assembly.GetAssembly(typeof (PSObject)), "AutomationExceptions", "ArgumentNull", (object) paramName) : throw new ArgumentNullException(nameof (paramName));
      PSArgumentNullException argumentNullException = new PSArgumentNullException(paramName, message);
      this.TraceException((Exception) argumentNullException);
      return argumentNullException;
    }

    internal PSArgumentNullException NewArgumentNullException(
      string paramName,
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (string.IsNullOrEmpty(paramName))
        throw this.NewArgumentNullException(nameof (paramName));
      if (string.IsNullOrEmpty(baseName))
        throw this.NewArgumentNullException(nameof (baseName));
      if (string.IsNullOrEmpty(resourceId))
        throw this.NewArgumentNullException(nameof (resourceId));
      string message = ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args);
      PSArgumentNullException argumentNullException = new PSArgumentNullException(paramName, message);
      this.TraceException((Exception) argumentNullException);
      return argumentNullException;
    }

    internal PSArgumentException NewArgumentException(string paramName)
    {
      PSArgumentException argumentException = !string.IsNullOrEmpty(paramName) ? new PSArgumentException(ResourceManagerCache.FormatResourceString(Assembly.GetAssembly(typeof (PSObject)), "AutomationExceptions", "Argument", (object) paramName), paramName) : throw new ArgumentNullException(nameof (paramName));
      this.TraceException((Exception) argumentException);
      return argumentException;
    }

    internal PSArgumentException NewArgumentException(
      string paramName,
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (string.IsNullOrEmpty(paramName))
        throw this.NewArgumentNullException(nameof (paramName));
      if (string.IsNullOrEmpty(baseName))
        throw this.NewArgumentNullException(nameof (baseName));
      if (string.IsNullOrEmpty(resourceId))
        throw this.NewArgumentNullException(nameof (resourceId));
      PSArgumentException argumentException = new PSArgumentException(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args), paramName);
      this.TraceException((Exception) argumentException);
      return argumentException;
    }

    internal PSInvalidOperationException NewInvalidOperationException()
    {
      PSInvalidOperationException operationException = new PSInvalidOperationException(ResourceManagerCache.FormatResourceString(Assembly.GetAssembly(typeof (PSObject)), "AutomationExceptions", "InvalidOperation", (object) new StackTrace().GetFrame(1).GetMethod().Name));
      this.TraceException((Exception) operationException);
      return operationException;
    }

    internal PSInvalidOperationException NewInvalidOperationException(
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (string.IsNullOrEmpty(baseName))
        throw this.NewArgumentNullException(nameof (baseName));
      if (string.IsNullOrEmpty(resourceId))
        throw this.NewArgumentNullException(nameof (resourceId));
      PSInvalidOperationException operationException = new PSInvalidOperationException(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args));
      this.TraceException((Exception) operationException);
      return operationException;
    }

    internal PSInvalidOperationException NewInvalidOperationException(
      Exception innerException,
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (string.IsNullOrEmpty(baseName))
        throw this.NewArgumentNullException(nameof (baseName));
      if (string.IsNullOrEmpty(resourceId))
        throw this.NewArgumentNullException(nameof (resourceId));
      PSInvalidOperationException operationException = new PSInvalidOperationException(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args), innerException);
      this.TraceException((Exception) operationException);
      return operationException;
    }

    internal PSNotSupportedException NewNotSupportedException()
    {
      PSNotSupportedException supportedException = new PSNotSupportedException(ResourceManagerCache.FormatResourceString(Assembly.GetAssembly(typeof (PSObject)), "AutomationExceptions", "NotSupported", (object) new StackTrace().GetFrame(0).ToString()));
      this.TraceException((Exception) supportedException);
      return supportedException;
    }

    internal PSNotSupportedException NewNotSupportedException(
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (string.IsNullOrEmpty(baseName))
        throw this.NewArgumentNullException(nameof (baseName));
      if (string.IsNullOrEmpty(resourceId))
        throw this.NewArgumentNullException(nameof (resourceId));
      PSNotSupportedException supportedException = new PSNotSupportedException(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args));
      this.TraceException((Exception) supportedException);
      return supportedException;
    }

    internal PSNotImplementedException NewNotImplementedException()
    {
      PSNotImplementedException implementedException = new PSNotImplementedException(ResourceManagerCache.FormatResourceString(Assembly.GetAssembly(typeof (PSObject)), "AutomationExceptions", "NotImplemented", (object) new StackTrace().GetFrame(0).ToString()));
      this.TraceException((Exception) implementedException);
      return implementedException;
    }

    internal PSArgumentOutOfRangeException NewArgumentOutOfRangeException(
      string paramName,
      object actualValue)
    {
      string message = !string.IsNullOrEmpty(paramName) ? ResourceManagerCache.FormatResourceString(Assembly.GetAssembly(typeof (PSObject)), "AutomationExceptions", "ArgumentOutOfRange", (object) paramName) : throw new ArgumentNullException(nameof (paramName));
      PSArgumentOutOfRangeException ofRangeException = new PSArgumentOutOfRangeException(paramName, actualValue, message);
      this.TraceException((Exception) ofRangeException);
      return ofRangeException;
    }

    internal PSArgumentOutOfRangeException NewArgumentOutOfRangeException(
      string paramName,
      object actualValue,
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (string.IsNullOrEmpty(paramName))
        throw this.NewArgumentNullException(nameof (paramName));
      if (string.IsNullOrEmpty(baseName))
        throw this.NewArgumentNullException(nameof (baseName));
      if (string.IsNullOrEmpty(resourceId))
        throw this.NewArgumentNullException(nameof (resourceId));
      string message = ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args);
      PSArgumentOutOfRangeException ofRangeException = new PSArgumentOutOfRangeException(paramName, actualValue, message);
      this.TraceException((Exception) ofRangeException);
      return ofRangeException;
    }

    internal PSObjectDisposedException NewObjectDisposedException(
      string objectName)
    {
      string message = !string.IsNullOrEmpty(objectName) ? ResourceManagerCache.FormatResourceString(Assembly.GetAssembly(typeof (PSObject)), "AutomationExceptions", "ObjectDisposed", (object) objectName) : throw this.NewArgumentNullException(nameof (objectName));
      PSObjectDisposedException disposedException = new PSObjectDisposedException(objectName, message);
      this.TraceException((Exception) disposedException);
      return disposedException;
    }

    internal PSTraceSource(string fullName, string name, string description, bool traceHeaders)
    {
      if (string.IsNullOrEmpty(fullName))
        throw new ArgumentNullException(nameof (fullName));
      try
      {
        this.fullName = fullName;
        this.name = name;
        if (string.Equals(Environment.GetEnvironmentVariable("MshEnableTrace"), "True", StringComparison.OrdinalIgnoreCase))
        {
          string attribute = this.TraceSource.Attributes[nameof (Options)];
          if (attribute != null)
            this.flags = (PSTraceSourceOptions) Enum.Parse(typeof (PSTraceSourceOptions), attribute, true);
        }
        this.showHeaders = traceHeaders;
        this.description = description;
      }
      catch (XmlException ex)
      {
        this.flags = PSTraceSourceOptions.None;
      }
      catch (ConfigurationException ex)
      {
        this.flags = PSTraceSourceOptions.None;
      }
    }

    internal void TraceGlobalAppDomainHeader()
    {
      if (PSTraceSource.globalTraceInitialized)
        return;
      this.OutputLine(PSTraceSourceOptions.All, "Initializing tracing for AppDomain: {0}", (object) AppDomain.CurrentDomain.FriendlyName);
      this.OutputLine(PSTraceSourceOptions.All, "\tCurrent time: {0}", (object) DateTime.Now);
      this.OutputLine(PSTraceSourceOptions.All, "\tOS Build: {0}", (object) Environment.OSVersion.ToString());
      this.OutputLine(PSTraceSourceOptions.All, "\tFramework Build: {0}\n", (object) Environment.Version.ToString());
      PSTraceSource.globalTraceInitialized = true;
    }

    internal void TracerObjectHeader(Assembly callingAssembly)
    {
      if (this.flags == PSTraceSourceOptions.None)
        return;
      this.OutputLine(PSTraceSourceOptions.All, "Creating tracer:");
      this.OutputLine(PSTraceSourceOptions.All, "\tCategory: {0}", (object) this.Name);
      this.OutputLine(PSTraceSourceOptions.All, "\tDescription: {0}", (object) this.Description);
      if (callingAssembly != null)
      {
        this.OutputLine(PSTraceSourceOptions.All, "\tAssembly: {0}", (object) callingAssembly.FullName);
        this.OutputLine(PSTraceSourceOptions.All, "\tAssembly Location: {0}", (object) callingAssembly.Location);
        this.OutputLine(PSTraceSourceOptions.All, "\tAssembly File Timestamp: {0}", (object) new FileInfo(callingAssembly.Location).CreationTime);
      }
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("\tFlags: ");
      stringBuilder.Append(this.flags.ToString());
      this.OutputLine(PSTraceSourceOptions.All, stringBuilder.ToString());
    }

    internal IDisposable TraceConstructor(object objBeingConstructed)
    {
      if ((this.flags & PSTraceSourceOptions.Constructor) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Constructor, "Enter Ctor {0}", "Leave Ctor {0}", PSTraceSource.GetObjectStringRepresentation(objBeingConstructed));
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceConstructor(
      object objBeingConstructed,
      string format,
      params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Constructor) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Constructor, "Enter Ctor {0}", "Leave Ctor {0}", PSTraceSource.GetObjectStringRepresentation(objBeingConstructed), format, args);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceConstructor(object objBeingConstructed, object objToTrace)
    {
      if ((this.flags & PSTraceSourceOptions.Constructor) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Constructor, "Enter Ctor {0}", "Leave Ctor {0}", PSTraceSource.GetObjectStringRepresentation(objBeingConstructed), "{0}", new object[1]
          {
            objToTrace == null ? (object) "null" : (object) objToTrace.ToString()
          });
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceDispose(object objBeingDisposed)
    {
      if ((this.flags & PSTraceSourceOptions.Dispose) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Dispose, "Enter Disposer {0}", "Leave Disposer {0}", PSTraceSource.GetObjectStringRepresentation(objBeingDisposed));
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceDispose(
      object objBeingDisposed,
      string format,
      params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Dispose) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Dispose, "Enter Disposer {0}", "Leave Disposer {0}", PSTraceSource.GetObjectStringRepresentation(objBeingDisposed), format, args);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceScope(string format, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Scope) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Scope, (string) null, (string) null, string.Empty, format, args);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal void TraceFinalizer(object objBeingFinalized)
    {
      if ((this.flags & PSTraceSourceOptions.Finalizer) == PSTraceSourceOptions.None)
        return;
      this.TraceFinalizerHelper(objBeingFinalized, "");
    }

    internal void TraceFinalizer(object objBeingFinalized, string format, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Finalizer) == PSTraceSourceOptions.None)
        return;
      this.TraceFinalizerHelper(objBeingFinalized, format, args);
    }

    private void TraceFinalizerHelper(
      object objBeingFinalized,
      string format,
      params object[] args)
    {
      try
      {
        StringBuilder stringBuilder = new StringBuilder("Finalize: ");
        if (objBeingFinalized != null)
          stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0}: {1:d}:", (object) objBeingFinalized.GetType(), (object) objBeingFinalized.GetHashCode());
        stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, format, args);
        this.OutputLine(PSTraceSourceOptions.Finalizer, stringBuilder.ToString());
      }
      catch
      {
      }
    }

    internal IDisposable TraceMethod()
    {
      if ((this.flags & PSTraceSourceOptions.Method) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Method, "Enter {0}:", "Leave {0}", PSTraceSource.GetCallingMethodNameAndParameters(1));
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceMethod(string format, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Method) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Method, "Enter {0}:", "Leave {0}", PSTraceSource.GetCallingMethodNameAndParameters(1), format, args);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceMethod(object o)
    {
      if ((this.flags & PSTraceSourceOptions.Method) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Method, "Enter {0}:", "Leave {0}", PSTraceSource.GetCallingMethodNameAndParameters(1), string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0}", o == null ? (object) "null" : (object) o.ToString()), new object[0]);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceProperty()
    {
      if ((this.flags & PSTraceSourceOptions.Property) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Property, "Enter property {0}:", "Leave property {0}", PSTraceSource.GetCallingMethodNameAndParameters(1));
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceProperty(string format, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Property) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Property, "Enter property {0}:", "Leave property {0}", PSTraceSource.GetCallingMethodNameAndParameters(1), format, args);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceProperty(object o)
    {
      if ((this.flags & PSTraceSourceOptions.Property) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Property, "Enter property {0}:", "Leave property {0}", PSTraceSource.GetCallingMethodNameAndParameters(1), string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0}", o == null ? (object) "null" : (object) o.ToString()), new object[0]);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal void TraceDelegate(object delegateObj)
    {
      if ((this.flags & PSTraceSourceOptions.Delegates) == PSTraceSourceOptions.None)
        return;
      this.TraceDelegateHelper(delegateObj, "");
    }

    internal void TraceDelegate(object delegateObj, string format, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Delegates) == PSTraceSourceOptions.None)
        return;
      if (format != null)
        this.TraceDelegateHelper(delegateObj, format, args);
      else
        this.TraceDelegateHelper(delegateObj, "");
    }

    private void TraceDelegateHelper(object delegateObj, string format, params object[] args)
    {
      try
      {
        Type type = delegateObj.GetType();
        StringBuilder stringBuilder = new StringBuilder("Delegating: ");
        stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0}: ", (object) type.Name);
        stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, format, args);
        this.OutputLine(PSTraceSourceOptions.Delegates, stringBuilder.ToString());
      }
      catch
      {
      }
    }

    internal IDisposable TraceDelegateHandler()
    {
      if ((this.flags & PSTraceSourceOptions.Delegates) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Delegates, "Enter delegate handler: {0}:", "Leave delegate handler: {0}", PSTraceSource.GetCallingMethodNameAndParameters(1), "", new object[0]);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceDelegateHandler(string format, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Delegates) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Delegates, "Enter delegate handler: {0}:", "Leave delegate handler: {0}", PSTraceSource.GetCallingMethodNameAndParameters(1), format, args);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal void TraceRaiseEvent(object eventObj)
    {
      if ((this.flags & PSTraceSourceOptions.Events) == PSTraceSourceOptions.None)
        return;
      this.TraceRaiseEventHelper(eventObj, "");
    }

    internal void TraceRaiseEvent(object eventObj, string format, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Events) == PSTraceSourceOptions.None)
        return;
      this.TraceRaiseEventHelper(eventObj, format, args);
    }

    private void TraceRaiseEventHelper(object eventObj, string format, params object[] args)
    {
      try
      {
        Type type = eventObj.GetType();
        StringBuilder stringBuilder = new StringBuilder("Raising Event: ");
        stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0}: ", (object) type.Name);
        if (format != null)
          stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, format, args);
        this.OutputLine(PSTraceSourceOptions.Events, stringBuilder.ToString());
      }
      catch
      {
      }
    }

    internal IDisposable TraceEventHandlers()
    {
      if ((this.flags & PSTraceSourceOptions.Events) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Events, "Enter event handler: {0}:", "Leave event handler: {0}", PSTraceSource.GetCallingMethodNameAndParameters(1), "", new object[0]);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal IDisposable TraceEventHandlers(string format, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Events) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Events, "Enter event handler: {0}:", "Leave event handler: {0}", PSTraceSource.GetCallingMethodNameAndParameters(1), format, args);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal void TraceException(Exception exceptionRecord)
    {
      if ((this.flags & PSTraceSourceOptions.Exception) == PSTraceSourceOptions.None)
        return;
      this.WriteException(exceptionRecord, true, "{0}: {1}\n{2}");
    }

    internal void TraceException(Exception exceptionRecord, bool traceInnerMostExceptionToo)
    {
      if ((this.flags & PSTraceSourceOptions.Exception) == PSTraceSourceOptions.None)
        return;
      this.WriteException(exceptionRecord, traceInnerMostExceptionToo, "{0}: {1}\n{2}");
    }

    private void WriteException(
      Exception exceptionRecord,
      bool traceInnerMostExceptionToo,
      string outputFormatter)
    {
      try
      {
        this.OutputLine(PSTraceSourceOptions.Exception, string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, outputFormatter, (object) exceptionRecord.GetType().ToString(), (object) exceptionRecord.Message, (object) exceptionRecord.StackTrace));
        if (!traceInnerMostExceptionToo || exceptionRecord.InnerException == null)
          return;
        this.WriteException(PSTraceSource.GetInnerMostException(exceptionRecord), false, "Inner-most {0}: {1}\n{2}");
      }
      catch
      {
      }
    }

    private static Exception GetInnerMostException(Exception exceptionRecord)
    {
      while (exceptionRecord.InnerException != null)
        exceptionRecord = exceptionRecord.InnerException;
      return exceptionRecord;
    }

    internal IDisposable TraceLock(string lockName)
    {
      if ((this.flags & PSTraceSourceOptions.Lock) != PSTraceSourceOptions.None)
      {
        try
        {
          return (IDisposable) new ScopeTracer(this, PSTraceSourceOptions.Lock, "Enter Lock: {0}", "Leave Lock: {0}", lockName);
        }
        catch
        {
        }
      }
      return (IDisposable) null;
    }

    internal void TraceLockAcquiring(string lockName)
    {
      if ((this.flags & PSTraceSourceOptions.Lock) == PSTraceSourceOptions.None)
        return;
      this.TraceLockHelper("Acquiring Lock: {0}", lockName);
    }

    internal void TraceLockAcquired(string lockName)
    {
      if ((this.flags & PSTraceSourceOptions.Lock) == PSTraceSourceOptions.None)
        return;
      this.TraceLockHelper("Enter Lock: {0}", lockName);
    }

    internal void TraceLockReleased(string lockName)
    {
      if ((this.flags & PSTraceSourceOptions.Lock) == PSTraceSourceOptions.None)
        return;
      this.TraceLockHelper("Leave Lock: {0}", lockName);
    }

    private void TraceLockHelper(string formatter, string lockName)
    {
      try
      {
        this.OutputLine(PSTraceSourceOptions.Lock, formatter, (object) lockName);
      }
      catch
      {
      }
    }

    internal void TraceError(string errorMessageFormat, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Error) == PSTraceSourceOptions.None)
        return;
      this.FormatOutputLine(PSTraceSourceOptions.Error, "ERROR: ", errorMessageFormat, args);
    }

    internal void TraceWarning(string warningMessageFormat, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Warning) == PSTraceSourceOptions.None)
        return;
      this.FormatOutputLine(PSTraceSourceOptions.Warning, "Warning: ", warningMessageFormat, args);
    }

    internal void TraceVerbose(string verboseMessageFormat, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.Verbose) == PSTraceSourceOptions.None)
        return;
      this.FormatOutputLine(PSTraceSourceOptions.Verbose, "Verbose: ", verboseMessageFormat, args);
    }

    internal void WriteLine(string format, params object[] args)
    {
      if ((this.flags & PSTraceSourceOptions.WriteLine) == PSTraceSourceOptions.None)
        return;
      this.FormatOutputLine(PSTraceSourceOptions.WriteLine, "", format, args);
    }

    internal void WriteLine(object arg)
    {
      if ((this.flags & PSTraceSourceOptions.WriteLine) == PSTraceSourceOptions.None)
        return;
      this.WriteLine("{0}", arg == null ? (object) "null" : (object) arg.ToString());
    }

    private void FormatOutputLine(
      PSTraceSourceOptions flag,
      string classFormatter,
      string format,
      params object[] args)
    {
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (classFormatter != null)
          stringBuilder.Append(classFormatter);
        if (format != null)
          stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, format, args);
        this.OutputLine(flag, stringBuilder.ToString());
      }
      catch
      {
      }
    }

    private static string GetCallingMethodNameAndParameters(int skipFrames)
    {
      StringBuilder stringBuilder = (StringBuilder) null;
      try
      {
        MethodBase method = new StackFrame(++skipFrames).GetMethod();
        Type declaringType = method.DeclaringType;
        stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0}.{1}(", (object) declaringType.Name, (object) method.Name);
        stringBuilder.Append(")");
      }
      catch
      {
      }
      return stringBuilder.ToString();
    }

    private static string GetObjectStringRepresentation(object obj) => string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0}: {1} ", (object) obj.GetType(), (object) obj.GetHashCode());

    private static StringBuilder GetLinePrefix(PSTraceSourceOptions flag)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, " {0,-11} ", (object) Enum.GetName(typeof (PSTraceSourceOptions), (object) flag));
      return stringBuilder;
    }

    private static void AddTab(ref StringBuilder lineBuilder)
    {
      int indentSize = Trace.IndentSize;
      int threadIndentLevel = PSTraceSource.ThreadIndentLevel;
      for (int index = 0; index < indentSize * threadIndentLevel; ++index)
        lineBuilder.Append(" ");
    }

    internal void OutputLine(PSTraceSourceOptions flag, string format, params object[] args)
    {
      if (this.alreadyTracing)
        return;
      this.alreadyTracing = true;
      try
      {
        StringBuilder lineBuilder = new StringBuilder();
        if (this.showHeaders)
          lineBuilder.Append((object) PSTraceSource.GetLinePrefix(flag));
        PSTraceSource.AddTab(ref lineBuilder);
        if (args != null && args.Length > 0)
        {
          for (int index = 0; index < args.Length; ++index)
          {
            if (args[index] == null)
              args[index] = (object) "null";
          }
          lineBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, format, args);
        }
        else
          lineBuilder.Append(format);
        this.TraceSource.TraceInformation(lineBuilder.ToString());
      }
      finally
      {
        this.alreadyTracing = false;
      }
    }

    internal static int ThreadIndentLevel
    {
      get
      {
        object data = Thread.GetData(PSTraceSource.threadIndentLevel);
        if (data == null)
        {
          int num = 0;
          Thread.SetData(PSTraceSource.threadIndentLevel, (object) num);
          data = Thread.GetData(PSTraceSource.threadIndentLevel);
        }
        return (int) data;
      }
      set
      {
        if (value < 0)
          return;
        Thread.SetData(PSTraceSource.threadIndentLevel, (object) value);
      }
    }

    public string Description
    {
      get => this.description;
      set => this.description = value;
    }

    internal bool ShowHeaders
    {
      get => this.showHeaders;
      set => this.showHeaders = value;
    }

    internal string FullName => this.fullName;

    internal TraceSource TraceSource
    {
      get
      {
        if (this.traceSource == null)
          this.traceSource = (TraceSource) new MonadTraceSource(this.name);
        return this.traceSource;
      }
    }

    public PSTraceSourceOptions Options
    {
      get => this.flags;
      set
      {
        this.flags = value;
        this.TraceSource.Switch.Level = (SourceLevels) this.flags;
      }
    }

    public StringDictionary Attributes => this.TraceSource.Attributes;

    public TraceListenerCollection Listeners => this.TraceSource.Listeners;

    public string Name => this.name;

    public SourceSwitch Switch
    {
      get => this.TraceSource.Switch;
      set => this.TraceSource.Switch = value;
    }

    internal static Dictionary<string, PSTraceSource> TraceCatalog => PSTraceSource.traceCatalog;

    internal static Dictionary<string, PSTraceSource> PreConfiguredTraceSource => PSTraceSource.preConfiguredTraceSource;
  }
}
