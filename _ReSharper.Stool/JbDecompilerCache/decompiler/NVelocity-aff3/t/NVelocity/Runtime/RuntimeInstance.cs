// Type: NVelocity.Runtime.RuntimeInstance
// Assembly: NVelocity, Version=1.1.1.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc
// Assembly location: C:\Users\Jonathan\Projects\Stool\packages\Castle.NVelocity.1.1.1\lib\net40\NVelocity.dll

using Commons.Collections;
using NVelocity;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Log;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using NVelocity.Runtime.Resource;
using NVelocity.Util;
using NVelocity.Util.Introspection;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NVelocity.Runtime
{
  public class RuntimeInstance : IRuntimeServices, IRuntimeLogger
  {
    private DefaultTraceListener debugOutput = new DefaultTraceListener();
    private VelocimacroFactory vmFactory;
    private SimplePool<Parser> parserPool;
    private bool initialized;
    private ExtendedProperties overridingProperties;
    private readonly ExtendedProperties configuration;
    private IResourceManager resourceManager;
    private readonly Introspector introspector;
    private readonly Hashtable applicationAttributes;
    private IUberspect uberSpect;
    private IDirectiveManager directiveManager;

    public ExtendedProperties Configuration
    {
      get
      {
        return this.configuration;
      }
      set
      {
        if (this.overridingProperties == null)
        {
          this.overridingProperties = value;
        }
        else
        {
          if (this.overridingProperties == value)
            return;
          this.overridingProperties.Combine(value);
        }
      }
    }

    public Introspector Introspector
    {
      get
      {
        return this.introspector;
      }
    }

    public IUberspect Uberspect
    {
      get
      {
        return this.uberSpect;
      }
    }

    public RuntimeInstance()
    {
      this.configuration = new ExtendedProperties();
      this.vmFactory = new VelocimacroFactory((IRuntimeServices) this);
      this.introspector = new Introspector((IRuntimeLogger) this);
      this.applicationAttributes = new Hashtable();
    }

    public void Init()
    {
      lock (this)
      {
        if (this.initialized)
          return;
        this.initializeProperties();
        this.initializeLogger();
        this.initializeResourceManager();
        this.initializeDirectives();
        this.initializeParserPool();
        this.initializeIntrospection();
        this.vmFactory.InitVelocimacro();
        this.initialized = true;
      }
    }

    private void initializeIntrospection()
    {
      string @string = this.GetString("runtime.introspector.uberspect");
      if (@string != null)
      {
        if (@string.Length > 0)
        {
          object newInstance;
          try
          {
            newInstance = SupportClass.CreateNewInstance(Type.GetType(@string));
          }
          catch (Exception ex)
          {
            string message = string.Format("The specified class for Uberspect ({0}) does not exist (or is not accessible to the current classlaoder.", (object) @string);
            this.Error((object) message);
            throw new Exception(message);
          }
          if (!(newInstance is IUberspect))
          {
            string message = string.Format("The specified class for Uberspect ({0}) does not implement org.apache.velocity.util.introspector.Uberspect. Velocity not initialized correctly.", (object) @string);
            this.Error((object) message);
            throw new Exception(message);
          }
          else
          {
            this.uberSpect = (IUberspect) newInstance;
            if (this.uberSpect is UberspectLoggable)
              ((UberspectLoggable) this.uberSpect).RuntimeLogger = (IRuntimeLogger) this;
            this.uberSpect.Init();
            return;
          }
        }
      }
      string message1 = "It appears that no class was specified as the Uberspect.  Please ensure that all configuration information is correct.";
      this.Error((object) message1);
      throw new Exception(message1);
    }

    private void setDefaultProperties()
    {
      try
      {
        this.configuration.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("NVelocity.Runtime.Defaults.nvelocity.properties"));
      }
      catch (Exception ex)
      {
        this.debugOutput.WriteLine(string.Format("Cannot get NVelocity Runtime default properties!\n{0}", (object) ex.Message));
        this.debugOutput.Flush();
      }
    }

    public void SetProperty(string key, object value)
    {
      if (this.overridingProperties == null)
        this.overridingProperties = new ExtendedProperties();
      this.overridingProperties.SetProperty(key, value);
    }

    public void AddProperty(string key, object value)
    {
      if (this.overridingProperties == null)
        this.overridingProperties = new ExtendedProperties();
      this.overridingProperties.AddProperty(key, value);
    }

    public void ClearProperty(string key)
    {
      if (this.overridingProperties == null)
        return;
      this.overridingProperties.ClearProperty(key);
    }

    public object GetProperty(string key)
    {
      return this.configuration.GetProperty(key);
    }

    private void initializeProperties()
    {
      if (!this.configuration.IsInitialized())
        this.setDefaultProperties();
      if (this.overridingProperties == null)
        return;
      this.configuration.Combine(this.overridingProperties);
    }

    public void Init(ExtendedProperties p)
    {
      this.overridingProperties = ExtendedProperties.ConvertProperties(p);
      this.Init();
    }

    public void Init(string configurationFile)
    {
      this.overridingProperties = new ExtendedProperties(configurationFile);
      this.Init();
    }

    private void initializeResourceManager()
    {
      IResourceManager resourceManager = (IResourceManager) this.applicationAttributes[(object) "resource.manager.class"];
      string @string = this.GetString("resource.manager.class");
      if (resourceManager == null && @string != null)
      {
        if (@string.Length > 0)
        {
          object instance;
          try
          {
            instance = Activator.CreateInstance(Type.GetType(@string));
          }
          catch (Exception ex)
          {
            string message = string.Format("The specified class for ResourceManager ({0}) does not exist.", (object) @string);
            this.Error((object) message);
            throw new Exception(message);
          }
          if (!(instance is IResourceManager))
          {
            string message = string.Format("The specified class for ResourceManager ({0}) does not implement ResourceManager. NVelocity not initialized correctly.", (object) @string);
            this.Error((object) message);
            throw new Exception(message);
          }
          else
          {
            this.resourceManager = (IResourceManager) instance;
            this.resourceManager.Initialize((IRuntimeServices) this);
            return;
          }
        }
      }
      if (resourceManager != null)
      {
        this.resourceManager = resourceManager;
        this.resourceManager.Initialize((IRuntimeServices) this);
      }
      else
      {
        string message = "It appears that no class was specified as the ResourceManager.  Please ensure that all configuration information is correct.";
        this.Error((object) message);
        throw new Exception(message);
      }
    }

    private void initializeLogger()
    {
    }

    private void initializeDirectives()
    {
      this.initializeDirectiveManager();
      ExtendedProperties extendedProperties = new ExtendedProperties();
      try
      {
        extendedProperties.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("NVelocity.Runtime.Defaults.directive.properties"));
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("Error loading directive.properties! Something is very wrong if these properties aren't being located. Either your Velocity distribution is incomplete or your Velocity jar file is corrupted!\n{0}", (object) ex.Message));
      }
      foreach (string directiveTypeName in (IEnumerable) extendedProperties.Values)
        this.directiveManager.Register(directiveTypeName);
      foreach (string directiveTypeName in this.configuration.GetStringArray("userdirective"))
        this.directiveManager.Register(directiveTypeName);
    }

    private void initializeDirectiveManager()
    {
      string @string = this.configuration.GetString("directive.manager");
      if (@string == null)
        throw new Exception("Looks like there's no 'directive.manager' configured. NVelocity can't go any further");
      string typeName = @string.Replace(';', ',');
      Type type = Type.GetType(typeName, false, false);
      if (type == (Type) null)
        throw new Exception(string.Format("The type {0} could not be resolved", (object) typeName));
      this.directiveManager = (IDirectiveManager) Activator.CreateInstance(type);
    }

    private void initializeParserPool()
    {
      int @int = this.GetInt("parser.pool.size", 40);
      this.parserPool = new SimplePool<Parser>(@int);
      for (int index = 0; index < @int; ++index)
        this.parserPool.put(this.CreateNewParser());
    }

    public Parser CreateNewParser()
    {
      return new Parser((IRuntimeServices) this)
      {
        Directives = this.directiveManager
      };
    }

    public SimpleNode Parse(TextReader reader, string templateName)
    {
      return this.Parse(reader, templateName, true);
    }

    public SimpleNode Parse(TextReader reader, string templateName, bool dumpNamespace)
    {
      SimpleNode simpleNode = (SimpleNode) null;
      Parser o = (Parser) this.parserPool.get();
      bool flag = false;
      if (o == null)
      {
        this.Error((object) "Runtime : ran out of parsers. Creating new.  Please increment the parser.pool.size property. The current value is too small.");
        o = this.CreateNewParser();
        if (o != null)
          flag = true;
      }
      if (o == null)
      {
        this.Error((object) "Runtime : ran out of parsers and unable to create more.");
      }
      else
      {
        try
        {
          if (dumpNamespace)
            this.DumpVMNamespace(templateName);
          simpleNode = o.Parse(reader, templateName);
        }
        finally
        {
          if (!flag)
            this.parserPool.put(o);
        }
      }
      return simpleNode;
    }

    public Template GetTemplate(string name)
    {
      return this.GetTemplate(name, this.GetString("input.encoding", "ISO-8859-1"));
    }

    public Template GetTemplate(string name, string encoding)
    {
      return (Template) this.resourceManager.GetResource(name, ResourceType.Template, encoding);
    }

    public ContentResource GetContent(string name)
    {
      return this.GetContent(name, this.GetString("input.encoding", "ISO-8859-1"));
    }

    public ContentResource GetContent(string name, string encoding)
    {
      return (ContentResource) this.resourceManager.GetResource(name, ResourceType.Content, encoding);
    }

    public string GetLoaderNameForResource(string resourceName)
    {
      return this.resourceManager.GetLoaderNameForResource(resourceName);
    }

    private bool showStackTrace()
    {
      if (this.configuration.IsInitialized())
        return this.GetBoolean("runtime.log.warn.stacktrace", false);
      else
        return false;
    }

    private void Log(NVelocity.Runtime.Log.LogLevel level, object message)
    {
    }

    public void Warn(object message)
    {
      this.Log(NVelocity.Runtime.Log.LogLevel.Warn, message);
    }

    public void Info(object message)
    {
      this.Log(NVelocity.Runtime.Log.LogLevel.Info, message);
    }

    public void Error(object message)
    {
      this.Log(NVelocity.Runtime.Log.LogLevel.Error, message);
    }

    public void Debug(object message)
    {
      this.Log(NVelocity.Runtime.Log.LogLevel.Debug, message);
    }

    public string GetString(string key, string defaultValue)
    {
      return this.configuration.GetString(key, defaultValue);
    }

    public Directive GetVelocimacro(string vmName, string templateName)
    {
      return this.vmFactory.GetVelocimacro(vmName, templateName);
    }

    public bool AddVelocimacro(string name, string macro, string[] argArray, string sourceTemplate)
    {
      return this.vmFactory.AddVelocimacro(name, macro, argArray, sourceTemplate);
    }

    public bool IsVelocimacro(string vmName, string templateName)
    {
      return this.vmFactory.IsVelocimacro(vmName, templateName);
    }

    public bool DumpVMNamespace(string ns)
    {
      return this.vmFactory.DumpVMNamespace(ns);
    }

    public string GetString(string key)
    {
      return this.configuration.GetString(key);
    }

    public int GetInt(string key)
    {
      return this.configuration.GetInt(key);
    }

    public int GetInt(string key, int defaultValue)
    {
      return this.configuration.GetInt(key, defaultValue);
    }

    public bool GetBoolean(string key, bool def)
    {
      return this.configuration.GetBoolean(key, def);
    }

    public object GetApplicationAttribute(object key)
    {
      return this.applicationAttributes[key];
    }

    public object SetApplicationAttribute(object key, object o)
    {
      return this.applicationAttributes[key] = o;
    }
  }
}
