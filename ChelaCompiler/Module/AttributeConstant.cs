using System.Collections.Generic;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Attribute instance constant data.
    /// </summary>
    public class AttributeConstant
    {
        private Class attributeClass;
        private Method attributeCtor;
        private List<ConstantValue> ctorArguments;
        private List<PropertyValue> propertyValues;

        /// <summary>
        /// Property-value pair.
        /// </summary>
        internal class PropertyValue
        {
            public Variable Property;
            public ConstantValue Value;

            public PropertyValue(Variable property, ConstantValue value)
            {
                this.Property = property;
                this.Value = value;
            }
        }

        public AttributeConstant (Class attributeClass, Method attributeCtor)
        {
            this.attributeClass = attributeClass;
            this.attributeCtor = attributeCtor;
            this.ctorArguments = new List<ConstantValue> ();
            this.propertyValues = new List<PropertyValue> ();
        }

        /// <summary>
        /// The attribute class.
        /// </summary>
        public Class AttributeClass {
            get {
                return attributeClass;
            }
        }

        /// <summary>
        /// The attribute constructor.
        /// </summary>
        public Method AttributeConstructor {
            get {
                return attributeCtor;
            }
        }

        /// <summary>
        /// Adds an argument value to the attribute constant.
        /// </summary>
        /// <param name="value">
        /// A <see cref="ConstantValue"/>
        /// </param>
        public void AddArgument(ConstantValue value)
        {
            this.ctorArguments.Add(value);
        }

        /// <summary>
        /// Adds a property value to the attribute constant.
        /// </summary>
        /// <param name="property">
        /// A <see cref="Variable"/>
        /// </param>
        /// <param name="value">
        /// A <see cref="ConstantValue"/>
        /// </param>
        public void AddPropertyValue(Variable property, ConstantValue value)
        {
            this.propertyValues.Add(new PropertyValue(property, value));
        }

        public void PrepareSerialization(ChelaModule module)
        {
            // Register the attribute class.
            module.RegisterType(attributeClass);

            // Register the attribute ctor.
            if(attributeCtor != null)
                module.RegisterMember(attributeCtor);

            // Register the properties.
            foreach(PropertyValue prop in propertyValues)
            {
                module.RegisterMember(prop.Property);
                module.RegisterType(prop.Property.GetVariableType());
            }
        }

        /// <summary>
        /// Writes the attribute constant into the module file.
        /// </summary>
        /// <param name="writer">
        /// A <see cref="ModuleWriter"/>
        /// </param>
        public void Write(ModuleWriter writer, ChelaModule module)
        {
            // Write the attribute class id.
            uint attributeClassId = module.RegisterMember(attributeClass);
            writer.Write(attributeClassId);

            // Write the attribute constructor id.
            uint attributeCtorId = module.RegisterMember(attributeCtor);
            writer.Write(attributeCtorId);

            // Write the arguments.
            byte numargs = (byte)ctorArguments.Count;
            writer.Write(numargs);
            for(int i = 0; i < numargs; ++i)
            {
                ConstantValue arg = ctorArguments[i];
                arg.WriteQualified(module, writer);
            }

            // Write the properties.
            byte numprops = (byte)propertyValues.Count;
            writer.Write(numprops);
            for(int i = 0; i < numprops; ++i)
            {
                PropertyValue propVal = propertyValues[i];
                uint propId = module.RegisterMember(propVal.Property);
                writer.Write(propId);
                propVal.Value.WriteQualified(module, writer);
            }
        }

        public static void Skip(ModuleReader reader)
        {
            // Skip the attribute class and ctor id.
            reader.Skip(8);

            // Read the number of arguments.
            int numargs = reader.ReadByte();

            // Skip them.
            for(int i = 0; i < numargs; ++i)
                ConstantValue.SkipQualified(reader);

            // Read the number of properties.
            int numprops = reader.ReadByte();

            // Skip them.
            for(int i = 0; i < numprops; ++i)
            {
                // Skip the property id.
                reader.Skip(4);

                // Skip the property value.
                ConstantValue.SkipQualified(reader);
            }
        }
    }
}
