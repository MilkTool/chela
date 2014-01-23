using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Function type.
    /// </summary>
    public class FunctionType: ChelaType, IEquatable<FunctionType>
    {
        private static SimpleSet<FunctionType> functionTypes = new SimpleSet<FunctionType> ();
        private IChelaType returnType;
        private IChelaType[] arguments;
        private bool variableArguments;
        private MemberFlags flags;
        private string name;
        private string displayName;
        private string fullName;

        protected FunctionType(IChelaType returnType, IChelaType[] arguments, bool variableArguments, MemberFlags flags)
        {
            this.returnType = returnType;
            this.arguments = arguments;
            this.variableArguments = variableArguments;
            this.flags = flags;
            this.name = null;
            this.displayName = null;
            this.fullName = null;
        }
     
        public override bool IsFunction()
        {
            return true;
        }
     
        /// <summary>
        /// Gets the return type.
        /// </summary>
        public IChelaType GetReturnType()
        {
            return this.returnType;
        }
     
        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public IList<IChelaType> GetArguments()
        {
            return this.arguments;
        }

        /// <summary>
        /// Gets the argument count.
        /// </summary>
        public int GetArgumentCount()
        {
            return this.arguments.Length;
        }
     
        /// <summary>
        /// Gets the argument in the specified index.
        /// </summary>
        public IChelaType GetArgument(int index)
        {
            return this.arguments[index];
        }
     
        /// <summary>
        /// Determines whether this function contains a variable
        /// number of parameters.
        /// </summary>
        public bool HasVariableArgument()
        {
            return this.variableArguments;
        }

        public MemberFlags GetFunctionFlags()
        {
            return this.flags;
        }

        public override int GetHashCode()
        {
            return GetHashCode(0, false, false);
        }

        /// <summary>
        /// Gets the hash code, but with the possibility of ignoring
        /// some arguments and the return type..
        /// </summary>
        public int GetHashCode(int ignore, bool noret, bool noflags)
        {
            // Add the function type size and variable arguments flag.
            int hash = arguments.Length ^ variableArguments.GetHashCode();

            // Add the return type hash code.
            if(!noret)
                hash ^= RuntimeHelpers.GetHashCode(returnType);

            // Add the flags hash code.
            if(!noflags)
                hash ^= flags.GetHashCode();

            // Add the arguments.
            int size = arguments.Length;
            for(int i = ignore; i < size; ++i)
            {
                // Get the argument.
                IChelaType arg = arguments[i];

                // Handle ref/out equality.
                if(arg.IsReference())
                {
                    ReferenceType refType = (ReferenceType)arg;
                    arg = refType.GetReferencedType();
                }

                // Add the argument hash.
                hash ^= RuntimeHelpers.GetHashCode(arg);
            }

            return hash;
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as FunctionType);
        }

        public bool Equals(FunctionType obj)
        {
            return Equals(obj, 0, false, false);
        }

        /// <summary>
        /// Checks for equality with the possibility of ignore some arguments
        /// and maybe the return type.
        /// </summary>
        public bool Equals(FunctionType obj, int ignore, bool noret, bool noflags)
        {
            // Reject comparison with null, make sure the lengths are the same.
            if(obj == null ||
                arguments.Length != obj.arguments.Length ||
                variableArguments != obj.variableArguments)
                return false;

            // Compare the return type.
            if(!noret && returnType != obj.returnType)
                return false;

            // Compare the flags.
            if(!noflags && flags != obj.flags)
                return false;

            // Compare the arguments.
            int size = arguments.Length;
            for(int i = ignore; i < size; ++i)
            {
                IChelaType leftArg = arguments[i];
                IChelaType rightArg = obj.arguments[i];

                // Handle ref/out equality.
                if(leftArg != rightArg && leftArg != null && rightArg != null &&
                    leftArg.IsReference() && rightArg.IsReference())
                {
                    ReferenceType leftRef = (ReferenceType)leftArg;
                    ReferenceType rightRef = (ReferenceType)rightArg;
                    leftArg = leftRef.GetReferencedType();
                    rightArg = rightRef.GetReferencedType();
                }

                // Reject if different.
                if(leftArg != rightArg)
                    return false;
            }

            // Everything is equal, return true.
            return true;
        }
     
        /// <summary>
        /// Gets the short name.
        /// </summary>
        public override string GetName()
        {
            if(name == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(returnType.GetName());
                builder.Append("(");
             
                int numargs = arguments.Length;
                for(int i = 0; i < numargs; i++)
                {
                    if(i > 0)
                        builder.Append(", ");
                 
                    if(IsVariable() && i + 1 == numargs)
                        builder.Append("params ");
                 
                    IChelaType arg = arguments[i];
                    builder.Append(arg.GetName());
                }
                builder.Append(")");
                name = builder.ToString();
            }
         
            return name;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string GetDisplayName()
        {
            if(displayName == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(returnType.GetDisplayName());
                builder.Append("(");
                 
                int numargs = arguments.Length;
                for(int i = 0; i < numargs; i++)
                {
                    if(i > 0)
                        builder.Append(", ");
    
                    if(IsVariable() && i + 1 == numargs)
                        builder.Append("params ");
    
                    IChelaType arg = arguments[i];
                    builder.Append(arg.GetDisplayName());
                }
                builder.Append(")");
                displayName = builder.ToString();
            }
    
            return displayName;
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public override string GetFullName()
        {
            if(fullName == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(returnType.GetFullName());
                builder.Append("(");
                 
                int numargs = arguments.Length;
                for(int i = 0; i < numargs; i++)
                {
                    if(i > 0)
                        builder.Append(",");
    
                    if(IsVariable() && i + 1 == numargs)
                        builder.Append("params ");
    
                    IChelaType arg = arguments[i];
                    builder.Append(arg.GetFullName());
                }
                builder.Append(")");
                fullName = builder.ToString();
            }
    
            return fullName;
        }

        public override IChelaType InstanceGeneric(GenericInstance args, ChelaModule instModule)
        {
            // Instance the return type.
            IChelaType returnType = this.returnType.InstanceGeneric(args, instModule);

            // Instance all of the parameters.
            List<IChelaType > parameters = new List<IChelaType>();
            for(int i = 0; i < arguments.Length; ++i)
            {
                IChelaType arg = arguments[i];
                parameters.Add(arg.InstanceGeneric(args, instModule));
            }

            // Return a function type with the instanced types.
            return Create(returnType, parameters, variableArguments);
        }

        /// <summary>
        /// Create a function type with the specified parameters.
        /// </summary>
        public static FunctionType Create(IChelaType ret, IChelaType[] arguments, bool variable, MemberFlags flags)
        {
            // Use an empty array when the arguments are null.
            if(arguments == null)
                arguments = new IChelaType[]{};

            // First create a new function.
            FunctionType function = new FunctionType(ret, arguments, variable, flags & MemberFlags.LanguageMask);
         
            // Try to add it into the set.
            return functionTypes.GetOrAdd(function);
        }

        public static FunctionType Create(IChelaType ret, IChelaType[] arguments, bool variable)
        {
            return Create(ret, arguments, variable, MemberFlags.Default);
        }

        public static FunctionType Create(IChelaType ret, List<IChelaType> arguments, bool variable, MemberFlags flags)
        {
            return Create(ret, arguments.ToArray(), variable, flags);
        }

        public static FunctionType Create(IChelaType ret, List<IChelaType> arguments, bool variable)
        {
            return Create(ret, arguments, variable, MemberFlags.Default);
        }

        public static FunctionType Create(IChelaType ret, List<IChelaType> arguments)
        {
            return Create(ret, arguments, false);
        }

        public static FunctionType Create(IChelaType ret)
        {
            return Create(ret, new List<IChelaType> (), false);
        }
    }
}

