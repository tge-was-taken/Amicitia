using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amicitia
{
    internal class ResourceProperty
    {
        public string Name { get; private set; }
        public TypeCode TypeCode { get; private set; }
        public object Value { get; private set; }

        public ResourceProperty(string name, object value)
        {
            Name = name;
            TypeCode = Type.GetTypeCode(value.GetType());
            Value = value;
        }

        public T GetValue<T>()
        {
            return (T)Value;
        }
    }
}
