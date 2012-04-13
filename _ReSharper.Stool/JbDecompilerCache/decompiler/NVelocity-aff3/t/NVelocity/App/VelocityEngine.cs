// Type: NVelocity.App.VelocityEngine
// Assembly: NVelocity, Version=1.1.1.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc
// Assembly location: C:\Users\Jonathan\Projects\Stool\packages\Castle.NVelocity.1.1.1\lib\net40\NVelocity.dll

using Commons.Collections;
using NVelocity;
using NVelocity.Context;
using NVelocity.Exception;
using NVelocity.Runtime;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.IO;
using System.Text;

namespace NVelocity.App
{
  public class VelocityEngine
  {
    private RuntimeInstance runtimeInstance = new RuntimeInstance();

    public VelocityEngine()
    {
    }

    public VelocityEngine(string propsFilename)
    {
      this.runtimeInstance.Init(propsFilename);
    }

    public VelocityEngine(ExtendedProperties p)
    {
      this.runtimeInstance.Init(p);
    }

    public void SetExtendedProperties(ExtendedProperties value)
    {
      this.runtimeInstance.Configuration = value;
    }

    public void Init()
    {
      this.runtimeInstance.Init();
    }

    public void Init(string propsFilename)
    {
      this.runtimeInstance.Init(propsFilename);
    }

    public void Init(ExtendedProperties p)
    {
      this.runtimeInstance.Init(p);
    }

    public void SetProperty(string key, object value)
    {
      this.runtimeInstance.SetProperty(key, value);
    }

    public void AddProperty(string key, object value)
    {
      this.runtimeInstance.AddProperty(key, value);
    }

    public void ClearProperty(string key)
    {
      this.runtimeInstance.ClearProperty(key);
    }

    public object GetProperty(string key)
    {
      return this.runtimeInstance.GetProperty(key);
    }

    public bool Evaluate(IContext context, TextWriter writer, string logTag, string inString)
    {
      return this.Evaluate(context, writer, logTag, (TextReader) new StringReader(inString));
    }

    [Obsolete("Use the overload that takes an TextReader")]
    public bool Evaluate(IContext context, TextWriter writer, string logTag, Stream instream)
    {
      string name = (string) null;
      TextReader reader;
      try
      {
        name = this.runtimeInstance.GetString("input.encoding", "ISO-8859-1");
        reader = (TextReader) new StreamReader(new StreamReader(instream, Encoding.GetEncoding(name)).BaseStream);
      }
      catch (IOException ex)
      {
        throw new ParseErrorException(string.Format("Unsupported input encoding : {0} for template {1}", (object) name, (object) logTag), (Exception) ex);
      }
      return this.Evaluate(context, writer, logTag, reader);
    }

    public bool Evaluate(IContext context, TextWriter writer, string logTag, TextReader reader)
    {
      SimpleNode simpleNode;
      try
      {
        simpleNode = this.runtimeInstance.Parse(reader, logTag);
      }
      catch (ParseException ex)
      {
        throw new ParseErrorException(ex.Message, (Exception) ex);
      }
      if (simpleNode == null)
        return false;
      InternalContextAdapterImpl contextAdapterImpl = new InternalContextAdapterImpl(context);
      contextAdapterImpl.PushCurrentTemplateName(logTag);
      try
      {
        try
        {
          simpleNode.Init((IInternalContextAdapter) contextAdapterImpl, (object) this.runtimeInstance);
        }
        catch (Exception ex)
        {
          this.runtimeInstance.Error((object) string.Format("Velocity.evaluate() : init exception for tag = {0} : {1}", (object) logTag, (object) ex));
        }
        simpleNode.Render((IInternalContextAdapter) contextAdapterImpl, writer);
      }
      finally
      {
        contextAdapterImpl.PopCurrentTemplateName();
      }
      return true;
    }

    public bool InvokeVelocimacro(string vmName, string logTag, string[] parameters, IContext context, TextWriter writer)
    {
      if (vmName == null || parameters == null || (context == null || writer == null) || logTag == null)
      {
        this.runtimeInstance.Error((object) "VelocityEngine.invokeVelocimacro() : invalid parameter");
        return false;
      }
      else if (!this.runtimeInstance.IsVelocimacro(vmName, logTag))
      {
        this.runtimeInstance.Error((object) string.Format("VelocityEngine.invokeVelocimacro() : VM '{0}' not registered.", (object) vmName));
        return false;
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder("#");
        stringBuilder.Append(vmName);
        stringBuilder.Append("(");
        for (int index = 0; index < parameters.Length; ++index)
        {
          stringBuilder.Append(" $");
          stringBuilder.Append(parameters[index]);
        }
        stringBuilder.Append(" )");
        try
        {
          return this.Evaluate(context, writer, logTag, ((object) stringBuilder).ToString());
        }
        catch (Exception ex)
        {
          this.runtimeInstance.Error((object) string.Format("VelocityEngine.invokeVelocimacro() : error {0}", (object) ex));
          throw;
        }
      }
    }

    [Obsolete("Use the overload that takes the encoding as parameter")]
    public bool MergeTemplate(string templateName, IContext context, TextWriter writer)
    {
      return this.MergeTemplate(templateName, this.runtimeInstance.GetString("input.encoding", "ISO-8859-1"), context, writer);
    }

    public bool MergeTemplate(string templateName, string encoding, IContext context, TextWriter writer)
    {
      Template template = this.runtimeInstance.GetTemplate(templateName, encoding);
      if (template == null)
      {
        this.runtimeInstance.Error((object) string.Format("Velocity.parseTemplate() failed loading template '{0}'", (object) templateName));
        return false;
      }
      else
      {
        template.Merge(context, writer);
        return true;
      }
    }

    public Template GetTemplate(string name)
    {
      return this.runtimeInstance.GetTemplate(name);
    }

    public Template GetTemplate(string name, string encoding)
    {
      return this.runtimeInstance.GetTemplate(name, encoding);
    }

    public bool TemplateExists(string templateName)
    {
      return this.runtimeInstance.GetLoaderNameForResource(templateName) != null;
    }

    public void Warn(object message)
    {
      this.runtimeInstance.Warn(message);
    }

    public void Info(object message)
    {
      this.runtimeInstance.Info(message);
    }

    public void Error(object message)
    {
      this.runtimeInstance.Error(message);
    }

    public void Debug(object message)
    {
      this.runtimeInstance.Debug(message);
    }

    public void SetApplicationAttribute(object key, object value)
    {
      this.runtimeInstance.SetApplicationAttribute(key, value);
    }
  }
}
