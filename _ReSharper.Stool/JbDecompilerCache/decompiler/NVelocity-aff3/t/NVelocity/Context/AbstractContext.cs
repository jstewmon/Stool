// Type: NVelocity.Context.AbstractContext
// Assembly: NVelocity, Version=1.1.1.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc
// Assembly location: C:\Users\Jonathan\Projects\Stool\packages\Castle.NVelocity.1.1.1\lib\net40\NVelocity.dll

using System;

namespace NVelocity.Context
{
  [Serializable]
  public abstract class AbstractContext : InternalContextBase, IContext
  {
    private readonly IContext innerContext;

    public object[] Keys
    {
      get
      {
        return this.InternalGetKeys();
      }
    }

    public IContext ChainedContext
    {
      get
      {
        return this.innerContext;
      }
    }

    public abstract int Count { get; }

    public AbstractContext()
    {
    }

    public AbstractContext(IContext inner)
    {
      this.innerContext = inner;
      IInternalEventContext internalEventContext = this.innerContext as IInternalEventContext;
      if (internalEventContext == null)
        return;
      this.AttachEventCartridge(internalEventContext.EventCartridge);
    }

    public abstract object InternalGet(string key);

    public abstract object InternalPut(string key, object value);

    public abstract bool InternalContainsKey(object key);

    public abstract object[] InternalGetKeys();

    public abstract object InternalRemove(object key);

    public object Put(string key, object value)
    {
      if (key == null)
        return (object) null;
      else
        return this.InternalPut(key, value);
    }

    public object Get(string key)
    {
      if (key == null)
        return (object) null;
      object obj = this.InternalGet(key);
      if (obj == null && this.innerContext != null)
        obj = this.innerContext.Get(key);
      return obj;
    }

    public bool ContainsKey(object key)
    {
      if (key == null)
        return false;
      else
        return this.InternalContainsKey(key);
    }

    public object Remove(object key)
    {
      if (key == null)
        return (object) null;
      else
        return this.InternalRemove(key);
    }
  }
}
