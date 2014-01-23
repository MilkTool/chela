using System;
namespace Chela.Compiler.Module
{
	[Flags]
	// Make sure to keep this synchronized with Member.hpp
	public enum MemberFlags
	{
		// Visibility.
		Internal = 0,
		Public = 1,
		Protected = 2,
        ProtectedInternal = 3,
		Private = 4,
        ImplicitVis = 5,
		VisibilityMask = 0x000F,

		// Instance.
		Instanced = 0<<4,
		Static = 1<<4,
		Virtual = 2<<4,
		Override = 3<<4,
		Constructor = 4<<4,
        Abstract = 5<<4,
        Contract = 6<<4,
        StaticConstructor = 7<<4,
		InstanceMask = 0x00F0,

		// Language.
		Native = 0<<8,
        Kernel = 1<<8,
        Runtime = 2<<8,
		Cdecl = 3<<8, // C functions/globals.
        StdCall =4<<8, // Pascal style calling convention
        ApiCall = 5<<8, // Stdcall under windows, cdecl under unixes.
		LanguageMask = 0x0F00,

		// Linkage
		Module = 0<<12,
		External = 1<<12,
		LinkageMask = 0xF000,

        // Inheritance
        NormalInheritance = 0<<16,
        Sealed = 1<<16,
        InheritanceMask = 0x000F0000,

        // Access
        DefaultAccess = 0<<20,
        ReadOnly= 1<<20,
        AccessMask = 0x00F00000,

        // Security.
        DefaultSecurity = 0<<24,
        Unsafe = 1<<24,
        SecurityMask = 0x0F000000,

        // Implementation
        DefaultImplFlag = 0<<28,
        Partial = 1<<28,
        ImplFlagMask = unchecked ((int)0xF0000000),

		Default = 0,
        InterfaceMember = Public | Contract,
	}
}

