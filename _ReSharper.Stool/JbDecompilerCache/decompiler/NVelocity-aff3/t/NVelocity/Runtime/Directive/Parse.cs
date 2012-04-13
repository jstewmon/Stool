// Type: NVelocity.Runtime.Directive.Parse
// Assembly: NVelocity, Version=1.1.1.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc
// Assembly location: C:\Users\Jonathan\Projects\Stool\packages\Castle.NVelocity.1.1.1\lib\net40\NVelocity.dll

using NVelocity;
using NVelocity.Context;
using NVelocity.Exception;
using NVelocity.Runtime.Parser.Node;
using NVelocity.Runtime.Resource;
using System;
using System.IO;
using System.Text;

namespace NVelocity.Runtime.Directive
{
  public class Parse : Directive
  {
    public override string Name
    {
      get
      {
        return "parse";
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    public override DirectiveType Type
    {
      get
      {
        return DirectiveType.LINE;
      }
    }

    public override bool Render(IInternalContextAdapter context, TextWriter writer, INode node)
    {
      object obj;
      if (!this.AssertArgument(node) || !this.AssertNodeHasValue(node, context, out obj))
        return false;
      string str = obj.ToString();
      this.AssertTemplateStack(context);
      Resource currentResource = context.CurrentResource;
      string encoding = currentResource != null ? currentResource.Encoding : (string) this.runtimeServices.GetProperty("input.encoding");
      Template template = this.GetTemplate(str, encoding, context);
      if (template == null || !this.RenderTemplate(template, str, writer, context))
        return false;
      else
        return true;
    }

    private bool AssertArgument(INode node)
    {
      bool flag = true;
      if (node.GetChild(0) == null)
      {
        this.runtimeServices.Error((object) "#parse() error :  null argument");
        flag = false;
      }
      return flag;
    }

    private bool AssertNodeHasValue(INode node, IInternalContextAdapter context, out object value)
    {
      bool flag = true;
      value = node.GetChild(0).Value(context);
      if (value == null)
      {
        this.runtimeServices.Error((object) "#parse() error :  null argument");
        flag = false;
      }
      return flag;
    }

    private bool AssertTemplateStack(IInternalContextAdapter context)
    {
      bool flag = true;
      object[] templateNameStack = context.TemplateNameStack;
      if (templateNameStack.Length >= this.runtimeServices.GetInt("directive.parse.max.depth", 20))
      {
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < templateNameStack.Length; ++index)
          stringBuilder.AppendFormat(" > {0}", (object[]) templateNameStack[index]);
        this.runtimeServices.Error((object) string.Format("Max recursion depth reached ({0}) File stack:{1}", (object) templateNameStack.Length, (object) stringBuilder));
        flag = false;
      }
      return flag;
    }

    private Template GetTemplate(string arg, string encoding, IInternalContextAdapter context)
    {
      try
      {
        return this.runtimeServices.GetTemplate(arg, encoding);
      }
      catch (ResourceNotFoundException ex)
      {
        this.runtimeServices.Error((object) string.Format("#parse(): cannot find template '{0}', called from template {1} at ({2}, {3})", (object) arg, (object) context.CurrentTemplateName, (object) this.Line, (object) this.Column));
        throw;
      }
      catch (ParseErrorException ex)
      {
        this.runtimeServices.Error((object) string.Format("#parse(): syntax error in #parse()-ed template '{0}', called from template {1} at ({2}, {3})", (object) arg, (object) context.CurrentTemplateName, (object) this.Line, (object) this.Column));
        throw;
      }
      catch (Exception ex)
      {
        this.runtimeServices.Error((object) string.Format("#parse() : arg = {0}.  Exception : {1}", (object) arg, (object) ex));
        return (Template) null;
      }
    }

    private bool RenderTemplate(Template template, string arg, TextWriter writer, IInternalContextAdapter context)
    {
      bool flag = true;
      try
      {
        context.PushCurrentTemplateName(arg);
        ((SimpleNode) template.Data).Render(context, writer);
      }
      catch (Exception ex)
      {
        throw;
      }
      finally
      {
        context.PopCurrentTemplateName();
      }
      return flag;
    }
  }
}
