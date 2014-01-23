using System;
using System.Collections.Generic;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Place holder type.
    /// </summary>
    public class PlaceHolderType: ChelaType, IEquatable<PlaceHolderType>
    {
        private int placeHolderId;
        private string name;
        private bool valueType;
        private bool hasDefCtor;
        private bool isNumber;
        private bool isInteger;
        private bool isFloatingPoint;
        private Structure[] bases;

        public PlaceHolderType (ChelaModule module, int id, string name, bool valueType, bool hasDefCtor, Structure[] bases)
            : base(module)
        {
            this.placeHolderId = id;
            this.name = name;
            this.valueType = valueType;
            this.hasDefCtor = hasDefCtor;
            this.isNumber = false;
            this.isInteger = false;
            this.isFloatingPoint = false;
            if(bases == null)
                this.bases = new Structure[0];
            else
                this.bases = bases;
        }

        public PlaceHolderType (ChelaModule module, int id, string name, bool valueType, Structure[] bases)
            : this(module, id, name, valueType, false, bases)
        {
        }

        public PlaceHolderType (ChelaModule module, string name, bool valueType, Structure[] bases)
            : this(module, module.CreatePlaceHolderId(), name, valueType, bases)
        {
        }

        public PlaceHolderType (ChelaModule module, string name, bool valueType, List<Structure> bases)
            : base(module)
        {
            this.placeHolderId = module.CreatePlaceHolderId();
            this.name = name;
            this.valueType = valueType;
            if(bases == null)
                this.bases = new Structure[0];
            else
                this.bases = bases.ToArray();
        }

        public int GetPlaceHolderId()
        {
            return placeHolderId;
        }

        public override string GetName ()
        {
            return name;
        }

        public override string GetFullName ()
        {
            return "<" + placeHolderId + ">";
        }

        public override bool IsPlaceHolderType ()
        {
            return true;
        }

        public override bool IsGenericType ()
        {
            return true;
        }

        public override bool IsNumber()
        {
            if(isNumber)
                return true;
            isNumber = Implements(GetModule().GetNumberConstraint());
            return isNumber;
        }

        public override bool IsInteger()
        {
            if(isInteger)
                return true;
            isInteger = Implements(GetModule().GetIntegerConstraint());
            return isInteger;
        }

        public override bool IsFloatingPoint()
        {
            if(isFloatingPoint)
                return true;
            isFloatingPoint = Implements(GetModule().GetFloatingPointConstraint());
            return isFloatingPoint;
        }

        public override uint GetSize()
        {
            return 0;
        }

        public bool IsValueType()
        {
            return valueType;
        }

        public bool HasDefaultConstructor()
        {
            return hasDefCtor;
        }

        public int GetBaseCount()
        {
            return bases.Length;
        }

        public Structure GetBase(int index)
        {
            return bases[index];
        }

        public bool Implements(Interface iface)
        {
            for(int i = 0; i < bases.Length; ++i)
            {
                Structure baseStruct = bases[i];
                if(baseStruct == iface || baseStruct.Implements(iface))
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return valueType.GetHashCode() ^ bases.Length;
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as PlaceHolderType);
        }

        public bool Equals(PlaceHolderType obj)
        {
            // Check first differences.
            if(obj == null || valueType != obj.valueType)
                return false;

            // Compare the bases.
            for(int i = 0; i < bases.Length; ++i)
            {
                Structure myBase = bases[i];
                bool found = false;
                for(int j = 0; j < obj.bases.Length; ++j)
                {
                    if(obj.bases[j] == myBase)
                    {
                        found = true;
                        break;
                    }
                }

                // Return if not found.
                if(!found)
                    return false;
            }

            // No differences found.
            return true;
        }

        public override IChelaType InstanceGeneric (GenericInstance args, ChelaModule instModule)
        {
            GenericPrototype prototype = args.GetPrototype();
            for(int i = 0; i < args.GetParameterCount(); ++i)
            {
                if(prototype.GetPlaceHolder(i) == this)
                {
                    IChelaType type = args.GetParameter(i);
                    if(type.IsPassedByReference())
                        return ReferenceType.Create(type);
                    else
                        return type;
                }
            }
            
            return this;
        }
    }
}

