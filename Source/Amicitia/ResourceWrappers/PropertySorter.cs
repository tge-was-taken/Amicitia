using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Amicitia.ResourceWrappers
{
    public class PropertySorter : ExpandableObjectConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var propertyDescriptors = TypeDescriptor.GetProperties(value, attributes);
            var orderedProperties = new List<PropertyOrderPair>();

            foreach (PropertyDescriptor descriptor in propertyDescriptors)
            {
                var attribute = descriptor.Attributes[typeof(OrderedProperty)];
                if (attribute == null)
                {
                    throw new Exception($"Missing {nameof(OrderedProperty)} for {value.GetType()}.{descriptor.Name}");
                }
                else
                {
                    var propertyOrderAttribute = (OrderedProperty)attribute;
                    var type = value.GetType();

                    orderedProperties.Add(new PropertyOrderPair(descriptor.Name, propertyOrderAttribute.Order, 0));
                }
            }

            orderedProperties.Sort();

            var propertyNames = new List<string>();
            foreach (var pop in orderedProperties)
            {
                propertyNames.Add(pop.Name);
            }

            return propertyDescriptors.Sort(propertyNames.ToArray());
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OrderedProperty : Attribute
    {
        public int Order { get; }

        public OrderedProperty([CallerLineNumber]int order = -1)
        {
            Order = order;
        }
    }

    public class PropertyOrderPair : IComparable
    {
        private readonly int mOrder;

        public int Rank { get; }

        public int Order => mOrder * Rank;

        public string Name { get; }

        public PropertyOrderPair(string name, int order, int rank)
        {
            mOrder = order;
            Name = name;
            Rank = rank;
        }

        public int CompareTo(object obj)
        {
            var otherPropertyOrder = obj as PropertyOrderPair;
            if (otherPropertyOrder == null)
                return -1;

            if (otherPropertyOrder.Order == Order)
            {
                string otherName = otherPropertyOrder.Name;
                return string.CompareOrdinal(Name, otherName);
            }
            else if (otherPropertyOrder.Order > Order)
            {
                return -1;
            }
            return 1;
        }
    }
}
