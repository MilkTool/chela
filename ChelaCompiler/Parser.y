%{
using System;
using System.Text;
using Chela.Compiler.Module;
using Chela.Compiler.Ast;

namespace Chela.Compiler
{
	internal class Parser
	{
		///
		/// Controls the verbosity of the errors produced by the parser
		///
		static public int yacc_verbose_flag = 0;

        /// Flags and mask.
        class MemberFlagsAndMask: TokenPosition
        {
            public MemberFlags flags;
            public MemberFlags mask;

            public MemberFlagsAndMask(MemberFlags flags, MemberFlags mask, TokenPosition position)
                : base(position)
            {
                this.flags = flags;
                this.mask = mask;
            }

            public MemberFlagsAndMask(MemberFlags flags, TokenPosition position)
                : base(position)
            {
                this.flags = flags;
                this.mask = (MemberFlags)(~0);
            }
        };

        /// Argument type and flags.
        internal class ArgTypeAndFlags: TokenPosition
        {
            public Expression type;
            public bool isParams;

            public ArgTypeAndFlags(Expression type, TokenPosition position, bool isParams)
                : base(position)
            {
                this.type = type;
                this.isParams = isParams;
            }

            public ArgTypeAndFlags(Expression type, TokenPosition position)
                : this(type, position, false)
            {
            }

            public ArgTypeAndFlags(Expression type)
                : this(type, type.GetPosition(), false)
            {
            }
        }

        internal class ConstraintChain
        {
            public bool valueType;
            public bool defCtor;
            public Expression baseExpr;
            public ConstraintChain next;

            public ConstraintChain(bool valueType, bool defCtor, Expression baseExpr)
            {
                this.valueType = valueType;
                this.defCtor = defCtor;
                this.baseExpr = baseExpr;
                this.next = null;
            }
        }

        private GenericConstraint BuildConstraintFromChain(string name, ConstraintChain chain, TokenPosition position)
        {
            // Extracted chain data.
            bool valueType = false;
            bool defCtor = false;
            Expression bases = null;

            // Iterator though the chains.
            while(chain != null)
            {
                // Get the chain base.
                if(chain.baseExpr != null)
                {
                    Expression newBase = chain.baseExpr;
                    newBase.SetNext(bases);
                    bases = newBase;
                }

                // Check the value type and default constructor.
                valueType = valueType || chain.valueType;
                defCtor = defCtor || chain.defCtor;

                // Check the next chain.
                chain = chain.next;
            }

            // Create the generic constraint.
            return new GenericConstraint(name, valueType, defCtor, bases, position);
        }
%}

/* Token declarations. */
%token EOF		0

/* Operator associativity. */
%right '='
%left '+' '-'
%left '/' '*' '%'
%left '.'

/* Character tokens */
%token<TokenPosition> '{' '}' '[' ']' '(' ')'
%token<TokenPosition> ';' '.'

/* Data types */
%token<TokenPosition> KBOOL
%token<TokenPosition> KBYTE
%token<TokenPosition> KSBYTE
%token<TokenPosition> KCHAR
%token<TokenPosition> KUCHAR
%token<TokenPosition> KDOUBLE
%token<TokenPosition> KFLOAT
%token<TokenPosition> KINT
%token<TokenPosition> KUINT
%token<TokenPosition> KLONG
%token<TokenPosition> KULONG
%token<TokenPosition> KSHORT
%token<TokenPosition> KUSHORT
%token<TokenPosition> KOBJECT
%token<TokenPosition> KSTRING
%token<TokenPosition> KSIZE_T
%token<TokenPosition> KPTRDIFF_T
%token<TokenPosition> KVOID
%token<TokenPosition> KCONST
%token<TokenPosition> KTYPEDEF

%token<TokenPosition> KVEC2
%token<TokenPosition> KVEC3
%token<TokenPosition> KVEC4
%token<TokenPosition> KDVEC2
%token<TokenPosition> KDVEC3
%token<TokenPosition> KDVEC4
%token<TokenPosition> KIVEC2
%token<TokenPosition> KIVEC3
%token<TokenPosition> KIVEC4
%token<TokenPosition> KBVEC2
%token<TokenPosition> KBVEC3
%token<TokenPosition> KBVEC4

%token<TokenPosition> KMAT2
%token<TokenPosition> KMAT2x3
%token<TokenPosition> KMAT2x4
%token<TokenPosition> KMAT3x2
%token<TokenPosition> KMAT3
%token<TokenPosition> KMAT3x4
%token<TokenPosition> KMAT4x2
%token<TokenPosition> KMAT4x3
%token<TokenPosition> KMAT4

%token<TokenPosition> KDMAT2
%token<TokenPosition> KDMAT2x3
%token<TokenPosition> KDMAT2x4
%token<TokenPosition> KDMAT3x2
%token<TokenPosition> KDMAT3
%token<TokenPosition> KDMAT3x4
%token<TokenPosition> KDMAT4x2
%token<TokenPosition> KDMAT4x3
%token<TokenPosition> KDMAT4

%token<TokenPosition> KIMAT2
%token<TokenPosition> KIMAT2x3
%token<TokenPosition> KIMAT2x4
%token<TokenPosition> KIMAT3x2
%token<TokenPosition> KIMAT3
%token<TokenPosition> KIMAT3x4
%token<TokenPosition> KIMAT4x2
%token<TokenPosition> KIMAT4x3
%token<TokenPosition> KIMAT4

/* Special values. */
%token<TokenPosition> KNULL
%token<TokenPosition> KBASE
%token<TokenPosition> KTHIS

/* Declarations */
%token<TokenPosition> KCLASS
%token<TokenPosition> KDELEGATE
%token<TokenPosition> KEVENT
%token<TokenPosition> KINTERFACE
%token<TokenPosition> KNAMESPACE
%token<TokenPosition> KSTRUCT
%token<TokenPosition> KENUM
%token<TokenPosition> KOPERATOR

/* Statements */
%token<TokenPosition> KBREAK
%token<TokenPosition> KCASE
%token<TokenPosition> KCATCH
%token<TokenPosition> KCONTINUE
%token<TokenPosition> KDO
%token<TokenPosition> KELSE
%token<TokenPosition> KFINALLY
%token<TokenPosition> KFOR
%token<TokenPosition> KFOREACH
%token<TokenPosition> KFIXED
%token<TokenPosition> KGOTO
%token<TokenPosition> KIF
%token<TokenPosition> KIN
%token<TokenPosition> KLOCK
%token<TokenPosition> KRETURN
%token<TokenPosition> KWHILE
%token<TokenPosition> KDELETE
%token<TokenPosition> KDEFAULT
%token<TokenPosition> KUSING
%token<TokenPosition> KSWITCH
%token<TokenPosition> KTHROW
%token<TokenPosition> KTRY

/* Member flags*/
%token<TokenPosition> KEXTERN
%token<TokenPosition> KPUBLIC
%token<TokenPosition> KINTERNAL
%token<TokenPosition> KPROTECTED
%token<TokenPosition> KPRIVATE
%token<TokenPosition> KKERNEL
%token<TokenPosition> KSTATIC
%token<TokenPosition> KVIRTUAL
%token<TokenPosition> KOVERRIDE
%token<TokenPosition> KABSTRACT
%token<TokenPosition> KSEALED
%token<TokenPosition> KREADONLY
%token<TokenPosition> KUNSAFE
%token<TokenPosition> KPARTIAL

/* Calling conventions */
%token<TokenPosition> KCDECL
%token<TokenPosition> KSTDCALL
%token<TokenPosition> KAPICALL

/* Operators */
%token<TokenPosition> LAND
%token<TokenPosition> LOR
%token<TokenPosition> LNOT
%token<TokenPosition> LBITSHIFT
%token<TokenPosition> RBITSHIFT
%token<TokenPosition> BITAND
%token<TokenPosition> BITOR
%token<TokenPosition> BITXOR
%token<TokenPosition> BITNOT
%token<TokenPosition> LEQ
%token<TokenPosition> GEQ
%token<TokenPosition> EQ
%token<TokenPosition> NEQ
%token<TokenPosition> IS
%token<TokenPosition> AS
%token<TokenPosition> ARROW
%token<TokenPosition> NEW
%token<TokenPosition> HEAPALLOC
%token<TokenPosition> STACKALLOC
%token<TokenPosition> CHECKED
%token<TokenPosition> UNCHECKED
%token<TokenPosition> REINTERPRET_CAST
%token<TokenPosition> '<'
%token<TokenPosition> '>'
%token<TokenPosition> '+'
%token<TokenPosition> '-'
%token<TokenPosition> '*'
%token<TokenPosition> '/'
%token<TokenPosition> '%'
%token<TokenPosition> '$'
%token<TokenPosition> '?'
%token<TokenPosition> '='
%token<TokenPosition> GENERIC_START
%token<TokenPosition> GENERIC_END
%token<TokenPosition> GENERIC_START_DOT
%token<TokenPosition> GENERIC_END_DOT

%token<TokenPosition> INCR
%token<TokenPosition> DECR
%token<TokenPosition> ADD_SET
%token<TokenPosition> SUB_SET
%token<TokenPosition> MUL_SET
%token<TokenPosition> DIV_SET
%token<TokenPosition> MOD_SET
%token<TokenPosition> OR_SET
%token<TokenPosition> XOR_SET
%token<TokenPosition> AND_SET
%token<TokenPosition> LSHIFT_SET
%token<TokenPosition> RSHIFT_SET
%token<TokenPosition> SIZEOF
%token<TokenPosition> TYPEOF
%token<TokenPosition> FUNC_PTR

/* Terminals types. */
%token<TokenValue> BYTE
%token<TokenValue> SBYTE
%token<TokenValue> SHORT
%token<TokenValue> USHORT
%token<TokenValue> INTEGER
%token<TokenValue> UINTEGER
%token<TokenValue> LONG
%token<TokenValue> ULONG
%token<TokenValue> BOOL
%token<TokenValue> FLOAT
%token<TokenValue> DOUBLE
%token<TokenValue> CHARACTER
%token<TokenValue> CSTRING
%token<TokenValue> STRING
%token<TokenValue> IDENTIFIER

/* Argument types */
%token<TokenValue> KREF
%token<TokenValue> KOUT
%token<TokenValue> KPARAMS

/* Non terminals types. */

%type<Expression> alloc_expression
%type<Expression> array_initializer_expression
%type<Expression> assignment_expression
%type<Expression> assignment_lexpression
%type<Expression> bitwise_expression
%type<Expression> call_expression
%type<Expression> call_arguments
%type<Expression> data_types
%type<Expression> default_expression
%type<Expression> equality_expression
%type<Expression> expression
%type<Expression> enum_type
%type<Expression> factor_expression
%type<Expression> indirection_expression
%type<Expression> logical_factor_expression
%type<Expression> logical_term_expression
%type<Expression> lvalue_base_expression
%type<Expression> lvalue_expression
%type<Expression> lvalue_call
%type<Expression> member_expression
%type<Expression> onlytype_expression
%type<Expression> onlytype_base_expression
%type<Expression> onlytype_noarray
%type<Expression> onlytype_arglist
%type<Expression> onlytype_arg
%type<Expression> new_expression
%type<Expression> number
%type<Expression> onlymember_expression
%type<Expression> prefix_expression
%type<Expression> postfix_expression
%type<Expression> primary_lvalue
%type<Expression> primary_expression
%type<Expression> refvalue_expression
%type<Expression> reinterpret_cast
%type<Expression> relation_expression
%type<Expression> shift_expression
%type<Expression> sizeof_expression
%type<Expression> string
%type<Expression> subscript_indices
%type<Expression> subindex_expression
%type<Expression> unary_expression
%type<Expression> safe_expression
%type<Expression> ternary_expression
%type<Expression> term_expression
%type<Expression> type_expression
%type<Expression> typeof_expression
%type<Expression> variable_reference
%type<Expression> for_cond
%type<Expression> check_expression

%type<AstNode> array_initializers
%type<AstNode> array_initializer_list
%type<AstNode> attribute_arguments_commasep
%type<AstNode> attribute_argument
%type<AstNode> attribute_arguments
%type<AstNode> attribute_instance
%type<AstNode> attribute_instances
%type<AstNode> assignment_statement
%type<AstNode> base_list
%type<AstNode> block_statement
%type<AstNode> break_statement
%type<AstNode> call_statement
%type<AstNode> catch_statement
%type<AstNode> catch_list
%type<AstNode> class_attributed_member
%type<AstNode> class_member
%type<AstNode> class_members
%type<AstNode> class_definition
%type<AstNode> const_primary_static
%type<AstNode> const_primary
%type<AstNode> continue_statement
%type<AstNode> ctor_initializer
%type<AstNode> delegate_definition
%type<AstNode> delete_statement;
%type<AstNode> do_statement
%type<AstNode> enum_constant
%type<AstNode> enum_constants
%type<AstNode> enum_definition
%type<AstNode> finally_statement
%type<AstNode> fixed_variable
%type<AstNode> fixed_variables
%type<AstNode> fixed_statement
%type<AstNode> foreach_statement
%type<AstNode> for_incr
%type<AstNode> for_statement
%type<AstNode> for_decls
%type<AstNode> function_arguments_comma_sep
%type<AstNode> function_arguments
%type<AstNode> function_argument
%type<AstNode> function_definition
%type<AstNode> function_prototype
%type<AstNode> generic_arglist
%type<AstNode> goto_case_statement
%type<AstNode> event_accessor
%type<AstNode> event_accessors
%type<AstNode> event_definition
%type<AstNode> field_decl
%type<AstNode> field_decls
%type<AstNode> field_definition
%type<AstNode> global_decl
%type<AstNode> global_decls
%type<AstNode> global_definition
%type<AstNode> if_statement
%type<AstNode> iface_event_accessor
%type<AstNode> iface_event_accessors
%type<AstNode> iface_event_definition
%type<AstNode> iface_method_prototype
%type<AstNode> iface_method
%type<AstNode> iface_property_accessor
%type<AstNode> iface_property_accessors
%type<AstNode> iface_property_definition
%type<AstNode> indexer_arg
%type<AstNode> indexer_args
%type<AstNode> interface_definition
%type<AstNode> interface_members
%type<AstNode> interface_member
%type<AstNode> lock_statement
%type<AstNode> localvar_statement
%type<AstNode> method_definition
%type<AstNode> method_prototype
%type<AstNode> namespace_declaration
%type<AstNode> namespace_declarations
%type<AstNode> namespace_definition
%type<AstNode> namespace_member
%type<AstNode> new_statement
%type<AstNode> parent_classes
%type<AstNode> prefix_statement
%type<AstNode> postfix_statement
%type<AstNode> property_accessor
%type<AstNode> property_accessors
%type<AstNode> property_definition
%type<AstNode> return_statement
%type<AstNode> start
%type<AstNode> statement
%type<AstNode> statements
%type<AstNode> statement_scope
%type<AstNode> struct_definition
%type<AstNode> typedef
%type<AstNode> switch_case
%type<AstNode> switch_cases
%type<AstNode> switch_statement
%type<AstNode> try_statement
%type<AstNode> throw_statement
%type<AstNode> toplevel_declarations
%type<AstNode> unsafe_block_statement
%type<AstNode> using_statement
%type<AstNode> using_object_statement
%type<AstNode> while_statement
%type<AstNode> variable_decl
%type<AstNode> variable_decls

/* Generics */
%type<GenericParameter> generic_argument
%type<GenericParameter> generic_arguments
%type<ConstraintChain> generic_constraint_name
%type<ConstraintChain> generic_constraint_chain
%type<GenericConstraint> generic_constraint
%type<GenericConstraint> generic_constraints
%type<GenericSignature> generic_signature

%type<object> function_name
%type<string> nested_name
%type<string> operator_name

%type<MemberFlags> global_flags
%type<MemberFlags> class_flags
%type<MemberFlags> member_flags
%type<MemberFlags> accessor_flags
%type<MemberFlags> funptr_cc

%type<MemberFlagsAndMask> visibility_flag
%type<MemberFlagsAndMask> language_flag
%type<MemberFlagsAndMask> instance_flag
%type<MemberFlagsAndMask> linkage_flag
%type<MemberFlagsAndMask> access_flag
%type<MemberFlagsAndMask> security_flag
%type<MemberFlagsAndMask> impl_flag
%type<MemberFlagsAndMask> inheritance_flag
%type<MemberFlagsAndMask> member_flag
%type<MemberFlagsAndMask> member_flag_list
%type<int> array_dimensions

%type<ArgTypeAndFlags> argtype_expression

%start start

%{
	AstNode AddList(AstNode prev, AstNode next)
	{
		if(prev != null)
		{
			if(next != null)
			{
				/*AstNode tail = prev.GetPrevCircular();
				tail.SetNextCircular(next);*/
                AstNode tail = prev;
                while(tail.GetNext() != null)
                    tail = tail.GetNext();
                tail.SetNext(next);
				return prev;
			}
			else
			{
				return prev;
			}
		}
		else
		{
			return next;
		}
	}
	
	void Error(TokenPosition position, string message)
	{
		throw new CompilerException(message, position);
	}

    void Error(string message)
    {
        throw new CompilerException(message, (TokenPosition)lexer.value());
    }
%}

%%
data_types: KBOOL	{ $$ = new TypeNode(TypeKind.Bool, $1); }
		  | KBYTE	{ $$ = new TypeNode(TypeKind.Byte, $1); }
		  | KSBYTE	{ $$ = new TypeNode(TypeKind.SByte, $1); }
		  | KCHAR	{ $$ = new TypeNode(TypeKind.Char, $1); }
		  | KUCHAR	{ $$ = new TypeNode(TypeKind.UChar, $1); }
		  | KDOUBLE	{ $$ = new TypeNode(TypeKind.Double, $1); }
		  | KFLOAT	{ $$ = new TypeNode(TypeKind.Float, $1); }
		  | KINT	{ $$ = new TypeNode(TypeKind.Int, $1); }
		  | KUINT	{ $$ = new TypeNode(TypeKind.UInt, $1); }
		  | KLONG	{ $$ = new TypeNode(TypeKind.Long, $1); }
		  | KULONG	{ $$ = new TypeNode(TypeKind.ULong, $1); }
		  | KSHORT	{ $$ = new TypeNode(TypeKind.Short, $1); }
		  | KUSHORT	{ $$ = new TypeNode(TypeKind.UShort, $1); }
		  | KOBJECT { $$ = new TypeNode(TypeKind.Object, $1); }
		  | KSTRING	{ $$ = new TypeNode(TypeKind.String, $1); }
		  | KSIZE_T	{ $$ = new TypeNode(TypeKind.Size, $1); }
		  | KVOID	{ $$ = new TypeNode(TypeKind.Void, $1); }
          | KPTRDIFF_T { $$ = new TypeNode(TypeKind.PtrDiff, $1); }

          | KVEC2   { $$ = new TypeNode(TypeKind.Float, 2, $1); }
          | KVEC3   { $$ = new TypeNode(TypeKind.Float, 3, $1); }
          | KVEC4   { $$ = new TypeNode(TypeKind.Float, 4, $1); }
          | KDVEC2  { $$ = new TypeNode(TypeKind.Double, 2, $1); }
          | KDVEC3  { $$ = new TypeNode(TypeKind.Double, 3, $1); }
          | KDVEC4  { $$ = new TypeNode(TypeKind.Double, 4, $1); }
          | KIVEC2  { $$ = new TypeNode(TypeKind.Int, 2, $1); }
          | KIVEC3  { $$ = new TypeNode(TypeKind.Int, 3, $1); }
          | KIVEC4  { $$ = new TypeNode(TypeKind.Int, 4, $1); }
          | KBVEC2  { $$ = new TypeNode(TypeKind.Bool, 2, $1); }
          | KBVEC3  { $$ = new TypeNode(TypeKind.Bool, 3, $1); }
          | KBVEC4  { $$ = new TypeNode(TypeKind.Bool, 4, $1); }

          | KMAT2    { $$ = new TypeNode(TypeKind.Float, 2, 2, $1); }
          | KMAT2x3  { $$ = new TypeNode(TypeKind.Float, 2, 3, $1); }
          | KMAT2x4  { $$ = new TypeNode(TypeKind.Float, 2, 4, $1); }
          | KMAT3x2  { $$ = new TypeNode(TypeKind.Float, 3, 2, $1); }
          | KMAT3    { $$ = new TypeNode(TypeKind.Float, 3, 3, $1); }
          | KMAT3x4  { $$ = new TypeNode(TypeKind.Float, 3, 4, $1); }
          | KMAT4x2  { $$ = new TypeNode(TypeKind.Float, 4, 2, $1); }
          | KMAT4x3  { $$ = new TypeNode(TypeKind.Float, 4, 3, $1); }
          | KMAT4    { $$ = new TypeNode(TypeKind.Float, 4, 4, $1); }

          | KDMAT2    { $$ = new TypeNode(TypeKind.Double, 2, 2, $1); }
          | KDMAT2x3  { $$ = new TypeNode(TypeKind.Double, 2, 3, $1); }
          | KDMAT2x4  { $$ = new TypeNode(TypeKind.Double, 2, 4, $1); }
          | KDMAT3x2  { $$ = new TypeNode(TypeKind.Double, 3, 2, $1); }
          | KDMAT3    { $$ = new TypeNode(TypeKind.Double, 3, 3, $1); }
          | KDMAT3x4  { $$ = new TypeNode(TypeKind.Double, 3, 4, $1); }
          | KDMAT4x2  { $$ = new TypeNode(TypeKind.Double, 4, 2, $1); }
          | KDMAT4x3  { $$ = new TypeNode(TypeKind.Double, 4, 3, $1); }
          | KDMAT4    { $$ = new TypeNode(TypeKind.Double, 4, 4, $1); }

          | KIMAT2    { $$ = new TypeNode(TypeKind.Int, 2, 2, $1); }
          | KIMAT2x3  { $$ = new TypeNode(TypeKind.Int, 2, 3, $1); }
          | KIMAT2x4  { $$ = new TypeNode(TypeKind.Int, 2, 4, $1); }
          | KIMAT3x2  { $$ = new TypeNode(TypeKind.Int, 3, 2, $1); }
          | KIMAT3    { $$ = new TypeNode(TypeKind.Int, 3, 3, $1); }
          | KIMAT3x4  { $$ = new TypeNode(TypeKind.Int, 3, 4, $1); }
          | KIMAT4x2  { $$ = new TypeNode(TypeKind.Int, 4, 2, $1); }
          | KIMAT4x3  { $$ = new TypeNode(TypeKind.Int, 4, 3, $1); }
          | KIMAT4    { $$ = new TypeNode(TypeKind.Int, 4, 4, $1); }
		  ;

onlymember_expression: onlymember_expression '.' IDENTIFIER    { $$ = new MemberAccess($1, (string)$3.GetValue(), $2); }
                     | IDENTIFIER                              { $$ = new VariableReference((string)$1.GetValue(), $1); }
                     ;

member_expression: primary_expression '.' IDENTIFIER	{ $$ = new MemberAccess($1, (string)$3.GetValue(), $2); }
			 	 ;

variable_reference: IDENTIFIER	{ $$ = new VariableReference((string)$1.GetValue(), $1); }
                  | KTHIS       { $$ = new VariableReference("this", $1); }
                  | KBASE       { $$ = new BaseExpression($1); }
				  ;
				 		  
number: BYTE      { $$ = new ByteConstant((byte)(ulong)$1.GetValue(), $1); }
      | SBYTE     { $$ = new SByteConstant((sbyte)(long)$1.GetValue(), $1); }
      | SHORT     { $$ = new ShortConstant((short)(long)$1.GetValue(), $1); }
      | USHORT    { $$ = new UShortConstant((ushort)(ulong)$1.GetValue(), $1); }
      | INTEGER	  { $$ = new IntegerConstant((int)(long)$1.GetValue(), $1); }
	  | UINTEGER  { $$ = new UIntegerConstant((uint)(ulong)$1.GetValue(), $1); }
      | LONG      { $$ = new LongConstant((long)$1.GetValue(), $1); }
      | ULONG     { $$ = new ULongConstant((ulong)$1.GetValue(), $1); }
	  | FLOAT	  { $$ = new FloatConstant((float)(double)$1.GetValue(), $1); }
	  | DOUBLE	  { $$ = new DoubleConstant((double)$1.GetValue(), $1); }
	  | BOOL	  { $$ = new BoolConstant((bool)$1.GetValue(), $1); }
	  | KNULL	  { $$ = new NullConstant($1); }
      | CHARACTER { $$ = new CharacterConstant((char)(long)$1.GetValue(), $1); }
	  ;
	  
string: STRING	{ $$ = new StringConstant((string)$1.GetValue(), $1); }
      | CSTRING { $$ = new CStringConstant((string)$1.GetValue(), $1); }
	  ;

subscript_indices: expression                       { $$ = $1; }
                 | subscript_indices ',' expression { $$ = (Expression)AddList($1, $3); }
                 ;

subindex_expression: primary_expression '[' subscript_indices ']'	{ $$ = new SubscriptAccess($1, $3, $2); }
                   | primary_expression '[' array_dimensions ']'    { $$ = new MakeArray($1, $3, $1.GetPosition()); }
				   ;
				   
indirection_expression: primary_expression ARROW IDENTIFIER	{ $$ = new IndirectAccess($1, (string)$3.GetValue(), $2); }
				 	  ;

primary_lvalue: data_types			{ $$ = $1; }
			  | variable_reference	{ $$ = $1; }
              | primary_lvalue GENERIC_START generic_arglist GENERIC_END { $$ = new GenericInstanceExpr($1, $3, $2); }
              | primary_lvalue GENERIC_START_DOT generic_arglist GENERIC_END_DOT { $$ = new GenericInstanceExpr($1, $3, $2); }
			  | primary_lvalue '.' IDENTIFIER { $$ = new MemberAccess($1, (string)$3.GetValue(), $2); }
              | primary_lvalue ARROW IDENTIFIER { $$ = new IndirectAccess($1, (string)$3.GetValue(), $2); }
			  ;

lvalue_base_expression: onlytype_base_expression     { $$ = $1; }
                      | '*' lvalue_expression        { $$ = new DereferenceOperation($2, $2.GetPosition()); }
                      ;

lvalue_expression: lvalue_base_expression     { $$ = $1; }
                 | lvalue_expression '.' IDENTIFIER { $$ = new MemberAccess($1, (string)$3.GetValue(), $2); }
                 | lvalue_expression ARROW IDENTIFIER { $$ = new IndirectAccess($1, (string)$3.GetValue(), $2); }
                 | lvalue_call
                 ;

const_primary_static: primary_lvalue        { $$ = $1; }
                    | KCONST primary_lvalue { $$ = new MakeConstant($2, $1); }
                    ;

array_dimensions: /* empty */ { $$ = 1; }
                | array_dimensions ',' /* empty*/  { $$ = $1 + 1;}
                ;

onlytype_noarray: const_primary_static			 { $$ = $1; }
                | onlytype_noarray '*' KCONST    { $$ = new MakeConstant(new MakePointer($1, $1.GetPosition()), $1.GetPosition()); }
				| onlytype_noarray '*'           { $$ = new MakePointer($1, $1.GetPosition()); }
                | onlytype_noarray FUNC_PTR funptr_cc '(' onlytype_arglist ')' { $$ = new MakeFunctionPointer($1, $5, $3, $1.GetPosition()); }
				;

onlytype_base_expression: onlytype_noarray                                      { $$ = $1; }
                        | onlytype_base_expression '[' array_dimensions ']'     { $$ = new MakeArray($1, $3, $1.GetPosition()); }
                        | onlytype_base_expression '[' subscript_indices ']'    { $$ = new SubscriptAccess($1, $3, $2); }
                        ;

onlytype_expression: onlytype_base_expression            { $$ = $1; $1.SetHints(Expression.TypeHint); }
                   ;

const_primary: primary_expression        { $$ = $1; }
             | KCONST primary_expression { $$ = new MakeConstant($2, $1); }
             ;

onlytype_arg: onlytype_expression               { $$ = $1; }
            | onlytype_expression IDENTIFIER    { $$ = $1; }
            ;

onlytype_arglist: /* empty */                       { $$ = null; }
                | onlytype_arg                      { $$ = $1; }
                | onlytype_arglist ',' onlytype_arg    { $$ = AddList($1, $3); }
                ;

array_initializer_expression: expression                    { $$ = $1; }
                            | '{' array_initializers '}'    { $$ = new ArrayExpression((Expression)$2, $1); }
                            ;

array_initializer_list: array_initializer_expression                        { $$ = $1; }
                      | array_initializer_list ',' array_initializer_expression { $$ = AddList($1, $3); }
                      ;

array_initializers: /* empty */                 { $$ = null; }
                  | array_initializer_list      { $$ = $1;   }
                  | array_initializer_list ','  { $$ = $1;   }
                  ;

new_expression: NEW onlytype_noarray '(' call_arguments ')'                                { $$ = new NewExpression($2, $4, $1);            }
			  | NEW onlytype_noarray '[' subscript_indices ']'	                           { $$ = new NewArrayExpression($2, $4, null, -1, $1); }
              | NEW onlytype_noarray '[' subscript_indices ']'  '{' array_initializers '}' { $$ = new NewArrayExpression($2, $4, $7, -1, $1);   }
              | NEW onlytype_noarray '[' array_dimensions ']'   '{' array_initializers '}' { $$ = new NewArrayExpression($2, null, $7, $4, $1); }
			  ;

alloc_expression: HEAPALLOC onlytype_noarray                         { $$ = new NewRawExpression($2, null, true, $1);          }
                | HEAPALLOC onlytype_noarray '(' call_arguments ')'  { $$ = new NewRawExpression($2, $4, true, $1);            }
                | HEAPALLOC onlytype_noarray '[' expression ']'      { $$ = new NewRawArrayExpression($2, $4, true, $1);    }
                | STACKALLOC onlytype_noarray                        { $$ = new NewRawExpression($2, null, false, $1);          }
                | STACKALLOC onlytype_noarray '(' call_arguments ')' { $$ = new NewRawExpression($2, $4, false, $1);            }
                | STACKALLOC onlytype_noarray '[' expression ']'     { $$ = new NewRawArrayExpression($2, $4, false, $1);    }
                ;

check_expression: CHECKED '(' expression ')'    { $$ = $3; }
                | UNCHECKED '(' expression ')'  { $$ = $3; }
                ;

postfix_expression: primary_expression INCR { $$ = new PostfixOperation(PostfixOperation.Increment, $1, $2); }
                  | primary_expression DECR { $$ = new PostfixOperation(PostfixOperation.Decrement, $1, $2); }
                  ;

postfix_statement: lvalue_expression INCR
                 {
                    Expression expr = new PostfixOperation(PostfixOperation.Increment, $1, $2);
                    $$ = new ExpressionStatement(expr, $2);
                 }
                 | lvalue_expression DECR
                 {
                    Expression expr = new PostfixOperation(PostfixOperation.Decrement, $1, $2);
                    $$ = new ExpressionStatement(expr, $2);
                 }
                 ;

prefix_expression: INCR refvalue_expression   { $$ = new PrefixOperation(PrefixOperation.Increment, $2, $1); }
                 | DECR refvalue_expression   { $$ = new PrefixOperation(PrefixOperation.Decrement, $2, $1); }
                 ;

prefix_statement: prefix_expression ';' { $$ = new ExpressionStatement($1, $1.GetPosition()); }
                ;

primary_expression: primary_lvalue		    { $$ = $1; }
			  	  | number					{ $$ = $1; }
				  | string					{ $$ = $1; }
				  | call_expression			{ $$ = $1; }
                  | check_expression        { $$ = $1;}
				  | subindex_expression		{ $$ = $1; }
				  | indirection_expression	{ $$ = $1; }
			      | member_expression		{ $$ = $1; }
			      | new_expression			{ $$ = $1; }
                  | alloc_expression        { $$ = $1; }
                  | postfix_expression      { $$ = $1; }
                  | sizeof_expression       { $$ = $1; }
                  | typeof_expression       { $$ = $1; }
                  | default_expression      { $$ = $1; }
                  | reinterpret_cast        { $$ = $1; }
				  | '(' expression ')'		{ $$ = $2; }
				  ;

sizeof_expression: SIZEOF '(' onlytype_expression ')' { $$ = new SizeOfExpression($3, $1); }
                 ;

typeof_expression: TYPEOF '(' onlytype_expression ')' { $$ = new TypeOfExpression($3, $1); }
                 ;

default_expression: KDEFAULT '(' onlytype_expression ')' { $$ = new DefaultExpression($3, $1); }
                   ;

refvalue_expression: const_primary     { $$ = $1; }
                    | '*' refvalue_expression { $$ = new DereferenceOperation($2, $2.GetPosition()); }
                    ;

reinterpret_cast: REINTERPRET_CAST GENERIC_START onlytype_expression GENERIC_END '(' expression ')' { $$ = new ReinterpretCast($3, $6, $1); }
                ;

funptr_cc: /* empty */   { $$ = MemberFlags.Default; }
         | language_flag { $$ = $1.flags;           }
         ;

type_expression: refvalue_expression                { $$ = $1; }
               | type_expression '*' KCONST         { $$ = new MakeConstant(new MakePointer($1, $1.GetPosition()), $1.GetPosition()); }
               | type_expression '*'                    { $$ = new MakePointer($1, $1.GetPosition()); }
               | type_expression '*' unary_expression   { $$ = new BinaryOperation(BinaryOperation.OpMul, $1, $3, $2); }
               | type_expression FUNC_PTR funptr_cc '(' onlytype_arglist ')' { $$ = new MakeFunctionPointer($1, $5, $3, $1.GetPosition()); }
               ;

unary_expression: refvalue_expression		               { $$ = $1; }
				| LNOT unary_expression	                   { $$ = new UnaryOperation(UnaryOperation.OpNot, $2, $1); }
				| BITNOT unary_expression                  { $$ = new UnaryOperation(UnaryOperation.OpBitNot, $2, $1); }
				| '+' unary_expression	                   { $$ = $2; }
				| '-' unary_expression	                   { $$ = new UnaryOperation(UnaryOperation.OpNeg, $2, $2.GetPosition()); }
                | /*&*/ BITAND unary_expression            { $$ = new AddressOfOperation($2, $1); }
                | KREF unary_expression                    { $$ = new RefExpression(false, $2, $1); }
                | KOUT unary_expression                    { $$ = new RefExpression(true, $2, $1); }
				| '(' type_expression ')' unary_expression { $$ = new CastOperation($2, $4, $1); }
                | '(' type_expression ')'                  { $$ = $2; }
                | prefix_expression                        { $$ = $1; }
				;
				
factor_expression: unary_expression							{ $$ = $1; }
				 | factor_expression '*' unary_expression	{ $$ = new BinaryOperation(BinaryOperation.OpMul, $1, $3, $2); }
				 | factor_expression '/' unary_expression	{ $$ = new BinaryOperation(BinaryOperation.OpDiv, $1, $3, $2); }
				 | factor_expression '%' unary_expression	{ $$ = new BinaryOperation(BinaryOperation.OpMod, $1, $3, $2); }
				 ;
				 
term_expression: factor_expression							{ $$ = $1; }
			   | term_expression '+' factor_expression		{ $$ = new BinaryOperation(BinaryOperation.OpAdd, $1, $3, $2); }
			   | term_expression '-' factor_expression		{ $$ = new BinaryOperation(BinaryOperation.OpSub, $1, $3, $2); }
			   ;
			   
shift_expression: term_expression								{ $$ = $1; }
			    | shift_expression LBITSHIFT term_expression	{ $$ = new BinaryOperation(BinaryOperation.OpBitLeft, $1, $3, $2); }
			    | shift_expression RBITSHIFT term_expression	{ $$ = new BinaryOperation(BinaryOperation.OpBitRight, $1, $3, $2); }
			    ;

relation_expression: shift_expression							{ $$ = $1; }
				   | relation_expression '<' shift_expression	{ $$ = new BinaryOperation(BinaryOperation.OpLT, $1, $3, $2); }
				   | relation_expression LEQ shift_expression	{ $$ = new BinaryOperation(BinaryOperation.OpLEQ, $1, $3, $2); }
				   | relation_expression GEQ shift_expression	{ $$ = new BinaryOperation(BinaryOperation.OpGEQ, $1, $3, $2); }
				   | relation_expression '>' shift_expression	{ $$ = new BinaryOperation(BinaryOperation.OpGT, $1, $3, $2); }
                   | relation_expression IS onlytype_expression { $$ = new IsExpression($1, $3, $2); }
                   | relation_expression AS onlytype_expression { $$ = new AsExpression($1, $3, $2); }
				   ;
				
equality_expression: relation_expression							{ $$ = $1; }
				   | equality_expression EQ	 relation_expression	{ $$ = new BinaryOperation(BinaryOperation.OpEQ, $1, $3, $2); }
				   | equality_expression NEQ relation_expression	{ $$ = new BinaryOperation(BinaryOperation.OpNEQ, $1, $3, $2); }
				   ;
				   
bitwise_expression: equality_expression								{ $$ = $1; }
				  | bitwise_expression BITAND equality_expression	{ $$ = new BinaryOperation(BinaryOperation.OpBitAnd, $1, $3, $2); }
				  | bitwise_expression BITOR equality_expression	{ $$ = new BinaryOperation(BinaryOperation.OpBitOr, $1, $3, $2); }
				  | bitwise_expression BITXOR equality_expression	{ $$ = new BinaryOperation(BinaryOperation.OpBitXor, $1, $3, $2); }
				  ;

logical_factor_expression: bitwise_expression                                { $$ = $1; }
                         | logical_factor_expression LAND bitwise_expression { $$ = new BinaryOperation(BinaryOperation.OpLAnd, $1, $3, $2); }
                         ;

logical_term_expression: logical_factor_expression								{ $$ = $1; }
				       | logical_term_expression LOR logical_factor_expression	{ $$ = new BinaryOperation(BinaryOperation.OpLOr, $1, $3, $2); }
				       ;

ternary_expression: logical_term_expression													{ $$ = $1;								   }
				  | logical_term_expression '?' logical_term_expression ':' logical_term_expression	{ $$ = new TernaryOperation($1, $3, $5, $2); }
				  ;

assignment_expression: refvalue_expression '=' expression		    { $$ = new AssignmentExpression($1, $3, $2); }
					 | refvalue_expression ADD_SET expression	    { $$ = new BinaryAssignOperation(BinaryOperation.OpAdd, $1, $3, $2);      }
					 | refvalue_expression SUB_SET expression	    { $$ = new BinaryAssignOperation(BinaryOperation.OpSub, $1, $3, $2);      }
                     | refvalue_expression MUL_SET expression       { $$ = new BinaryAssignOperation(BinaryOperation.OpMul, $1, $3, $2);      }
					 | refvalue_expression DIV_SET expression	    { $$ = new BinaryAssignOperation(BinaryOperation.OpDiv, $1, $3, $2);      }
					 | refvalue_expression MOD_SET expression	    { $$ = new BinaryAssignOperation(BinaryOperation.OpMod, $1, $3, $2);      }
					 | refvalue_expression OR_SET expression		{ $$ = new BinaryAssignOperation(BinaryOperation.OpBitOr, $1, $3, $2);    }
					 | refvalue_expression XOR_SET expression	    { $$ = new BinaryAssignOperation(BinaryOperation.OpBitXor, $1, $3, $2);   }
					 | refvalue_expression AND_SET expression	    { $$ = new BinaryAssignOperation(BinaryOperation.OpBitAnd, $1, $3, $2);   }
					 | refvalue_expression LSHIFT_SET expression	{ $$ = new BinaryAssignOperation(BinaryOperation.OpBitLeft, $1, $3, $2);  }
					 | refvalue_expression RSHIFT_SET expression	{ $$ = new BinaryAssignOperation(BinaryOperation.OpBitRight, $1, $3, $2); }
					 ;

assignment_lexpression: lvalue_expression '=' expression       { $$ = new AssignmentExpression($1, $3, $2); }
                  | lvalue_expression ADD_SET expression       { $$ = new BinaryAssignOperation(BinaryOperation.OpAdd, $1, $3, $2);      }
                  | lvalue_expression SUB_SET expression       { $$ = new BinaryAssignOperation(BinaryOperation.OpSub, $1, $3, $2);      }
                  | lvalue_expression MUL_SET expression       { $$ = new BinaryAssignOperation(BinaryOperation.OpMul, $1, $3, $2);      }
                  | lvalue_expression DIV_SET expression       { $$ = new BinaryAssignOperation(BinaryOperation.OpDiv, $1, $3, $2);      }
                  | lvalue_expression MOD_SET expression       { $$ = new BinaryAssignOperation(BinaryOperation.OpMod, $1, $3, $2);      }
                  | lvalue_expression OR_SET expression        { $$ = new BinaryAssignOperation(BinaryOperation.OpBitOr, $1, $3, $2);    }
                  | lvalue_expression XOR_SET expression       { $$ = new BinaryAssignOperation(BinaryOperation.OpBitXor, $1, $3, $2);   }
                  | lvalue_expression AND_SET expression       { $$ = new BinaryAssignOperation(BinaryOperation.OpBitAnd, $1, $3, $2);   }
                  | lvalue_expression LSHIFT_SET expression    { $$ = new BinaryAssignOperation(BinaryOperation.OpBitLeft, $1, $3, $2);  }
                  | lvalue_expression RSHIFT_SET expression    { $$ = new BinaryAssignOperation(BinaryOperation.OpBitRight, $1, $3, $2); }
                  ;

assignment_statement: assignment_lexpression ';' { $$ = new ExpressionStatement($1, $1.GetPosition()); }
                 ;

safe_expression: ternary_expression { $$ = $1; }
               ;

expression: ternary_expression		{ $$ = $1; }
		  | assignment_expression	{ $$ = $1; }
		  ;


		  
block_statement: '{' statements '}'	{ $$ = new BlockNode($2, $1);}
			   ;

unsafe_block_statement: KUNSAFE '{' statements '}'    { $$ = new UnsafeBlockNode($3, $1); }
                      ;

call_arguments: /* empty */						{ $$ = null;			}
			  | expression						{ $$ = $1;				}
			  | call_arguments ',' expression	{ $$ = AddList($1, $3); }
			  ;
			  
call_expression: primary_expression '(' call_arguments ')'	{ $$ = new CallExpression($1, $3, $2); }
			   ;

lvalue_call: lvalue_expression '(' call_arguments ')' { $$ = new CallExpression($1, $3, $2); }
           ;

call_statement: lvalue_call ';' { $$ = new ExpressionStatement($1, $2); }
			  ;

new_statement: new_expression ';' { $$ = new ExpressionStatement($1, $1.GetPosition()); }
             ;

return_statement: KRETURN expression ';'  { $$ = new ReturnStatement($2, $1); }
				| KRETURN ';'			  { $$ = new ReturnStatement(null, $1); }
                | IDENTIFIER KRETURN expression ';'
                {
                    if(!$1.GetValue().Equals("yield"))
                        Error($1, "expected yield identifier before return.");
                    $$ = new ReturnStatement($3, true, $1);
                }
				;

if_statement: KIF '(' expression ')' statement					{ $$ = new IfStatement($3, $5, null, $1); }
			| KIF '(' expression ')' statement KELSE statement	{ $$ = new IfStatement($3, $5, $7, $1); }
			;

switch_case: KCASE expression ':' statements  { $$ = new CaseLabel($2, $4, $1); }
           | KDEFAULT ':' statements          { $$ = new CaseLabel(null, $3, $1); }
           ;

switch_cases: switch_case               { $$ = $1; }
            | switch_cases switch_case  { $$ = AddList($1, $2); }
            ;

switch_statement: KSWITCH '(' expression ')' '{' switch_cases '}'  { $$ = new SwitchStatement($3, $6, $1); }
                ;

while_statement: KWHILE '(' expression ')' statement	{ $$ = new WhileStatement($3, $5, $1); }
			   ;
			   
do_statement: KDO statement KWHILE '(' expression ')'	{ $$ = new DoWhileStatement($5, $2, $1); }
			;

for_incr:						{ $$ = null; }
		| assignment_expression	{ $$ = new ExpressionStatement($1, $1.GetPosition());; }
		| call_expression		{ $$ = new ExpressionStatement($1, $1.GetPosition());; }
        | prefix_expression     { $$ = new ExpressionStatement($1, $1.GetPosition());; }
        | postfix_expression    { $$ = new ExpressionStatement($1, $1.GetPosition());; }
		;
					
for_cond: ';'			 { $$ = null; }
		| expression ';' { $$ = $1; }
		;
		
for_decls: ';'	{ $$ = null; }
		 | localvar_statement	{ $$ = $1; }
		 ;
		 
for_statement: KFOR '(' for_decls for_cond for_incr ')' statement_scope	{ $$ = new ForStatement($3, $4, $5, $7, $1); }
			 ;

foreach_statement: KFOREACH '(' onlytype_expression IDENTIFIER KIN expression ')' statement_scope
                    { $$ = new ForEachStatement($3, (string)$4.GetValue(), $6, $8, $1); }
                 ;

break_statement: KBREAK ';'	{ $$ = new BreakStatement($1); }
			   ;

continue_statement: KCONTINUE ';'	{ $$ = new ContinueStatement($1); }
			   ;

goto_case_statement: KGOTO KCASE expression ';' { $$ = new GotoCaseStatement($3, $1);   }
                   | KGOTO KDEFAULT ';'         { $$ = new GotoCaseStatement(null, $1); }
                   ;

catch_statement: KCATCH '(' onlytype_expression ')' statement             { $$ = new CatchStatement($3, null, $5, $1); }
               | KCATCH '(' onlytype_expression IDENTIFIER ')' statement { $$ = new CatchStatement($3, (string)$4.GetValue(), $6, $1); }
               ;

catch_list: catch_statement          { $$ = $1; }
          | catch_list catch_statement  { $$ = AddList($1, $2); }
          ;

finally_statement: KFINALLY statement { $$ = new FinallyStatement($2, $1); }
                 ;

try_statement: KTRY statement catch_list finally_statement { $$ = new TryStatement($2, $3, $4, $1); }
             | KTRY statement catch_list                   { $$ = new TryStatement($2, $3, null, $1); }
             | KTRY statement finally_statement            { $$ = new TryStatement($2, null, $3, $1); }
             ;

variable_decl: IDENTIFIER					{$$ = new VariableDeclaration((string)$1.GetValue(), null, $1); }
			 | IDENTIFIER '=' expression	{$$ = new VariableDeclaration((string)$1.GetValue(), $3, $1); }
			 ;
			 
variable_decls: variable_decl						{ $$ = $1;				}
			  | variable_decls ',' variable_decl	{ $$ = AddList($1, $3); }
			  ;
			  
localvar_statement: onlytype_expression variable_decls ';'	{ $$ = new LocalVariablesDeclaration($1, (VariableDeclaration)$2, $1.GetPosition()); }
				  ;

lock_statement: KLOCK '(' expression ')' statement { $$ = new LockStatement($3, $5, $1); }
              ;

delete_statement: KDELETE expression ';'	{ $$ = new DeleteStatement($2, $1); }
				| KDELETE '[' ']' expression ';'	{ $$ = new DeleteRawArrayStatement($4, $1); }
				;

throw_statement: KTHROW expression ';' { $$ = new ThrowStatement($2, $1); }
               ;

using_object_statement: KUSING '(' onlytype_expression variable_decls ')' statement
                      {
                        LocalVariablesDeclaration decls = new LocalVariablesDeclaration($3, (VariableDeclaration)$4, $3.GetPosition());
                        $$ = new UsingObjectStatement(decls, $6, $1);
                      }
                      ;

fixed_variable: IDENTIFIER '=' expression   { $$ = new FixedVariableDecl((string)$1.GetValue(), $3, $1);}
              ;

fixed_variables: fixed_variable                     { $$ = $1; }
               | fixed_variables ',' fixed_variable { $$ = AddList($1, $3); }
               ;

fixed_statement: KFIXED '(' onlytype_expression fixed_variables ')' statement
               { $$ = new FixedStatement($3, $4, $6, $1); }
               ;

statement: ';'						{ $$ = new NullStatement($1); }
		 | assignment_statement		{ $$ = $1; }
		 | block_statement			{ $$ = $1; }
		 | call_statement			{ $$ = $1; }
         | new_statement            { $$ = $1;}
         | prefix_statement         { $$ = $1; }
         | postfix_statement        { $$ = $1; }
		 | return_statement		 	{ $$ = $1; }
		 | while_statement			{ $$ = $1; }
         | fixed_statement          { $$ = $1; }
		 | for_statement			{ $$ = $1; }
         | foreach_statement        { $$ = $1; }
		 | delete_statement			{ $$ = $1; }
		 | do_statement				{ $$ = $1; }
		 | if_statement				{ $$ = $1; }
		 | break_statement			{ $$ = $1; }
		 | continue_statement		{ $$ = $1; }
         | goto_case_statement      { $$ = $1; }
		 | localvar_statement		{ $$ = $1; }
         | lock_statement           { $$ = $1; }
         | switch_statement         { $$ = $1; }
         | throw_statement          { $$ = $1; }
         | try_statement            { $$ = $1; }
         | unsafe_block_statement   { $$ = $1; }
         | using_object_statement   { $$ = $1; }
		 ;
		 
statements: /* empty */				{ $$ = null; }
		  | statements statement	{ $$ = AddList($1, $2); }
		  ;

statement_scope: statement
               {
                   if($1 is BlockNode)
                   {
                       BlockNode block = (BlockNode) $1;
                       $$ = block.GetChildren();
                   }
                   else
                   {
                       $$ = $1;
                   }
                   $$ = $1;
               }
               ;

attribute_argument: IDENTIFIER '=' safe_expression   { $$ = new AttributeArgument($3, (string)$1.GetValue(), $1); }
                  | safe_expression                  { $$ = new AttributeArgument($1, null, $1.GetPosition()); }
                  ;

attribute_arguments_commasep: attribute_argument                                   { $$ = $1; }
                            | attribute_arguments_commasep ',' attribute_argument  { $$ = AddList($1, $3); }
                            ;

attribute_arguments: /* empty */                    { $$ = null; }
                   | attribute_arguments_commasep   { $$ = $1;   }

attribute_instance: onlymember_expression '(' attribute_arguments ')' { $$ = new AttributeInstance($1, (AttributeArgument)$3, $1.GetPosition());   }
                  | onlymember_expression                             { $$ = new AttributeInstance($1, null, $1.GetPosition()); }
                  ;

attribute_instances: attribute_instance                         { $$ = $1; }
                   | attribute_instances ',' attribute_instance { $$ = AddList($1, $3); }
                   ;

argtype_expression: onlytype_expression           { $$ = new ArgTypeAndFlags($1); }
                  | onlytype_expression '$'       { $$ = new ArgTypeAndFlags(new MakeReference($1, ReferenceFlow.In,    true,  $2), $1.GetPosition()); }
                  | KREF onlytype_expression      { $$ = new ArgTypeAndFlags(new MakeReference($2, ReferenceFlow.InOut, false, $1), $1); }
                  | KREF onlytype_expression '$'  { $$ = new ArgTypeAndFlags(new MakeReference($2, ReferenceFlow.InOut, true,  $1), $1); }
                  | KOUT onlytype_expression      { $$ = new ArgTypeAndFlags(new MakeReference($2, ReferenceFlow.Out,   false, $1), $1); }
                  | KOUT onlytype_expression '$'  { $$ = new ArgTypeAndFlags(new MakeReference($2, ReferenceFlow.Out,   true,  $1), $1); }
                  | KPARAMS onlytype_expression   { $$ = new ArgTypeAndFlags($2, $1, true); }
                  | KREADONLY onlytype_expression
                  {
                    MakeArray makeArray = $2 as MakeArray;
                    if(makeArray == null)
                        Error($1, "only array arguments can be readonly.");
                    makeArray.IsReadOnly = true;
                    $$ = new ArgTypeAndFlags(makeArray, $1);
                  }
                  ;

generic_argument: IDENTIFIER { $$ = new GenericParameter((string)$1.GetValue(), $1); }
                ;

generic_arguments: generic_argument                       { $$ = $1; }
                 | generic_arguments ',' generic_argument { $$ = (GenericParameter)AddList($1, $3); }
                 ;

generic_constraint_name: onlytype_expression    { $$ = new ConstraintChain(false, false, $1); }
                       | KSTRUCT                { $$ = new ConstraintChain(true, false, null); }
                       | NEW '(' ')'            { $$ = new ConstraintChain(false, true, null); }
                       ;

generic_constraint_chain: generic_constraint_name                                { $$ = $1; }
                        | generic_constraint_chain ',' generic_constraint_name
                        {
                            // The order doesn't matter.
                            ConstraintChain chain = $3;
                            chain.next = $1;
                            $$ = chain;
                        }
                        ;

generic_constraint: IDENTIFIER IDENTIFIER ':' generic_constraint_chain
                  {
                      if((string)$1.GetValue() != "where")
                          Error($1, "expected generic constraint");
                      $$ = BuildConstraintFromChain((string)$2.GetValue(), $4, $1);
                  }
                  ;

generic_constraints: /* empty */                            { $$ = null; }
                   | generic_constraints generic_constraint { $$ = AddList($1, $2); }
                   ;

generic_signature: /* empty */                                     { $$ = null; }
                 | GENERIC_START generic_arguments GENERIC_END   { $$ = new GenericSignature($2, null, $1); }
                 ;

generic_arglist: onlytype_expression                      { $$ = $1; }
               | generic_arglist ',' onlytype_expression  { $$ = AddList($1, $3); }
               ;


function_argument: argtype_expression				{ $$ = new FunctionArgument($1.type, $1.isParams, "", $1); }
				 | argtype_expression IDENTIFIER	{ $$ = new FunctionArgument($1.type, $1.isParams, (string)$2.GetValue(), $1); }
				 ;

function_arguments_comma_sep: function_argument                           { $$ = $1;              }
                            | function_arguments ',' function_argument    { $$ = AddList($1, $3); }
                            ;

function_arguments: /* empty */		             { $$ = null; }
                  | function_arguments_comma_sep { $$ = $1;   }
				  ;

operator_name: KOPERATOR '+'    { $$ = "Op_Add"; }
             | KOPERATOR '-'    { $$ = "Op_Sub"; }
             | KOPERATOR '*'    { $$ = "Op_Mul"; }
             | KOPERATOR '/'    { $$ = "Op_Div"; }
             | KOPERATOR '%'    { $$ = "Op_Mod"; }
             | KOPERATOR '<'    { $$ = "Op_Lt";  }
             | KOPERATOR '>'    { $$ = "Op_Gt";  }
             | KOPERATOR LEQ    { $$ = "Op_Leq"; }
             | KOPERATOR GEQ    { $$ = "Op_Geq"; }
             | KOPERATOR EQ     { $$ = "Op_Eq";  }
             | KOPERATOR NEQ    { $$ = "Op_Neq"; }
             | KOPERATOR BITAND { $$ = "Op_And"; }
             | KOPERATOR BITXOR { $$ = "Op_Xor"; }
             | KOPERATOR BITOR  { $$ = "Op_Or";  }
             | KOPERATOR BITNOT { $$ = "Op_Not"; }
             ;

function_name: IDENTIFIER                 { $$ = (string)$1.GetValue(); }
           | function_name '.' IDENTIFIER
           {
               Expression baseExpr = null;
               if($1 is Expression)
                   baseExpr = (Expression)$1;
               else
                   baseExpr = new VariableReference((string)$1, $2);
               $$ = new MemberAccess(baseExpr, (string)$3.GetValue(), $2);
           }
           | function_name '.' KTHIS
           {
               Expression baseExpr = null;
               if($1 is Expression)
                   baseExpr = (Expression)$1;
               else
                   baseExpr = new VariableReference((string)$1, $2);
               $$ = new MemberAccess(baseExpr, "Op_Index", $2);
           }
           | function_name GENERIC_START_DOT generic_arglist GENERIC_END_DOT
           {
               Expression baseExpr = null;
               if($1 is Expression)
                   baseExpr = (Expression)$1;
               else
                   baseExpr = new VariableReference((string)$1, $2);
               $$ = new GenericInstanceExpr(baseExpr, $3, $2);
           }
           ;

function_prototype: global_flags onlytype_expression IDENTIFIER generic_signature
                     '(' function_arguments ')' generic_constraints
				  {
                    GenericSignature genSign = $4;
                    if(genSign != null)
                        genSign.SetConstraints($8);
                    $$ = new FunctionPrototype($1, $2, (FunctionArgument)$6, (string)$3.GetValue(), $4, $2.GetPosition());
                  }
				  ;
				  
function_definition: function_prototype block_statement { $$ = new FunctionDefinition((FunctionPrototype)$1, $2, $1.GetPosition()); }
				   ;

ctor_initializer: /* empty */                  { $$ = null; }
                | ':' KBASE '(' call_arguments ')' { $$ = new ConstructorInitializer(true, $4, $2); }
                | ':' KTHIS '(' call_arguments ')' { $$ = new ConstructorInitializer(false, $4, $2); }
                ;

method_prototype: member_flags onlytype_expression operator_name generic_signature
                   '(' function_arguments ')' generic_constraints
				{
                    GenericSignature genSign = $4;
                    if(genSign != null)
                        genSign.SetConstraints($8);
                    $$ = new FunctionPrototype($1, $2, (FunctionArgument)$6, $3, $4, $2.GetPosition());
                }
                | member_flags onlytype_expression function_name generic_signature
                   '(' function_arguments ')' generic_constraints
                {
                    GenericSignature genSign = $4;
                    if(genSign != null)
                        genSign.SetConstraints($8);
                    if($3 is string)
                        $$ = new FunctionPrototype($1, $2, (FunctionArgument)$6, (string)$3, $4, $2.GetPosition());
                    else
                        $$ = new FunctionPrototype($1, $2, (FunctionArgument)$6, (Expression)$3, $4, $2.GetPosition());
                }
				| member_flags IDENTIFIER '(' function_arguments ')' ctor_initializer
				{
					MemberFlags instance = ($1 & MemberFlags.InstanceMask);
					if(instance != 0 && instance != MemberFlags.Static)
						Error($2, "constructors cannot be virtual/override/abstract.");
                    MemberFlags flags = $1 & ~MemberFlags.InstanceMask;
                    if(instance == MemberFlags.Static)
                        flags |= MemberFlags.StaticConstructor;
                    else
                        flags |= MemberFlags.Constructor;

					FunctionPrototype proto = new FunctionPrototype(flags, null,
						(FunctionArgument)$4, (string)$2.GetValue(), $2);
                    proto.SetConstructorInitializer((ConstructorInitializer)$6);
                    $$ = proto;
				}
                | BITNOT IDENTIFIER '(' ')'
                {
                    FunctionPrototype proto = new FunctionPrototype(MemberFlags.Protected | MemberFlags.Override,
                                                    null, null, "Finalize", $1);
                    proto.SetDestructorName((string)$2.GetValue());
                    $$ = proto;
                }
				;
				  
method_definition: method_prototype block_statement	{ $$ = new FunctionDefinition((FunctionPrototype)$1, $2, $1.GetPosition()); }
				   ;

iface_method_prototype: onlytype_expression IDENTIFIER generic_signature
                        '(' function_arguments ')' generic_constraints
                      {
                          GenericSignature genSign = $3;
                          if(genSign != null)
                              genSign.SetConstraints($7);

                          $$ = new FunctionPrototype(MemberFlags.InterfaceMember, $1, (FunctionArgument)$5, (string)$2.GetValue(), $3, $1.GetPosition());
                      }
                      ;

iface_method: iface_method_prototype ';' { $$ = new FunctionDefinition((FunctionPrototype)$1, null, $1.GetPosition());}
            ;

visibility_flag: KPUBLIC	          { $$ = new MemberFlagsAndMask(MemberFlags.Public, MemberFlags.VisibilityMask, $1); }
			   | KINTERNAL            { $$ = new MemberFlagsAndMask(MemberFlags.Internal, MemberFlags.VisibilityMask, $1); }
			   | KPROTECTED           { $$ = new MemberFlagsAndMask(MemberFlags.Protected, MemberFlags.VisibilityMask, $1); }
               | KPROTECTED KINTERNAL { $$ = new MemberFlagsAndMask(MemberFlags.ProtectedInternal, MemberFlags.VisibilityMask, $1); }
			   | KPRIVATE             { $$ = new MemberFlagsAndMask(MemberFlags.Private, MemberFlags.VisibilityMask, $1); }
			   ;
			   
language_flag: KCDECL	{ $$ = new MemberFlagsAndMask(MemberFlags.Cdecl,   MemberFlags.LanguageMask, $1); }
             | KSTDCALL { $$ = new MemberFlagsAndMask(MemberFlags.StdCall, MemberFlags.LanguageMask, $1); }
             | KAPICALL { $$ = new MemberFlagsAndMask(MemberFlags.ApiCall, MemberFlags.LanguageMask, $1); }
			 | KKERNEL	{ $$ = new MemberFlagsAndMask(MemberFlags.Kernel,  MemberFlags.LanguageMask, $1); }
			 ;
			 
instance_flag: KSTATIC	   { $$ = new MemberFlagsAndMask(MemberFlags.Static, MemberFlags.InstanceMask, $1);   }
			 | KVIRTUAL	   { $$ = new MemberFlagsAndMask(MemberFlags.Virtual, MemberFlags.InstanceMask, $1);  }
			 | KOVERRIDE   { $$ = new MemberFlagsAndMask(MemberFlags.Override, MemberFlags.InstanceMask, $1); }
             | KABSTRACT   { $$ = new MemberFlagsAndMask(MemberFlags.Abstract, MemberFlags.InstanceMask, $1); }
			 ;

linkage_flag: KEXTERN     { $$ = new MemberFlagsAndMask(MemberFlags.External, MemberFlags.InstanceMask, $1); }
            ;

access_flag: KREADONLY    { $$ = new MemberFlagsAndMask(MemberFlags.ReadOnly, MemberFlags.AccessMask, $1); }
           ;

security_flag: KUNSAFE    { $$ = new MemberFlagsAndMask(MemberFlags.Unsafe, MemberFlags.SecurityMask, $1); }
           ;

impl_flag: KPARTIAL     { $$ = new MemberFlagsAndMask(MemberFlags.Partial, MemberFlags.ImplFlagMask, $1); }
         ;

inheritance_flag: KSEALED { $$ = new MemberFlagsAndMask(MemberFlags.Sealed, MemberFlags.InheritanceMask, $1); }
                ;

member_flag: visibility_flag  { $$ = $1; }
           | language_flag    { $$ = $1; }
           | instance_flag    { $$ = $1; }
           | linkage_flag     { $$ = $1; }
           | access_flag      { $$ = $1; }
           | security_flag    { $$ = $1; }
           | impl_flag        { $$ = $1; }
           | inheritance_flag { $$ = $1; }
           ;

member_flag_list: member_flag   { $$ = $1; }
                | member_flag_list member_flag
                {
                    if(($1.flags & $2.mask) != MemberFlags.Default)
                        Error($2, "incompatible member flag.");
                    $$ = new MemberFlagsAndMask($1.flags | $2.flags, $1.mask | $2.mask, $1);
                }
                ;

member_flags: /* empty */       { $$ = MemberFlags.Default; }
            | member_flag_list  { $$ = $1.flags; }
            ;
			
global_flags: member_flags
            {
                if(($1 & MemberFlags.InstanceMask) != MemberFlags.Default)
                    Error("invalid global member flags");
                $$ = $1 | MemberFlags.Static;
            }
			;

class_flags: member_flags { $$ = $1; }
           ;

field_decl: IDENTIFIER '=' expression	{ $$ = new FieldDeclaration((string)$1.GetValue(), $3, $1); }
		  | IDENTIFIER					{ $$ = new FieldDeclaration((string)$1.GetValue(), null, $1); }
		  ;
		  
field_decls: field_decl					{ $$ = $1; }
		   | field_decls ',' field_decl	{ $$ = AddList($1, $3); }
		   ;
		   
field_definition: member_flags onlytype_expression field_decls ';'	{ $$ = new FieldDefinition($1, $2, (FieldDeclaration)$3, $2.GetPosition()); }
				;

accessor_flags: /* empty */     { $$ = MemberFlags.ImplicitVis; }
              | visibility_flag { $$ = $1; }
              ;

property_accessor: accessor_flags IDENTIFIER block_statement
                   {
                     string name = (string)$2.GetValue();
                     if(name == "get")
                         $$ = new GetAccessorDefinition($1, $3, $2);
                     else if(name == "set")
                         $$ = new SetAccessorDefinition($1, $3, $2);
                     else
                         Error($2, "only get and set property accessors are supported.");
                   }
                 | accessor_flags IDENTIFIER ';'
                   {
                     string name = (string)$2.GetValue();
                     if(name == "get")
                         $$ = new GetAccessorDefinition($1, null, $2);
                     else if(name == "set")
                         $$ = new SetAccessorDefinition($1, null, $2);
                     else
                         Error($2, "only get and set property accessors are supported.");
                   }
                  ;

property_accessors: property_accessor                       { $$ = $1; }
                  | property_accessors property_accessor    { $$ = AddList($1, $2); }
                  ;

indexer_arg: onlytype_expression IDENTIFIER { $$ = new FunctionArgument($1, (string)$2.GetValue(), $1.GetPosition()); }
           ;

indexer_args: indexer_arg                  { $$ = $1; }
            | indexer_args ',' indexer_arg { $$ = AddList($1, $3);}
            ;

property_definition: member_flags onlytype_expression function_name '{' property_accessors '}'
                   {
                       if($3 is string)
                           $$ = new PropertyDefinition($1, $2, (string)$3, null, $5, $2.GetPosition());
                       else
                           $$ = new PropertyDefinition($1, $2, (Expression)$3, null, $5, $2.GetPosition());
                   }
                   |
                   member_flags onlytype_expression function_name '[' indexer_args ']' '{' property_accessors '}'
                   {
                       if($3 is string)
                           $$ = new PropertyDefinition($1, $2, (string)$3, $5, $8, $2.GetPosition());
                       else
                           $$ = new PropertyDefinition($1, $2, (Expression)$3, $5, $8, $2.GetPosition());
                   }
                   | member_flags onlytype_expression KTHIS '[' indexer_args ']' '{' property_accessors '}'
                     { $$ = new PropertyDefinition($1, $2, "this", $5, $8, $2.GetPosition()); }

                   ;

iface_property_accessor: accessor_flags IDENTIFIER ';'             { $$ = new GetAccessorDefinition($1, null, $2);}
                         {
                           string name = (string)$2.GetValue();
                           if(name == "get")
                               $$ = new GetAccessorDefinition($1, null, $2);
                           else if(name == "set")
                               $$ = new SetAccessorDefinition($1, null, $2);
                           else
                               Error($2, "only get and set property accessors are supported.");
                         }
                       ;

iface_property_accessors: iface_property_accessor                       { $$ = $1; }
                        | iface_property_accessors iface_property_accessor    { $$ = AddList($1, $2); }
                        ;

iface_property_definition: onlytype_expression IDENTIFIER '{' iface_property_accessors '}'
                           { $$ = new PropertyDefinition(MemberFlags.InterfaceMember, $1, (string)$2.GetValue(), null, $4, $1.GetPosition()); }
                         |  onlytype_expression KTHIS '[' indexer_args ']' '{' iface_property_accessors '}'
                           { $$ = new PropertyDefinition(MemberFlags.InterfaceMember, $1, "this", $4, $7, $1.GetPosition()); }
                         ;

event_accessor: accessor_flags IDENTIFIER block_statement  { $$ = new EventAccessorDefinition($1, (string)$2.GetValue(), $3, $2); }
              | accessor_flags IDENTIFIER ';'              { $$ = new EventAccessorDefinition($1, (string)$2.GetValue(), null, $2);}
              ;

event_accessors: event_accessor                 { $$ = $1; }
               | event_accessors event_accessor { $$ = AddList($1, $2); }
               ;

event_definition: member_flags KEVENT onlytype_expression IDENTIFIER '{' event_accessors '}'
                  {
                    if($5 == null)
                        Error($2, "explicit event definition body cannot be empty.");
                    $$ = new EventDefinition($1, $3, (string)$4.GetValue(), $6, $2);
                  }

                | member_flags KEVENT onlytype_expression IDENTIFIER ';'
                  {
                    $$ = new EventDefinition($1, $3, (string)$4.GetValue(), null, $2);
                  }
                ;

iface_event_accessor: accessor_flags IDENTIFIER ';'              { $$ = new EventAccessorDefinition($1, (string)$2.GetValue(), null, $2);}
                    ;

iface_event_accessors: iface_event_accessor                       { $$ = $1; }
                     | iface_event_accessors iface_event_accessor { $$ = AddList($1, $2); }
                     ;

iface_event_definition: KEVENT onlytype_expression IDENTIFIER '{' iface_event_accessors '}'
                  {
                    if($4 == null)
                        Error($1, "explicit event definition body cannot be empty.");
                    $$ = new EventDefinition(MemberFlags.InterfaceMember, $2, (string)$3.GetValue(), $5, $1);
                  }

                | KEVENT onlytype_expression IDENTIFIER ';'
                  {
                    $$ = new EventDefinition(MemberFlags.InterfaceMember, $2, (string)$3.GetValue(), null, $1);
                  }
                ;

class_member: method_prototype ';'	 { $$ = $1; }
			| method_definition		 { $$ = $1; }
            | event_definition       { $$ = $1; }
			| field_definition		 { $$ = $1; }
            | property_definition    { $$ = $1; }
            | class_definition       { $$ = $1; }
            | delegate_definition    { $$ = $1; }
            | struct_definition      { $$ = $1; }
            | interface_definition   { $$ = $1; }
            | enum_definition        { $$ = $1; }
            | typedef                { $$ = $1; }
            | ';'                    { $$ = null; }
			;

class_attributed_member: '[' attribute_instances ']' class_member { $4.SetAttributes($2); $$ = $4;}
                       | class_member                            { $$ = $1; }
                       ;

class_members: /* empty*/					            { $$ = null;			}
			 | class_members class_attributed_member	{ $$ = AddList($1, $2); }
			 ;

interface_member: iface_method              { $$ = $1; }
                | iface_event_definition    { $$ = $1; }
                | iface_property_definition { $$ = $1; }
                ;

interface_members: /* empty */                        { $$ = null; }
                 | interface_members interface_member { $$ = AddList($1, $2); }
                 ;

base_list: onlytype_expression					{ $$ = $1;				}
		 | base_list ',' onlytype_expression	{ $$ = AddList($1, $3); }
		 ;
		 
parent_classes: /* empty*/		{ $$ = null; }
			  | ':' base_list	{ $$ = $2;	 }
			  ;
	
class_definition: class_flags KCLASS IDENTIFIER generic_signature parent_classes generic_constraints
				 '{' class_members '}'
					{
                      GenericSignature genSign = $4;
                      if(genSign != null)
                        genSign.SetConstraints($6);
					  $$ = new ClassDefinition($1, (string)$3.GetValue(), genSign, $5, $8, $2);
					}
				 ;
				
struct_definition: class_flags KSTRUCT IDENTIFIER generic_signature parent_classes generic_constraints
					'{' class_members '}'
					{
                      GenericSignature genSign = $4;
                      if(genSign != null)
                        genSign.SetConstraints($6);
					  $$ = new StructDefinition($1, (string)$3.GetValue(), genSign, $5, $8, $2);
					}
				 ;
interface_definition: class_flags KINTERFACE IDENTIFIER generic_signature parent_classes generic_constraints
                    '{' interface_members '}'
                    {
                        GenericSignature genSign = $4;
                        if(genSign != null)
                          genSign.SetConstraints($6);
                        $$ = new InterfaceDefinition($1, (string)$3.GetValue(), genSign, $5, $8, $2);
                    }
                    ;

delegate_definition: class_flags KDELEGATE onlytype_expression IDENTIFIER generic_signature '(' function_arguments ')' generic_constraints ';'
                    {
                        GenericSignature genSign = $5;
                        if(genSign != null)
                          genSign.SetConstraints($9);
                        $$ = new DelegateDefinition($1, $3, $7, (string)$4.GetValue(), genSign, $2);
                    }
                    ;

enum_constant: IDENTIFIER                { $$ = new EnumConstantDefinition((string)$1.GetValue(), null, $1); }
             | IDENTIFIER '=' expression { $$ = new EnumConstantDefinition((string)$1.GetValue(), $3, $1);   }
             ;

enum_constants: /* empty */                      { $$ = null; }
              | enum_constant                    { $$ = $1; }
              | enum_constants ',' enum_constant { $$ = AddList($1, $3); }
              | enum_constants ','               { $$ = $1; }
              ;

enum_type: /* empty */         { $$ = null; }
         | ':' onlytype_expression { $$ = $2; }
         ;
enum_definition: class_flags KENUM IDENTIFIER enum_type
                 '{' enum_constants '}'
               {
                    $$ = new EnumDefinition($1, (string)$3.GetValue(), $6, $4, $2);
               }
               ;

global_definition: global_flags onlytype_expression global_decls ';'
                   {
                    MemberFlags instance = ($1 & MemberFlags.InstanceMask);
                    if(instance != MemberFlags.Static)
                        Error($2.GetPosition(), "global variables must be static.");
                     $$ = new FieldDefinition($1, $2, (FieldDeclaration)$3, $2.GetPosition());
                   }
                ;

global_decl: IDENTIFIER '=' expression   { $$ = new FieldDeclaration((string)$1.GetValue(), $3, $1); }
           | IDENTIFIER                  { $$ = new FieldDeclaration((string)$1.GetValue(), null, $1); }
           ;

global_decls: global_decl                  { $$ = $1; }
            | global_decls ',' global_decl { $$ = AddList($1, $3); }
            ;

typedef: class_flags KTYPEDEF onlytype_expression IDENTIFIER ';' { $$ = new TypedefDefinition($1, $3, (string)$4.GetValue(), $2); }
       ;

using_statement: KUSING onlymember_expression ';'                   { $$ = new UsingStatement($2, $1); }
               | KUSING IDENTIFIER '=' onlymember_expression ';'    { $$ = new AliasDeclaration((string)$2.GetValue(), $4, $1); }
               ;


namespace_member: namespace_definition		{ $$ = $1; }
				| function_prototype ';'	{ $$ = $1; }
				| function_definition		{ $$ = $1; }
				| class_definition			{ $$ = $1; }
                | delegate_definition       { $$ = $1; }
                | event_definition          { $$ = $1; }
				| struct_definition			{ $$ = $1; }
                | interface_definition      { $$ = $1; }
                | enum_definition           { $$ = $1; }
                | global_definition         { $$ = $1; }
                | using_statement           { $$ = $1; }
                | typedef                   { $$ = $1; }
				| ';'						{ $$ = null; }
				;

namespace_declaration: '[' attribute_instances ']' namespace_member { $4.SetAttributes($2); $$ = $4; }
                     | namespace_member                             { $$ = $1; }
                     ;

namespace_declarations: /* empty */										{ $$ = null; }
					  | namespace_declarations namespace_declaration	{ $$ = AddList($1, $2); }
					  ;

nested_name: IDENTIFIER						{ $$ = (string)$1.GetValue(); }
		   | nested_name '.' IDENTIFIER		{ $$ = $1 + "." + (string)$3.GetValue(); }
		   ;
		   
namespace_definition: KNAMESPACE nested_name '{' namespace_declarations '}' {$$ = new NamespaceDefinition($2, $4, $1); }
					;

toplevel_declarations: /* empty*/									{ $$ = null;}
					 | toplevel_declarations namespace_declaration	{ $$ = AddList($1, $2); }
					 ;
					 
start: toplevel_declarations EOF	{ $$ = $1; }
	 ;
	 
%%

	/// The lexer.
	Lexer lexer;
	
	public Parser()
	{
		lexer = null;
	}
	
	public Lexer Lexer {
		get {
			return lexer;
		}
		set {
			lexer = value;
		}
	}
		
	public AstNode Parse()
	{
		eof_token = Token.EOF;
		AstNode ret = null;
		try
		{
			ret = (AstNode)yyparse(lexer);
		}
		catch(yyParser.yyException e)
		{
            try
            {
                string[] expecting = yyExpecting(yyExpectingState);
                if(expecting.Length != 0)
                {
                    StringBuilder message = new StringBuilder();
                    message.Append("expecting ");
                    for(int i = 0; i < expecting.Length; i++)
                    {
                        if(expecting[i] == null) continue;
                        if(i > 0)
                            message.Append(", ");
                        message.Append(expecting[i]);
                    }
    
                    message.Append(" got instead '");
                    int gotToken = ((TokenValue)lexer.value()).GetToken();
                    if(gotToken < 128)
                        message.Append(((char)gotToken).ToString());
                    else
                    {
                        if(gotToken < yyNames.Length)
                        {
                            string tokenName = yyNames[gotToken];
                            if(!string.IsNullOrEmpty(tokenName))
                                message.Append(tokenName);
                            else
                                message.Append(gotToken.ToString());
                        }
                        else
                            message.Append(gotToken.ToString());
                    }
                    message.Append("'");
    
                    Error((TokenPosition)lexer.value(), message.ToString());
                }
                else
                {
    			    Error((TokenPosition)lexer.value(), e.Message);
                }
            }
            catch(System.IndexOutOfRangeException)
            {
                Error((TokenPosition)lexer.value(), e.Message);
            }
		}
		return ret;
	}
	
/* The end*/
}
