// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Diagnostics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Diagnostics;
using System.Text;

namespace System.Management.Automation
{
  internal sealed class Diagnostics
  {
    private static object throwInsteadOfAssertLock = (object) 1;
    private static bool throwInsteadOfAssert = false;
    [TraceSource("ASSERT", "ASSERT")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ASSERT", "Msh Assertions");

    internal static string StackTrace(int framesToSkip)
    {
      StackFrame[] frames = new System.Diagnostics.StackTrace(true).GetFrames();
      StringBuilder stringBuilder = new StringBuilder();
      int num = 10 + framesToSkip;
      for (int index = framesToSkip; index < frames.Length && index < num; ++index)
      {
        StackFrame stackFrame = frames[index];
        stringBuilder.Append(stackFrame.ToString());
      }
      return stringBuilder.ToString();
    }

    internal static bool ThrowInsteadOfAssert
    {
      get
      {
        lock (System.Management.Automation.Diagnostics.throwInsteadOfAssertLock)
          return System.Management.Automation.Diagnostics.throwInsteadOfAssert;
      }
      set
      {
        lock (System.Management.Automation.Diagnostics.throwInsteadOfAssertLock)
          System.Management.Automation.Diagnostics.throwInsteadOfAssert = value;
      }
    }

    private Diagnostics()
    {
    }

    [Conditional("DEBUG")]
    [Conditional("ASSERTIONS_TRACE")]
    internal static void Assert(bool condition, string whyThisShouldNeverHappen) => System.Management.Automation.Diagnostics.Assert(condition, whyThisShouldNeverHappen, string.Empty);

    [Conditional("DEBUG")]
    [Conditional("ASSERTIONS_TRACE")]
    internal static void Assert(
      bool condition,
      string whyThisShouldNeverHappen,
      string detailMessage)
    {
      if (System.Management.Automation.Diagnostics.ThrowInsteadOfAssert && !condition)
      {
        AssertException assertException = new AssertException("ASSERT: " + whyThisShouldNeverHappen + "  " + detailMessage + " ");
        System.Management.Automation.Diagnostics.tracer.TraceException((Exception) assertException);
        throw assertException;
      }
      Debug.Assert(condition, whyThisShouldNeverHappen, detailMessage);
    }
  }
}
