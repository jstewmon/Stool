// Type: NVelocity.VelocityContext
// Assembly: NVelocity, Version=1.1.1.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc
// Assembly location: C:\Users\Jonathan\Projects\Stool\packages\Castle.NVelocity.1.1.1\lib\net40\NVelocity.dll

using NVelocity.Context;
using System;
using System.Collections;

namespace NVelocity
{
  public class VelocityContext : AbstractContext
  {
    private Hashtable context;

    public override int Count
    {
      get
      {
        return this.context.Count;
      }
    }

    public VelocityContext()
      : this((Hashtable) null, (IContext) null)
    {
    }

    public VelocityContext(Hashtable context)
      : this(context, (IContext) null)
    {
    }

    public VelocityContext(IContext innerContext)
      : this((Hashtable) null, innerContext)
    {
    }

    public VelocityContext(Hashtable context, IContext innerContext)
      : base(innerContext)
    {
      this.context = context == null ? new Hashtable() : context;
    }

    public override object InternalGet(string key)
    {
      return this.context[(object) key];
    }

    public override object InternalPut(string key, object value)
    {
      return this.context[(object) key] = value;
    }

    public override bool InternalContainsKey(object key)
    {
      return this.context.ContainsKey(key);
    }

    public override object[] InternalGetKeys()
    {
      object[] objArray = new object[this.context.Keys.Count];
      this.context.Keys.CopyTo((Array) objArray, 0);
      return objArray;
    }

    public override object InternalRemove(object key)
    {
      object obj = this.context[key];
      this.context.Remove(key);
      return obj;
    }
  }
}
