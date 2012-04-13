// Type: NVelocity.Runtime.Directive.Include
// Assembly: NVelocity, Version=1.1.1.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc
// Assembly location: C:\Users\Jonathan\Projects\Stool\packages\Castle.NVelocity.1.1.1\lib\net40\NVelocity.dll

using NVelocity.Context;
using NVelocity.Exception;
using NVelocity.Runtime;
using NVelocity.Runtime.Parser.Node;
using NVelocity.Runtime.Resource;
using System;
using System.IO;

namespace NVelocity.Runtime.Directive
{
  public class Include : Directive
  {
    private string outputMsgStart = string.Empty;
    private string outputMsgEnd = string.Empty;

    public override string Name
    {
      get
      {
        return "include";
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

    public override void Init(IRuntimeServices rs, IInternalContextAdapter context, INode node)
    {
      base.Init(rs, context, node);
      this.outputMsgStart = this.runtimeServices.GetString("directive.include.output.errormsg.start");
      this.outputMsgStart = string.Format("{0} ", (object) this.outputMsgStart);
      this.outputMsgEnd = this.runtimeServices.GetString("directive.include.output.errormsg.end");
      this.outputMsgEnd = string.Format(" {0}", (object) this.outputMsgEnd);
    }

    public override bool Render(IInternalContextAdapter context, TextWriter writer, INode node)
    {
      int childrenCount = node.ChildrenCount;
      for (int i = 0; i < childrenCount; ++i)
      {
        INode child = node.GetChild(i);
        if (child.Type == 6 || child.Type == 14)
        {
          if (!this.RenderOutput(child, context, writer))
            this.OutputErrorToStream(writer, string.Format("error with arg {0} please see log.", (object) i));
        }
        else
        {
          this.runtimeServices.Error((object) string.Format("#include() error : invalid argument type : {0}", (object) child.ToString()));
          this.OutputErrorToStream(writer, string.Format("error with arg {0} please see log.", (object) i));
        }
      }
      return true;
    }

    private bool RenderOutput(INode node, IInternalContextAdapter context, TextWriter writer)
    {
      if (node == null)
      {
        this.runtimeServices.Error((object) "#include() error :  null argument");
        return false;
      }
      else
      {
        object obj = node.Value(context);
        if (obj == null)
        {
          this.runtimeServices.Error((object) "#include() error :  null argument");
          return false;
        }
        else
        {
          string name = obj.ToString();
          Resource resource = (Resource) null;
          Resource currentResource = context.CurrentResource;
          try
          {
            string encoding = currentResource != null ? currentResource.Encoding : (string) this.runtimeServices.GetProperty("input.encoding");
            resource = (Resource) this.runtimeServices.GetContent(name, encoding);
          }
          catch (ResourceNotFoundException ex)
          {
            this.runtimeServices.Error((object) string.Format("#include(): cannot find resource '{0}', called from template {1} at ({2}, {3})", (object) name, (object) context.CurrentTemplateName, (object) this.Line, (object) this.Column));
            throw;
          }
          catch (Exception ex)
          {
            this.runtimeServices.Error((object) string.Format("#include(): arg = '{0}',  called from template {1} at ({2}, {3}) : {4}", (object) name, (object) context.CurrentTemplateName, (object) this.Line, (object) this.Column, (object) ex));
          }
          if (resource == null)
            return false;
          writer.Write((string) resource.Data);
          return true;
        }
      }
    }

    private void OutputErrorToStream(TextWriter writer, string msg)
    {
      if (this.outputMsgStart == null || this.outputMsgEnd == null)
        return;
      writer.Write(this.outputMsgStart);
      writer.Write(msg);
      writer.Write(this.outputMsgEnd);
    }
  }
}
