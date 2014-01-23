using System;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Pseudo variable used to suppress virtual addressing.
    /// </summary>
    public class DirectPropertySlot: Variable
    {
        private PropertyVariable property;

        public DirectPropertySlot(PropertyVariable property)
            : base(property.GetVariableType(), property.GetModule())
        {
            this.property = property;
        }

        public override bool IsDirectPropertySlot()
        {
            return true;
        }

        public PropertyVariable Property {
            get {
                return property;
            }
        }
    }
}

