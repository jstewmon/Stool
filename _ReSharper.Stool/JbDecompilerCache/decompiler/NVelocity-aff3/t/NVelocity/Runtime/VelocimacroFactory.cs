// Type: NVelocity.Runtime.VelocimacroFactory
// Assembly: NVelocity, Version=1.1.1.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc
// Assembly location: C:\Users\Jonathan\Projects\Stool\packages\Castle.NVelocity.1.1.1\lib\net40\NVelocity.dll

using NVelocity;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Resource;
using System;
using System.Collections;

namespace NVelocity.Runtime
{
  public class VelocimacroFactory
  {
    private bool addNewAllowed = true;
    private IRuntimeServices runtimeServices;
    private VelocimacroManager velocimacroManager;
    private bool replaceAllowed;
    private bool templateLocal;
    private bool blather;
    private bool autoReloadLibrary;
    private ArrayList macroLibVec;
    private Hashtable libModMap;

    private bool TemplateLocalInline
    {
      get
      {
        return this.templateLocal;
      }
      set
      {
        this.templateLocal = value;
      }
    }

    private bool AddMacroPermission
    {
      set
      {
        this.addNewAllowed = value;
      }
    }

    private bool ReplacementPermission
    {
      set
      {
        this.replaceAllowed = value;
      }
    }

    private bool Blather
    {
      get
      {
        return this.blather;
      }
      set
      {
        this.blather = value;
      }
    }

    private bool Autoload
    {
      get
      {
        return this.autoReloadLibrary;
      }
      set
      {
        this.autoReloadLibrary = value;
      }
    }

    public VelocimacroFactory(IRuntimeServices rs)
    {
      this.runtimeServices = rs;
      this.libModMap = new Hashtable();
      this.velocimacroManager = new VelocimacroManager(this.runtimeServices);
    }

    public void InitVelocimacro()
    {
      lock (this)
      {
        this.ReplacementPermission = true;
        this.Blather = true;
        this.LogVMMessageInfo("Velocimacro : initialization starting.");
        this.velocimacroManager.NamespaceUsage = false;
        object local_0 = this.runtimeServices.GetProperty("velocimacro.library");
        if (local_0 != null)
        {
          if (local_0 is ArrayList)
            this.macroLibVec = (ArrayList) local_0;
          else if (local_0 is string)
          {
            this.macroLibVec = new ArrayList();
            this.macroLibVec.Add(local_0);
          }
          for (int local_1 = 0; local_1 < this.macroLibVec.Count; ++local_1)
          {
            string local_2 = (string) this.macroLibVec[local_1];
            if (local_2 != null && !local_2.Equals(string.Empty))
            {
              this.velocimacroManager.RegisterFromLib = true;
              this.LogVMMessageInfo(string.Format("Velocimacro : adding VMs from VM library template : {0}", (object) local_2));
              try
              {
                Template local_3 = this.runtimeServices.GetTemplate(local_2);
                this.libModMap[(object) local_2] = (object) new VelocimacroFactory.Twonk(this)
                {
                  template = local_3,
                  modificationTime = local_3.LastModified
                };
              }
              catch (Exception exception_0)
              {
                this.LogVMMessageInfo(string.Format("Velocimacro : error using  VM library template {0} : {1}", (object) local_2, (object) exception_0));
              }
              this.LogVMMessageInfo("Velocimacro :  VM library template macro registration complete.");
              this.velocimacroManager.RegisterFromLib = false;
            }
          }
        }
        this.AddMacroPermission = true;
        if (!this.runtimeServices.GetBoolean("velocimacro.permissions.allow.inline", true))
        {
          this.AddMacroPermission = false;
          this.LogVMMessageInfo("Velocimacro : allowInline = false : VMs can not be defined inline in templates");
        }
        else
          this.LogVMMessageInfo("Velocimacro : allowInline = true : VMs can be defined inline in templates");
        this.ReplacementPermission = false;
        if (this.runtimeServices.GetBoolean("velocimacro.permissions.allow.inline.to.replace.global", false))
        {
          this.ReplacementPermission = true;
          this.LogVMMessageInfo("Velocimacro : allowInlineToOverride = true : VMs defined inline may replace previous VM definitions");
        }
        else
          this.LogVMMessageInfo("Velocimacro : allowInlineToOverride = false : VMs defined inline may NOT replace previous VM definitions");
        this.velocimacroManager.NamespaceUsage = true;
        this.TemplateLocalInline = this.runtimeServices.GetBoolean("velocimacro.permissions.allow.inline.local.scope", false);
        if (this.TemplateLocalInline)
          this.LogVMMessageInfo("Velocimacro : allowInlineLocal = true : VMs defined inline will be local to their defining template only.");
        else
          this.LogVMMessageInfo("Velocimacro : allowInlineLocal = false : VMs defined inline will be  global in scope if allowed.");
        this.velocimacroManager.TemplateLocalInlineVM = this.TemplateLocalInline;
        this.Blather = this.runtimeServices.GetBoolean("velocimacro.messages.on", true);
        if (this.Blather)
          this.LogVMMessageInfo("Velocimacro : messages on  : VM system will output logging messages");
        else
          this.LogVMMessageInfo("Velocimacro : messages off : VM system will be quiet");
        this.Autoload = this.runtimeServices.GetBoolean("velocimacro.library.autoreload", false);
        if (this.Autoload)
          this.LogVMMessageInfo("Velocimacro : autoload on  : VM system will automatically reload global library macros");
        else
          this.LogVMMessageInfo("Velocimacro : autoload off  : VM system will not automatically reload global library macros");
        this.runtimeServices.Info((object) "Velocimacro : initialization complete.");
      }
    }

    public bool AddVelocimacro(string name, string macroBody, string[] argArray, string sourceTemplate)
    {
      if (name == null || macroBody == null || (argArray == null || sourceTemplate == null))
      {
        this.LogVMMessageWarn("Velocimacro : VM addition rejected : programmer error : arg null");
        return false;
      }
      else
      {
        if (!this.CanAddVelocimacro(name, sourceTemplate))
          return false;
        lock (this)
          this.velocimacroManager.AddVM(name, macroBody, argArray, sourceTemplate);
        if (this.blather)
        {
          string str = string.Format("#{0}", (object) argArray[0]) + "(";
          for (int index = 1; index < argArray.Length; ++index)
            str = str + " " + argArray[index];
          this.LogVMMessageInfo(string.Format("Velocimacro : added new VM : {0}", (object) (str + " ) : source = " + sourceTemplate)));
        }
        return true;
      }
    }

    private bool CanAddVelocimacro(string name, string sourceTemplate)
    {
      if (this.Autoload)
      {
        for (int index = 0; index < this.macroLibVec.Count; ++index)
        {
          if (((string) this.macroLibVec[index]).Equals(sourceTemplate))
            return true;
        }
      }
      if (!this.addNewAllowed)
      {
        this.LogVMMessageWarn(string.Format("Velocimacro : VM addition rejected : {0} : inline VMs not allowed.", (object) name));
        return false;
      }
      else
      {
        if (this.templateLocal || !this.IsVelocimacro(name, sourceTemplate) || this.replaceAllowed)
          return true;
        this.LogVMMessageWarn(string.Format("Velocimacro : VM addition rejected : {0} : inline not allowed to replace existing VM", (object) name));
        return false;
      }
    }

    private void LogVMMessageInfo(string s)
    {
      if (!this.blather)
        return;
      this.runtimeServices.Info((object) s);
    }

    private void LogVMMessageWarn(string s)
    {
      if (!this.blather)
        return;
      this.runtimeServices.Warn((object) s);
    }

    public bool IsVelocimacro(string vm, string sourceTemplate)
    {
      lock (this)
      {
        if (this.velocimacroManager.get(vm, sourceTemplate) != null)
          return true;
      }
      return false;
    }

    public Directive GetVelocimacro(string vmName, string sourceTemplate)
    {
      VelocimacroProxy velocimacroProxy = (VelocimacroProxy) null;
      lock (this)
      {
        velocimacroProxy = this.velocimacroManager.get(vmName, sourceTemplate);
        if (velocimacroProxy != null)
        {
          if (this.Autoload)
          {
            string local_1 = this.velocimacroManager.GetLibraryName(vmName, sourceTemplate);
            if (local_1 != null)
            {
              try
              {
                VelocimacroFactory.Twonk local_2 = (VelocimacroFactory.Twonk) this.libModMap[(object) local_1];
                if (local_2 != null)
                {
                  Template local_3 = local_2.template;
                  long local_4 = local_2.modificationTime;
                  long local_5 = local_3.ResourceLoader.GetLastModified((Resource) local_3);
                  if (local_5 > local_4)
                  {
                    this.LogVMMessageInfo(string.Format("Velocimacro : autoload reload for VMs from VM library template : {0}", (object) local_1));
                    local_2.modificationTime = local_5;
                    Template local_3_1 = this.runtimeServices.GetTemplate(local_1);
                    local_2.template = local_3_1;
                    local_2.modificationTime = local_3_1.LastModified;
                  }
                }
              }
              catch (Exception exception_0)
              {
                this.LogVMMessageInfo(string.Format("Velocimacro : error using  VM library template {0} : {1}", (object) local_1, (object) exception_0));
              }
              velocimacroProxy = this.velocimacroManager.get(vmName, sourceTemplate);
            }
          }
        }
      }
      return (Directive) velocimacroProxy;
    }

    public bool DumpVMNamespace(string ns)
    {
      return this.velocimacroManager.DumpNamespace(ns);
    }

    private class Twonk
    {
      private VelocimacroFactory enclosingInstance;
      public Template template;
      public long modificationTime;

      public VelocimacroFactory Enclosing_Instance
      {
        get
        {
          return this.enclosingInstance;
        }
      }

      public Twonk(VelocimacroFactory enclosingInstance)
      {
        this.InitBlock(enclosingInstance);
      }

      private void InitBlock(VelocimacroFactory enclosingInstance)
      {
        this.enclosingInstance = enclosingInstance;
      }
    }
  }
}
