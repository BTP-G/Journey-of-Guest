using GuestUnion;
using System;

namespace JoG.Attributes {

    /// <summary>标记字段从配置文件自动注入，key为配置项名称。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class FromConfigAttribute : Attribute {
        public string Key { get; }

        public FromConfigAttribute(string key) {
            if (key.IsNullOrWhiteSpace()) {
                throw new ArgumentException("Config key cannot be null or whitespace.", nameof(key));
            }
            Key = key;
        }
    }
}