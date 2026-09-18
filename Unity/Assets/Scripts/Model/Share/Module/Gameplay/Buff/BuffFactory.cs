using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    /// <summary>
    /// Buff 工厂组件
    /// </summary>
    [Code]
    public class BuffFactory : Singleton<BuffFactory>, ISingletonAwake
    {
        public readonly Dictionary<Type, IBuffRunner> allBuffRuners = new();
        
        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof(BuffRunnerAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(BuffRunnerAttribute), false);

                foreach (object attr in attrs)
                {
                    BuffRunnerAttribute buffRunnerAttribute = (BuffRunnerAttribute)attr;
                    IBuffRunner obj = (IBuffRunner)Activator.CreateInstance(type);
                    allBuffRuners[obj.Type] = obj;
                }
            }
        }

        public IBuffRunner GetRunner(Type buffDataType)
        {
            if (this.allBuffRuners.TryGetValue(buffDataType, out var  runner))
            {
                return runner;
            }

            return (from kvp in this.allBuffRuners where buffDataType.IsSubclassOf(kvp.Key) select kvp.Value).FirstOrDefault();
        }
        
    }
}