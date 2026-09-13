using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ET
{
    public static class ConfigPathSettings
    {
        private static readonly List<string> startConfigs = new List<string>()
        {
            "StartMachineConfigCategory", 
            "StartProcessConfigCategory", 
            "StartSceneConfigCategory", 
            "StartZoneConfigCategory",
        };
        
        private static readonly List<string> skillConfigs = new List<string>()
        {
            "BuffConfigCategory",
            "SkillConfigCategory", 
            "SkillLevelConfigCategory",
            "SkillDamageConfigCategory",
            "ReactionConfigCategory"
        };

        public static string GetPath(string configName)
        {
            if (startConfigs.Contains(configName))
            {
                return $"{Options.Instance.StartConfig}/{configName}";
            }
            else if (skillConfigs.Contains(configName))
            {
                return $"Skill/{configName}";
            }
            return configName;
        }
        
    }
    
    [Invoke]
    public class GetAllConfigBytes: AInvokeHandler<ConfigLoader.GetAllConfigBytes, ETTask<Dictionary<Type, byte[]>>>
    {
        public override async ETTask<Dictionary<Type, byte[]>> Handle(ConfigLoader.GetAllConfigBytes args)
        {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            HashSet<Type> configTypes = CodeTypes.Instance.GetTypes(typeof (ConfigAttribute));
            
            if (Define.IsEditor)
            {
                string ct = "cs";
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
                CodeMode codeMode = globalConfig.CodeMode;
                switch (codeMode)
                {
                    case CodeMode.Client:
                        ct = "c";
                        break;
                    case CodeMode.Server:
                        ct = "s";
                        break;
                    case CodeMode.ClientServer:
                        ct = "cs";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                foreach (Type configType in configTypes)
                {
                    string configFilePath = $"../Config/Excel/{ct}/{ConfigPathSettings.GetPath(configType.Name)}.bytes";
                    output[configType] = File.ReadAllBytes(configFilePath);
                }
            }
            else
            {
                foreach (Type type in configTypes)
                {
                    TextAsset v = await ResourcesComponent.Instance.LoadAssetAsync<TextAsset>($"Assets/Bundles/Config/{type.Name}.bytes");
                    output[type] = v.bytes;
                }
            }

            return output;
        }
    }
    
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigLoader.GetOneConfigBytes, ETTask<byte[]>>
    {
        public override async ETTask<byte[]> Handle(ConfigLoader.GetOneConfigBytes args)
        {
            string ct = "cs";
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            CodeMode codeMode = globalConfig.CodeMode;
            switch (codeMode)
            {
                case CodeMode.Client:
                    ct = "c";
                    break;
                case CodeMode.Server:
                    ct = "s";
                    break;
                case CodeMode.ClientServer:
                    ct = "cs";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };

            string configName = args.ConfigName;
            
            string configFilePath = $"../Config/Excel/{ct}/{ConfigPathSettings.GetPath(configName)}.bytes";
            await ETTask.CompletedTask;
            return File.ReadAllBytes(configFilePath);
        }
    }
}