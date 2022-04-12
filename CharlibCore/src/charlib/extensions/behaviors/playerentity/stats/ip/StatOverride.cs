
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProtoBuf;
using Vintagestory.API.Common;

namespace charlib {
  namespace ip {
    namespace debug {
      public interface IStatOverride {
        Type KeyType {get;}
        Type SelfType {get;}
        public string ShortName();
        public void Register(ICoreAPI api);
      }
      public interface IStatOverrideKind<T> : IStatOverride {
        public T? overrideValue {get;set;}
        public S WithValue<S>(T value) where S : class, IStatOverrideKind<T>;
      }
      [JsonObject(MemberSerialization.OptIn)]
      [ProtoContract]
      public abstract class StatOverride<SELF,K,C,T> 
        : IStatOverride, IKey<K,C,T>, IStatOverrideKind<T>
        where SELF : StatOverride<SELF,K,C,T>, IStatOverrideKind<T>, new()
        where K : IKey<K,C,T> 
        where C : context.IHasPlayer {
        
        public static SELF? Type = null;
        public Type SelfType {get;} = typeof(SELF);
        public Type KeyType {get;} = typeof(K);
        public virtual string ShortName() {
          return nameof(SELF);
        }
        public override string ToString()
        {
          return $"[{ShortName()} = {overrideValue}]";
        }

        [JsonProperty]
        [ProtoMember(1)]
        public T? overrideValue {get;set;} = default(T);

        S IStatOverrideKind<T>.WithValue<S>(T value) {
          S? self = this as S;
          if (self != null) {
            overrideValue = value;
            return self;
          } else {
            throw new InvalidCastException();
          }
        }
        public static T? Resolve(C c, T? t) {
          SELF? stat = c.Player?.Entity
            ?.GetBehavior<PlayerDict>()
            ?.Get<SELF>();
          if(stat != null) {
            return stat.overrideValue;
          } else {
            return t;
          }
        }
        public void Register(ICoreAPI api) {
          PlayerDict.RegisterHandler<SELF>(api);
          CharLib.State.ChainRegistry.Register<K,C,T>(
            new Chainable<C,T>(Resolve, Chain.PriorityLast)
          );
        }

        public override bool Equals(object y)
        {
          T? yt = y as SELF == null ? default(T) : (y as SELF)!.overrideValue;
          return overrideValue?.Equals(yt) ?? false;
        }

        public override int GetHashCode()
        {
          return this.overrideValue?.GetHashCode() ?? 0;
        }

      }
    }
  }
}