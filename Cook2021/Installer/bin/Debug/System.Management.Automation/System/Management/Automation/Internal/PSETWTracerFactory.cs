// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSETWTracerFactory
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.IO;
using System.Reflection;
using System.Security;

namespace System.Management.Automation.Internal
{
  internal class PSETWTracerFactory
  {
    private static string assemblyFile = Utils.GetApplicationBase(Utils.DefaultPowerShellShellID) + "\\pspluginwkr.dll";
    private static IETWTracerLoader tracerLoader;
    internal static IETWTracer EmptyTracer = (IETWTracer) new PSETWEmptyTracer();
    private static bool tracerAvailable = false;

    static PSETWTracerFactory()
    {
      PSETWTracerFactory.tracerAvailable = PSETWTracerFactory.IsTracerSupportedOnOS();
      if (!PSETWTracerFactory.tracerAvailable)
        return;
      Assembly assembly = (Assembly) null;
      try
      {
        assembly = Assembly.LoadFrom(PSETWTracerFactory.assemblyFile);
      }
      catch (ArgumentNullException ex)
      {
      }
      catch (FileNotFoundException ex)
      {
      }
      catch (FileLoadException ex)
      {
      }
      catch (BadImageFormatException ex)
      {
      }
      catch (SecurityException ex)
      {
      }
      catch (ArgumentException ex)
      {
      }
      catch (PathTooLongException ex)
      {
      }
      if (assembly == null)
        return;
      PSETWTracerFactory.tracerLoader = (IETWTracerLoader) assembly.GetType("System.Management.Automation.Internal.PSETWTracerLoader").GetMethod("GetInstance").Invoke((object) null, (object[]) null);
    }

    internal static IETWTracer GetETWTracer(PSKeyword keyword) => PSETWTracerFactory.tracerLoader != null ? PSETWTracerFactory.tracerLoader.GetETWTracer(keyword) : PSETWTracerFactory.EmptyTracer;

    private static bool IsTracerSupportedOnOS() => Environment.OSVersion.Version.Major >= 6;
  }
}
