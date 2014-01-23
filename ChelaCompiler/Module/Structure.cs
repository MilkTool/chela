using System;
using System.Collections.Generic;
using System.Text;
using Chela.Compiler.Ast;

namespace Chela.Compiler.Module
{
	public class Structure: Scope, IChelaType
	{
		internal Dictionary<string, ScopeMember> members;
        internal List<ScopeMember> memberList;
		protected string name;
		internal MemberFlags flags;
		protected Scope parentScope;
		protected Structure baseStructure;
        protected List<Structure> interfaces;
		private List<FieldVariable> slots;
		private List<Method> virtualSlots;
        private List<ContractImplementation> contracts;
        private List<FieldDeclaration> fieldInitializations;
		protected FunctionGroup constructor;
        private Function staticConstructor;
        private Structure classInstance;
        private Structure selfInstance;
        private GenericPrototype genericPrototype;
        protected bool fixedInheritance;
        private int readedFieldCount;
        private int readedVMethodCount;
        private bool finishedLoad;
        private TokenPosition position;
        private bool incompleteBases;

        private struct ContractImplementation
        {
            public Structure Interface;
            public Method Contract;
            public Method FirstImplementation;
        };

        protected Structure (ChelaModule module)
            : base(module)
        {
            this.members = new Dictionary<string, ScopeMember> ();
            this.memberList = new List<ScopeMember> ();
            this.name = string.Empty;
            this.flags = MemberFlags.Default;
            this.parentScope = null;
            this.baseStructure = null;
            this.interfaces = new List<Structure> ();
            this.slots = new List<FieldVariable> ();
            this.virtualSlots = new List<Method> ();
            this.contracts = new List<ContractImplementation> ();
            this.fieldInitializations = new List<FieldDeclaration> ();
            this.constructor = null;
            this.staticConstructor = null;
            this.fixedInheritance = false;
            this.finishedLoad = true;
            this.incompleteBases = true;
            this.genericPrototype = GenericPrototype.Empty;
        }

        public Structure (string name, MemberFlags flags, Scope parentScope)
			: this(parentScope.GetModule())
		{
			this.name = name;
			this.flags = flags;
			this.parentScope = parentScope;
            this.genericPrototype = GenericPrototype.Empty;
		}

        public override string ToString ()
        {
            return GetFullName();
        }

		public override string GetName ()
		{
			return this.name;
		}

		public override MemberFlags GetFlags ()
		{
			return this.flags;
		}
		
		public override bool IsType ()
		{
			return true;
		}

        public override bool IsInterface ()
        {
            return false;
        }

		public override bool IsStructure ()
		{
			return true;
		}

        public virtual bool IsAbstract()
        {
            return (flags & MemberFlags.InstanceMask) == MemberFlags.Abstract ||
                IsInterface();
        }

		public virtual bool IsPrimitive()
		{
			return false;
		}

        public virtual bool IsFirstClass()
        {
            return IsPrimitive() || IsVector() || IsMatrix();
        }

		public virtual bool IsMetaType()
		{
			return false;
		}
		
		public virtual bool IsNumber()
		{
			return false;
		}
		
		public virtual bool IsInteger()
		{
			return false;
		}
		
		public virtual bool IsFloat()
		{
			return false;
		}
		
		public virtual bool IsDouble()
		{
			return false;
		}
		
		public virtual bool IsFloatingPoint()
		{
			return false;
		}
		
		public virtual bool IsUnsigned()
		{
			return false;
		}
		
		public virtual bool IsVoid()
		{
			return false;
		}

        public virtual bool IsArray()
        {
            return false;
        }

        public virtual bool IsVector()
        {
            return false;
        }

        public virtual bool IsMatrix()
        {
            return false;
        }

        public virtual bool IsConstant()
        {
            return false;
        }
		
		public virtual bool IsPointer()
		{
			return false;
		}
		
		public virtual bool IsReference()
		{
			return false;
		}

        public virtual bool IsOutReference()
        {
            return false;
        }

        public virtual bool IsStreamReference()
        {
            return false;
        }

        public virtual bool IsIncompleteType()
        {
            return false;
        }

        public virtual bool IsPlaceHolderType()
        {
            return false;
        }

        public virtual bool IsTypeInstance()
        {
            return false;
        }

        public virtual bool IsGenericBaseType()
        {
            return false;
        }

        public virtual bool IsGenericType()
        {
            return IsGeneric();
        }

        public virtual uint GetSize()
		{
			return 0;
		}

        public virtual uint GetComponentSize()
        {
            return GetSize();
        }

        public virtual bool IsAmbiguity()
        {
            return false;
        }

        public virtual void CheckAmbiguity(TokenPosition where)
        {
        }

        public virtual Structure GetTemplateBuilding()
        {
            return this;
        }

        public override bool IsGeneric()
        {
            if(genericPrototype.GetPlaceHolderCount() != 0)
            {
                GenericInstance inst = GetGenericInstance();
                if(inst == null || !inst.IsComplete())
                    return true;
            }

            if(parentScope == null)
                throw new ModuleException("cannot check IsGeneric() without a parent.");

            return parentScope.IsGeneric();
        }

        // This is required due to an egg-chicken problem in generic instantiation.
        public bool IncompleteBases {
            get {
                return incompleteBases;
            }
        }

        // Notify about the completion of bases.
        public void CompletedBases()
        {
            incompleteBases = false;
        }

        private class InstanceName: IEquatable<InstanceName>
        {
            private Structure template;
            private GenericInstance instance;
            private ChelaModule instanceModule;

            public InstanceName(Structure template, GenericInstance instance, ChelaModule instanceModule)
            {
                this.template = template;
                this.instance = instance;
                this.instanceModule = instanceModule;
            }

            public override int GetHashCode ()
            {
                return template.GetHashCode() ^ instance.GetHashCode() ^
                       instanceModule.GetHashCode();
            }

            public override bool Equals (object obj)
            {
                // Avoid casting.
                if(obj == this)
                    return true;
                return Equals(obj as InstanceName);
            }

            public bool Equals(InstanceName obj)
            {
                return obj != null &&
                    template == obj.template &&
                    instanceModule == obj.instanceModule &&
                    instance.Equals(obj.instance);
            }
        }

        private static Dictionary<InstanceName, Structure> genericInstances = new Dictionary<InstanceName, Structure> ();

        public virtual IChelaType InstanceGeneric(GenericInstance instance, ChelaModule instanceModule)
        {
            // Don't instance if I'm not generic.
            if(!IsGeneric())
                return this;

            // Find the existing instance.
            Structure ret;
            InstanceName name = new InstanceName(this, instance, instanceModule);
            if(genericInstances.TryGetValue(name, out ret))
                return ret;

            // Use here the particular instance.
            GenericInstance particularInstance = GetGenericPrototype().InstanceFrom(instance, instanceModule);

            // Create a new instance.
            ret = new StructureInstance(this, particularInstance, GetParentScope(), instanceModule);
            genericInstances.Add(name, ret);
            return ret;
        }

        public override ScopeMember InstanceMember (ScopeMember factory, GenericInstance instance)
        {
            // Don't instance if I'm not generic.
            if(!IsGeneric())
                return this;

            // Find the existing instance.
            Structure ret;
            InstanceName name = new InstanceName(this, instance, factory.GetModule());
            if(genericInstances.TryGetValue(name, out ret))
                return ret;

            // Use here the particular instance.
            GenericInstance particularInstance = GetGenericPrototype().InstanceFrom(instance, factory.GetModule());

            // Create a new instance.
            ret = new StructureInstance(this, particularInstance, factory, factory.GetModule());
            genericInstances.Add(name, ret);
            return ret;
        }

        public Structure GetClassInstance(ChelaModule module)
        {
            // Check the parent scope.
            Scope parentScope = GetParentScope();
            if(!parentScope.IsClass() && !parentScope.IsStructure() && !parentScope.IsInterface())
                return this;

            // Get the self instance of the parent.
            Structure parentSelf = ((Structure)parentScope).GetSelfInstance();

            // Avoid work.
            if(!parentSelf.IsTypeInstance())
                return this;

            // Find my instance there.
            ScopeMember myMember = parentSelf.FindMember(GetName());
            if(myMember.IsTypeGroup())
            {
                TypeGroup tgroup = (TypeGroup)myMember;
                foreach(TypeGroupName gname in tgroup.GetBuildings())
                {
                    Structure instance = gname.GetBuilding();
                    if(instance.GetTemplateBuilding() == GetTemplateBuilding())
                        return instance;
                }

                throw new ModuleException("Failed to find my own instance.");
            }
            else
            {
                return (Structure)myMember;
            }
        }

        public Structure GetClassInstance()
        {
            // Return the cached instance.
            if(classInstance != null)
                return classInstance;

            classInstance = GetClassInstance(GetModule());
            return classInstance;
        }

        public Structure GetSelfInstance(ChelaModule module)
        {
            // Get the class instance.
            Structure classInstance = null;
            if(module == GetModule()) // Make sure the cache is created.
                classInstance = GetClassInstance();
            else
                classInstance = GetClassInstance(module);

            // Try to instance it.
            GenericPrototype proto = classInstance.GetGenericPrototype();
            int numargs = proto.GetPlaceHolderCount();
            if(numargs != 0)
            {
                IChelaType[] args = new IChelaType[numargs];
                for(int i = 0; i < numargs; ++i)
                    args[i] = proto.GetPlaceHolder(i);
                GenericInstance genInstance = new GenericInstance(proto, args);
                return (Structure)classInstance.InstanceGeneric(genInstance, module);
            }
            else
            {
                return classInstance;
            }
        }

        public Structure GetSelfInstance()
        {
            // Return the cached instance.
            if(selfInstance != null)
                return selfInstance;

            selfInstance = GetSelfInstance(GetModule());
            return selfInstance;
        }

		public virtual bool IsDerivedFrom(Structure candidate)
		{
			Structure current = baseStructure;
			while(current != null)
			{
				if(candidate == current)
					return true;
				current = current.baseStructure;
			}
			
			return false;
		}

        public virtual bool Implements(Structure test)
        {
            if(baseStructure != null && baseStructure.Implements(test))
                return true;

            foreach(Structure iface in interfaces)
            {
                if(iface == test || iface.Implements(test))
                    return true;
            }
            
            return false;
        }

        public override GenericPrototype GetGenericPrototype()
        {
            return genericPrototype;
        }

        public void SetGenericPrototype(GenericPrototype genericPrototype)
        {
            this.genericPrototype = genericPrototype;
        }

        public bool IsBasedIn(Structure test)
        {
            return test == this || IsDerivedFrom(test) || Implements(test);
        }

		public override ScopeMember FindMember (string member)
		{
            ScopeMember ret;
            if(this.members.TryGetValue(member, out ret))
                return ret;
            else
                return null;
		}
		
		public override ScopeMember FindMemberRecursive(string member)
		{
            // Find the member in myself.
			ScopeMember ret = (ScopeMember)FindMember(member);
			if(!IsRecursiveContinue(ret))
				return ret;

            // Find in the base structure.
            if(baseStructure != null)
            {
				ScopeMember next = baseStructure.FindMemberRecursive(member);
                if(!RecursiveMerge(ref ret, next))
                    return ret;
            }

            // Find in the interfaces.
            if(ret == null)
            {
                foreach(Structure iface in interfaces)
                {
                    ret = iface.FindMemberRecursive(member);
                    if(ret != null)
                        return ret;
                }
            }

            return ret;
		}
		
		public override ICollection<ScopeMember> GetMembers ()
		{
			return this.members.Values;
		}
		
		public int GetVirtualSlotCount()
		{
			return this.virtualSlots.Count;
		}

		private bool MatchFunctionType(FunctionType a, FunctionType b, int skiparg)
		{
			if(skiparg == 0)
				return a == b;
			
			if(a.GetArgumentCount() < skiparg || b.GetArgumentCount() < skiparg ||
			   a.GetArgumentCount() != b.GetArgumentCount())
				return false;
			
			if(a.HasVariableArgument() != b.HasVariableArgument())
				return false;
			
			if(a.GetReturnType() != b.GetReturnType())
				return false;
			
			for(int i = skiparg; i < a.GetArgumentCount(); i++)
			{
				if(a.GetArgument(i) != b.GetArgument(i))
					return false;
			}
			
			return true;
		}
		
		private Function FindFunctionRecursive(Function function, string memberName,
            List<Method> contracts, int skiparg)
		{
			// Find the member with the same name.
            Function res = null;
			ScopeMember match;
            if(!members.TryGetValue(memberName, out match))
                match = null;

			if(match != null && (match.IsFunction() || match.IsFunctionGroup()))
			{
				// Check the function.
				if(match.IsFunction() && match.IsStatic() == function.IsStatic() &&
				   match.GetName() == function.GetName())
                {
                    // TODO: Check the correctness of this.
					res = (Function)match;
                    if(!res.IsStatic())
                        contracts.Add((Method)res);
                }
				// Check the group.
				else if(match.IsFunctionGroup())
				{
					// Find the function in the group.
					FunctionGroup fgroup = (FunctionGroup) match;
					foreach(FunctionGroupName gname in fgroup.GetFunctions())
					{
						if(gname.IsStatic() != function.IsStatic())
							continue;
						
						// Check if the function matches.
						Function candidate = gname.GetFunction();
						if(MatchFunctionType(candidate.GetFunctionType(), function.GetFunctionType(), skiparg))
                        {
                            if(candidate.IsContract())
                                contracts.Add((Method)candidate);
                            if(res == null)
                                res = candidate;
                        }
					}
				}
			}

			// Look at the base structure.
			if(baseStructure != null)
			{
				Function found = baseStructure.FindFunctionRecursive(function, memberName, contracts, skiparg);
				if(res == null)
                    res = found;
 			}

            // Find the function in the interfaces.
            foreach(Structure iface in interfaces)
            {
                Function found = iface.FindFunctionRecursive(function, memberName, contracts, skiparg);
                if(res == null)
                    res = found;
            }

			return res;
		}
		
		private Function FindBaseFunctionAndContracts(Function function, string memberName,
                List<Method> contracts, int skiparg)
		{
			// Find the function in the base.
			Function res = null;
			if(baseStructure != null)
			{
				Function method = baseStructure.FindFunctionRecursive(function, memberName, contracts, skiparg);
				if(res == null)
					res = method;
			}

            // Find the function in the interfaces.
            foreach(Structure iface in interfaces)
            {
                Function method = iface.FindFunctionRecursive(function, memberName, contracts, skiparg);
                if(res == null)
                    res = method;
            }
			
			return res;
		}

        private Method FindContractRecursive(Structure iface, Method contract)
        {
            // Check all of the implementations.
            foreach(ContractImplementation impl in contracts)
            {
                // Check the interface.
                if(impl.Interface != iface)
                    continue;

                // Return the first implementation.
                if(impl.Contract == contract)
                    return impl.FirstImplementation;
            }

            // Check in the base.
            if(baseStructure != null)
                return baseStructure.FindContractRecursive(iface, contract);

            // Nothing found.
            return null;
        }

        private void ImplementContract(Method implementation, Method contract)
        {
            // Get the contract interface.
            Structure iface = (Structure)contract.GetParentScope();

            // Check if the contract is already implemented.
            Method oldImplementation = FindContractRecursive(iface, contract);
            if(oldImplementation != null)
            {
                // TODO: Check for error
                return;
            }

            // Not implemented, create the implementation.
            ContractImplementation impl = new ContractImplementation();
            impl.Interface = iface;
            impl.Contract = contract;
            impl.FirstImplementation = implementation;
            contracts.Add(impl);
        }

        private void ImplementContracts(Method implementation, List<Method> contracts)
        {
            foreach(Method contract in contracts)
                ImplementContract(implementation, contract);
        }

		private void FixFunction(Function function, string memberName)
		{
			// Ignore static functions.
			if(function.IsStatic() || function.IsStaticConstructor())
				return;
			
			// Find the same function in base structures.
            List<Method> contracts = new List<Method> ();
			Method baseMethod = null;

            // Use the explicit contract
            Method method = (Method) function;
            if(method.GetExplicitContract() != null)
            {
                baseMethod = (Method) method.GetExplicitContract();
                contracts.Add(baseMethod);
            }
            else
            {
                Function baseFunction = FindBaseFunctionAndContracts(function, memberName, contracts, 1);
                baseMethod = baseFunction != null && baseFunction.IsMethod() ? (Method)baseFunction : null;
            }

			// Check the virtual/override flags.
            if(method.IsAbstract() || method.IsContract())
            {
                if(baseMethod != null && !baseMethod.IsContract() && !baseMethod.IsAbstract())
                    Error("cannot convert into abstract an inherited method.");

                if(baseMethod != null)
                {
                    if((method.IsContract() && baseMethod.IsContract()) ||
                       (method.IsAbstract() && baseMethod.IsAbstract()))
                    {
                        method.vslot = baseMethod.vslot;
                    }
                    else
                    {
                        method.vslot = (int)GetVirtualSlotCount();
                        virtualSlots.Add(method);
                        if(method.IsAbstract())
                            ImplementContracts(method, contracts);
                    }
                }
                else
                {
                    method.vslot = (int)GetVirtualSlotCount();
                    virtualSlots.Add(method);
                }
            }
			else if(method.IsVirtual())
			{
				if(baseMethod != null && !baseMethod.IsContract())
                    method.Error("virtual method shadows a previous implementation",
                        method.GetDisplayName());
				method.vslot = (int)GetVirtualSlotCount();
				virtualSlots.Add(method);

                // Support interfaces
                if(baseMethod != null && baseMethod.IsContract())
                    ImplementContracts(method, contracts);
			}
			else if(method.IsOverride())
			{
                // Base method must exist.
                if(baseMethod == null)
                    method.Error("cannot override an inexistent method.");
				// Check the base function.
				if(!baseMethod.IsVirtual() && !baseMethod.IsOverride() && !baseMethod.IsAbstract())
					method.Error("cannot override a not virtual/override method.");
                if(baseMethod.IsSealed())
                    method.Error("cannot override a sealed method.");
				
				// Set the vslot.
				method.vslot = baseMethod.vslot;
                virtualSlots[method.vslot] = method;
			}
            else if(!method.IsStatic())
            {
                // Support interfaces
                if(baseMethod != null && baseMethod.IsContract())
                    ImplementContracts(method, contracts);
            }
		}
		
		private void FixVTable()
		{
			// Fix the parent vtable.
			if(baseStructure != null)
            {
                // Copy the base virtual slots.
                for(int i = 0; i < baseStructure.virtualSlots.Count; ++i)
                    virtualSlots.Add(baseStructure.virtualSlots[i]);
            }

            // Fix the interfaces vtable.
			foreach(Structure iface in interfaces)
            {
                // Copy the interfaces slot.
                if(IsInterface())
                {
                    for(int i = 0; i < iface.virtualSlots.Count; ++i)
                        virtualSlots.Add(iface.virtualSlots[i]);
                }
            }

			// Iterate the functions.
			foreach(ScopeMember member in memberList)
			{
				if(member.IsFunction())
				{
					Function function = (Function) member;
					FixFunction(function, function.GetName());
				}
				else if(member.IsFunctionGroup())
				{
					FunctionGroup fgroup = (FunctionGroup) member;
					foreach(FunctionGroupName gname in fgroup.GetFunctions())
						FixFunction(gname.GetFunction(), fgroup.GetName());
				}
			}

            // Check for abstract methods.
            if(!IsAbstract())
            {
                StringBuilder message = new StringBuilder();
                message.Append("found abstract methods in not abstract class ");
                message.Append(GetFullName());
                message.Append(":");

                bool isAbstract = false;
                for(int i = 0; i < virtualSlots.Count; ++i)
                {
                    Function method = (Function)virtualSlots[i];
                    if(method == null)
                        Error("found null vslot.");

                    if(method.IsAbstract() || method.IsContract())
                    {
                        message.Append("\n\t");
                        message.Append(method.GetFullName());
                        isAbstract = true;
                    }
                }

                // Throw an exception for abstract methods.
                if(isAbstract)
                    throw new ModuleException(message.ToString());

                // TODO: Check for contract implementations.
            }
		}

        private void FixSlots()
        {
            if(IsInterface())
                return;

            // Recount the slot indices.
            int index = GetBaseSlot();
            foreach(FieldVariable slot in slots)
                slot.slot = index++;
        }

        public virtual void FixInheritance()
        {
            // Only fix once.
            if(fixedInheritance)
                return;
            fixedInheritance = true;

            // Use the correct base.
            if(IsClass() && baseStructure == null)
            {
                // Make sure object is a super class.
                Structure objectClass = (Structure)GetModule().TypeMap(ChelaType.GetObjectType());
                if(this != objectClass)
                    baseStructure = objectClass;
            }

            // Fix the base structure.
            if(baseStructure != null)
            {
                baseStructure.FixInheritance();
                if(baseStructure.IsSealed())
                    Error("cannot inherit from a sealed class.");
            }

            // Fix the interfaces inheritance.
            foreach(Structure iface in interfaces)
                iface.FixInheritance();

            // Fix the vtable.
            FixVTable();

            // Fix the slots.
            FixSlots();
        }


        private void AddMember(ScopeMember member)
        {
            memberList.Add(member);
            members.Add(member.GetName(), member);
        }
		
		public void AddFunction(string groupName, Function function)
		{
			// Perform function adding checks.
			bool isConstructor = false;
			if(!function.IsStatic() && !function.IsStaticConstructor())
			{
				// Cast it into a method.
				Method method = (Method)function;
				if(method.IsConstructor())
					isConstructor = true;

                if((method.IsAbstract() || method.IsContract()) &&
                    !IsAbstract())
                    throw new ModuleException("cannot have abstract method " + method.GetFullName() +
                        " in no abstract class " + GetFullName());
			}
            else if(function.IsStaticConstructor())
            {
                if(staticConstructor != null)
                    throw new ModuleException("only one static constructor per class is allowed.");
                staticConstructor = function;

                // Don't use a function group for the static constructor.
                staticConstructor.SetName("<sctor>");
                AddMember(staticConstructor);
                return;
            }
			
			// Get the existent function group.
			if(isConstructor)
				groupName = "<ctor>";
			ScopeMember member = FindMember(groupName);
			FunctionGroup fgroup;
			if(member == null && !function.IsCdecl())
			{
				fgroup = new FunctionGroup(groupName, this);
				member = fgroup;
				AddMember(fgroup);
			}
			else
			{
				if(member != null && !member.IsFunctionGroup() && !function.IsCdecl())
					throw new ModuleException("Cannot add a function into something that isn't a function group.");
                else if(function.IsCdecl())
                    throw new ModuleException("Cannot add an overloading cdecl function.");
                fgroup = (FunctionGroup)member;
			}
			
			// Add the function.
			fgroup.Insert(function);
			
			// Set the constructor group.
			if(isConstructor && this.constructor == null)
				this.constructor = fgroup;
		}

        public void AddProperty(PropertyVariable property)
        {
            // Add the accessors.
            if(property.GetAccessor != null)
                AddFunction("get_" + property.GetName(), property.GetAccessor);
            if(property.SetAccessor != null)
                AddFunction("set_" + property.GetName(), property.SetAccessor);

            // Add the property.
            AddMember(property);
        }

        public void AddEvent(EventVariable eventVariable)
        {
            // Add the accessors.
            if(eventVariable.AddModifier != null)
                AddFunction("add_" + eventVariable.GetName(), eventVariable.AddModifier);
            if(eventVariable.RemoveModifier != null)
                AddFunction("remove_" + eventVariable.GetName(), eventVariable.RemoveModifier);

            // Add the property.
            AddMember(eventVariable);
        }

		public int GetBaseSlot()
		{
			if(baseStructure != null)
				return baseStructure.GetTotalSlotCount();
			else
				return 0;
		}
		
		public int GetSlotCount()
		{
			return this.slots.Count;
		}
		
		public int GetTotalSlotCount()
		{
			return GetBaseSlot() + GetSlotCount();
		}

        public void AddTypeName(TypeNameMember typeName)
        {
            // Store it.
            AddMember(typeName);
        }

        public void AddType(IChelaType type)
        {
            // Cast the type.
            ScopeMember member = (ScopeMember)type;

            // Store it.
            AddMember(member);
        }

		public void AddField(FieldVariable field)
		{
			if(!field.IsStatic())
			{
                if(IsInteger())
                    throw new ModuleException("interfaces cannot have slots.");
				// Set the field slot.
				field.slot = (int)GetTotalSlotCount();
				slots.Add(field);
			}
			
			// Add the field.
            AddMember(field);
		}		

        public ICollection<FieldDeclaration> GetFieldInitializations()
        {
            return fieldInitializations;
        }

        public void AddFieldInitialization(FieldDeclaration decl)
        {
            fieldInitializations.Add(decl);
        }

		public override Scope GetParentScope()
		{
			return this.parentScope;
		}
		
		public virtual Structure GetBase()
		{
			return this.baseStructure;
		}
		
		public void SetBase(Structure baseStructure)
		{
            // Prevent cyclic inheritance.
            if(baseStructure == this || (baseStructure != null && baseStructure.IsDerivedFrom(this)))
                throw new ModuleException("Cyclic inheritance is invalid.");

            // Store the new base.
            this.baseStructure = baseStructure;

            // Make sure this is a valid base.
            if(baseStructure.IsClass() != IsClass())
            {
                if(IsStructure() && baseStructure.IsClass())
                {
                    Class valueType = GetModule().GetValueTypeClass();
                    if(baseStructure == valueType ||
                       baseStructure.IsDerivedFrom(valueType))
                        return;
                }
            }
            else
            {
                return;
            }

            throw new ModuleException("Incompatible base class.");
		}

        public virtual int GetInterfaceCount()
        {
            return interfaces.Count;
        }

        public Structure GetInterface(int index)
        {
            return interfaces[index];
        }

        public void AddInterface(Structure iface)
        {
            // Prevent cyclic interfaces.
            if(iface == this || iface.Implements(this))
                throw new ModuleException("Cyclic interfaces are invalid.");

            // Don't add old interfaces.
            foreach(Structure oldInterface in this.interfaces)
                if(oldInterface == iface)
                    return;

            this.interfaces.Add(iface);
        }

        public Function GetStaticConstructor()
        {
            return this.staticConstructor;
        }

		public virtual FunctionGroup GetConstructor()
		{
			return this.constructor;
		}
		
		internal override void PrepareSerialization()
		{
            // Prepare myself.
			base.PrepareSerialization();

            // Register the base class, and the interfaces.
            ChelaModule module = GetModule();
            module.RegisterMember(baseStructure);
            foreach(Structure iface in interfaces)
                module.RegisterMember(iface);

            // Register the generic prototype.
            genericPrototype.PrepareSerialization(module);

            // Register the contracts.
            foreach(ContractImplementation impl in contracts)
            {
                module.RegisterMember(impl.Interface);
                module.RegisterMember(impl.Contract);
                module.RegisterMember(impl.FirstImplementation);
            }

            // Prepare the children.
			foreach(ScopeMember member in memberList)
				member.PrepareSerialization();
		}

        internal override void PrepareDebug(DebugEmitter debugEmitter)
        {
            debugEmitter.AddStructure(this);
            foreach(ScopeMember member in members.Values)
                member.PrepareDebug(debugEmitter);
        }
		
		public override void Write (ModuleWriter writer)
		{
            // Write the member header
            MemberHeader mheader = new MemberHeader();
            mheader.memberName = (uint)GetModule().RegisterString(GetName());
            mheader.memberFlags = (uint)GetFlags();
            mheader.memberSize =
                (uint)(interfaces.Count*4 +
                       members.Count*4 +
                       contracts.Count*12 +
                       StructureHeader.HeaderSize +
                       genericPrototype.GetSize());
            mheader.memberAttributes = GetAttributeCount();

            if(IsClass())
                mheader.memberType = (byte)MemberHeaderType.Class;
            else if(IsStructure())
                mheader.memberType = (byte)MemberHeaderType.Structure;
            else if(IsInterface())
                mheader.memberType = (byte)MemberHeaderType.Interface;

            mheader.Write(writer);

            // Write the attributes.
            WriteAttributes(writer);

            // Write the generic prototype.
            ChelaModule module = GetModule();
            genericPrototype.Write(writer, module);

			// Write the structure header.
			StructureHeader header = new StructureHeader();
			header.baseStructure = baseStructure != null ? module.RegisterMember(baseStructure) : 0;
            header.numinterfaces = (ushort)interfaces.Count;
			header.numfields = (ushort)GetTotalSlotCount();
			header.numvmethods = (ushort)GetVirtualSlotCount();
            header.numcontracts = (ushort)contracts.Count;
            header.nummembers = (ushort)members.Count;
			header.Write(writer);

            // Write the interface ids.
            foreach(Structure iface in this.interfaces)
                writer.Write((uint)module.RegisterMember(iface));

            // Write the contracts.
            foreach(ContractImplementation impl in contracts)
            {
                writer.Write((uint)module.RegisterMember(impl.Interface));
                writer.Write((uint)module.RegisterMember(impl.Contract));
                writer.Write((uint)module.RegisterMember(impl.FirstImplementation));
            }

			// Write the member ids.
			foreach(ScopeMember member in this.memberList)
				writer.Write((uint)member.GetSerialId());
		}

        internal static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal structure and register it.
            Structure building = new Structure(module);
            module.RegisterMember(building);

            // Read the name.
            building.name = module.GetString(header.memberName);
            building.flags = (MemberFlags)header.memberFlags;

            // Skip the structure elements.
            reader.Skip(header.memberSize);
        }

        internal override void Read(ModuleReader reader, MemberHeader header)
        {
            // Get the module.
            ChelaModule module = GetModule();

            // Read the generic prototype.
            genericPrototype = GenericPrototype.Read(reader, GetModule());

            // Read the header.
            StructureHeader sheader = new StructureHeader();
            sheader.Read(reader);

            // Read the base structure.
            baseStructure = (Structure)module.GetMember(sheader.baseStructure);

            // Read the interfaces.
            for(int i = 0; i < sheader.numinterfaces; ++i)
            {
                Structure iface = (Structure)module.GetMember(reader.ReadUInt());
                if(iface != null)
                    this.interfaces.Add(iface);
            }

            // Read the contracts
            for(int i = 0; i < sheader.numcontracts; ++i)
            {
                ContractImplementation contract;
                contract.Interface = (Structure)module.GetMember(reader.ReadUInt());
                contract.Contract = (Method)module.GetMember(reader.ReadUInt());
                contract.FirstImplementation = (Method)module.GetMember(reader.ReadUInt());
                contracts.Add(contract);
            }

            // Read the members.
            for(int i = 0; i < sheader.nummembers; ++i)
            {
                ScopeMember member = module.GetMember(reader.ReadUInt());
                AddMember(member);
            }

            // Store the fields and vmethod count.
            readedFieldCount = sheader.numfields;
            readedVMethodCount = sheader.numvmethods;
            finishedLoad = false;

            // The bases are loaded now.
            CompletedBases();
        }

        private void UpdateReadedFunction(Function function)
        {
            // Store the function in his slot.
            if(function.IsMethod())
            {
                Method method = (Method)function;
                if(method.IsVirtual() || method.IsAbstract() ||
                   (method.IsContract() && IsInterface()))
                    virtualSlots[method.GetVSlot()] = method;
            }

            // Store the static constructor.
            if(function.IsStaticConstructor())
                staticConstructor = function;
        }

        internal override void FinishLoad()
        {
            // Only finish loading once
            if(finishedLoad)
                return;
            finishedLoad = true;

            // Used to specify the slots.
            int baseSlot = 0;
            int numfields = readedFieldCount;

            // Reserve space the virtual methods.
            int numvmethods = readedVMethodCount;
            virtualSlots.Capacity = numvmethods;
            for(int i = 0; i < numvmethods; ++i)
                virtualSlots.Add(null);

            // Finish loading the parent.
            if(baseStructure != null)
            {
                baseStructure.FinishLoad();

                // Use the parent virtual methods.
                for(int i = 0; i < baseStructure.virtualSlots.Count; ++i)
                    virtualSlots[i] = baseStructure.virtualSlots[i];

                // Compute base slot.
                baseSlot = baseStructure.readedFieldCount;
                numfields -= baseSlot;
            }

            // Reserve space for the slots.
            slots.Capacity = numfields;
            for(int i = 0; i < numfields; ++i)
                slots.Add(null);

            // Update the children.
            foreach(ScopeMember child in members.Values)
            {
                // Finish the child loading.
                child.FinishLoad();
                
                // Store the child slot.
                if(child.IsVariable() && !child.IsStatic())
                {
                    Variable childVar = (Variable)child;
                    if(childVar.IsField())
                    {
                        FieldVariable field = (FieldVariable)childVar;
                        slots[field.GetSlot() - baseSlot] = field;
                    }
                }
                else if(child.IsFunction())
                {
                    // Update the function.
                    UpdateReadedFunction((Function)child);
                }
                else if(child.IsFunctionGroup())
                {
                    FunctionGroup fgroup = (FunctionGroup)child;
                    bool ctorGroup = false;
                    foreach(FunctionGroupName fgroupName in fgroup.GetFunctions())
                    {
                        Function function = fgroupName.GetFunction();

                        // Check if the function is a constructor.
                        if(function.IsConstructor())
                            ctorGroup = true;

                        // Update the function.
                        UpdateReadedFunction(function);
                    }

                    // Store the constructor group.
                    if(ctorGroup)
                        this.constructor = fgroup;
                }
            }

            // Set the fixed vtable and slots flags.
            fixedInheritance = true;
        }

        internal List<Structure> genericInstanceList = new List<Structure> ();
        
        internal override void UpdateParent (Scope parentScope)
        {
            // Store the new parent.
            this.parentScope = parentScope;

            // Update my instances scopes.
            foreach(Structure instance in genericInstanceList)
                instance.parentScope = parentScope;

            // Update the relationships.
            foreach(ScopeMember child in members.Values)
                child.UpdateParent(this);
        }

		public override void Dump ()
		{
            string kind = "unknown";
            if(IsClass())
                kind = "class";
            else if(IsStructure())
                kind = "struct";
            else if(IsInterface())
                kind = "interface";

			Dumper.Printf("%s %s %s(%d, %d): %s", GetFlagsString(), kind, name,
			              GetTotalSlotCount(), GetVirtualSlotCount(),
			              baseStructure != null ? baseStructure.GetFullName() : "None");
            string ifaces = "";
            foreach(Structure iface in this.interfaces)
            {
                if(ifaces == "")
                    ifaces = iface.GetFullName();
                else
                    ifaces += ", " + iface.GetFullName();
            }
            Dumper.Printf("implements %s", ifaces);

			Dumper.Printf("{");
			{
				Dumper.Incr();
				
				// Dump the members.
				foreach(ScopeMember member in memberList)
					member.Dump();

                // Dump the contracts.
                if(contracts.Count > 0)
                {
                    Dumper.Printf("contracts {");
                    Dumper.Incr();
                    foreach(ContractImplementation contract in contracts)
                    {
                        Dumper.Printf("[%s]%s = %s;", contract.Interface.GetFullName(),
                                      contract.Contract.GetName(),
                                      contract.FirstImplementation.GetName());
                    }
                    Dumper.Decr();
                    Dumper.Printf("}");
                }

                Dumper.Decr();
			}
			Dumper.Printf("}");
		}

        /// <summary>
        /// Dumps the inheritance graph. Useful for debugging purposes.
        /// </summary>
        protected void DumpGraph(Structure self)
        {
            Dumper.Printf("+%s %s", self.IsInterface() ? "i": "", self.GetFullName());
            Dumper.Incr();

            // Dump parents.
            Structure baseBuilding = self.GetBase();
            if(baseBuilding != null)
                DumpGraph(baseBuilding);
            for(int i = 0; i < self.GetInterfaceCount(); ++i)
                DumpGraph(self.GetInterface(i));

            foreach(ScopeMember member in self.GetMembers())
                Dumper.Printf("- %s", member.GetFullName());
            Dumper.Decr();
        }

        public override TokenPosition Position {
            get {
                return position;
            }
            set {
                position = value;
            }
        }
	}
}

