using System;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Reference type.
    /// </summary>
    public class ReferenceType: ChelaType, IEquatable<ReferenceType>
    {
        private static SimpleSet<ReferenceType> referenceTypes = new SimpleSet<ReferenceType> ();
        private IChelaType referencedType;
        private string name;
        private string displayName;
        private string fullName;
        private ReferenceFlow referenceFlow;
        private bool streamReference;
     
        internal ReferenceType(IChelaType actualType, ReferenceFlow flow, bool streamReference)
        {
            this.referencedType = actualType;
            this.name = null;
            this.fullName = null;
            this.referenceFlow = flow;
            this.streamReference = streamReference;
        }
     
        public override bool IsReference()
        {
            return true;
        }

        public override bool IsOutReference()
        {
            return referenceFlow == ReferenceFlow.Out;
        }

        public ReferenceFlow GetReferenceFlow()
        {
            return referenceFlow;
        }

        public override bool IsStreamReference()
        {
            return streamReference;
        }

        public override bool IsGenericType()
        {
            return referencedType.IsGenericType();
        }
     
        /// <summary>
        /// Gets the short name.
        /// </summary>
        public override string GetName()
        {
            if(referencedType == null)
                return "<null>&";

            if(name == null)
            {
                switch(referenceFlow)
                {
                case ReferenceFlow.In:
                    name = "in ";
                    break;
                case ReferenceFlow.Out:
                    name = "out ";
                    break;
                case ReferenceFlow.InOut:
                default:
                    name = "";
                    break;
                }

                name += referencedType.GetName();
                if(streamReference)
                    name += "$&";
                else
                    name += "&";
            }
            return name;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string GetDisplayName()
        {
            if(referencedType == null)
                return "null";

            if(displayName == null)
            {
                switch(referenceFlow)
                {
                case ReferenceFlow.In:
                    displayName = "in ";
                    break;
                case ReferenceFlow.Out:
                    displayName = "out ";
                    break;
                case ReferenceFlow.InOut:
                default:
                    if(referencedType.IsPassedByReference())
                        displayName = "";
                    else
                        displayName = "ref ";
                    break;
                }

                displayName += referencedType.GetDisplayName();
            }

            return displayName;
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public override string GetFullName()
        {
            if(referencedType == null)
                return "<null>&";

            if(fullName == null)
            {
                switch(referenceFlow)
                {
                case ReferenceFlow.In:
                    fullName = "in ";
                    break;
                case ReferenceFlow.Out:
                    fullName = "out ";
                    break;
                case ReferenceFlow.InOut:
                default:
                    fullName = "";
                    break;
                }

                fullName += referencedType.GetFullName();
                if(streamReference)
                    fullName += "$&";
                else
                    fullName += "&";
            }

            return fullName;
        }

        public override IChelaType InstanceGeneric(GenericInstance args, ChelaModule instModule)
        {
            return Create(referencedType.InstanceGeneric(args, instModule), referenceFlow, streamReference);
        }
             
        public IChelaType GetReferencedType()
        {
            return referencedType;
        }

        public override int GetHashCode()
        {
            return referenceFlow.GetHashCode() ^
                   streamReference.GetHashCode() ^
                   RuntimeHelpers.GetHashCode(referencedType);
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as ReferenceType);
        }

        public bool Equals(ReferenceType obj)
        {
            return obj != null &&
                   referencedType == obj.referencedType &&
                   referenceFlow == obj.referenceFlow &&
                   streamReference == obj.streamReference;
        }
     
        public static ReferenceType Create(IChelaType actualType, ReferenceFlow flow, bool streamReference)
        {
            ReferenceType reference = new ReferenceType(actualType, flow, streamReference);
            return referenceTypes.GetOrAdd(reference);
        }

        public static ReferenceType Create(IChelaType actualType, bool outRef)
        {
            return Create(actualType, outRef ? ReferenceFlow.Out : ReferenceFlow.InOut, false);
        }

        public static ReferenceType Create(IChelaType actualType, ReferenceFlow flow)
        {
            return Create(actualType, flow, false);
        }

        public static ReferenceType Create(IChelaType actualType)
        {
            return Create(actualType, ReferenceFlow.InOut, false);
        }
    }
}

