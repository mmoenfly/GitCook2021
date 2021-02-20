// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PathInfoStack
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  public sealed class PathInfoStack : Stack<PathInfo>
  {
    [TraceSource("PathInfoStack", "An object that represents a location stack.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PathInfoStack), "An object that represents a location stack.");
    private string stackName;

    internal PathInfoStack(string stackName, Stack<PathInfo> locationStack)
    {
      using (PathInfoStack.tracer.TraceConstructor((object) this))
      {
        if (locationStack == null)
          throw PathInfoStack.tracer.NewArgumentNullException(nameof (locationStack));
        this.stackName = !string.IsNullOrEmpty(stackName) ? stackName : throw PathInfoStack.tracer.NewArgumentException(nameof (stackName));
        PathInfo[] array = new PathInfo[locationStack.Count];
        locationStack.CopyTo(array, 0);
        for (int index = array.Length - 1; index >= 0; --index)
          this.Push(array[index]);
      }
    }

    public string Name => this.stackName;
  }
}
