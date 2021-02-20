// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ModuleSpecification
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
  internal class ModuleSpecification
  {
    private System.Guid? guid;
    private string name;
    private Version version;

    public ModuleSpecification(string moduleName)
    {
      this.name = !string.IsNullOrEmpty(moduleName) ? moduleName : throw new ArgumentNullException(nameof (moduleName));
      this.version = (Version) null;
      this.guid = new System.Guid?();
    }

    public ModuleSpecification(Hashtable moduleSpecification)
    {
      if (moduleSpecification == null)
        throw new ArgumentNullException(nameof (moduleSpecification));
      StringBuilder stringBuilder = new StringBuilder();
      foreach (DictionaryEntry dictionaryEntry in moduleSpecification)
      {
        if (dictionaryEntry.Key.ToString().Equals("ModuleName", StringComparison.OrdinalIgnoreCase))
          this.name = (string) LanguagePrimitives.ConvertTo(dictionaryEntry.Value, typeof (string), (IFormatProvider) CultureInfo.InvariantCulture);
        else if (dictionaryEntry.Key.ToString().Equals("ModuleVersion", StringComparison.OrdinalIgnoreCase))
          this.version = (Version) LanguagePrimitives.ConvertTo(dictionaryEntry.Value, typeof (Version), (IFormatProvider) CultureInfo.InvariantCulture);
        else if (dictionaryEntry.Key.ToString().Equals("GUID", StringComparison.OrdinalIgnoreCase))
        {
          this.guid = (System.Guid?) LanguagePrimitives.ConvertTo(dictionaryEntry.Value, typeof (System.Guid?), (IFormatProvider) CultureInfo.InvariantCulture);
        }
        else
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(", ");
          stringBuilder.Append("'");
          stringBuilder.Append(dictionaryEntry.Key.ToString());
          stringBuilder.Append("'");
        }
      }
      if (stringBuilder.Length != 0)
        throw new ArgumentException(ResourceManagerCache.FormatResourceString("Modules", "InvalidModuleSpecificationMember", (object) "ModuleName, ModuleVersion, GUID", (object) stringBuilder));
      if (string.IsNullOrEmpty(this.name))
        throw new MissingMemberException(ResourceManagerCache.FormatResourceString("Modules", "RequiredModuleMissingModuleName"));
      if (this.version == (Version) null)
        throw new MissingMemberException(ResourceManagerCache.FormatResourceString("Modules", "RequiredModuleMissingModuleVersion"));
    }

    public System.Guid? Guid => this.guid;

    public string Name => this.name;

    public Version Version => this.version;
  }
}
