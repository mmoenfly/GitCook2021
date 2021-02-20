// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.NativeCultureResolver
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.PowerShell
{
  internal static class NativeCultureResolver
  {
    private static CultureInfo m_uiCulture = (CultureInfo) null;
    private static CultureInfo m_Culture = (CultureInfo) null;
    private static object m_syncObject = new object();
    private static int MUI_LANGUAGE_NAME = 8;
    private static int MUI_CONSOLE_FILTER = 256;
    private static int MUI_MERGE_USER_FALLBACK = 32;
    private static int MUI_MERGE_SYSTEM_FALLBACK = 16;

    internal static CultureInfo UICulture
    {
      get
      {
        if (NativeCultureResolver.m_uiCulture == null)
        {
          lock (NativeCultureResolver.m_syncObject)
          {
            if (NativeCultureResolver.m_uiCulture == null)
            {
              if (!NativeCultureResolver.IsVistaAndLater())
              {
                NativeCultureResolver.m_uiCulture = NativeCultureResolver.EmulateDownLevel();
                return NativeCultureResolver.m_uiCulture;
              }
              string preferredUiLangs = NativeCultureResolver.GetUserPreferredUILangs();
              if (!string.IsNullOrEmpty(preferredUiLangs))
              {
                try
                {
                  string[] strArray = preferredUiLangs.Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
                  string name = strArray[0];
                  string[] fallbacks = (string[]) null;
                  if (strArray.Length > 1)
                  {
                    fallbacks = new string[strArray.Length - 1];
                    Array.Copy((Array) strArray, 1, (Array) fallbacks, 0, strArray.Length - 1);
                  }
                  NativeCultureResolver.m_uiCulture = (CultureInfo) new VistaCultureInfo(name, fallbacks);
                  return NativeCultureResolver.m_uiCulture;
                }
                catch (ArgumentException ex)
                {
                }
              }
              NativeCultureResolver.m_uiCulture = NativeCultureResolver.EmulateDownLevel();
              return NativeCultureResolver.m_uiCulture;
            }
          }
        }
        return (CultureInfo) NativeCultureResolver.m_uiCulture.Clone();
      }
    }

    internal static CultureInfo Culture
    {
      get
      {
        if (NativeCultureResolver.m_Culture == null)
        {
          lock (NativeCultureResolver.m_syncObject)
          {
            if (NativeCultureResolver.m_Culture == null)
            {
              try
              {
                if (!NativeCultureResolver.IsVistaAndLater())
                {
                  NativeCultureResolver.m_Culture = new CultureInfo(NativeCultureResolver.GetUserDefaultLCID());
                }
                else
                {
                  StringBuilder lpLocaleName = new StringBuilder(16);
                  NativeCultureResolver.m_Culture = NativeCultureResolver.GetUserDefaultLocaleName(lpLocaleName, 16) != 0 ? new CultureInfo(lpLocaleName.ToString().Trim()) : CultureInfo.CurrentCulture;
                }
                NativeCultureResolver.m_Culture = CultureInfo.CreateSpecificCulture(NativeCultureResolver.m_Culture.GetConsoleFallbackUICulture().Name);
              }
              catch (ArgumentException ex)
              {
                NativeCultureResolver.m_Culture = CultureInfo.CurrentCulture;
              }
            }
          }
        }
        return NativeCultureResolver.m_Culture;
      }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern ushort GetUserDefaultUILanguage();

    private static CultureInfo EmulateDownLevel() => new CultureInfo((int) NativeCultureResolver.GetUserDefaultUILanguage()).GetConsoleFallbackUICulture();

    private static bool IsVistaAndLater() => Environment.OSVersion.Version.Major >= 6;

    private static string GetUserPreferredUILangs()
    {
      long pulNumLanguages = 0;
      int pcchLanguagesBuffer = 0;
      string str = "";
      if (!NativeCultureResolver.SetThreadPreferredUILanguages(NativeCultureResolver.MUI_CONSOLE_FILTER, (StringBuilder) null, IntPtr.Zero) || !NativeCultureResolver.GetThreadPreferredUILanguages(NativeCultureResolver.MUI_LANGUAGE_NAME | NativeCultureResolver.MUI_MERGE_SYSTEM_FALLBACK | NativeCultureResolver.MUI_MERGE_USER_FALLBACK, out pulNumLanguages, (byte[]) null, out pcchLanguagesBuffer))
        return str;
      byte[] numArray = new byte[pcchLanguagesBuffer * 2];
      if (!NativeCultureResolver.GetThreadPreferredUILanguages(NativeCultureResolver.MUI_LANGUAGE_NAME | NativeCultureResolver.MUI_MERGE_SYSTEM_FALLBACK | NativeCultureResolver.MUI_MERGE_USER_FALLBACK, out pulNumLanguages, numArray, out pcchLanguagesBuffer))
        return str;
      try
      {
        return Encoding.Unicode.GetString(numArray).Trim().ToLowerInvariant();
      }
      catch (ArgumentNullException ex)
      {
      }
      catch (DecoderFallbackException ex)
      {
      }
      return str;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetUserDefaultLCID();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetUserDefaultLocaleName(
      [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpLocaleName,
      int cchLocaleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool SetThreadPreferredUILanguages(
      int dwFlags,
      StringBuilder pwszLanguagesBuffer,
      IntPtr pulNumLanguages);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetThreadPreferredUILanguages(
      int dwFlags,
      out long pulNumLanguages,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pwszLanguagesBuffer,
      out int pcchLanguagesBuffer);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern short SetThreadUILanguage(short langId);
  }
}
