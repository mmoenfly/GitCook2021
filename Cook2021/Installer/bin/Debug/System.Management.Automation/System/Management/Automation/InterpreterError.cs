// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.InterpreterError
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Resources;

namespace System.Management.Automation
{
  internal static class InterpreterError
  {
    [TraceSource("Parser", "Parser")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");

    internal static RuntimeException NewInterpreterException(
      object targetObject,
      Type exceptionType,
      Token errToken,
      string resourceIdAndErrorId,
      params object[] args)
    {
      return InterpreterError.NewInterpreterExceptionWithInnerException(targetObject, exceptionType, errToken, resourceIdAndErrorId, (Exception) null, args);
    }

    internal static RuntimeException NewInterpreterExceptionWithInnerException(
      object targetObject,
      Type exceptionType,
      Token errToken,
      string resourceIdAndErrorId,
      Exception innerException,
      params object[] args)
    {
      if (string.IsNullOrEmpty(resourceIdAndErrorId))
        throw InterpreterError.tracer.NewArgumentException(nameof (resourceIdAndErrorId));
      RuntimeException runtimeException;
      try
      {
        string message = args == null || args.Length == 0 ? ResourceManagerCache.GetResourceString("Parser", resourceIdAndErrorId) : ResourceManagerCache.FormatResourceString("Parser", resourceIdAndErrorId, args);
        runtimeException = !string.IsNullOrEmpty(message) ? InterpreterError.NewInterpreterExceptionByMessage(exceptionType, errToken, message, resourceIdAndErrorId, innerException) : InterpreterError.NewBackupInterpreterException(exceptionType, errToken, resourceIdAndErrorId, (Exception) null);
      }
      catch (InvalidOperationException ex)
      {
        runtimeException = InterpreterError.NewBackupInterpreterException(exceptionType, errToken, resourceIdAndErrorId, (Exception) ex);
      }
      catch (MissingManifestResourceException ex)
      {
        runtimeException = InterpreterError.NewBackupInterpreterException(exceptionType, errToken, resourceIdAndErrorId, (Exception) ex);
      }
      catch (FormatException ex)
      {
        runtimeException = InterpreterError.NewBackupInterpreterException(exceptionType, errToken, resourceIdAndErrorId, (Exception) ex);
      }
      runtimeException.SetTargetObject(targetObject);
      return runtimeException;
    }

    internal static RuntimeException NewInterpreterExceptionByMessage(
      Type exceptionType,
      Token errToken,
      string message,
      string errorId,
      Exception innerException)
    {
      RuntimeException runtimeException;
      if (exceptionType == typeof (ParseException))
        runtimeException = (RuntimeException) new ParseException(message, errorId, innerException);
      else if (exceptionType == typeof (IncompleteParseException))
      {
        runtimeException = errToken == null || !errToken.EndOfInput() || !string.IsNullOrEmpty(errToken.File) ? (RuntimeException) new ParseException(message, errorId, innerException) : (RuntimeException) new IncompleteParseException(message, errorId, innerException);
      }
      else
      {
        runtimeException = new RuntimeException(message, innerException);
        runtimeException.SetErrorId(errorId);
        runtimeException.SetErrorCategory(ErrorCategory.InvalidOperation);
      }
      if (errToken != null)
        runtimeException.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, errToken));
      InterpreterError.tracer.TraceException((Exception) runtimeException);
      return runtimeException;
    }

    private static RuntimeException NewBackupInterpreterException(
      Type exceptionType,
      Token errToken,
      string errorId,
      Exception innerException)
    {
      string message;
      if (innerException == null)
        message = ResourceManagerCache.FormatResourceString("Parser", "BackupParserMessage", (object) errorId);
      else
        message = ResourceManagerCache.FormatResourceString("Parser", "BackupParserMessageWithException", (object) errorId, (object) innerException.Message);
      return InterpreterError.NewInterpreterExceptionByMessage(exceptionType, errToken, message, errorId, innerException);
    }
  }
}
