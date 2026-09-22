using System;
using System.Collections.Generic;

namespace ET
{
    [Code]
    public class PassiveDispatcher: Singleton<PassiveDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<int, IPassiveExecutor> allExecutors = new();
        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof(PassiveExecutorAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(PassiveExecutorAttribute), false);

                foreach (object attr in attrs)
                {
                    PassiveExecutorAttribute passAttr = (PassiveExecutorAttribute)attr;
                    IPassiveExecutor obj = (IPassiveExecutor)Activator.CreateInstance(type);
                    this.allExecutors[passAttr.PassiveId] = obj;
                }
            }
        }

        public async ETTask<bool> Execute<T>(Unit owner, PassiveDataBase passiveData, T args) where T: IPassiveEvent
        {
            try
            {
                if (this.allExecutors.TryGetValue(passiveData.PassiveId, out var  executor))
                {
                    if (executor is not APassiveExecutor<T> aExecutor)
                    {
                        Log.Error($"PassiveExecutor type error: {executor.GetType().FullName}");
                        return false;
                    }
                    bool ret = await aExecutor.Execute(owner, passiveData, args);
                    return ret;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return false;
        }
    }
}