// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 2 "Parser.y"
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
#line 511 "Parser.y"
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
#line default

  /** error output stream.
      It should be changeable.
    */
  public System.IO.TextWriter ErrorOutput = System.Console.Out;

  /** simplified error message.
      @see <a href="#yyerror(java.lang.String, java.lang.String[])">yyerror</a>
    */
  public void yyerror (string message) {
    yyerror(message, null);
  }

  /* An EOF token */
  public int eof_token;

  /** (syntax) error message.
      Can be overwritten to control message format.
      @param message text to be displayed.
      @param expected vector of acceptable tokens, if available.
    */
  public void yyerror (string message, string[] expected) {
    if ((yacc_verbose_flag > 0) && (expected != null) && (expected.Length  > 0)) {
      ErrorOutput.Write (message+", expecting");
      for (int n = 0; n < expected.Length; ++ n)
        ErrorOutput.Write (" "+expected[n]);
        ErrorOutput.WriteLine ();
    } else
      ErrorOutput.WriteLine (message);
  }

  /** debugging support, requires the package jay.yydebug.
      Set to null to suppress debugging messages.
    */
//t  internal yydebug.yyDebug debug;

  protected const int yyFinal = 1;
//t // Put this array into a separate class so it is only initialized if debugging is actually used
//t // Use MarshalByRefObject to disable inlining
//t class YYRules : MarshalByRefObject {
//t  public static readonly string [] yyRule = {
//t    "$accept : start",
//t    "data_types : KBOOL",
//t    "data_types : KBYTE",
//t    "data_types : KSBYTE",
//t    "data_types : KCHAR",
//t    "data_types : KUCHAR",
//t    "data_types : KDOUBLE",
//t    "data_types : KFLOAT",
//t    "data_types : KINT",
//t    "data_types : KUINT",
//t    "data_types : KLONG",
//t    "data_types : KULONG",
//t    "data_types : KSHORT",
//t    "data_types : KUSHORT",
//t    "data_types : KOBJECT",
//t    "data_types : KSTRING",
//t    "data_types : KSIZE_T",
//t    "data_types : KVOID",
//t    "data_types : KPTRDIFF_T",
//t    "data_types : KVEC2",
//t    "data_types : KVEC3",
//t    "data_types : KVEC4",
//t    "data_types : KDVEC2",
//t    "data_types : KDVEC3",
//t    "data_types : KDVEC4",
//t    "data_types : KIVEC2",
//t    "data_types : KIVEC3",
//t    "data_types : KIVEC4",
//t    "data_types : KBVEC2",
//t    "data_types : KBVEC3",
//t    "data_types : KBVEC4",
//t    "data_types : KMAT2",
//t    "data_types : KMAT2x3",
//t    "data_types : KMAT2x4",
//t    "data_types : KMAT3x2",
//t    "data_types : KMAT3",
//t    "data_types : KMAT3x4",
//t    "data_types : KMAT4x2",
//t    "data_types : KMAT4x3",
//t    "data_types : KMAT4",
//t    "data_types : KDMAT2",
//t    "data_types : KDMAT2x3",
//t    "data_types : KDMAT2x4",
//t    "data_types : KDMAT3x2",
//t    "data_types : KDMAT3",
//t    "data_types : KDMAT3x4",
//t    "data_types : KDMAT4x2",
//t    "data_types : KDMAT4x3",
//t    "data_types : KDMAT4",
//t    "data_types : KIMAT2",
//t    "data_types : KIMAT2x3",
//t    "data_types : KIMAT2x4",
//t    "data_types : KIMAT3x2",
//t    "data_types : KIMAT3",
//t    "data_types : KIMAT3x4",
//t    "data_types : KIMAT4x2",
//t    "data_types : KIMAT4x3",
//t    "data_types : KIMAT4",
//t    "onlymember_expression : onlymember_expression '.' IDENTIFIER",
//t    "onlymember_expression : IDENTIFIER",
//t    "member_expression : primary_expression '.' IDENTIFIER",
//t    "variable_reference : IDENTIFIER",
//t    "variable_reference : KTHIS",
//t    "variable_reference : KBASE",
//t    "number : BYTE",
//t    "number : SBYTE",
//t    "number : SHORT",
//t    "number : USHORT",
//t    "number : INTEGER",
//t    "number : UINTEGER",
//t    "number : LONG",
//t    "number : ULONG",
//t    "number : FLOAT",
//t    "number : DOUBLE",
//t    "number : BOOL",
//t    "number : KNULL",
//t    "number : CHARACTER",
//t    "string : STRING",
//t    "string : CSTRING",
//t    "subscript_indices : expression",
//t    "subscript_indices : subscript_indices ',' expression",
//t    "subindex_expression : primary_expression '[' subscript_indices ']'",
//t    "subindex_expression : primary_expression '[' array_dimensions ']'",
//t    "indirection_expression : primary_expression ARROW IDENTIFIER",
//t    "primary_lvalue : data_types",
//t    "primary_lvalue : variable_reference",
//t    "primary_lvalue : primary_lvalue GENERIC_START generic_arglist GENERIC_END",
//t    "primary_lvalue : primary_lvalue GENERIC_START_DOT generic_arglist GENERIC_END_DOT",
//t    "primary_lvalue : primary_lvalue '.' IDENTIFIER",
//t    "primary_lvalue : primary_lvalue ARROW IDENTIFIER",
//t    "lvalue_base_expression : onlytype_base_expression",
//t    "lvalue_base_expression : '*' lvalue_expression",
//t    "lvalue_expression : lvalue_base_expression",
//t    "lvalue_expression : lvalue_expression '.' IDENTIFIER",
//t    "lvalue_expression : lvalue_expression ARROW IDENTIFIER",
//t    "lvalue_expression : lvalue_call",
//t    "const_primary_static : primary_lvalue",
//t    "const_primary_static : KCONST primary_lvalue",
//t    "array_dimensions :",
//t    "array_dimensions : array_dimensions ','",
//t    "onlytype_noarray : const_primary_static",
//t    "onlytype_noarray : onlytype_noarray '*' KCONST",
//t    "onlytype_noarray : onlytype_noarray '*'",
//t    "onlytype_noarray : onlytype_noarray FUNC_PTR funptr_cc '(' onlytype_arglist ')'",
//t    "onlytype_base_expression : onlytype_noarray",
//t    "onlytype_base_expression : onlytype_base_expression '[' array_dimensions ']'",
//t    "onlytype_base_expression : onlytype_base_expression '[' subscript_indices ']'",
//t    "onlytype_expression : onlytype_base_expression",
//t    "const_primary : primary_expression",
//t    "const_primary : KCONST primary_expression",
//t    "onlytype_arg : onlytype_expression",
//t    "onlytype_arg : onlytype_expression IDENTIFIER",
//t    "onlytype_arglist :",
//t    "onlytype_arglist : onlytype_arg",
//t    "onlytype_arglist : onlytype_arglist ',' onlytype_arg",
//t    "array_initializer_expression : expression",
//t    "array_initializer_expression : '{' array_initializers '}'",
//t    "array_initializer_list : array_initializer_expression",
//t    "array_initializer_list : array_initializer_list ',' array_initializer_expression",
//t    "array_initializers :",
//t    "array_initializers : array_initializer_list",
//t    "array_initializers : array_initializer_list ','",
//t    "new_expression : NEW onlytype_noarray '(' call_arguments ')'",
//t    "new_expression : NEW onlytype_noarray '[' subscript_indices ']'",
//t    "new_expression : NEW onlytype_noarray '[' subscript_indices ']' '{' array_initializers '}'",
//t    "new_expression : NEW onlytype_noarray '[' array_dimensions ']' '{' array_initializers '}'",
//t    "alloc_expression : HEAPALLOC onlytype_noarray",
//t    "alloc_expression : HEAPALLOC onlytype_noarray '(' call_arguments ')'",
//t    "alloc_expression : HEAPALLOC onlytype_noarray '[' expression ']'",
//t    "alloc_expression : STACKALLOC onlytype_noarray",
//t    "alloc_expression : STACKALLOC onlytype_noarray '(' call_arguments ')'",
//t    "alloc_expression : STACKALLOC onlytype_noarray '[' expression ']'",
//t    "check_expression : CHECKED '(' expression ')'",
//t    "check_expression : UNCHECKED '(' expression ')'",
//t    "postfix_expression : primary_expression INCR",
//t    "postfix_expression : primary_expression DECR",
//t    "postfix_statement : lvalue_expression INCR",
//t    "postfix_statement : lvalue_expression DECR",
//t    "prefix_expression : INCR refvalue_expression",
//t    "prefix_expression : DECR refvalue_expression",
//t    "prefix_statement : prefix_expression ';'",
//t    "primary_expression : primary_lvalue",
//t    "primary_expression : number",
//t    "primary_expression : string",
//t    "primary_expression : call_expression",
//t    "primary_expression : check_expression",
//t    "primary_expression : subindex_expression",
//t    "primary_expression : indirection_expression",
//t    "primary_expression : member_expression",
//t    "primary_expression : new_expression",
//t    "primary_expression : alloc_expression",
//t    "primary_expression : postfix_expression",
//t    "primary_expression : sizeof_expression",
//t    "primary_expression : typeof_expression",
//t    "primary_expression : default_expression",
//t    "primary_expression : reinterpret_cast",
//t    "primary_expression : '(' expression ')'",
//t    "sizeof_expression : SIZEOF '(' onlytype_expression ')'",
//t    "typeof_expression : TYPEOF '(' onlytype_expression ')'",
//t    "default_expression : KDEFAULT '(' onlytype_expression ')'",
//t    "refvalue_expression : const_primary",
//t    "refvalue_expression : '*' refvalue_expression",
//t    "reinterpret_cast : REINTERPRET_CAST GENERIC_START onlytype_expression GENERIC_END '(' expression ')'",
//t    "funptr_cc :",
//t    "funptr_cc : language_flag",
//t    "type_expression : refvalue_expression",
//t    "type_expression : type_expression '*' KCONST",
//t    "type_expression : type_expression '*'",
//t    "type_expression : type_expression '*' unary_expression",
//t    "type_expression : type_expression FUNC_PTR funptr_cc '(' onlytype_arglist ')'",
//t    "unary_expression : refvalue_expression",
//t    "unary_expression : LNOT unary_expression",
//t    "unary_expression : BITNOT unary_expression",
//t    "unary_expression : '+' unary_expression",
//t    "unary_expression : '-' unary_expression",
//t    "unary_expression : BITAND unary_expression",
//t    "unary_expression : KREF unary_expression",
//t    "unary_expression : KOUT unary_expression",
//t    "unary_expression : '(' type_expression ')' unary_expression",
//t    "unary_expression : '(' type_expression ')'",
//t    "unary_expression : prefix_expression",
//t    "factor_expression : unary_expression",
//t    "factor_expression : factor_expression '*' unary_expression",
//t    "factor_expression : factor_expression '/' unary_expression",
//t    "factor_expression : factor_expression '%' unary_expression",
//t    "term_expression : factor_expression",
//t    "term_expression : term_expression '+' factor_expression",
//t    "term_expression : term_expression '-' factor_expression",
//t    "shift_expression : term_expression",
//t    "shift_expression : shift_expression LBITSHIFT term_expression",
//t    "shift_expression : shift_expression RBITSHIFT term_expression",
//t    "relation_expression : shift_expression",
//t    "relation_expression : relation_expression '<' shift_expression",
//t    "relation_expression : relation_expression LEQ shift_expression",
//t    "relation_expression : relation_expression GEQ shift_expression",
//t    "relation_expression : relation_expression '>' shift_expression",
//t    "relation_expression : relation_expression IS onlytype_expression",
//t    "relation_expression : relation_expression AS onlytype_expression",
//t    "equality_expression : relation_expression",
//t    "equality_expression : equality_expression EQ relation_expression",
//t    "equality_expression : equality_expression NEQ relation_expression",
//t    "bitwise_expression : equality_expression",
//t    "bitwise_expression : bitwise_expression BITAND equality_expression",
//t    "bitwise_expression : bitwise_expression BITOR equality_expression",
//t    "bitwise_expression : bitwise_expression BITXOR equality_expression",
//t    "logical_factor_expression : bitwise_expression",
//t    "logical_factor_expression : logical_factor_expression LAND bitwise_expression",
//t    "logical_term_expression : logical_factor_expression",
//t    "logical_term_expression : logical_term_expression LOR logical_factor_expression",
//t    "ternary_expression : logical_term_expression",
//t    "ternary_expression : logical_term_expression '?' logical_term_expression ':' logical_term_expression",
//t    "assignment_expression : refvalue_expression '=' expression",
//t    "assignment_expression : refvalue_expression ADD_SET expression",
//t    "assignment_expression : refvalue_expression SUB_SET expression",
//t    "assignment_expression : refvalue_expression MUL_SET expression",
//t    "assignment_expression : refvalue_expression DIV_SET expression",
//t    "assignment_expression : refvalue_expression MOD_SET expression",
//t    "assignment_expression : refvalue_expression OR_SET expression",
//t    "assignment_expression : refvalue_expression XOR_SET expression",
//t    "assignment_expression : refvalue_expression AND_SET expression",
//t    "assignment_expression : refvalue_expression LSHIFT_SET expression",
//t    "assignment_expression : refvalue_expression RSHIFT_SET expression",
//t    "assignment_lexpression : lvalue_expression '=' expression",
//t    "assignment_lexpression : lvalue_expression ADD_SET expression",
//t    "assignment_lexpression : lvalue_expression SUB_SET expression",
//t    "assignment_lexpression : lvalue_expression MUL_SET expression",
//t    "assignment_lexpression : lvalue_expression DIV_SET expression",
//t    "assignment_lexpression : lvalue_expression MOD_SET expression",
//t    "assignment_lexpression : lvalue_expression OR_SET expression",
//t    "assignment_lexpression : lvalue_expression XOR_SET expression",
//t    "assignment_lexpression : lvalue_expression AND_SET expression",
//t    "assignment_lexpression : lvalue_expression LSHIFT_SET expression",
//t    "assignment_lexpression : lvalue_expression RSHIFT_SET expression",
//t    "assignment_statement : assignment_lexpression ';'",
//t    "safe_expression : ternary_expression",
//t    "expression : ternary_expression",
//t    "expression : assignment_expression",
//t    "block_statement : '{' statements '}'",
//t    "unsafe_block_statement : KUNSAFE '{' statements '}'",
//t    "call_arguments :",
//t    "call_arguments : expression",
//t    "call_arguments : call_arguments ',' expression",
//t    "call_expression : primary_expression '(' call_arguments ')'",
//t    "lvalue_call : lvalue_expression '(' call_arguments ')'",
//t    "call_statement : lvalue_call ';'",
//t    "new_statement : new_expression ';'",
//t    "return_statement : KRETURN expression ';'",
//t    "return_statement : KRETURN ';'",
//t    "return_statement : IDENTIFIER KRETURN expression ';'",
//t    "if_statement : KIF '(' expression ')' statement",
//t    "if_statement : KIF '(' expression ')' statement KELSE statement",
//t    "switch_case : KCASE expression ':' statements",
//t    "switch_case : KDEFAULT ':' statements",
//t    "switch_cases : switch_case",
//t    "switch_cases : switch_cases switch_case",
//t    "switch_statement : KSWITCH '(' expression ')' '{' switch_cases '}'",
//t    "while_statement : KWHILE '(' expression ')' statement",
//t    "do_statement : KDO statement KWHILE '(' expression ')'",
//t    "for_incr :",
//t    "for_incr : assignment_expression",
//t    "for_incr : call_expression",
//t    "for_incr : prefix_expression",
//t    "for_incr : postfix_expression",
//t    "for_cond : ';'",
//t    "for_cond : expression ';'",
//t    "for_decls : ';'",
//t    "for_decls : localvar_statement",
//t    "for_statement : KFOR '(' for_decls for_cond for_incr ')' statement_scope",
//t    "foreach_statement : KFOREACH '(' onlytype_expression IDENTIFIER KIN expression ')' statement_scope",
//t    "break_statement : KBREAK ';'",
//t    "continue_statement : KCONTINUE ';'",
//t    "goto_case_statement : KGOTO KCASE expression ';'",
//t    "goto_case_statement : KGOTO KDEFAULT ';'",
//t    "catch_statement : KCATCH '(' onlytype_expression ')' statement",
//t    "catch_statement : KCATCH '(' onlytype_expression IDENTIFIER ')' statement",
//t    "catch_list : catch_statement",
//t    "catch_list : catch_list catch_statement",
//t    "finally_statement : KFINALLY statement",
//t    "try_statement : KTRY statement catch_list finally_statement",
//t    "try_statement : KTRY statement catch_list",
//t    "try_statement : KTRY statement finally_statement",
//t    "variable_decl : IDENTIFIER",
//t    "variable_decl : IDENTIFIER '=' expression",
//t    "variable_decls : variable_decl",
//t    "variable_decls : variable_decls ',' variable_decl",
//t    "localvar_statement : onlytype_expression variable_decls ';'",
//t    "lock_statement : KLOCK '(' expression ')' statement",
//t    "delete_statement : KDELETE expression ';'",
//t    "delete_statement : KDELETE '[' ']' expression ';'",
//t    "throw_statement : KTHROW expression ';'",
//t    "using_object_statement : KUSING '(' onlytype_expression variable_decls ')' statement",
//t    "fixed_variable : IDENTIFIER '=' expression",
//t    "fixed_variables : fixed_variable",
//t    "fixed_variables : fixed_variables ',' fixed_variable",
//t    "fixed_statement : KFIXED '(' onlytype_expression fixed_variables ')' statement",
//t    "statement : ';'",
//t    "statement : assignment_statement",
//t    "statement : block_statement",
//t    "statement : call_statement",
//t    "statement : new_statement",
//t    "statement : prefix_statement",
//t    "statement : postfix_statement",
//t    "statement : return_statement",
//t    "statement : while_statement",
//t    "statement : fixed_statement",
//t    "statement : for_statement",
//t    "statement : foreach_statement",
//t    "statement : delete_statement",
//t    "statement : do_statement",
//t    "statement : if_statement",
//t    "statement : break_statement",
//t    "statement : continue_statement",
//t    "statement : goto_case_statement",
//t    "statement : localvar_statement",
//t    "statement : lock_statement",
//t    "statement : switch_statement",
//t    "statement : throw_statement",
//t    "statement : try_statement",
//t    "statement : unsafe_block_statement",
//t    "statement : using_object_statement",
//t    "statements :",
//t    "statements : statements statement",
//t    "statement_scope : statement",
//t    "attribute_argument : IDENTIFIER '=' safe_expression",
//t    "attribute_argument : safe_expression",
//t    "attribute_arguments_commasep : attribute_argument",
//t    "attribute_arguments_commasep : attribute_arguments_commasep ',' attribute_argument",
//t    "attribute_arguments :",
//t    "attribute_arguments : attribute_arguments_commasep",
//t    "attribute_instance : onlymember_expression '(' attribute_arguments ')'",
//t    "attribute_instance : onlymember_expression",
//t    "attribute_instances : attribute_instance",
//t    "attribute_instances : attribute_instances ',' attribute_instance",
//t    "argtype_expression : onlytype_expression",
//t    "argtype_expression : onlytype_expression '$'",
//t    "argtype_expression : KREF onlytype_expression",
//t    "argtype_expression : KREF onlytype_expression '$'",
//t    "argtype_expression : KOUT onlytype_expression",
//t    "argtype_expression : KOUT onlytype_expression '$'",
//t    "argtype_expression : KPARAMS onlytype_expression",
//t    "argtype_expression : KREADONLY onlytype_expression",
//t    "generic_argument : IDENTIFIER",
//t    "generic_arguments : generic_argument",
//t    "generic_arguments : generic_arguments ',' generic_argument",
//t    "generic_constraint_name : onlytype_expression",
//t    "generic_constraint_name : KSTRUCT",
//t    "generic_constraint_name : NEW '(' ')'",
//t    "generic_constraint_chain : generic_constraint_name",
//t    "generic_constraint_chain : generic_constraint_chain ',' generic_constraint_name",
//t    "generic_constraint : IDENTIFIER IDENTIFIER ':' generic_constraint_chain",
//t    "generic_constraints :",
//t    "generic_constraints : generic_constraints generic_constraint",
//t    "generic_signature :",
//t    "generic_signature : GENERIC_START generic_arguments GENERIC_END",
//t    "generic_arglist : onlytype_expression",
//t    "generic_arglist : generic_arglist ',' onlytype_expression",
//t    "function_argument : argtype_expression",
//t    "function_argument : argtype_expression IDENTIFIER",
//t    "function_arguments_comma_sep : function_argument",
//t    "function_arguments_comma_sep : function_arguments ',' function_argument",
//t    "function_arguments :",
//t    "function_arguments : function_arguments_comma_sep",
//t    "operator_name : KOPERATOR '+'",
//t    "operator_name : KOPERATOR '-'",
//t    "operator_name : KOPERATOR '*'",
//t    "operator_name : KOPERATOR '/'",
//t    "operator_name : KOPERATOR '%'",
//t    "operator_name : KOPERATOR '<'",
//t    "operator_name : KOPERATOR '>'",
//t    "operator_name : KOPERATOR LEQ",
//t    "operator_name : KOPERATOR GEQ",
//t    "operator_name : KOPERATOR EQ",
//t    "operator_name : KOPERATOR NEQ",
//t    "operator_name : KOPERATOR BITAND",
//t    "operator_name : KOPERATOR BITXOR",
//t    "operator_name : KOPERATOR BITOR",
//t    "operator_name : KOPERATOR BITNOT",
//t    "function_name : IDENTIFIER",
//t    "function_name : function_name '.' IDENTIFIER",
//t    "function_name : function_name '.' KTHIS",
//t    "function_name : function_name GENERIC_START_DOT generic_arglist GENERIC_END_DOT",
//t    "function_prototype : global_flags onlytype_expression IDENTIFIER generic_signature '(' function_arguments ')' generic_constraints",
//t    "function_definition : function_prototype block_statement",
//t    "ctor_initializer :",
//t    "ctor_initializer : ':' KBASE '(' call_arguments ')'",
//t    "ctor_initializer : ':' KTHIS '(' call_arguments ')'",
//t    "method_prototype : member_flags onlytype_expression operator_name generic_signature '(' function_arguments ')' generic_constraints",
//t    "method_prototype : member_flags onlytype_expression function_name generic_signature '(' function_arguments ')' generic_constraints",
//t    "method_prototype : member_flags IDENTIFIER '(' function_arguments ')' ctor_initializer",
//t    "method_prototype : BITNOT IDENTIFIER '(' ')'",
//t    "method_definition : method_prototype block_statement",
//t    "iface_method_prototype : onlytype_expression IDENTIFIER generic_signature '(' function_arguments ')' generic_constraints",
//t    "iface_method : iface_method_prototype ';'",
//t    "visibility_flag : KPUBLIC",
//t    "visibility_flag : KINTERNAL",
//t    "visibility_flag : KPROTECTED",
//t    "visibility_flag : KPROTECTED KINTERNAL",
//t    "visibility_flag : KPRIVATE",
//t    "language_flag : KCDECL",
//t    "language_flag : KSTDCALL",
//t    "language_flag : KAPICALL",
//t    "language_flag : KKERNEL",
//t    "instance_flag : KSTATIC",
//t    "instance_flag : KVIRTUAL",
//t    "instance_flag : KOVERRIDE",
//t    "instance_flag : KABSTRACT",
//t    "linkage_flag : KEXTERN",
//t    "access_flag : KREADONLY",
//t    "security_flag : KUNSAFE",
//t    "impl_flag : KPARTIAL",
//t    "inheritance_flag : KSEALED",
//t    "member_flag : visibility_flag",
//t    "member_flag : language_flag",
//t    "member_flag : instance_flag",
//t    "member_flag : linkage_flag",
//t    "member_flag : access_flag",
//t    "member_flag : security_flag",
//t    "member_flag : impl_flag",
//t    "member_flag : inheritance_flag",
//t    "member_flag_list : member_flag",
//t    "member_flag_list : member_flag_list member_flag",
//t    "member_flags :",
//t    "member_flags : member_flag_list",
//t    "global_flags : member_flags",
//t    "class_flags : member_flags",
//t    "field_decl : IDENTIFIER '=' expression",
//t    "field_decl : IDENTIFIER",
//t    "field_decls : field_decl",
//t    "field_decls : field_decls ',' field_decl",
//t    "field_definition : member_flags onlytype_expression field_decls ';'",
//t    "accessor_flags :",
//t    "accessor_flags : visibility_flag",
//t    "property_accessor : accessor_flags IDENTIFIER block_statement",
//t    "property_accessor : accessor_flags IDENTIFIER ';'",
//t    "property_accessors : property_accessor",
//t    "property_accessors : property_accessors property_accessor",
//t    "indexer_arg : onlytype_expression IDENTIFIER",
//t    "indexer_args : indexer_arg",
//t    "indexer_args : indexer_args ',' indexer_arg",
//t    "property_definition : member_flags onlytype_expression function_name '{' property_accessors '}'",
//t    "property_definition : member_flags onlytype_expression function_name '[' indexer_args ']' '{' property_accessors '}'",
//t    "property_definition : member_flags onlytype_expression KTHIS '[' indexer_args ']' '{' property_accessors '}'",
//t    "$$1 :",
//t    "iface_property_accessor : accessor_flags IDENTIFIER ';' $$1",
//t    "iface_property_accessors : iface_property_accessor",
//t    "iface_property_accessors : iface_property_accessors iface_property_accessor",
//t    "iface_property_definition : onlytype_expression IDENTIFIER '{' iface_property_accessors '}'",
//t    "iface_property_definition : onlytype_expression KTHIS '[' indexer_args ']' '{' iface_property_accessors '}'",
//t    "event_accessor : accessor_flags IDENTIFIER block_statement",
//t    "event_accessor : accessor_flags IDENTIFIER ';'",
//t    "event_accessors : event_accessor",
//t    "event_accessors : event_accessors event_accessor",
//t    "event_definition : member_flags KEVENT onlytype_expression IDENTIFIER '{' event_accessors '}'",
//t    "event_definition : member_flags KEVENT onlytype_expression IDENTIFIER ';'",
//t    "iface_event_accessor : accessor_flags IDENTIFIER ';'",
//t    "iface_event_accessors : iface_event_accessor",
//t    "iface_event_accessors : iface_event_accessors iface_event_accessor",
//t    "iface_event_definition : KEVENT onlytype_expression IDENTIFIER '{' iface_event_accessors '}'",
//t    "iface_event_definition : KEVENT onlytype_expression IDENTIFIER ';'",
//t    "class_member : method_prototype ';'",
//t    "class_member : method_definition",
//t    "class_member : event_definition",
//t    "class_member : field_definition",
//t    "class_member : property_definition",
//t    "class_member : class_definition",
//t    "class_member : delegate_definition",
//t    "class_member : struct_definition",
//t    "class_member : interface_definition",
//t    "class_member : enum_definition",
//t    "class_member : typedef",
//t    "class_member : ';'",
//t    "class_attributed_member : '[' attribute_instances ']' class_member",
//t    "class_attributed_member : class_member",
//t    "class_members :",
//t    "class_members : class_members class_attributed_member",
//t    "interface_member : iface_method",
//t    "interface_member : iface_event_definition",
//t    "interface_member : iface_property_definition",
//t    "interface_members :",
//t    "interface_members : interface_members interface_member",
//t    "base_list : onlytype_expression",
//t    "base_list : base_list ',' onlytype_expression",
//t    "parent_classes :",
//t    "parent_classes : ':' base_list",
//t    "class_definition : class_flags KCLASS IDENTIFIER generic_signature parent_classes generic_constraints '{' class_members '}'",
//t    "struct_definition : class_flags KSTRUCT IDENTIFIER generic_signature parent_classes generic_constraints '{' class_members '}'",
//t    "interface_definition : class_flags KINTERFACE IDENTIFIER generic_signature parent_classes generic_constraints '{' interface_members '}'",
//t    "delegate_definition : class_flags KDELEGATE onlytype_expression IDENTIFIER generic_signature '(' function_arguments ')' generic_constraints ';'",
//t    "enum_constant : IDENTIFIER",
//t    "enum_constant : IDENTIFIER '=' expression",
//t    "enum_constants :",
//t    "enum_constants : enum_constant",
//t    "enum_constants : enum_constants ',' enum_constant",
//t    "enum_constants : enum_constants ','",
//t    "enum_type :",
//t    "enum_type : ':' onlytype_expression",
//t    "enum_definition : class_flags KENUM IDENTIFIER enum_type '{' enum_constants '}'",
//t    "global_definition : global_flags onlytype_expression global_decls ';'",
//t    "global_decl : IDENTIFIER '=' expression",
//t    "global_decl : IDENTIFIER",
//t    "global_decls : global_decl",
//t    "global_decls : global_decls ',' global_decl",
//t    "typedef : class_flags KTYPEDEF onlytype_expression IDENTIFIER ';'",
//t    "using_statement : KUSING onlymember_expression ';'",
//t    "using_statement : KUSING IDENTIFIER '=' onlymember_expression ';'",
//t    "namespace_member : namespace_definition",
//t    "namespace_member : function_prototype ';'",
//t    "namespace_member : function_definition",
//t    "namespace_member : class_definition",
//t    "namespace_member : delegate_definition",
//t    "namespace_member : event_definition",
//t    "namespace_member : struct_definition",
//t    "namespace_member : interface_definition",
//t    "namespace_member : enum_definition",
//t    "namespace_member : global_definition",
//t    "namespace_member : using_statement",
//t    "namespace_member : typedef",
//t    "namespace_member : ';'",
//t    "namespace_declaration : '[' attribute_instances ']' namespace_member",
//t    "namespace_declaration : namespace_member",
//t    "namespace_declarations :",
//t    "namespace_declarations : namespace_declarations namespace_declaration",
//t    "nested_name : IDENTIFIER",
//t    "nested_name : nested_name '.' IDENTIFIER",
//t    "namespace_definition : KNAMESPACE nested_name '{' namespace_declarations '}'",
//t    "toplevel_declarations :",
//t    "toplevel_declarations : toplevel_declarations namespace_declaration",
//t    "start : toplevel_declarations EOF",
//t  };
//t public static string getRule (int index) {
//t    return yyRule [index];
//t }
//t}
  protected static readonly string [] yyNames = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,"'$'","'%'",null,
    null,"'('","')'","'*'","'+'","','","'-'","'.'","'/'",null,null,null,
    null,null,null,null,null,null,null,"':'","';'","'<'","'='","'>'",
    "'?'",null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,"'['",null,"']'",null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,"'{'",null,"'}'",null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,"KBOOL","KBYTE","KSBYTE","KCHAR","KUCHAR","KDOUBLE","KFLOAT",
    "KINT","KUINT","KLONG","KULONG","KSHORT","KUSHORT","KOBJECT",
    "KSTRING","KSIZE_T","KPTRDIFF_T","KVOID","KCONST","KTYPEDEF","KVEC2",
    "KVEC3","KVEC4","KDVEC2","KDVEC3","KDVEC4","KIVEC2","KIVEC3","KIVEC4",
    "KBVEC2","KBVEC3","KBVEC4","KMAT2","KMAT2x3","KMAT2x4","KMAT3x2",
    "KMAT3","KMAT3x4","KMAT4x2","KMAT4x3","KMAT4","KDMAT2","KDMAT2x3",
    "KDMAT2x4","KDMAT3x2","KDMAT3","KDMAT3x4","KDMAT4x2","KDMAT4x3",
    "KDMAT4","KIMAT2","KIMAT2x3","KIMAT2x4","KIMAT3x2","KIMAT3",
    "KIMAT3x4","KIMAT4x2","KIMAT4x3","KIMAT4","KNULL","KBASE","KTHIS",
    "KCLASS","KDELEGATE","KEVENT","KINTERFACE","KNAMESPACE","KSTRUCT",
    "KENUM","KOPERATOR","KBREAK","KCASE","KCATCH","KCONTINUE","KDO",
    "KELSE","KFINALLY","KFOR","KFOREACH","KFIXED","KGOTO","KIF","KIN",
    "KLOCK","KRETURN","KWHILE","KDELETE","KDEFAULT","KUSING","KSWITCH",
    "KTHROW","KTRY","KEXTERN","KPUBLIC","KINTERNAL","KPROTECTED",
    "KPRIVATE","KKERNEL","KSTATIC","KVIRTUAL","KOVERRIDE","KABSTRACT",
    "KSEALED","KREADONLY","KUNSAFE","KPARTIAL","KCDECL","KSTDCALL",
    "KAPICALL","LAND","LOR","LNOT","LBITSHIFT","RBITSHIFT","BITAND",
    "BITOR","BITXOR","BITNOT","LEQ","GEQ","EQ","NEQ","IS","AS","ARROW",
    "NEW","HEAPALLOC","STACKALLOC","CHECKED","UNCHECKED",
    "REINTERPRET_CAST","GENERIC_START","GENERIC_END","GENERIC_START_DOT",
    "GENERIC_END_DOT","INCR","DECR","ADD_SET","SUB_SET","MUL_SET",
    "DIV_SET","MOD_SET","OR_SET","XOR_SET","AND_SET","LSHIFT_SET",
    "RSHIFT_SET","SIZEOF","TYPEOF","FUNC_PTR","BYTE","SBYTE","SHORT",
    "USHORT","INTEGER","UINTEGER","LONG","ULONG","BOOL","FLOAT","DOUBLE",
    "CHARACTER","CSTRING","STRING","IDENTIFIER","KREF","KOUT","KPARAMS",
  };

  /** index-checked interface to yyNames[].
      @param token single character or %token value.
      @return token name or [illegal] or [unknown].
    */
//t  public static string yyname (int token) {
//t    if ((token < 0) || (token > yyNames.Length)) return "[illegal]";
//t    string name;
//t    if ((name = yyNames[token]) != null) return name;
//t    return "[unknown]";
//t  }

  int yyExpectingState;
  /** computes list of expected tokens on error by tracing the tables.
      @param state for which to compute the list.
      @return list of token names.
    */
  protected int [] yyExpectingTokens (int state){
    int token, n, len = 0;
    bool[] ok = new bool[yyNames.Length];
    if ((n = yySindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
          ++ len;
          ok[token] = true;
        }
    if ((n = yyRindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
          ++ len;
          ok[token] = true;
        }
    int [] result = new int [len];
    for (n = token = 0; n < len;  ++ token)
      if (ok[token]) result[n++] = token;
    return result;
  }
  protected string[] yyExpecting (int state) {
    int [] tokens = yyExpectingTokens (state);
    string [] result = new string[tokens.Length];
    for (int n = 0; n < tokens.Length;  n++)
      result[n++] = yyNames[tokens [n]];
    return result;
  }

  /** the generated parser, with debugging messages.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @param yydebug debug message writer implementing yyDebug, or null.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  internal Object yyparse (yyParser.yyInput yyLex, Object yyd)
				 {
//t    this.debug = (yydebug.yyDebug)yyd;
    return yyparse(yyLex);
  }

  /** initial size and increment of the state/value stack [default 256].
      This is not final so that it can be overwritten outside of invocations
      of yyparse().
    */
  protected int yyMax;

  /** executed at the beginning of a reduce action.
      Used as $$ = yyDefault($1), prior to the user-specified action, if any.
      Can be overwritten to provide deep copy, etc.
      @param first value for $1, or null.
      @return first.
    */
  protected Object yyDefault (Object first) {
    return first;
  }

	static int[] global_yyStates;
	static object[] global_yyVals;
	protected bool use_global_stacks;
	object[] yyVals;					// value stack
	object yyVal;						// value stack ptr
	int yyToken;						// current input
	int yyTop;

  /** the generated parser.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  internal Object yyparse (yyParser.yyInput yyLex)
  {
    if (yyMax <= 0) yyMax = 256;		// initial size
    int yyState = 0;                   // state stack ptr
    int [] yyStates;               	// state stack 
    yyVal = null;
    yyToken = -1;
    int yyErrorFlag = 0;				// #tks to shift
	if (use_global_stacks && global_yyStates != null) {
		yyVals = global_yyVals;
		yyStates = global_yyStates;
   } else {
		yyVals = new object [yyMax];
		yyStates = new int [yyMax];
		if (use_global_stacks) {
			global_yyVals = yyVals;
			global_yyStates = yyStates;
		}
	}

    /*yyLoop:*/ for (yyTop = 0;; ++ yyTop) {
      if (yyTop >= yyStates.Length) {			// dynamically increase
        global::System.Array.Resize (ref yyStates, yyStates.Length+yyMax);
        global::System.Array.Resize (ref yyVals, yyVals.Length+yyMax);
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;
//t      if (debug != null) debug.push(yyState, yyVal);

      /*yyDiscarded:*/ while (true) {	// discarding a token does not change stack
        int yyN;
        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
//t            if (debug != null)
//t              debug.lex(yyState, yyToken, yyname(yyToken), yyLex.value());
          }
          if ((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
//t            if (debug != null)
//t              debug.shift(yyState, yyTable[yyN], yyErrorFlag-1);
            yyState = yyTable[yyN];		// shift to yyN
            yyVal = yyLex.value();
            yyToken = -1;
            if (yyErrorFlag > 0) -- yyErrorFlag;
            goto continue_yyLoop;
          }
          if ((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
            yyN = yyTable[yyN];			// reduce (yyN)
          else
            switch (yyErrorFlag) {
  
            case 0:
              yyExpectingState = yyState;
              // yyerror(String.Format ("syntax error, got token `{0}'", yyname (yyToken)), yyExpecting(yyState));
//t              if (debug != null) debug.error("syntax error");
              if (yyToken == 0 /*eof*/ || yyToken == eof_token) throw new yyParser.yyUnexpectedEof ();
              goto case 1;
            case 1: case 2:
              yyErrorFlag = 3;
              do {
                if ((yyN = yySindex[yyStates[yyTop]]) != 0
                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
                    && yyCheck[yyN] == Token.yyErrorCode) {
//t                  if (debug != null)
//t                    debug.shift(yyStates[yyTop], yyTable[yyN], 3);
                  yyState = yyTable[yyN];
                  yyVal = yyLex.value();
                  goto continue_yyLoop;
                }
//t                if (debug != null) debug.pop(yyStates[yyTop]);
              } while (-- yyTop >= 0);
//t              if (debug != null) debug.reject();
              throw new yyParser.yyException("irrecoverable syntax error");
  
            case 3:
              if (yyToken == 0) {
//t                if (debug != null) debug.reject();
                throw new yyParser.yyException("irrecoverable syntax error at end-of-file");
              }
//t              if (debug != null)
//t                debug.discard(yyState, yyToken, yyname(yyToken),
//t  							yyLex.value());
              yyToken = -1;
              goto continue_yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1-yyLen[yyN];
//t        if (debug != null)
//t          debug.reduce(yyState, yyStates[yyV-1], yyN, YYRules.getRule (yyN), yyLen[yyN]);
        yyVal = yyV > yyTop ? null : yyVals[yyV]; // yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
        switch (yyN) {
case 1:
#line 548 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Bool, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 2:
#line 549 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Byte, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 3:
#line 550 "Parser.y"
  { yyVal = new TypeNode(TypeKind.SByte, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 4:
#line 551 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Char, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 5:
#line 552 "Parser.y"
  { yyVal = new TypeNode(TypeKind.UChar, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 6:
#line 553 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 7:
#line 554 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 8:
#line 555 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 9:
#line 556 "Parser.y"
  { yyVal = new TypeNode(TypeKind.UInt, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 10:
#line 557 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Long, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 11:
#line 558 "Parser.y"
  { yyVal = new TypeNode(TypeKind.ULong, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 12:
#line 559 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Short, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 13:
#line 560 "Parser.y"
  { yyVal = new TypeNode(TypeKind.UShort, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 14:
#line 561 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Object, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 15:
#line 562 "Parser.y"
  { yyVal = new TypeNode(TypeKind.String, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 16:
#line 563 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Size, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 17:
#line 564 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Void, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 18:
#line 565 "Parser.y"
  { yyVal = new TypeNode(TypeKind.PtrDiff, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 19:
#line 567 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 20:
#line 568 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 21:
#line 569 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 22:
#line 570 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 23:
#line 571 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 24:
#line 572 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 25:
#line 573 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 26:
#line 574 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 27:
#line 575 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 28:
#line 576 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Bool, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 29:
#line 577 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Bool, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 30:
#line 578 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Bool, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 31:
#line 580 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 2, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 32:
#line 581 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 2, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 33:
#line 582 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 2, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 34:
#line 583 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 3, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 35:
#line 584 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 3, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 36:
#line 585 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 3, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 37:
#line 586 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 4, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 38:
#line 587 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 4, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 39:
#line 588 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Float, 4, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 40:
#line 590 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 2, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 41:
#line 591 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 2, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 42:
#line 592 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 2, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 43:
#line 593 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 3, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 44:
#line 594 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 3, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 45:
#line 595 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 3, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 46:
#line 596 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 4, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 47:
#line 597 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 4, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 48:
#line 598 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Double, 4, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 49:
#line 600 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 2, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 50:
#line 601 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 2, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 51:
#line 602 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 2, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 52:
#line 603 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 3, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 53:
#line 604 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 3, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 54:
#line 605 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 3, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 55:
#line 606 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 4, 2, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 56:
#line 607 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 4, 3, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 57:
#line 608 "Parser.y"
  { yyVal = new TypeNode(TypeKind.Int, 4, 4, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 58:
#line 611 "Parser.y"
  { yyVal = new MemberAccess(((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 59:
#line 612 "Parser.y"
  { yyVal = new VariableReference((string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 60:
#line 615 "Parser.y"
  { yyVal = new MemberAccess(((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 61:
#line 618 "Parser.y"
  { yyVal = new VariableReference((string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 62:
#line 619 "Parser.y"
  { yyVal = new VariableReference("this", ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 63:
#line 620 "Parser.y"
  { yyVal = new BaseExpression(((TokenPosition)yyVals[0+yyTop])); }
  break;
case 64:
#line 623 "Parser.y"
  { yyVal = new ByteConstant((byte)(ulong)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 65:
#line 624 "Parser.y"
  { yyVal = new SByteConstant((sbyte)(long)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 66:
#line 625 "Parser.y"
  { yyVal = new ShortConstant((short)(long)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 67:
#line 626 "Parser.y"
  { yyVal = new UShortConstant((ushort)(ulong)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 68:
#line 627 "Parser.y"
  { yyVal = new IntegerConstant((int)(long)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 69:
#line 628 "Parser.y"
  { yyVal = new UIntegerConstant((uint)(ulong)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 70:
#line 629 "Parser.y"
  { yyVal = new LongConstant((long)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 71:
#line 630 "Parser.y"
  { yyVal = new ULongConstant((ulong)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 72:
#line 631 "Parser.y"
  { yyVal = new FloatConstant((float)(double)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 73:
#line 632 "Parser.y"
  { yyVal = new DoubleConstant((double)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 74:
#line 633 "Parser.y"
  { yyVal = new BoolConstant((bool)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 75:
#line 634 "Parser.y"
  { yyVal = new NullConstant(((TokenPosition)yyVals[0+yyTop])); }
  break;
case 76:
#line 635 "Parser.y"
  { yyVal = new CharacterConstant((char)(long)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 77:
#line 638 "Parser.y"
  { yyVal = new StringConstant((string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 78:
#line 639 "Parser.y"
  { yyVal = new CStringConstant((string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 79:
#line 642 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 80:
#line 643 "Parser.y"
  { yyVal = (Expression)AddList(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop])); }
  break;
case 81:
#line 646 "Parser.y"
  { yyVal = new SubscriptAccess(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 82:
#line 647 "Parser.y"
  { yyVal = new MakeArray(((Expression)yyVals[-3+yyTop]), ((int)yyVals[-1+yyTop]), ((Expression)yyVals[-3+yyTop]).GetPosition()); }
  break;
case 83:
#line 650 "Parser.y"
  { yyVal = new IndirectAccess(((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 84:
#line 653 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 85:
#line 654 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 86:
#line 655 "Parser.y"
  { yyVal = new GenericInstanceExpr(((Expression)yyVals[-3+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 87:
#line 656 "Parser.y"
  { yyVal = new GenericInstanceExpr(((Expression)yyVals[-3+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 88:
#line 657 "Parser.y"
  { yyVal = new MemberAccess(((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 89:
#line 658 "Parser.y"
  { yyVal = new IndirectAccess(((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 90:
#line 661 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 91:
#line 662 "Parser.y"
  { yyVal = new DereferenceOperation(((Expression)yyVals[0+yyTop]), ((Expression)yyVals[0+yyTop]).GetPosition()); }
  break;
case 92:
#line 665 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 93:
#line 666 "Parser.y"
  { yyVal = new MemberAccess(((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 94:
#line 667 "Parser.y"
  { yyVal = new IndirectAccess(((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 96:
#line 671 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 97:
#line 672 "Parser.y"
  { yyVal = new MakeConstant(((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 98:
#line 675 "Parser.y"
  { yyVal = 1; }
  break;
case 99:
#line 676 "Parser.y"
  { yyVal = ((int)yyVals[-1+yyTop]) + 1;}
  break;
case 100:
#line 679 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 101:
#line 680 "Parser.y"
  { yyVal = new MakeConstant(new MakePointer(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[-2+yyTop]).GetPosition()), ((Expression)yyVals[-2+yyTop]).GetPosition()); }
  break;
case 102:
#line 681 "Parser.y"
  { yyVal = new MakePointer(((Expression)yyVals[-1+yyTop]), ((Expression)yyVals[-1+yyTop]).GetPosition()); }
  break;
case 103:
#line 682 "Parser.y"
  { yyVal = new MakeFunctionPointer(((Expression)yyVals[-5+yyTop]), ((Expression)yyVals[-1+yyTop]), ((MemberFlags)yyVals[-3+yyTop]), ((Expression)yyVals[-5+yyTop]).GetPosition()); }
  break;
case 104:
#line 685 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 105:
#line 686 "Parser.y"
  { yyVal = new MakeArray(((Expression)yyVals[-3+yyTop]), ((int)yyVals[-1+yyTop]), ((Expression)yyVals[-3+yyTop]).GetPosition()); }
  break;
case 106:
#line 687 "Parser.y"
  { yyVal = new SubscriptAccess(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 107:
#line 690 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); ((Expression)yyVals[0+yyTop]).SetHints(Expression.TypeHint); }
  break;
case 108:
#line 693 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 109:
#line 694 "Parser.y"
  { yyVal = new MakeConstant(((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 110:
#line 697 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 111:
#line 698 "Parser.y"
  { yyVal = ((Expression)yyVals[-1+yyTop]); }
  break;
case 112:
#line 701 "Parser.y"
  { yyVal = null; }
  break;
case 113:
#line 702 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 114:
#line 703 "Parser.y"
  { yyVal = AddList(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop])); }
  break;
case 115:
#line 706 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 116:
#line 707 "Parser.y"
  { yyVal = new ArrayExpression((Expression)((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 117:
#line 710 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 118:
#line 711 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop])); }
  break;
case 119:
#line 714 "Parser.y"
  { yyVal = null; }
  break;
case 120:
#line 715 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]);   }
  break;
case 121:
#line 716 "Parser.y"
  { yyVal = ((AstNode)yyVals[-1+yyTop]);   }
  break;
case 122:
#line 719 "Parser.y"
  { yyVal = new NewExpression(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-4+yyTop]));            }
  break;
case 123:
#line 720 "Parser.y"
  { yyVal = new NewArrayExpression(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), null, -1, ((TokenPosition)yyVals[-4+yyTop])); }
  break;
case 124:
#line 721 "Parser.y"
  { yyVal = new NewArrayExpression(((Expression)yyVals[-6+yyTop]), ((Expression)yyVals[-4+yyTop]), ((AstNode)yyVals[-1+yyTop]), -1, ((TokenPosition)yyVals[-7+yyTop]));   }
  break;
case 125:
#line 722 "Parser.y"
  { yyVal = new NewArrayExpression(((Expression)yyVals[-6+yyTop]), null, ((AstNode)yyVals[-1+yyTop]), ((int)yyVals[-4+yyTop]), ((TokenPosition)yyVals[-7+yyTop])); }
  break;
case 126:
#line 725 "Parser.y"
  { yyVal = new NewRawExpression(((Expression)yyVals[0+yyTop]), null, true, ((TokenPosition)yyVals[-1+yyTop]));          }
  break;
case 127:
#line 726 "Parser.y"
  { yyVal = new NewRawExpression(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), true, ((TokenPosition)yyVals[-4+yyTop]));            }
  break;
case 128:
#line 727 "Parser.y"
  { yyVal = new NewRawArrayExpression(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), true, ((TokenPosition)yyVals[-4+yyTop]));    }
  break;
case 129:
#line 728 "Parser.y"
  { yyVal = new NewRawExpression(((Expression)yyVals[0+yyTop]), null, false, ((TokenPosition)yyVals[-1+yyTop]));          }
  break;
case 130:
#line 729 "Parser.y"
  { yyVal = new NewRawExpression(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), false, ((TokenPosition)yyVals[-4+yyTop]));            }
  break;
case 131:
#line 730 "Parser.y"
  { yyVal = new NewRawArrayExpression(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), false, ((TokenPosition)yyVals[-4+yyTop]));    }
  break;
case 132:
#line 733 "Parser.y"
  { yyVal = ((Expression)yyVals[-1+yyTop]); }
  break;
case 133:
#line 734 "Parser.y"
  { yyVal = ((Expression)yyVals[-1+yyTop]); }
  break;
case 134:
#line 737 "Parser.y"
  { yyVal = new PostfixOperation(PostfixOperation.Increment, ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 135:
#line 738 "Parser.y"
  { yyVal = new PostfixOperation(PostfixOperation.Decrement, ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 136:
  case_136();
  break;
case 137:
  case_137();
  break;
case 138:
#line 753 "Parser.y"
  { yyVal = new PrefixOperation(PrefixOperation.Increment, ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 139:
#line 754 "Parser.y"
  { yyVal = new PrefixOperation(PrefixOperation.Decrement, ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 140:
#line 757 "Parser.y"
  { yyVal = new ExpressionStatement(((Expression)yyVals[-1+yyTop]), ((Expression)yyVals[-1+yyTop]).GetPosition()); }
  break;
case 141:
#line 760 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 142:
#line 761 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 143:
#line 762 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 144:
#line 763 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 145:
#line 764 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]);}
  break;
case 146:
#line 765 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 147:
#line 766 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 148:
#line 767 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 149:
#line 768 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 150:
#line 769 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 151:
#line 770 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 152:
#line 771 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 153:
#line 772 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 154:
#line 773 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 155:
#line 774 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 156:
#line 775 "Parser.y"
  { yyVal = ((Expression)yyVals[-1+yyTop]); }
  break;
case 157:
#line 778 "Parser.y"
  { yyVal = new SizeOfExpression(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 158:
#line 781 "Parser.y"
  { yyVal = new TypeOfExpression(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 159:
#line 784 "Parser.y"
  { yyVal = new DefaultExpression(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 160:
#line 787 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 161:
#line 788 "Parser.y"
  { yyVal = new DereferenceOperation(((Expression)yyVals[0+yyTop]), ((Expression)yyVals[0+yyTop]).GetPosition()); }
  break;
case 162:
#line 791 "Parser.y"
  { yyVal = new ReinterpretCast(((Expression)yyVals[-4+yyTop]), ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-6+yyTop])); }
  break;
case 163:
#line 794 "Parser.y"
  { yyVal = MemberFlags.Default; }
  break;
case 164:
#line 795 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]).flags;           }
  break;
case 165:
#line 798 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 166:
#line 799 "Parser.y"
  { yyVal = new MakeConstant(new MakePointer(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[-2+yyTop]).GetPosition()), ((Expression)yyVals[-2+yyTop]).GetPosition()); }
  break;
case 167:
#line 800 "Parser.y"
  { yyVal = new MakePointer(((Expression)yyVals[-1+yyTop]), ((Expression)yyVals[-1+yyTop]).GetPosition()); }
  break;
case 168:
#line 801 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpMul, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 169:
#line 802 "Parser.y"
  { yyVal = new MakeFunctionPointer(((Expression)yyVals[-5+yyTop]), ((Expression)yyVals[-1+yyTop]), ((MemberFlags)yyVals[-3+yyTop]), ((Expression)yyVals[-5+yyTop]).GetPosition()); }
  break;
case 170:
#line 805 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 171:
#line 806 "Parser.y"
  { yyVal = new UnaryOperation(UnaryOperation.OpNot, ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 172:
#line 807 "Parser.y"
  { yyVal = new UnaryOperation(UnaryOperation.OpBitNot, ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 173:
#line 808 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 174:
#line 809 "Parser.y"
  { yyVal = new UnaryOperation(UnaryOperation.OpNeg, ((Expression)yyVals[0+yyTop]), ((Expression)yyVals[0+yyTop]).GetPosition()); }
  break;
case 175:
#line 810 "Parser.y"
  { yyVal = new AddressOfOperation(((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 176:
#line 811 "Parser.y"
  { yyVal = new RefExpression(false, ((Expression)yyVals[0+yyTop]), ((TokenValue)yyVals[-1+yyTop])); }
  break;
case 177:
#line 812 "Parser.y"
  { yyVal = new RefExpression(true, ((Expression)yyVals[0+yyTop]), ((TokenValue)yyVals[-1+yyTop])); }
  break;
case 178:
#line 813 "Parser.y"
  { yyVal = new CastOperation(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 179:
#line 814 "Parser.y"
  { yyVal = ((Expression)yyVals[-1+yyTop]); }
  break;
case 180:
#line 815 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 181:
#line 818 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 182:
#line 819 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpMul, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 183:
#line 820 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpDiv, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 184:
#line 821 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpMod, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 185:
#line 824 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 186:
#line 825 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpAdd, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 187:
#line 826 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpSub, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 188:
#line 829 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 189:
#line 830 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpBitLeft, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 190:
#line 831 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpBitRight, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 191:
#line 834 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 192:
#line 835 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpLT, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 193:
#line 836 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpLEQ, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 194:
#line 837 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpGEQ, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 195:
#line 838 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpGT, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 196:
#line 839 "Parser.y"
  { yyVal = new IsExpression(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 197:
#line 840 "Parser.y"
  { yyVal = new AsExpression(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 198:
#line 843 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 199:
#line 844 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpEQ, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 200:
#line 845 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpNEQ, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 201:
#line 848 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 202:
#line 849 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpBitAnd, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 203:
#line 850 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpBitOr, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 204:
#line 851 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpBitXor, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 205:
#line 854 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 206:
#line 855 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpLAnd, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 207:
#line 858 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 208:
#line 859 "Parser.y"
  { yyVal = new BinaryOperation(BinaryOperation.OpLOr, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 209:
#line 862 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]);								   }
  break;
case 210:
#line 863 "Parser.y"
  { yyVal = new TernaryOperation(((Expression)yyVals[-4+yyTop]), ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 211:
#line 866 "Parser.y"
  { yyVal = new AssignmentExpression(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 212:
#line 867 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpAdd, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 213:
#line 868 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpSub, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 214:
#line 869 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpMul, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 215:
#line 870 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpDiv, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 216:
#line 871 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpMod, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 217:
#line 872 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitOr, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));    }
  break;
case 218:
#line 873 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitXor, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));   }
  break;
case 219:
#line 874 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitAnd, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));   }
  break;
case 220:
#line 875 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitLeft, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));  }
  break;
case 221:
#line 876 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitRight, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 222:
#line 879 "Parser.y"
  { yyVal = new AssignmentExpression(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 223:
#line 880 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpAdd, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 224:
#line 881 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpSub, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 225:
#line 882 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpMul, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 226:
#line 883 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpDiv, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 227:
#line 884 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpMod, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));      }
  break;
case 228:
#line 885 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitOr, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));    }
  break;
case 229:
#line 886 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitXor, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));   }
  break;
case 230:
#line 887 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitAnd, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));   }
  break;
case 231:
#line 888 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitLeft, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));  }
  break;
case 232:
#line 889 "Parser.y"
  { yyVal = new BinaryAssignOperation(BinaryOperation.OpBitRight, ((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 233:
#line 892 "Parser.y"
  { yyVal = new ExpressionStatement(((Expression)yyVals[-1+yyTop]), ((Expression)yyVals[-1+yyTop]).GetPosition()); }
  break;
case 234:
#line 895 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 235:
#line 898 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 236:
#line 899 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 237:
#line 904 "Parser.y"
  { yyVal = new BlockNode(((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop]));}
  break;
case 238:
#line 907 "Parser.y"
  { yyVal = new UnsafeBlockNode(((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 239:
#line 910 "Parser.y"
  { yyVal = null;			}
  break;
case 240:
#line 911 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]);				}
  break;
case 241:
#line 912 "Parser.y"
  { yyVal = AddList(((Expression)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop])); }
  break;
case 242:
#line 915 "Parser.y"
  { yyVal = new CallExpression(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 243:
#line 918 "Parser.y"
  { yyVal = new CallExpression(((Expression)yyVals[-3+yyTop]), ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 244:
#line 921 "Parser.y"
  { yyVal = new ExpressionStatement(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 245:
#line 924 "Parser.y"
  { yyVal = new ExpressionStatement(((Expression)yyVals[-1+yyTop]), ((Expression)yyVals[-1+yyTop]).GetPosition()); }
  break;
case 246:
#line 927 "Parser.y"
  { yyVal = new ReturnStatement(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 247:
#line 928 "Parser.y"
  { yyVal = new ReturnStatement(null, ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 248:
  case_248();
  break;
case 249:
#line 937 "Parser.y"
  { yyVal = new IfStatement(((Expression)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), null, ((TokenPosition)yyVals[-4+yyTop])); }
  break;
case 250:
#line 938 "Parser.y"
  { yyVal = new IfStatement(((Expression)yyVals[-4+yyTop]), ((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-6+yyTop])); }
  break;
case 251:
#line 941 "Parser.y"
  { yyVal = new CaseLabel(((Expression)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 252:
#line 942 "Parser.y"
  { yyVal = new CaseLabel(null, ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 253:
#line 945 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 254:
#line 946 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 255:
#line 949 "Parser.y"
  { yyVal = new SwitchStatement(((Expression)yyVals[-4+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-6+yyTop])); }
  break;
case 256:
#line 952 "Parser.y"
  { yyVal = new WhileStatement(((Expression)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-4+yyTop])); }
  break;
case 257:
#line 955 "Parser.y"
  { yyVal = new DoWhileStatement(((Expression)yyVals[-1+yyTop]), ((AstNode)yyVals[-4+yyTop]), ((TokenPosition)yyVals[-5+yyTop])); }
  break;
case 258:
#line 958 "Parser.y"
  { yyVal = null; }
  break;
case 259:
#line 959 "Parser.y"
  { yyVal = new ExpressionStatement(((Expression)yyVals[0+yyTop]), ((Expression)yyVals[0+yyTop]).GetPosition());; }
  break;
case 260:
#line 960 "Parser.y"
  { yyVal = new ExpressionStatement(((Expression)yyVals[0+yyTop]), ((Expression)yyVals[0+yyTop]).GetPosition());; }
  break;
case 261:
#line 961 "Parser.y"
  { yyVal = new ExpressionStatement(((Expression)yyVals[0+yyTop]), ((Expression)yyVals[0+yyTop]).GetPosition());; }
  break;
case 262:
#line 962 "Parser.y"
  { yyVal = new ExpressionStatement(((Expression)yyVals[0+yyTop]), ((Expression)yyVals[0+yyTop]).GetPosition());; }
  break;
case 263:
#line 965 "Parser.y"
  { yyVal = null; }
  break;
case 264:
#line 966 "Parser.y"
  { yyVal = ((Expression)yyVals[-1+yyTop]); }
  break;
case 265:
#line 969 "Parser.y"
  { yyVal = null; }
  break;
case 266:
#line 970 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 267:
#line 973 "Parser.y"
  { yyVal = new ForStatement(((AstNode)yyVals[-4+yyTop]), ((Expression)yyVals[-3+yyTop]), ((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-6+yyTop])); }
  break;
case 268:
#line 977 "Parser.y"
  { yyVal = new ForEachStatement(((Expression)yyVals[-5+yyTop]), (string)((TokenValue)yyVals[-4+yyTop]).GetValue(), ((Expression)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-7+yyTop])); }
  break;
case 269:
#line 980 "Parser.y"
  { yyVal = new BreakStatement(((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 270:
#line 983 "Parser.y"
  { yyVal = new ContinueStatement(((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 271:
#line 986 "Parser.y"
  { yyVal = new GotoCaseStatement(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-3+yyTop]));   }
  break;
case 272:
#line 987 "Parser.y"
  { yyVal = new GotoCaseStatement(null, ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 273:
#line 990 "Parser.y"
  { yyVal = new CatchStatement(((Expression)yyVals[-2+yyTop]), null, ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-4+yyTop])); }
  break;
case 274:
#line 991 "Parser.y"
  { yyVal = new CatchStatement(((Expression)yyVals[-3+yyTop]), (string)((TokenValue)yyVals[-2+yyTop]).GetValue(), ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-5+yyTop])); }
  break;
case 275:
#line 994 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 276:
#line 995 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 277:
#line 998 "Parser.y"
  { yyVal = new FinallyStatement(((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 278:
#line 1001 "Parser.y"
  { yyVal = new TryStatement(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 279:
#line 1002 "Parser.y"
  { yyVal = new TryStatement(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop]), null, ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 280:
#line 1003 "Parser.y"
  { yyVal = new TryStatement(((AstNode)yyVals[-1+yyTop]), null, ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 281:
#line 1006 "Parser.y"
  {yyVal = new VariableDeclaration((string)((TokenValue)yyVals[0+yyTop]).GetValue(), null, ((TokenValue)yyVals[0+yyTop])); }
  break;
case 282:
#line 1007 "Parser.y"
  {yyVal = new VariableDeclaration((string)((TokenValue)yyVals[-2+yyTop]).GetValue(), ((Expression)yyVals[0+yyTop]), ((TokenValue)yyVals[-2+yyTop])); }
  break;
case 283:
#line 1010 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]);				}
  break;
case 284:
#line 1011 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 285:
#line 1014 "Parser.y"
  { yyVal = new LocalVariablesDeclaration(((Expression)yyVals[-2+yyTop]), (VariableDeclaration)((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-2+yyTop]).GetPosition()); }
  break;
case 286:
#line 1017 "Parser.y"
  { yyVal = new LockStatement(((Expression)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-4+yyTop])); }
  break;
case 287:
#line 1020 "Parser.y"
  { yyVal = new DeleteStatement(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 288:
#line 1021 "Parser.y"
  { yyVal = new DeleteRawArrayStatement(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-4+yyTop])); }
  break;
case 289:
#line 1024 "Parser.y"
  { yyVal = new ThrowStatement(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 290:
  case_290();
  break;
case 291:
#line 1034 "Parser.y"
  { yyVal = new FixedVariableDecl((string)((TokenValue)yyVals[-2+yyTop]).GetValue(), ((Expression)yyVals[0+yyTop]), ((TokenValue)yyVals[-2+yyTop]));}
  break;
case 292:
#line 1037 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 293:
#line 1038 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 294:
#line 1042 "Parser.y"
  { yyVal = new FixedStatement(((Expression)yyVals[-3+yyTop]), ((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-5+yyTop])); }
  break;
case 295:
#line 1045 "Parser.y"
  { yyVal = new NullStatement(((TokenPosition)yyVals[0+yyTop])); }
  break;
case 296:
#line 1046 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 297:
#line 1047 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 298:
#line 1048 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 299:
#line 1049 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]);}
  break;
case 300:
#line 1050 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 301:
#line 1051 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 302:
#line 1052 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 303:
#line 1053 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 304:
#line 1054 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 305:
#line 1055 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 306:
#line 1056 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 307:
#line 1057 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 308:
#line 1058 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 309:
#line 1059 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 310:
#line 1060 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 311:
#line 1061 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 312:
#line 1062 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 313:
#line 1063 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 314:
#line 1064 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 315:
#line 1065 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 316:
#line 1066 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 317:
#line 1067 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 318:
#line 1068 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 319:
#line 1069 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 320:
#line 1072 "Parser.y"
  { yyVal = null; }
  break;
case 321:
#line 1073 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 322:
  case_322();
  break;
case 323:
#line 1091 "Parser.y"
  { yyVal = new AttributeArgument(((Expression)yyVals[0+yyTop]), (string)((TokenValue)yyVals[-2+yyTop]).GetValue(), ((TokenValue)yyVals[-2+yyTop])); }
  break;
case 324:
#line 1092 "Parser.y"
  { yyVal = new AttributeArgument(((Expression)yyVals[0+yyTop]), null, ((Expression)yyVals[0+yyTop]).GetPosition()); }
  break;
case 325:
#line 1095 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 326:
#line 1096 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 327:
#line 1099 "Parser.y"
  { yyVal = null; }
  break;
case 328:
#line 1100 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]);   }
  break;
case 329:
#line 1102 "Parser.y"
  { yyVal = new AttributeInstance(((Expression)yyVals[-3+yyTop]), (AttributeArgument)((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-3+yyTop]).GetPosition());   }
  break;
case 330:
#line 1103 "Parser.y"
  { yyVal = new AttributeInstance(((Expression)yyVals[0+yyTop]), null, ((Expression)yyVals[0+yyTop]).GetPosition()); }
  break;
case 331:
#line 1106 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 332:
#line 1107 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 333:
#line 1110 "Parser.y"
  { yyVal = new ArgTypeAndFlags(((Expression)yyVals[0+yyTop])); }
  break;
case 334:
#line 1111 "Parser.y"
  { yyVal = new ArgTypeAndFlags(new MakeReference(((Expression)yyVals[-1+yyTop]), ReferenceFlow.In,    true,  ((TokenPosition)yyVals[0+yyTop])), ((Expression)yyVals[-1+yyTop]).GetPosition()); }
  break;
case 335:
#line 1112 "Parser.y"
  { yyVal = new ArgTypeAndFlags(new MakeReference(((Expression)yyVals[0+yyTop]), ReferenceFlow.InOut, false, ((TokenValue)yyVals[-1+yyTop])), ((TokenValue)yyVals[-1+yyTop])); }
  break;
case 336:
#line 1113 "Parser.y"
  { yyVal = new ArgTypeAndFlags(new MakeReference(((Expression)yyVals[-1+yyTop]), ReferenceFlow.InOut, true,  ((TokenValue)yyVals[-2+yyTop])), ((TokenValue)yyVals[-2+yyTop])); }
  break;
case 337:
#line 1114 "Parser.y"
  { yyVal = new ArgTypeAndFlags(new MakeReference(((Expression)yyVals[0+yyTop]), ReferenceFlow.Out,   false, ((TokenValue)yyVals[-1+yyTop])), ((TokenValue)yyVals[-1+yyTop])); }
  break;
case 338:
#line 1115 "Parser.y"
  { yyVal = new ArgTypeAndFlags(new MakeReference(((Expression)yyVals[-1+yyTop]), ReferenceFlow.Out,   true,  ((TokenValue)yyVals[-2+yyTop])), ((TokenValue)yyVals[-2+yyTop])); }
  break;
case 339:
#line 1116 "Parser.y"
  { yyVal = new ArgTypeAndFlags(((Expression)yyVals[0+yyTop]), ((TokenValue)yyVals[-1+yyTop]), true); }
  break;
case 340:
  case_340();
  break;
case 341:
#line 1127 "Parser.y"
  { yyVal = new GenericParameter((string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenValue)yyVals[0+yyTop])); }
  break;
case 342:
#line 1130 "Parser.y"
  { yyVal = ((GenericParameter)yyVals[0+yyTop]); }
  break;
case 343:
#line 1131 "Parser.y"
  { yyVal = (GenericParameter)AddList(((GenericParameter)yyVals[-2+yyTop]), ((GenericParameter)yyVals[0+yyTop])); }
  break;
case 344:
#line 1134 "Parser.y"
  { yyVal = new ConstraintChain(false, false, ((Expression)yyVals[0+yyTop])); }
  break;
case 345:
#line 1135 "Parser.y"
  { yyVal = new ConstraintChain(true, false, null); }
  break;
case 346:
#line 1136 "Parser.y"
  { yyVal = new ConstraintChain(false, true, null); }
  break;
case 347:
#line 1139 "Parser.y"
  { yyVal = ((ConstraintChain)yyVals[0+yyTop]); }
  break;
case 348:
  case_348();
  break;
case 349:
  case_349();
  break;
case 350:
#line 1157 "Parser.y"
  { yyVal = null; }
  break;
case 351:
#line 1158 "Parser.y"
  { yyVal = AddList(((GenericConstraint)yyVals[-1+yyTop]), ((GenericConstraint)yyVals[0+yyTop])); }
  break;
case 352:
#line 1161 "Parser.y"
  { yyVal = null; }
  break;
case 353:
#line 1162 "Parser.y"
  { yyVal = new GenericSignature(((GenericParameter)yyVals[-1+yyTop]), null, ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 354:
#line 1165 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 355:
#line 1166 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop])); }
  break;
case 356:
#line 1170 "Parser.y"
  { yyVal = new FunctionArgument(((ArgTypeAndFlags)yyVals[0+yyTop]).type, ((ArgTypeAndFlags)yyVals[0+yyTop]).isParams, "", ((ArgTypeAndFlags)yyVals[0+yyTop])); }
  break;
case 357:
#line 1171 "Parser.y"
  { yyVal = new FunctionArgument(((ArgTypeAndFlags)yyVals[-1+yyTop]).type, ((ArgTypeAndFlags)yyVals[-1+yyTop]).isParams, (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((ArgTypeAndFlags)yyVals[-1+yyTop])); }
  break;
case 358:
#line 1174 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]);              }
  break;
case 359:
#line 1175 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 360:
#line 1178 "Parser.y"
  { yyVal = null; }
  break;
case 361:
#line 1179 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]);   }
  break;
case 362:
#line 1182 "Parser.y"
  { yyVal = "Op_Add"; }
  break;
case 363:
#line 1183 "Parser.y"
  { yyVal = "Op_Sub"; }
  break;
case 364:
#line 1184 "Parser.y"
  { yyVal = "Op_Mul"; }
  break;
case 365:
#line 1185 "Parser.y"
  { yyVal = "Op_Div"; }
  break;
case 366:
#line 1186 "Parser.y"
  { yyVal = "Op_Mod"; }
  break;
case 367:
#line 1187 "Parser.y"
  { yyVal = "Op_Lt";  }
  break;
case 368:
#line 1188 "Parser.y"
  { yyVal = "Op_Gt";  }
  break;
case 369:
#line 1189 "Parser.y"
  { yyVal = "Op_Leq"; }
  break;
case 370:
#line 1190 "Parser.y"
  { yyVal = "Op_Geq"; }
  break;
case 371:
#line 1191 "Parser.y"
  { yyVal = "Op_Eq";  }
  break;
case 372:
#line 1192 "Parser.y"
  { yyVal = "Op_Neq"; }
  break;
case 373:
#line 1193 "Parser.y"
  { yyVal = "Op_And"; }
  break;
case 374:
#line 1194 "Parser.y"
  { yyVal = "Op_Xor"; }
  break;
case 375:
#line 1195 "Parser.y"
  { yyVal = "Op_Or";  }
  break;
case 376:
#line 1196 "Parser.y"
  { yyVal = "Op_Not"; }
  break;
case 377:
#line 1199 "Parser.y"
  { yyVal = (string)((TokenValue)yyVals[0+yyTop]).GetValue(); }
  break;
case 378:
  case_378();
  break;
case 379:
  case_379();
  break;
case 380:
  case_380();
  break;
case 381:
  case_381();
  break;
case 382:
#line 1239 "Parser.y"
  { yyVal = new FunctionDefinition((FunctionPrototype)((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop]), ((AstNode)yyVals[-1+yyTop]).GetPosition()); }
  break;
case 383:
#line 1242 "Parser.y"
  { yyVal = null; }
  break;
case 384:
#line 1243 "Parser.y"
  { yyVal = new ConstructorInitializer(true, ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 385:
#line 1244 "Parser.y"
  { yyVal = new ConstructorInitializer(false, ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 386:
  case_386();
  break;
case 387:
  case_387();
  break;
case 388:
  case_388();
  break;
case 389:
  case_389();
  break;
case 390:
#line 1291 "Parser.y"
  { yyVal = new FunctionDefinition((FunctionPrototype)((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop]), ((AstNode)yyVals[-1+yyTop]).GetPosition()); }
  break;
case 391:
  case_391();
  break;
case 392:
#line 1305 "Parser.y"
  { yyVal = new FunctionDefinition((FunctionPrototype)((AstNode)yyVals[-1+yyTop]), null, ((AstNode)yyVals[-1+yyTop]).GetPosition());}
  break;
case 393:
#line 1308 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Public, MemberFlags.VisibilityMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 394:
#line 1309 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Internal, MemberFlags.VisibilityMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 395:
#line 1310 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Protected, MemberFlags.VisibilityMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 396:
#line 1311 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.ProtectedInternal, MemberFlags.VisibilityMask, ((TokenPosition)yyVals[-1+yyTop])); }
  break;
case 397:
#line 1312 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Private, MemberFlags.VisibilityMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 398:
#line 1315 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Cdecl,   MemberFlags.LanguageMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 399:
#line 1316 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.StdCall, MemberFlags.LanguageMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 400:
#line 1317 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.ApiCall, MemberFlags.LanguageMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 401:
#line 1318 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Kernel,  MemberFlags.LanguageMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 402:
#line 1321 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Static, MemberFlags.InstanceMask, ((TokenPosition)yyVals[0+yyTop]));   }
  break;
case 403:
#line 1322 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Virtual, MemberFlags.InstanceMask, ((TokenPosition)yyVals[0+yyTop]));  }
  break;
case 404:
#line 1323 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Override, MemberFlags.InstanceMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 405:
#line 1324 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Abstract, MemberFlags.InstanceMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 406:
#line 1327 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.External, MemberFlags.InstanceMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 407:
#line 1330 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.ReadOnly, MemberFlags.AccessMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 408:
#line 1333 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Unsafe, MemberFlags.SecurityMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 409:
#line 1336 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Partial, MemberFlags.ImplFlagMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 410:
#line 1339 "Parser.y"
  { yyVal = new MemberFlagsAndMask(MemberFlags.Sealed, MemberFlags.InheritanceMask, ((TokenPosition)yyVals[0+yyTop])); }
  break;
case 411:
#line 1342 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 412:
#line 1343 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 413:
#line 1344 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 414:
#line 1345 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 415:
#line 1346 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 416:
#line 1347 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 417:
#line 1348 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 418:
#line 1349 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 419:
#line 1352 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 420:
  case_420();
  break;
case 421:
#line 1361 "Parser.y"
  { yyVal = MemberFlags.Default; }
  break;
case 422:
#line 1362 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]).flags; }
  break;
case 423:
  case_423();
  break;
case 424:
#line 1373 "Parser.y"
  { yyVal = ((MemberFlags)yyVals[0+yyTop]); }
  break;
case 425:
#line 1376 "Parser.y"
  { yyVal = new FieldDeclaration((string)((TokenValue)yyVals[-2+yyTop]).GetValue(), ((Expression)yyVals[0+yyTop]), ((TokenValue)yyVals[-2+yyTop])); }
  break;
case 426:
#line 1377 "Parser.y"
  { yyVal = new FieldDeclaration((string)((TokenValue)yyVals[0+yyTop]).GetValue(), null, ((TokenValue)yyVals[0+yyTop])); }
  break;
case 427:
#line 1380 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 428:
#line 1381 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 429:
#line 1384 "Parser.y"
  { yyVal = new FieldDefinition(((MemberFlags)yyVals[-3+yyTop]), ((Expression)yyVals[-2+yyTop]), (FieldDeclaration)((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-2+yyTop]).GetPosition()); }
  break;
case 430:
#line 1387 "Parser.y"
  { yyVal = MemberFlags.ImplicitVis; }
  break;
case 431:
#line 1388 "Parser.y"
  { yyVal = ((MemberFlagsAndMask)yyVals[0+yyTop]); }
  break;
case 432:
  case_432();
  break;
case 433:
  case_433();
  break;
case 434:
#line 1413 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 435:
#line 1414 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 436:
#line 1417 "Parser.y"
  { yyVal = new FunctionArgument(((Expression)yyVals[-1+yyTop]), (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((Expression)yyVals[-1+yyTop]).GetPosition()); }
  break;
case 437:
#line 1420 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 438:
#line 1421 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]));}
  break;
case 439:
  case_439();
  break;
case 440:
  case_440();
  break;
case 441:
#line 1440 "Parser.y"
  { yyVal = new PropertyDefinition(((MemberFlags)yyVals[-8+yyTop]), ((Expression)yyVals[-7+yyTop]), "this", ((AstNode)yyVals[-4+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-7+yyTop]).GetPosition()); }
  break;
case 442:
#line 1444 "Parser.y"
  { yyVal = new GetAccessorDefinition(((MemberFlags)yyVals[-2+yyTop]), null, ((TokenValue)yyVals[-1+yyTop]));}
  break;
case 443:
  case_443();
  break;
case 444:
#line 1456 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 445:
#line 1457 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 446:
#line 1461 "Parser.y"
  { yyVal = new PropertyDefinition(MemberFlags.InterfaceMember, ((Expression)yyVals[-4+yyTop]), (string)((TokenValue)yyVals[-3+yyTop]).GetValue(), null, ((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-4+yyTop]).GetPosition()); }
  break;
case 447:
#line 1463 "Parser.y"
  { yyVal = new PropertyDefinition(MemberFlags.InterfaceMember, ((Expression)yyVals[-7+yyTop]), "this", ((AstNode)yyVals[-4+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-7+yyTop]).GetPosition()); }
  break;
case 448:
#line 1466 "Parser.y"
  { yyVal = new EventAccessorDefinition(((MemberFlags)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[-1+yyTop]).GetValue(), ((AstNode)yyVals[0+yyTop]), ((TokenValue)yyVals[-1+yyTop])); }
  break;
case 449:
#line 1467 "Parser.y"
  { yyVal = new EventAccessorDefinition(((MemberFlags)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[-1+yyTop]).GetValue(), null, ((TokenValue)yyVals[-1+yyTop]));}
  break;
case 450:
#line 1470 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 451:
#line 1471 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 452:
  case_452();
  break;
case 453:
#line 1484 "Parser.y"
  {
                    yyVal = new EventDefinition(((MemberFlags)yyVals[-4+yyTop]), ((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[-1+yyTop]).GetValue(), null, ((TokenPosition)yyVals[-3+yyTop]));
                  }
  break;
case 454:
#line 1487 "Parser.y"
  { yyVal = new EventAccessorDefinition(((MemberFlags)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[-1+yyTop]).GetValue(), null, ((TokenValue)yyVals[-1+yyTop]));}
  break;
case 455:
#line 1490 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 456:
#line 1491 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 457:
  case_457();
  break;
case 458:
#line 1504 "Parser.y"
  {
                    yyVal = new EventDefinition(MemberFlags.InterfaceMember, ((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[-1+yyTop]).GetValue(), null, ((TokenPosition)yyVals[-3+yyTop]));
                  }
  break;
case 459:
#line 1507 "Parser.y"
  { yyVal = ((AstNode)yyVals[-1+yyTop]); }
  break;
case 460:
#line 1508 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 461:
#line 1509 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 462:
#line 1510 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 463:
#line 1511 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 464:
#line 1512 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 465:
#line 1513 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 466:
#line 1514 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 467:
#line 1515 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 468:
#line 1516 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 469:
#line 1517 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 470:
#line 1518 "Parser.y"
  { yyVal = null; }
  break;
case 471:
#line 1521 "Parser.y"
  { ((AstNode)yyVals[0+yyTop]).SetAttributes(((AstNode)yyVals[-2+yyTop])); yyVal = ((AstNode)yyVals[0+yyTop]);}
  break;
case 472:
#line 1522 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 473:
#line 1525 "Parser.y"
  { yyVal = null;			}
  break;
case 474:
#line 1526 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 475:
#line 1529 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 476:
#line 1530 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 477:
#line 1531 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 478:
#line 1534 "Parser.y"
  { yyVal = null; }
  break;
case 479:
#line 1535 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 480:
#line 1538 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]);				}
  break;
case 481:
#line 1539 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((Expression)yyVals[0+yyTop])); }
  break;
case 482:
#line 1542 "Parser.y"
  { yyVal = null; }
  break;
case 483:
#line 1543 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]);	 }
  break;
case 484:
  case_484();
  break;
case 485:
  case_485();
  break;
case 486:
  case_486();
  break;
case 487:
  case_487();
  break;
case 488:
#line 1584 "Parser.y"
  { yyVal = new EnumConstantDefinition((string)((TokenValue)yyVals[0+yyTop]).GetValue(), null, ((TokenValue)yyVals[0+yyTop])); }
  break;
case 489:
#line 1585 "Parser.y"
  { yyVal = new EnumConstantDefinition((string)((TokenValue)yyVals[-2+yyTop]).GetValue(), ((Expression)yyVals[0+yyTop]), ((TokenValue)yyVals[-2+yyTop]));   }
  break;
case 490:
#line 1588 "Parser.y"
  { yyVal = null; }
  break;
case 491:
#line 1589 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 492:
#line 1590 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 493:
#line 1591 "Parser.y"
  { yyVal = ((AstNode)yyVals[-1+yyTop]); }
  break;
case 494:
#line 1594 "Parser.y"
  { yyVal = null; }
  break;
case 495:
#line 1595 "Parser.y"
  { yyVal = ((Expression)yyVals[0+yyTop]); }
  break;
case 496:
#line 1601 "Parser.y"
  {
                    yyVal = new EnumDefinition(((MemberFlags)yyVals[-6+yyTop]), (string)((TokenValue)yyVals[-4+yyTop]).GetValue(), ((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-3+yyTop]), ((TokenPosition)yyVals[-5+yyTop]));
               }
  break;
case 497:
  case_497();
  break;
case 498:
#line 1613 "Parser.y"
  { yyVal = new FieldDeclaration((string)((TokenValue)yyVals[-2+yyTop]).GetValue(), ((Expression)yyVals[0+yyTop]), ((TokenValue)yyVals[-2+yyTop])); }
  break;
case 499:
#line 1614 "Parser.y"
  { yyVal = new FieldDeclaration((string)((TokenValue)yyVals[0+yyTop]).GetValue(), null, ((TokenValue)yyVals[0+yyTop])); }
  break;
case 500:
#line 1617 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 501:
#line 1618 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 502:
#line 1621 "Parser.y"
  { yyVal = new TypedefDefinition(((MemberFlags)yyVals[-4+yyTop]), ((Expression)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[-1+yyTop]).GetValue(), ((TokenPosition)yyVals[-3+yyTop])); }
  break;
case 503:
#line 1624 "Parser.y"
  { yyVal = new UsingStatement(((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop])); }
  break;
case 504:
#line 1625 "Parser.y"
  { yyVal = new AliasDeclaration((string)((TokenValue)yyVals[-3+yyTop]).GetValue(), ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-4+yyTop])); }
  break;
case 505:
#line 1629 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 506:
#line 1630 "Parser.y"
  { yyVal = ((AstNode)yyVals[-1+yyTop]); }
  break;
case 507:
#line 1631 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 508:
#line 1632 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 509:
#line 1633 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 510:
#line 1634 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 511:
#line 1635 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 512:
#line 1636 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 513:
#line 1637 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 514:
#line 1638 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 515:
#line 1639 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 516:
#line 1640 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 517:
#line 1641 "Parser.y"
  { yyVal = null; }
  break;
case 518:
#line 1644 "Parser.y"
  { ((AstNode)yyVals[0+yyTop]).SetAttributes(((AstNode)yyVals[-2+yyTop])); yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 519:
#line 1645 "Parser.y"
  { yyVal = ((AstNode)yyVals[0+yyTop]); }
  break;
case 520:
#line 1648 "Parser.y"
  { yyVal = null; }
  break;
case 521:
#line 1649 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 522:
#line 1652 "Parser.y"
  { yyVal = (string)((TokenValue)yyVals[0+yyTop]).GetValue(); }
  break;
case 523:
#line 1653 "Parser.y"
  { yyVal = ((string)yyVals[-2+yyTop]) + "." + (string)((TokenValue)yyVals[0+yyTop]).GetValue(); }
  break;
case 524:
#line 1656 "Parser.y"
  {yyVal = new NamespaceDefinition(((string)yyVals[-3+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-4+yyTop])); }
  break;
case 525:
#line 1659 "Parser.y"
  { yyVal = null;}
  break;
case 526:
#line 1660 "Parser.y"
  { yyVal = AddList(((AstNode)yyVals[-1+yyTop]), ((AstNode)yyVals[0+yyTop])); }
  break;
case 527:
#line 1663 "Parser.y"
  { yyVal = ((AstNode)yyVals[-1+yyTop]); }
  break;
#line default
        }
        yyTop -= yyLen[yyN];
        yyState = yyStates[yyTop];
        int yyM = yyLhs[yyN];
        if (yyState == 0 && yyM == 0) {
//t          if (debug != null) debug.shift(0, yyFinal);
          yyState = yyFinal;
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
//t            if (debug != null)
//t               debug.lex(yyState, yyToken,yyname(yyToken), yyLex.value());
          }
          if (yyToken == 0) {
//t            if (debug != null) debug.accept(yyVal);
            return yyVal;
          }
          goto continue_yyLoop;
        }
        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
//t        if (debug != null) debug.shift(yyStates[yyTop], yyState);
	 goto continue_yyLoop;
      continue_yyDiscarded: ;	// implements the named-loop continue: 'continue yyDiscarded'
      }
    continue_yyLoop: ;		// implements the named-loop continue: 'continue yyLoop'
    }
  }

/*
 All more than 3 lines long rules are wrapped into a method
*/
void case_136()
#line 742 "Parser.y"
{
                    Expression expr = new PostfixOperation(PostfixOperation.Increment, ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[0+yyTop]));
                    yyVal = new ExpressionStatement(expr, ((TokenPosition)yyVals[0+yyTop]));
                 }

void case_137()
#line 747 "Parser.y"
{
                    Expression expr = new PostfixOperation(PostfixOperation.Decrement, ((Expression)yyVals[-1+yyTop]), ((TokenPosition)yyVals[0+yyTop]));
                    yyVal = new ExpressionStatement(expr, ((TokenPosition)yyVals[0+yyTop]));
                 }

void case_248()
#line 930 "Parser.y"
{
                    if(!((TokenValue)yyVals[-3+yyTop]).GetValue().Equals("yield"))
                        Error(((TokenValue)yyVals[-3+yyTop]), "expected yield identifier before return.");
                    yyVal = new ReturnStatement(((Expression)yyVals[-1+yyTop]), true, ((TokenValue)yyVals[-3+yyTop]));
                }

void case_290()
#line 1028 "Parser.y"
{
                        LocalVariablesDeclaration decls = new LocalVariablesDeclaration(((Expression)yyVals[-3+yyTop]), (VariableDeclaration)((AstNode)yyVals[-2+yyTop]), ((Expression)yyVals[-3+yyTop]).GetPosition());
                        yyVal = new UsingObjectStatement(decls, ((AstNode)yyVals[0+yyTop]), ((TokenPosition)yyVals[-5+yyTop]));
                      }

void case_322()
#line 1077 "Parser.y"
{
                   if(((AstNode)yyVals[0+yyTop]) is BlockNode)
                   {
                       BlockNode block = (BlockNode) ((AstNode)yyVals[0+yyTop]);
                       yyVal = block.GetChildren();
                   }
                   else
                   {
                       yyVal = ((AstNode)yyVals[0+yyTop]);
                   }
                   yyVal = ((AstNode)yyVals[0+yyTop]);
               }

void case_340()
#line 1118 "Parser.y"
{
                    MakeArray makeArray = ((Expression)yyVals[0+yyTop]) as MakeArray;
                    if(makeArray == null)
                        Error(((TokenPosition)yyVals[-1+yyTop]), "only array arguments can be readonly.");
                    makeArray.IsReadOnly = true;
                    yyVal = new ArgTypeAndFlags(makeArray, ((TokenPosition)yyVals[-1+yyTop]));
                  }

void case_348()
#line 1141 "Parser.y"
{
                            /* The order doesn't matter.*/
                            ConstraintChain chain = ((ConstraintChain)yyVals[0+yyTop]);
                            chain.next = ((ConstraintChain)yyVals[-2+yyTop]);
                            yyVal = chain;
                        }

void case_349()
#line 1150 "Parser.y"
{
                      if((string)((TokenValue)yyVals[-3+yyTop]).GetValue() != "where")
                          Error(((TokenValue)yyVals[-3+yyTop]), "expected generic constraint");
                      yyVal = BuildConstraintFromChain((string)((TokenValue)yyVals[-2+yyTop]).GetValue(), ((ConstraintChain)yyVals[0+yyTop]), ((TokenValue)yyVals[-3+yyTop]));
                  }

void case_378()
#line 1201 "Parser.y"
{
               Expression baseExpr = null;
               if(((object)yyVals[-2+yyTop]) is Expression)
                   baseExpr = (Expression)((object)yyVals[-2+yyTop]);
               else
                   baseExpr = new VariableReference((string)((object)yyVals[-2+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));
               yyVal = new MemberAccess(baseExpr, (string)((TokenValue)yyVals[0+yyTop]).GetValue(), ((TokenPosition)yyVals[-1+yyTop]));
           }

void case_379()
#line 1210 "Parser.y"
{
               Expression baseExpr = null;
               if(((object)yyVals[-2+yyTop]) is Expression)
                   baseExpr = (Expression)((object)yyVals[-2+yyTop]);
               else
                   baseExpr = new VariableReference((string)((object)yyVals[-2+yyTop]), ((TokenPosition)yyVals[-1+yyTop]));
               yyVal = new MemberAccess(baseExpr, "Op_Index", ((TokenPosition)yyVals[-1+yyTop]));
           }

void case_380()
#line 1219 "Parser.y"
{
               Expression baseExpr = null;
               if(((object)yyVals[-3+yyTop]) is Expression)
                   baseExpr = (Expression)((object)yyVals[-3+yyTop]);
               else
                   baseExpr = new VariableReference((string)((object)yyVals[-3+yyTop]), ((TokenPosition)yyVals[-2+yyTop]));
               yyVal = new GenericInstanceExpr(baseExpr, ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-2+yyTop]));
           }

void case_381()
#line 1231 "Parser.y"
{
                    GenericSignature genSign = ((GenericSignature)yyVals[-4+yyTop]);
                    if(genSign != null)
                        genSign.SetConstraints(((GenericConstraint)yyVals[0+yyTop]));
                    yyVal = new FunctionPrototype(((MemberFlags)yyVals[-7+yyTop]), ((Expression)yyVals[-6+yyTop]), (FunctionArgument)((AstNode)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[-5+yyTop]).GetValue(), ((GenericSignature)yyVals[-4+yyTop]), ((Expression)yyVals[-6+yyTop]).GetPosition());
                  }

void case_386()
#line 1249 "Parser.y"
{
                    GenericSignature genSign = ((GenericSignature)yyVals[-4+yyTop]);
                    if(genSign != null)
                        genSign.SetConstraints(((GenericConstraint)yyVals[0+yyTop]));
                    yyVal = new FunctionPrototype(((MemberFlags)yyVals[-7+yyTop]), ((Expression)yyVals[-6+yyTop]), (FunctionArgument)((AstNode)yyVals[-2+yyTop]), ((string)yyVals[-5+yyTop]), ((GenericSignature)yyVals[-4+yyTop]), ((Expression)yyVals[-6+yyTop]).GetPosition());
                }

void case_387()
#line 1257 "Parser.y"
{
                    GenericSignature genSign = ((GenericSignature)yyVals[-4+yyTop]);
                    if(genSign != null)
                        genSign.SetConstraints(((GenericConstraint)yyVals[0+yyTop]));
                    if(((object)yyVals[-5+yyTop]) is string)
                        yyVal = new FunctionPrototype(((MemberFlags)yyVals[-7+yyTop]), ((Expression)yyVals[-6+yyTop]), (FunctionArgument)((AstNode)yyVals[-2+yyTop]), (string)((object)yyVals[-5+yyTop]), ((GenericSignature)yyVals[-4+yyTop]), ((Expression)yyVals[-6+yyTop]).GetPosition());
                    else
                        yyVal = new FunctionPrototype(((MemberFlags)yyVals[-7+yyTop]), ((Expression)yyVals[-6+yyTop]), (FunctionArgument)((AstNode)yyVals[-2+yyTop]), (Expression)((object)yyVals[-5+yyTop]), ((GenericSignature)yyVals[-4+yyTop]), ((Expression)yyVals[-6+yyTop]).GetPosition());
                }

void case_388()
#line 1267 "Parser.y"
{
					MemberFlags instance = (((MemberFlags)yyVals[-5+yyTop]) & MemberFlags.InstanceMask);
					if(instance != 0 && instance != MemberFlags.Static)
						Error(((TokenValue)yyVals[-4+yyTop]), "constructors cannot be virtual/override/abstract.");
                    MemberFlags flags = ((MemberFlags)yyVals[-5+yyTop]) & ~MemberFlags.InstanceMask;
                    if(instance == MemberFlags.Static)
                        flags |= MemberFlags.StaticConstructor;
                    else
                        flags |= MemberFlags.Constructor;

					FunctionPrototype proto = new FunctionPrototype(flags, null,
						(FunctionArgument)((AstNode)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[-4+yyTop]).GetValue(), ((TokenValue)yyVals[-4+yyTop]));
                    proto.SetConstructorInitializer((ConstructorInitializer)((AstNode)yyVals[0+yyTop]));
                    yyVal = proto;
				}

void case_389()
#line 1283 "Parser.y"
{
                    FunctionPrototype proto = new FunctionPrototype(MemberFlags.Protected | MemberFlags.Override,
                                                    null, null, "Finalize", ((TokenPosition)yyVals[-3+yyTop]));
                    proto.SetDestructorName((string)((TokenValue)yyVals[-2+yyTop]).GetValue());
                    yyVal = proto;
                }

void case_391()
#line 1296 "Parser.y"
{
                          GenericSignature genSign = ((GenericSignature)yyVals[-4+yyTop]);
                          if(genSign != null)
                              genSign.SetConstraints(((GenericConstraint)yyVals[0+yyTop]));

                          yyVal = new FunctionPrototype(MemberFlags.InterfaceMember, ((Expression)yyVals[-6+yyTop]), (FunctionArgument)((AstNode)yyVals[-2+yyTop]), (string)((TokenValue)yyVals[-5+yyTop]).GetValue(), ((GenericSignature)yyVals[-4+yyTop]), ((Expression)yyVals[-6+yyTop]).GetPosition());
                      }

void case_420()
#line 1354 "Parser.y"
{
                    if((((MemberFlagsAndMask)yyVals[-1+yyTop]).flags & ((MemberFlagsAndMask)yyVals[0+yyTop]).mask) != MemberFlags.Default)
                        Error(((MemberFlagsAndMask)yyVals[0+yyTop]), "incompatible member flag.");
                    yyVal = new MemberFlagsAndMask(((MemberFlagsAndMask)yyVals[-1+yyTop]).flags | ((MemberFlagsAndMask)yyVals[0+yyTop]).flags, ((MemberFlagsAndMask)yyVals[-1+yyTop]).mask | ((MemberFlagsAndMask)yyVals[0+yyTop]).mask, ((MemberFlagsAndMask)yyVals[-1+yyTop]));
                }

void case_423()
#line 1366 "Parser.y"
{
                if((((MemberFlags)yyVals[0+yyTop]) & MemberFlags.InstanceMask) != MemberFlags.Default)
                    Error("invalid global member flags");
                yyVal = ((MemberFlags)yyVals[0+yyTop]) | MemberFlags.Static;
            }

void case_432()
#line 1392 "Parser.y"
{
                     string name = (string)((TokenValue)yyVals[-1+yyTop]).GetValue();
                     if(name == "get")
                         yyVal = new GetAccessorDefinition(((MemberFlags)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenValue)yyVals[-1+yyTop]));
                     else if(name == "set")
                         yyVal = new SetAccessorDefinition(((MemberFlags)yyVals[-2+yyTop]), ((AstNode)yyVals[0+yyTop]), ((TokenValue)yyVals[-1+yyTop]));
                     else
                         Error(((TokenValue)yyVals[-1+yyTop]), "only get and set property accessors are supported.");
                   }

void case_433()
#line 1402 "Parser.y"
{
                     string name = (string)((TokenValue)yyVals[-1+yyTop]).GetValue();
                     if(name == "get")
                         yyVal = new GetAccessorDefinition(((MemberFlags)yyVals[-2+yyTop]), null, ((TokenValue)yyVals[-1+yyTop]));
                     else if(name == "set")
                         yyVal = new SetAccessorDefinition(((MemberFlags)yyVals[-2+yyTop]), null, ((TokenValue)yyVals[-1+yyTop]));
                     else
                         Error(((TokenValue)yyVals[-1+yyTop]), "only get and set property accessors are supported.");
                   }

void case_439()
#line 1425 "Parser.y"
{
                       if(((object)yyVals[-3+yyTop]) is string)
                           yyVal = new PropertyDefinition(((MemberFlags)yyVals[-5+yyTop]), ((Expression)yyVals[-4+yyTop]), (string)((object)yyVals[-3+yyTop]), null, ((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-4+yyTop]).GetPosition());
                       else
                           yyVal = new PropertyDefinition(((MemberFlags)yyVals[-5+yyTop]), ((Expression)yyVals[-4+yyTop]), (Expression)((object)yyVals[-3+yyTop]), null, ((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-4+yyTop]).GetPosition());
                   }

void case_440()
#line 1433 "Parser.y"
{
                       if(((object)yyVals[-6+yyTop]) is string)
                           yyVal = new PropertyDefinition(((MemberFlags)yyVals[-8+yyTop]), ((Expression)yyVals[-7+yyTop]), (string)((object)yyVals[-6+yyTop]), ((AstNode)yyVals[-4+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-7+yyTop]).GetPosition());
                       else
                           yyVal = new PropertyDefinition(((MemberFlags)yyVals[-8+yyTop]), ((Expression)yyVals[-7+yyTop]), (Expression)((object)yyVals[-6+yyTop]), ((AstNode)yyVals[-4+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-7+yyTop]).GetPosition());
                   }

void case_443()
#line 1445 "Parser.y"
{
                           string name = (string)((TokenValue)yyVals[-2+yyTop]).GetValue();
                           if(name == "get")
                               yyVal = new GetAccessorDefinition(((MemberFlags)yyVals[-3+yyTop]), null, ((TokenValue)yyVals[-2+yyTop]));
                           else if(name == "set")
                               yyVal = new SetAccessorDefinition(((MemberFlags)yyVals[-3+yyTop]), null, ((TokenValue)yyVals[-2+yyTop]));
                           else
                               Error(((TokenValue)yyVals[-2+yyTop]), "only get and set property accessors are supported.");
                         }

void case_452()
#line 1475 "Parser.y"
{
                    if(((TokenPosition)yyVals[-2+yyTop]) == null)
                        Error(((TokenPosition)yyVals[-5+yyTop]), "explicit event definition body cannot be empty.");
                    yyVal = new EventDefinition(((MemberFlags)yyVals[-6+yyTop]), ((Expression)yyVals[-4+yyTop]), (string)((TokenValue)yyVals[-3+yyTop]).GetValue(), ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-5+yyTop]));
                  }

void case_457()
#line 1495 "Parser.y"
{
                    if(((TokenPosition)yyVals[-2+yyTop]) == null)
                        Error(((TokenPosition)yyVals[-5+yyTop]), "explicit event definition body cannot be empty.");
                    yyVal = new EventDefinition(MemberFlags.InterfaceMember, ((Expression)yyVals[-4+yyTop]), (string)((TokenValue)yyVals[-3+yyTop]).GetValue(), ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-5+yyTop]));
                  }

void case_484()
#line 1548 "Parser.y"
{
                      GenericSignature genSign = ((GenericSignature)yyVals[-5+yyTop]);
                      if(genSign != null)
                        genSign.SetConstraints(((GenericConstraint)yyVals[-3+yyTop]));
					  yyVal = new ClassDefinition(((MemberFlags)yyVals[-8+yyTop]), (string)((TokenValue)yyVals[-6+yyTop]).GetValue(), genSign, ((AstNode)yyVals[-4+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-7+yyTop]));
					}

void case_485()
#line 1558 "Parser.y"
{
                      GenericSignature genSign = ((GenericSignature)yyVals[-5+yyTop]);
                      if(genSign != null)
                        genSign.SetConstraints(((GenericConstraint)yyVals[-3+yyTop]));
					  yyVal = new StructDefinition(((MemberFlags)yyVals[-8+yyTop]), (string)((TokenValue)yyVals[-6+yyTop]).GetValue(), genSign, ((AstNode)yyVals[-4+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-7+yyTop]));
					}

void case_486()
#line 1567 "Parser.y"
{
                        GenericSignature genSign = ((GenericSignature)yyVals[-5+yyTop]);
                        if(genSign != null)
                          genSign.SetConstraints(((GenericConstraint)yyVals[-3+yyTop]));
                        yyVal = new InterfaceDefinition(((MemberFlags)yyVals[-8+yyTop]), (string)((TokenValue)yyVals[-6+yyTop]).GetValue(), genSign, ((AstNode)yyVals[-4+yyTop]), ((AstNode)yyVals[-1+yyTop]), ((TokenPosition)yyVals[-7+yyTop]));
                    }

void case_487()
#line 1576 "Parser.y"
{
                        GenericSignature genSign = ((GenericSignature)yyVals[-5+yyTop]);
                        if(genSign != null)
                          genSign.SetConstraints(((GenericConstraint)yyVals[-1+yyTop]));
                        yyVal = new DelegateDefinition(((MemberFlags)yyVals[-9+yyTop]), ((Expression)yyVals[-7+yyTop]), ((AstNode)yyVals[-3+yyTop]), (string)((TokenValue)yyVals[-6+yyTop]).GetValue(), genSign, ((TokenPosition)yyVals[-8+yyTop]));
                    }

void case_497()
#line 1605 "Parser.y"
{
                    MemberFlags instance = (((MemberFlags)yyVals[-3+yyTop]) & MemberFlags.InstanceMask);
                    if(instance != MemberFlags.Static)
                        Error(((Expression)yyVals[-2+yyTop]).GetPosition(), "global variables must be static.");
                     yyVal = new FieldDefinition(((MemberFlags)yyVals[-3+yyTop]), ((Expression)yyVals[-2+yyTop]), (FieldDeclaration)((AstNode)yyVals[-1+yyTop]), ((Expression)yyVals[-2+yyTop]).GetPosition());
                   }

#line default
   static readonly short [] yyLhs  = {              -1,
    8,    8,    8,    8,    8,    8,    8,    8,    8,    8,
    8,    8,    8,    8,    8,    8,    8,    8,    8,    8,
    8,    8,    8,    8,    8,    8,    8,    8,    8,    8,
    8,    8,    8,    8,    8,    8,    8,    8,    8,    8,
    8,    8,    8,    8,    8,    8,    8,    8,    8,    8,
    8,    8,    8,    8,    8,    8,    8,   28,   28,   20,
   47,   47,   47,   27,   27,   27,   27,   27,   27,   27,
   27,   27,   27,   27,   27,   27,   38,   38,   39,   39,
   40,   40,   14,   31,   31,   31,   31,   31,   31,   17,
   17,   18,   18,   18,   18,   68,   68,  174,  174,   23,
   23,   23,   23,   22,   22,   22,   21,   69,   69,   25,
   25,   24,   24,   24,    2,    2,   51,   51,   50,   50,
   50,   26,   26,   26,   26,    1,    1,    1,    1,    1,
    1,   49,   49,   30,   30,  127,  127,   29,   29,  126,
   32,   32,   32,   32,   32,   32,   32,   32,   32,   32,
   32,   32,   32,   32,   32,   32,   37,   46,    9,   33,
   33,   34,  163,  163,   45,   45,   45,   45,   45,   41,
   41,   41,   41,   41,   41,   41,   41,   41,   41,   41,
   13,   13,   13,   13,   44,   44,   44,   36,   36,   36,
   35,   35,   35,   35,   35,   35,   35,   10,   10,   10,
    5,    5,    5,    5,   15,   15,   16,   16,   43,   43,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    3,
    3,    4,    4,    4,    4,    4,    4,    4,    4,    4,
    4,    4,   57,   42,   11,   11,   59,  143,    7,    7,
    7,    6,   19,   61,  124,  131,  131,  131,  102,  102,
  137,  137,  138,  138,  139,  146,   74,   83,   83,   83,
   83,   83,   48,   48,   85,   85,   84,   82,   60,   70,
   92,   92,   62,   62,   63,   63,   78,  140,  140,  140,
  147,  147,  148,  148,  117,  116,   73,   73,  141,  145,
   79,   80,   80,   81,  132,  132,  132,  132,  132,  132,
  132,  132,  132,  132,  132,  132,  132,  132,  132,  132,
  132,  132,  132,  132,  132,  132,  132,  132,  132,  133,
  133,  134,   53,   53,   52,   52,   54,   54,   55,   55,
   56,   56,  175,  175,  175,  175,  175,  175,  175,  175,
  149,  150,  150,  151,  151,  151,  152,  152,  153,  154,
  154,  155,  155,   91,   91,   88,   88,   86,   86,   87,
   87,  158,  158,  158,  158,  158,  158,  158,  158,  158,
  158,  158,  158,  158,  158,  158,  156,  156,  156,  156,
   90,   89,   71,   71,   71,  119,  119,  119,  119,  118,
  106,  107,  164,  164,  164,  164,  164,  165,  165,  165,
  165,  166,  166,  166,  166,  167,  168,  169,  170,  171,
  172,  172,  172,  172,  172,  172,  172,  172,  173,  173,
  161,  161,  159,  160,   96,   96,   97,   97,   98,  162,
  162,  128,  128,  129,  129,  111,  112,  112,  130,  130,
  130,  176,  108,  109,  109,  110,  110,   93,   93,   94,
   94,   95,   95,  103,  104,  104,  105,  105,   65,   65,
   65,   65,   65,   65,   65,   65,   65,   65,   65,   65,
   64,   64,   66,   66,  115,  115,  115,  114,  114,   58,
   58,  125,  125,   67,  135,  113,   72,   75,   75,   76,
   76,   76,   76,   12,   12,   77,  101,   99,   99,  100,
  100,  136,  144,  144,  123,  123,  123,  123,  123,  123,
  123,  123,  123,  123,  123,  123,  123,  120,  120,  121,
  121,  157,  157,  122,  142,  142,    0,
  };
   static readonly short [] yyLen = {           2,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    3,    1,    3,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    3,
    4,    4,    3,    1,    1,    4,    4,    3,    3,    1,
    2,    1,    3,    3,    1,    1,    2,    0,    2,    1,
    3,    2,    6,    1,    4,    4,    1,    1,    2,    1,
    2,    0,    1,    3,    1,    3,    1,    3,    0,    1,
    2,    5,    5,    8,    8,    2,    5,    5,    2,    5,
    5,    4,    4,    2,    2,    2,    2,    2,    2,    2,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    3,    4,    4,    4,    1,
    2,    7,    0,    1,    1,    3,    2,    3,    6,    1,
    2,    2,    2,    2,    2,    2,    2,    4,    3,    1,
    1,    3,    3,    3,    1,    3,    3,    1,    3,    3,
    1,    3,    3,    3,    3,    3,    3,    1,    3,    3,
    1,    3,    3,    3,    1,    3,    1,    3,    1,    5,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    3,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    3,
    3,    3,    2,    1,    1,    1,    3,    4,    0,    1,
    3,    4,    4,    2,    2,    3,    2,    4,    5,    7,
    4,    3,    1,    2,    7,    5,    6,    0,    1,    1,
    1,    1,    1,    2,    1,    1,    7,    8,    2,    2,
    4,    3,    5,    6,    1,    2,    2,    4,    3,    3,
    1,    3,    1,    3,    3,    5,    3,    5,    3,    6,
    3,    1,    3,    6,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    0,
    2,    1,    3,    1,    1,    3,    0,    1,    4,    1,
    1,    3,    1,    2,    2,    3,    2,    3,    2,    2,
    1,    1,    3,    1,    1,    3,    1,    3,    4,    0,
    2,    0,    3,    1,    3,    1,    2,    1,    3,    0,
    1,    2,    2,    2,    2,    2,    2,    2,    2,    2,
    2,    2,    2,    2,    2,    2,    1,    3,    3,    4,
    8,    2,    0,    5,    5,    8,    8,    6,    4,    2,
    7,    2,    1,    1,    1,    2,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    2,
    0,    1,    1,    1,    3,    1,    1,    3,    4,    0,
    1,    3,    3,    1,    2,    2,    1,    3,    6,    9,
    9,    0,    4,    1,    2,    5,    8,    3,    3,    1,
    2,    7,    5,    3,    1,    2,    6,    4,    2,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    4,    1,    0,    2,    1,    1,    1,    0,    2,    1,
    3,    0,    2,    9,    9,    9,   10,    1,    3,    0,
    1,    3,    2,    0,    2,    7,    4,    3,    1,    1,
    3,    5,    3,    5,    1,    2,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    4,    1,    0,
    2,    1,    3,    5,    0,    2,    2,
  };
   static readonly short [] yyDefRed = {          525,
    0,    0,  527,    0,  517,    0,    0,  406,  393,  394,
    0,  397,  401,  402,  403,  404,  405,  410,  407,  408,
  409,  398,  399,  400,  508,  509,  513,  507,    0,  510,
  514,  512,  526,  505,  519,  511,  516,  515,    0,    0,
    0,  411,  412,  413,  414,  415,  416,  417,  418,  419,
    0,   59,    0,  331,    0,  522,    0,    0,    0,  396,
  320,  506,  382,    1,    2,    3,    4,    5,    6,    7,
    8,    9,   10,   11,   12,   13,   14,   15,   16,   18,
   17,    0,   19,   20,   21,   22,   23,   24,   25,   26,
   27,   28,   29,   30,   31,   32,   33,   34,   35,   36,
   37,   38,   39,   40,   41,   42,   43,   44,   45,   46,
   47,   48,   49,   50,   51,   52,   53,   54,   55,   56,
   57,   63,   62,   61,   84,    0,    0,    0,    0,   85,
  100,    0,    0,    0,    0,    0,    0,    0,  420,    0,
    0,    0,    0,    0,  520,    0,  503,    0,    0,    0,
  500,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   58,    0,    0,    0,
    0,    0,   75,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   64,   65,   66,
   67,   68,   69,   70,   71,   74,   72,   73,   76,   78,
   77,    0,    0,    0,  150,    0,  144,  154,    0,    0,
  147,    0,    0,  148,  149,  142,  180,  151,    0,    0,
  170,  155,    0,    0,  152,  143,  146,  181,  324,  234,
    0,  153,  145,    0,  325,    0,  160,  518,  332,  523,
    0,    0,    0,  237,  295,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   92,    0,    0,    0,    0,    0,
    0,  296,  297,  310,  298,  311,  307,  308,  304,  306,
  305,  312,  309,  314,  313,  299,  300,  301,  302,  321,
  315,  317,  316,  318,  319,  303,    0,    0,    0,  497,
    0,  236,   79,    0,    0,  235,    0,  101,    0,  164,
   88,   89,  354,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  173,  174,    0,  161,    0,    0,    0,
    0,    0,  171,  175,  172,    0,    0,    0,    0,    0,
    0,  138,  139,    0,    0,    0,  176,  177,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  134,  135,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  329,  524,  521,  504,
    0,   95,    0,  269,  270,    0,    0,    0,    0,    0,
    0,    0,    0,  247,    0,    0,    0,    0,    0,    0,
    0,    0,  320,    0,  233,    0,    0,    0,    0,  136,
  137,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  244,    0,  283,    0,  245,  140,  498,  341,  342,
    0,    0,    0,  501,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  106,    0,  105,   99,    0,
   86,    0,   87,  502,    0,  350,    0,  350,  350,  495,
    0,    0,  453,  156,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  323,
    0,    0,    0,    0,    0,  183,  182,  184,    0,    0,
    0,   60,    0,    0,    0,  240,   83,    0,    0,  196,
  197,    0,    0,    0,    0,    0,    0,  326,    0,  265,
    0,  266,    0,    0,    0,  272,    0,    0,  246,    0,
    0,  287,    0,    0,  289,    0,    0,  275,    0,  280,
    0,    0,  222,   93,    0,   94,  223,  224,  225,  226,
  227,  228,  229,  230,  231,  232,    0,  285,    0,  353,
    0,    0,    0,    0,    0,    0,  361,    0,  358,    0,
  211,  212,  213,  214,  215,  216,  217,  218,  219,  220,
  221,   80,    0,    0,  113,  355,  480,    0,    0,    0,
    0,    0,    0,  491,    0,  450,    0,    0,  431,    0,
  168,  178,    0,  159,    0,    0,    0,    0,    0,    0,
    0,  132,  133,    0,  157,  158,    0,   81,   82,  242,
    0,    0,  263,    0,    0,    0,    0,  292,    0,  271,
    0,    0,    0,    0,    0,    0,    0,  277,  276,  278,
  238,  248,  243,  282,  284,  343,  340,    0,    0,  339,
  334,  350,    0,  357,  111,  103,    0,    0,  473,    0,
  351,    0,  478,  473,    0,  496,    0,  452,  451,    0,
    0,    0,    0,  122,  128,  127,  131,  130,    0,    0,
  241,    0,  264,  259,    0,  261,    0,    0,    0,    0,
    0,    0,    0,    0,  286,  256,  288,    0,    0,    0,
  336,  338,    0,  359,  114,  481,    0,    0,  350,    0,
    0,  489,  492,  449,  448,    0,    0,    0,    0,  257,
    0,    0,  291,  294,  293,    0,  290,    0,    0,  253,
    0,    0,    0,  484,    0,  470,    0,  474,  472,  464,
  465,  468,  461,  462,  467,  460,    0,  463,  466,  469,
    0,    0,    0,  486,    0,    0,  476,    0,  475,  477,
  479,  485,  169,    0,  117,  115,    0,    0,    0,  162,
  322,  267,    0,  250,    0,  320,  255,  254,  273,    0,
    0,    0,  459,  390,    0,    0,  345,    0,  344,  347,
    0,  487,    0,    0,    0,  392,    0,  124,    0,  125,
  268,  320,    0,  274,    0,    0,    0,    0,    0,    0,
  427,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  116,  118,    0,  471,  389,    0,    0,  362,  363,  365,
  364,  366,  373,  375,  374,  376,  369,  370,  371,  372,
  367,  368,    0,  429,    0,    0,    0,    0,    0,    0,
    0,  346,  348,    0,  458,    0,  437,    0,  444,    0,
    0,    0,    0,    0,  425,    0,  428,  379,  378,  434,
    0,    0,    0,    0,    0,    0,  455,    0,    0,  436,
    0,    0,  446,  445,    0,    0,    0,  388,    0,  439,
  435,    0,    0,  380,    0,    0,  457,  456,    0,    0,
  438,  442,  350,    0,    0,    0,  433,  432,    0,  350,
  350,  454,    0,  443,    0,    0,    0,    0,    0,    0,
    0,  447,    0,    0,  441,  440,  384,  385,
  };
  protected static readonly short [] yyDgoto  = {             1,
  205,  755,  302,  264,  206,  207,  495,  125,  208,  209,
  496,  322,  210,  211,  212,  213,  265,  266,  267,  214,
  268,  127,  128,  574,  575,  215,  216,   53,  217,  218,
  219,  220,  304,  222,  223,  224,  225,  226,  305,  227,
  228,  229,  306,  231,  330,  232,  130,  615,  233,  757,
  758,  234,  235,  236,   54,   55,  272,  578,  273,  274,
  275,  528,  529,  728,  729,  697,   25,  131,  237,  276,
  878,   26,  277,  278,  584,  585,   27,  530,  618,  619,
  279,  280,  679,  281,  511,  557,  558,  559,   28,   29,
  314,  282,  586,  587,   30,  801,  802,  734,  151,  152,
   31,  283,  867,  868,  747,  748,  749,  849,  850,  750,
  847,  848,   32,  700,  751,  284,  285,  736,  737,   33,
  241,   34,   35,  286,  456,  287,  288,  860,  861,  738,
  289,  290,  148,  762,   36,   37,  720,  721,  291,  292,
  293,    2,  294,   38,  295,  296,  424,  425,  430,  431,
  780,  781,  651,  579,  299,  803,   57,  804,   39,   40,
   41,  862,  309,  589,   43,   44,   45,   46,   47,   48,
   49,   50,   51,  307,  560,  904,
  };
  protected static readonly short [] yySindex = {            0,
    0,  568,    0, -314,    0, -258, -231,    0,    0,    0,
 -153,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   32,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 6268,  315,
 -127,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 1815,    0,  497,    0,   46,    0,   28,   56,  365,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 6348,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -200,  134,  -25,   47,    0,
    0, 6268, -192, 6268, -173, -168,  -92, 6268,    0,  -74,
 2226, 1379, -314,  -57,    0, -314,    0, 3728,   47,  -40,
    0,   93, 2393,   21,  252,  -50,   -5, 6268, 6268,   55,
 -116,   57, -116, -116,  406,   69,    0, 2393, 2393, 2894,
 2393, 3059,    0,  479, 2393, 2393, 2393, 6268, 6268, 6268,
  510,  516,  106, 2894, 2894,  563,  618,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  467, 2393, 2393,    0,  358,    0,    0,   62,  502,
    0,  302,  -10,    0,    0,    0,    0,    0,   47,  128,
    0,    0,  196,  229,    0,    0,    0,    0,    0,    0,
  239,    0,    0,  574,    0,  615,    0,    0,    0,    0,
 5315,  454, 1742,    0,    0,  620,  637, 4117,  693,  694,
  696, -182,  707,  710, 1378,  716, 1563,  717,  724, 2393,
 4117,  582,  424,  708,    0, 1099,  712,  345,  134,  714,
  721,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 2393,  348,  736,    0,
  360,    0,    0,  619,   89,    0,  144,    0,  742,    0,
    0,    0,    0,   -9,  -12,  727,  729, -116,  729,  729,
 6268,  666,   53,    0,    0, 2393,    0,  747,  619,   -1,
  128, 6268,    0,    0,    0,  -14,   -3,    7, 2393, 2393,
 6268,    0,    0, 6268, 6268, 2393,    0,    0, 2393, 2393,
 2393, 2393, 2393, 2393, 2393, 2393, 2393, 2393, 2393,  369,
 2393, 2393,  370,    0,    0, 2393, 2393, 6268, 6268, 2393,
 2393, 2393, 2393, 2393, 2393, 2226,    0,    0,    0,    0,
   19,    0,  134,    0,    0,  451, 3235, 6268, 6268, 2393,
  738, 2393, 2393,    0,  739, 2393,  702,  740, 6268, 2393,
  743, -204,    0, 2393,    0, 2393,  382, 2393,  387,    0,
    0, 2393, 2393, 2393, 2393, 2393, 2393, 2393, 2393, 2393,
 2393,    0,  748,    0,  120,    0,    0,    0,    0,    0,
   -6, 5489,  749,    0, 2393, 2393, 2393, 2393, 2393, 2393,
 2393, 2393, 2393, 2393, 2393,    0, 2393,    0,    0, 6268,
    0, 6268,    0,    0, 6268,    0,  777,    0,    0,    0,
  397,  410,    0,    0, 2561, 2393,  252,  781, 2393, 2393,
 2393, 2393, 2393, 2393,  782,  783,  436,  785,  789,    0,
   62,   62,   62,  196,  196,    0,    0,    0,  358,  302,
  -27,    0,  162,  167,  126,    0,    0,  229,  229,    0,
    0,  229,  229,  239,  239,  502,  502,    0,  791,    0,
 1842,    0,  412,  414,  773,    0,  795,  796,    0,  797,
 2393,    0,  345,  800,    0,  804, 4117,    0, -204,    0,
 3924,  786,    0,    0,  377,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 2393,    0,  345,    0,
  348, 6268, 6268, 6268, 6268,  810,    0,  411,    0,  426,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  427,  418,    0,    0,    0,  808,  -89, 5489,
  -69,  -68,  801,    0,   22,    0,  132,  443,    0, 3059,
    0,    0,  825,    0,  222,  224,  461,  774,  511,  776,
  529,    0,    0,  826,    0,    0, 2393,    0,    0,    0,
 2393, 2393,    0,  811, 2728,  532,  817,    0,  537,    0,
 4117, 4117, 4117,  845,  538,  787, 6268,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  870,  873,    0,
    0,    0, 5489,    0,    0,    0, 6268, 6268,    0,  490,
    0,  549,    0,    0, 2393,    0,  397,    0,    0,  113,
 6268,  792,  793,    0,    0,    0,    0,    0, 2393,  545,
    0,  893,    0,    0,    0,    0,    0,  619,  894, 2393,
 2393, 4117,  414,  606,    0,    0,    0, 4117,   34,  -37,
    0,    0,  493,    0,    0,    0, 1844,  878,    0, 5424,
 5155,    0,    0,    0,    0,  551, 2038, 2038,  899,    0,
 4117,  902,    0,    0,    0, 4117,    0, 2393,  886,    0,
  -53, 4117,  910,    0, -314,    0,  533,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  116,    0,    0,    0,
 6203, 6058,  -39,    0, 6268, -250,    0,  896,    0,    0,
    0,    0,    0, 2038,    0,    0,  828,  912,  832,    0,
    0,    0, 4117,    0,  900,    0,    0,    0,    0, 4117,
  233,  919,    0,    0,  920, -234,    0,  921,    0,    0,
  918,    0,  548,  879,  -60,    0,  846,    0, 2038,    0,
    0,    0, 4117,    0, 1193,  931, 5489,  882, 1552,  913,
    0,  148,   68, -116,  934, 6058,  118, 6268,  410,  936,
    0,    0, 4117,    0,    0,  578, 6268,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 2393,    0,  556, -248,  410, 6268, 6268,  940,
  941,    0,    0,  410,    0,  562,    0,  242,    0,  172,
  576, 5489,  942,  248,    0,  913,    0,    0,    0,    0,
  261,  580,  251,   16, 5489, 5489,    0,  313,  583,    0,
  901, 6268,    0,    0,  946,  588,  290,    0,  903,    0,
    0,  121,  904,    0,  589,  602,    0,    0,  964,  410,
    0,    0,    0,  985,  993,  410,    0,    0,  410,    0,
    0,    0,  362,    0,  493, 2393, 2393,  393,  401,  493,
  493,    0,  604,  608,    0,    0,    0,    0,
  };
  protected static readonly short [] yyRindex = {            0,
    0, 5822,    0,    0,    0,    0,    0,    0,    0,    0,
 5657,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 5989,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 5891,    0,  270,    0,    0,    0,    0,  488,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 1197, 1033,  905,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 1000, 5822,    0,    0,    0,    0,    0,    0,  995,  299,
    0,    0,  281,  814,  994,    0,    0,    0,    0,    0,
  -15,    0,  -15,  -15,  922,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 2348,    0,    0,    0,  145,    0,    0, 3025, 4865,
    0,   23,  174,    0,    0,    0,    0,    0, 4429, 3365,
    0,    0, 5213,  187,    0,    0,    0,    0,    0,    0,
 4726,    0,    0, 1002,    0,    0,    0,    0,    0,    0,
 5822,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 1068,    0,    0,    0, 1317,    0,  487,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 4685,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  -67, 1004,  -67,  -67,
    0,    0,    0,    0,    0,    0,    0,    0, 1106,    0,
 4470,    0,    0,    0,    0,    0, 4608, 4646,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  281,  616,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  500,    0, 1519,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  616,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  220,    0,    0,    0,    0,    0,    0,    0,
    0,  642,  260,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  650,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   36,  625,    0,    0,    4, 4525,  994,    0,  281,  616,
    0,  616,    0,  616,    0,    0,    0,    0,    0,    0,
 5250, 5258, 5274, 5229, 5237,    0,    0,    0,  230,  137,
    0,    0,    0,    0,    0,    0,    0, 5075, 5100,    0,
    0, 5160, 5189, 4819, 5050, 4889, 5025,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 3532,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  -33,    0,    0,    0,  656,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  665,    0,    0,    0,    0,  -62,    0,  642,
    0,    0,   59,    0,    0,    0,  625,    0,    0,    9,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0, 1006,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  -19,  -17,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   60,    0,    0,    0,
  650, 4236,    0,    0,    0,    0,    0,    0,    0,  176,
    0,    0,    0,    0,  938,    0, 1136,    0,    0,    0,
    0,    0,    0, 3336,    0,    0,    0,    0,    0,    0,
    0,    0,  140,    0,    0,    0, 5822,    0,    0,    0,
 5822,    0,    0,    0,    0,    0,  923,  923,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  350,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  923,    0,    0,    0,  924,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  -13,    0,    0,    0,    0,    0,
  -54,    0,    0,    0, 1004,    0,    0,    0,  925,    0,
    0,    0,  -28,    0, 5822,    0,  642,    0,    0,   43,
    0,    0, 1004, 1004,    0,    0,    0,    0,  625,    0,
    0,    0,  -20,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  625,    0,    0,    0,
    0,    0,    0,  625,    0,    0,    0,    0,    0,  625,
    0,  642,  150,    0,    0,  328,    0,    0,    0,    0,
  625,    0,    0,    0,  642,  642,    0,  625,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  625,
    0,    0,    0,    0,    0,  625,    0,    0,  625,    0,
    0,    0,  625,    0,  992,  616,  616,  625,  625,  153,
  155,    0,    0,    0,    0,    0,    0,    0,
  };
  protected static readonly short [] yyGindex = {            0,
    0,  263,  444,    0,  703,  446, -399,    0,    0,  390,
   30,    0,  342,    0,  704, -297,    0,  820,  821,    0,
  -23, -142,  158,  404,  419,  -35,    0,   51,  -21,  452,
  -38, -165,  636,    0,  368,  322,    0,    0, -319,    0,
 1604,  722,  -65,  351,    0,    0,    0,    0,    0, -623,
    0,    0,  695,    0,  927,  347,    0,    0,  -29,    0,
    0,  546,    0,    0,  283,  422, -602,    0,    0,    0,
    0, -579,    0,    0,  423,    0, -573,  552,  399,    0,
    0,    0,    0,    0,    0,    0, -532,  440,    0,    0,
 -157,    0,  498,    0, -571,  249,    0,    0,  788,    0,
    0,    0,  219,    0,    0,    0,    0, -771,  200,    0,
  225, -694, -552,    0,    0,    0,  711,    0,    0,  858,
    0,    0,  958,    0,  407,    0,    0, -373, -401,    0,
    0, -225, -393,  340, -544, -543,  383,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  557,  584,  554,    0,
  303,    0,    0, -440, -149,    0,    0,    0,    0,    0,
 -541, -449,  644,    1, -125,    0,    0,    0,    0,    0,
    0, 1061,    0, -304,    0,    0,
  };
  protected static readonly short [] yyTable = {            63,
  129,  315,   42,  722,  349,  269,  331,  333,  535,  531,
  333,  317,  588,  319,  320,  126,  154,  581,  582,  782,
  297,  335,  386,  337,  335,  470,  337,  154,   61,  310,
  607,  452,   61,  649,  452,  402,  472,  551,  154,  466,
  465,  493,  352,  149,  167,  167,  474,  652,  154,  166,
  166,   42,  359,  653,  654,  482,  494,   59,  408,  452,
  483,  491,  809,  207,  407,  657,  207,  784,  349,  858,
  597,  767,  599,  144,  601,  230,  469,   61,  874,  490,
  207,  207,  377,  798,  759,  207,  426,  471,  377,  143,
   62,  799,  156,  129,  730,  129,  252,  473,  730,  129,
  383,  426,  488,  493,  251,  269,   52,  352,  160,  129,
  162,  463,  270,  836,  166,  207,  146,  731,  269,  129,
  129,  731,  854,  732,  526,  733,  271,  732,  527,  733,
  787,  874,  447,  377,  313,  313,  301,  588,  142,  129,
  129,  129,   42,  863,  735,  390,  656,  207,  735,  595,
  145,  300,  739,  740,   61,  741,  739,  740,  838,  741,
  490,  391,   56,  549,  596,  377,  610,  362,  457,  611,
  785,  704,  859,  360,  773,  462,  845,  208,  548,  897,
  208,  446,  303,  488,  493,  205,  800,  449,  205,   58,
  837,  835,  730,  138,  208,  208,  242,   60,  381,  208,
  328,  693,  205,  205,  129,  447,  834,  205,  383,  129,
  449,  387,  270,  386,  209,  731,  210,  209,  361,  210,
  150,  732,  129,  733,  153,  270,  271,  191,  161,  208,
  191,  209,  209,  210,  210,   61,  448,  205,   61,  271,
  844,   42,  735,   61,  191,  191,  191,  163,  191,  191,
  739,  740,  164,  741,  608,  370,  658,  371,  743,  609,
  281,  208,  381,  281,  816,  447,  209,  449,  210,  205,
  206,  298,  383,  206,  718,  387,  143,  386,  281,  191,
  230,  374,  129,  375,  395,  872,  398,  206,  206,  401,
  719,  872,  206,  129,  872,  308,  873,  460,  209,  252,
  210,  628,  129,  499,   61,  129,  129,  251,  468,  670,
  230,  191,   61,  330,  662,  252,  663,  477,  499,  876,
  478,  479,  206,  251,   98,  795,  428,  298,  165,  129,
  129,  650,  885,  886,  871,  336,  337,  338,  352,  358,
  879,  310,  499,  883,  500,  501,  167,  298,  129,  129,
  129,  650,  650,  482,  206,  328,  358,  499,  483,  851,
  129,  718,  330,  240,  513,  514,  349,   61,  475,  476,
  311,  426,  793,   98,   61,  523,   61,  719,  453,  451,
  155,  650,  550,  723,  269,  880,  426,  333,  269,  207,
  303,  155,   61,  129,  869,  684,  685,  686,  813,  409,
  851,  335,  155,  337,  467,  352,  884,   61,  556,  167,
  140,  129,  155,  129,  166,  312,  129,  633,  869,  515,
  611,  517,  518,  147,  331,  520,  573,  157,  576,  524,
  377,  577,  377,  532,  158,  533,  159,  887,  352,  353,
  851,  537,  538,  539,  540,  541,  542,  543,  544,  545,
  546,  642,  905,  851,  643,  298,  714,  839,  646,  910,
  911,  647,  717,  321,  561,  562,  563,  564,  565,  566,
  567,  568,  569,  570,  571,  316,  572,  318,  269,  269,
  269,    9,   10,   11,   12,  761,  912,  881,  129,  323,
  764,  270,  129,  341,  908,  270,  769,  909,  303,  140,
  598,  664,  600,  208,  611,  271,  913,  914,  363,  271,
  205,  205,  380,  129,  129,  129,  129,  915,  332,  364,
  365,    9,   10,   11,   12,  916,   90,  346,  637,  638,
  639,  640,   90,   59,  881,  881,  141,  761,  356,  269,
  614,  129,  140,  355,  794,  269,   59,   90,  354,  339,
  624,  666,  191,  191,  611,  340,  556,  191,  191,  191,
   91,  191,  191,  191,  191,  191,  191,    3,  269,  668,
  366,  367,  611,  269,  368,  369,  634,  682,  688,  269,
  683,  549,  129,  129,  129,  270,  270,  270,  129,  699,
  132,  753,  643,  676,  647,  206,  206,  372,  373,  271,
  271,  271,  344,  690,  129,   13,  894,  895,  129,  129,
    9,   10,   11,   12,   22,   23,   24,  376,  853,  556,
  269,  643,  129,  573,  696,  424,    5,  269,  893,  900,
  705,  643,  643,  133,  134,  810,  135,  573,  136,  137,
  671,  672,  901,  129,  917,  643,  270,  611,  918,  129,
  269,  611,  270,  840,  841,  377,  239,  345,    4,  239,
  271,  129,    9,   10,   11,   12,  271,  357,  424,  424,
  269,  424,  129,  424,  424,  270,  746,  129,  384,  435,
  270,  864,  360,  129,  702,  360,  270,  498,  499,  271,
  112,  502,  503,  112,  271,  385,  356,   42,  709,  356,
  271,   42,  129,  129,  403,  110,  129,  774,  110,  712,
  713,    9,   10,   11,   12,  506,  507,  776,  779,  484,
  485,  783,  504,  505,  129,  458,  459,  270,  349,  350,
  351,  129,  387,  388,  270,  389,  756,  756,  481,  482,
  483,  271,    9,   10,   11,   12,  392,  765,  271,  393,
    9,   10,   11,   12,  129,  396,  399,  270,  129,    9,
   10,   11,   12,  400,  404,  423,  405,  129,  429,  129,
  422,  271,  426,  556,  129,  432,  221,  270,  129,  427,
  433,  450,  779,  756,  846,  454,  455,  464,  461,  492,
  497,  271,  509,  846,  521,   42,  516,  519,  522,  129,
  129,  525,  534,  221,  221,  327,  329,  536,  547,  297,
  221,  221,  221,  129,  846,  313,  580,  583,  756,  342,
  343,  594,  602,  603,  604,  605,  129,  129,  556,  606,
  612,  620,  616,  129,  617,  621,  622,  623,  221,  221,
  626,  556,  556,  627,  632,  641,  644,  645,  846,  102,
  102,  648,  898,  102,  102,  102,  102,  102,  102,  102,
  102,  655,  855,  660,  661,  669,  665,   90,  667,  673,
  680,  102,  102,  102,  102,  102,  102,  681,   90,   90,
   90,   90,   90,   90,   90,   90,   90,   90,   90,   90,
    6,   91,   91,   91,   91,   91,   91,   91,   91,   91,
   91,   91,   91,  687,  102,  691,  102,  107,  692,  689,
  698,  358,    7,  650,  707,  708,    8,    9,   10,   11,
   12,   13,   14,   15,   16,   17,   18,   19,   20,   21,
   22,   23,   24,  710,  711,  742,  102,  716,  102,  760,
   96,   96,  763,  766,   96,   96,   96,   96,   96,   96,
  770,   96,  788,  772,  786,  789,  790,  792,  796,  797,
  805,  806,   96,   96,   96,   96,   96,   96,  807,  808,
  811,  815,  817,  833,  842,  852,  856,  144,  260,  865,
  866,  221,  870,  144,  221,  221,  221,  221,  221,  221,
  221,  221,  221,  221,  221,   96,  875,   96,  144,  877,
  882,  221,  221,  889,  892,  221,  221,  221,  221,  221,
  221,  221,  436,  437,  438,  439,  440,  441,  442,  443,
  444,  445,  902,  890,  906,  896,  899,   96,  144,   96,
   97,   97,  907,  163,   97,   97,   97,   97,   97,   97,
  327,   97,  328,  352,  494,  430,  258,  119,  120,  121,
  391,  812,   97,   97,   97,   97,   97,   97,  674,  489,
  675,  490,  381,  382,  706,  695,  677,  480,  104,  239,
  508,  771,  104,  104,  629,  701,  104,  814,  104,  703,
  630,  715,  694,  857,  659,   97,  888,   97,  434,  903,
  104,  104,  104,  104,  104,  104,  891,  512,  379,  238,
  221,  221,  791,  768,  636,  635,  625,   61,  843,   61,
  593,  139,    0,   61,    0,    0,    0,   97,    0,   97,
    0,    0,    0,  104,    0,  104,    0,    0,   61,    0,
    0,  102,    0,    0,    0,    0,    0,    0,  408,  102,
    0,    0,  170,    0,  407,    0,  165,  165,  170,    0,
  170,    0,  170,    0,    0,  104,    0,  104,   61,  406,
    0,    0,    0,    0,    0,  170,    0,  170,  170,    0,
    0,    0,    0,    0,    0,  151,  262,    0,    0,  102,
  102,  151,  102,  102,  102,  102,  102,    0,  102,  102,
  102,  102,  102,  102,  102,    0,  151,    0,    0,    0,
    0,    0,  102,    0,  102,  102,  102,  102,  102,  102,
  102,  102,  102,  102,  102,  102,  102,    0,    0,  102,
    0,    0,   96,    0,    0,    0,  151,    0,    0,    0,
   96,    0,  107,    0,  102,    0,    0,  107,    0,    0,
  107,    0,  221,    0,    0,    0,    0,    0,    0,    0,
  678,  726,    0,    0,  107,  107,  107,    0,  107,  107,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   96,   96,    0,   96,   96,   96,   96,   96,    0,   96,
   96,   96,   96,   96,   96,    0,    0,    0,    0,  107,
    0,    0,    0,   96,    0,   96,   96,   96,   96,   96,
   96,   96,   96,   96,   96,   96,   96,   96,    0,    0,
   96,    0,   97,    0,    0,    0,    0,    0,  144,  107,
   97,  107,    0,    0,    0,   96,    0,    0,    0,  144,
  144,  144,  144,  144,  144,  144,  144,  144,  144,  144,
  144,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  104,    0,    0,    0,    0,    0,   95,    0,  104,    0,
   97,   97,   95,   97,   97,   97,   97,   97,    0,   97,
   97,   97,   97,   97,   97,    0,    0,   95,    0,    0,
    0,    0,    0,   97,    0,   97,   97,   97,   97,   97,
   97,   97,   97,   97,   97,   97,   97,   97,  104,  104,
   97,    0,    0,  104,  104,  104,    0,  104,  104,  104,
  104,  104,  104,  104,    0,   97,    0,  171,    0,  170,
  168,  104,  169,  104,  104,  104,  104,  104,  104,  104,
  104,  104,  104,  104,  104,  104,  394,    5,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   61,    0,
    0,    0,    0,  104,    0,   61,    0,   61,    0,   61,
   61,   61,   61,   61,   61,   61,   61,   61,   61,   61,
   61,  170,  170,   61,  170,  170,  170,  170,  170,  409,
  170,  170,  170,  170,  170,  170,    0,    0,   61,    0,
  410,  411,  412,  413,  414,  415,  416,  417,  418,  419,
  420,  421,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  165,    0,    0,  107,    0,  151,    0,    0,    0,
    0,    0,  107,    0,    0,    0,    0,  151,  151,  151,
  151,  151,  151,  151,  151,  151,  151,  151,  151,    0,
    0,    8,    9,   10,   11,   12,   13,   14,   15,   16,
   17,   18,   19,   20,   21,   22,   23,   24,   90,    0,
    0,    0,  107,  107,   90,    0,  727,  107,  107,  107,
    0,  107,  107,  107,  107,  107,  107,    0,    0,   90,
    0,    0,    0,    0,    0,  107,    0,  107,  822,    0,
    0,    0,    0,  821,  818,    0,  819,    0,  820,    0,
    0,    0,  171,    0,  170,  168,    0,  169,    0,    0,
    0,  831,    0,  832,    0,    0,    0,  107,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   64,   65,   66,   67,   68,   69,
   70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
   80,   81,  172,  397,   83,   84,   85,   86,   87,   88,
   89,   90,   91,   92,   93,   94,   95,   96,   97,   98,
   99,  100,  101,  102,  103,  104,  105,  106,  107,  108,
  109,  110,  111,  112,  113,  114,  115,  116,  117,  118,
  119,  120,  121,  173,  122,  123,    0,   95,    0,    0,
    0,    6,    0,    0,    0,    0,    0,    0,   95,   95,
   95,   95,   95,   95,   95,   95,   95,   95,   95,   95,
    0,  174,    0,    7,    0,    0,    0,    8,    9,   10,
   11,   12,   13,   14,   15,   16,   17,   18,   19,   20,
   21,   22,   23,   24,    0,  175,    0,    0,  176,    0,
    0,  177,    0,    0,    0,    0,    0,    0,    0,  178,
  179,  180,  181,  182,  183,    0,    0,    0,    0,  184,
  185,  324,  325,    0,    0,    0,    0,    0,  333,  334,
  335,  186,  187,  243,  188,  189,  190,  191,  192,  193,
  194,  195,  196,  197,  198,  199,  200,  201,  124,  203,
  204,    0,    0,    0,    0,    0,  347,  348,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   64,
   65,   66,   67,   68,   69,   70,   71,   72,   73,   74,
   75,   76,   77,   78,   79,   80,   81,  172,    0,   83,
   84,   85,   86,   87,   88,   89,   90,   91,   92,   93,
   94,   95,   96,   97,   98,   99,  100,  101,  102,  103,
  104,  105,  106,  107,  108,  109,  110,  111,  112,  113,
  114,  115,  116,  117,  118,  119,  120,  121,  173,  122,
  123,  171,    0,  170,  168,    0,  169,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   90,
  613,    0,  726,    0,    0,    0,  174,    0,    0,    0,
   90,   90,   90,   90,   90,   90,   90,   90,   90,   90,
   90,   90,  823,  824,  825,  826,  827,  828,  829,  830,
  175,    0,    0,  176,  725,    0,  177,    0,    0,    0,
    0,    0,    0,    0,  178,  179,  180,  181,  182,  183,
    0,    0,    0,    0,  184,  185,    0,  486,  487,  488,
    0,    0,    0,    0,    0,    0,  186,  187,  724,  188,
  189,  190,  191,  192,  193,  194,  195,  196,  197,  198,
  199,  200,  201,  124,  203,  204,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   64,   65,
   66,   67,   68,   69,   70,   71,   72,   73,   74,   75,
   76,   77,   78,   79,   80,   81,   82,    0,   83,   84,
   85,   86,   87,   88,   89,   90,   91,   92,   93,   94,
   95,   96,   97,   98,   99,  100,  101,  102,  103,  104,
  105,  106,  107,  108,  109,  110,  111,  112,  113,  114,
  115,  116,  117,  118,  119,  120,  121,    0,  122,  123,
    0,    0,    0,    0,    0,    0,    0,    0,  591,  592,
    0,    0,    0,    0,    0,    0,    0,  171,    0,  170,
  168,    0,  169,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   64,   65,
   66,   67,   68,   69,   70,   71,   72,   73,   74,   75,
   76,   77,   78,   79,   80,   81,  172,    0,   83,   84,
   85,   86,   87,   88,   89,   90,   91,   92,   93,   94,
   95,   96,   97,   98,   99,  100,  101,  102,  103,  104,
  105,  106,  107,  108,  109,  110,  111,  112,  113,  114,
  115,  116,  117,  118,  119,  120,  121,  173,  122,  123,
  754,    0,  124,    8,    9,   10,   11,   12,   13,   14,
   15,   16,   17,   18,   19,   20,   21,   22,   23,   24,
    0,    0,    0,    0,    0,  174,    0,    0,    0,    0,
    0,    0,    8,    9,   10,   11,   12,   13,   14,   15,
   16,   17,   18,   19,   20,   21,   22,   23,   24,  175,
    0,    0,  176,    0,    0,  177,    0,  727,    0,    0,
    0,    0,    0,  178,  179,  180,  181,  182,  183,    0,
    0,    0,    0,  184,  185,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  186,  187,    0,  188,  189,
  190,  191,  192,  193,  194,  195,  196,  197,  198,  199,
  200,  201,  124,  203,  204,  171,    0,  170,  168,    0,
  169,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   64,   65,   66,   67,   68,   69,
   70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
   80,   81,  172,    0,   83,   84,   85,   86,   87,   88,
   89,   90,   91,   92,   93,   94,   95,   96,   97,   98,
   99,  100,  101,  102,  103,  104,  105,  106,  107,  108,
  109,  110,  111,  112,  113,  114,  115,  116,  117,  118,
  119,  120,  121,  173,  122,  123,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  174,    0,    0,   61,    0,    0,   61,   61,   61,
   61,   61,   61,   61,   61,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  175,    0,   61,  176,   61,
   61,  177,    0,    0,    0,    0,    0,    0,    0,  178,
  179,  180,  181,  182,  183,    0,    0,    0,    0,  184,
  185,    0,  171,    0,  170,  168,    0,  169,   61,    0,
    0,  186,  187,    0,  188,  189,  190,  191,  192,  193,
  194,  195,  196,  197,  198,  199,  200,  201,  124,  203,
  204,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   64,   65,   66,   67,   68,   69,   70,   71,
   72,   73,   74,   75,   76,   77,   78,   79,   80,   81,
  172,    0,   83,   84,   85,   86,   87,   88,   89,   90,
   91,   92,   93,   94,   95,   96,   97,   98,   99,  100,
  101,  102,  103,  104,  105,  106,  107,  108,  109,  110,
  111,  112,  113,  114,  115,  116,  117,  118,  119,  120,
  121,  173,  122,  123,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  174,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  175,    0,    0,  176,    0,    0,  177,
  171,    0,    0,  168,    0,  169,    0,  178,  179,  180,
  181,  182,  183,    0,    0,    0,    0,  184,  185,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  186,
  187,    0,  188,  189,  190,  191,  192,  193,  194,  195,
  196,  197,  198,  199,  200,  201,  202,  203,  204,   64,
   65,   66,   67,   68,   69,   70,   71,   72,   73,   74,
   75,   76,   77,   78,   79,   80,   81,  172,    0,   83,
   84,   85,   86,   87,   88,   89,   90,   91,   92,   93,
   94,   95,   96,   97,   98,   99,  100,  101,  102,  103,
  104,  105,  106,  107,  108,  109,  110,  111,  112,  113,
  114,  115,  116,  117,  118,  119,  120,  121,  173,  122,
  123,    0,    0,   61,   61,    0,   61,   61,   61,   61,
   61,    0,   61,   61,   61,   61,   61,   61,   61,    0,
    0,    0,    0,    0,    0,   61,  174,   61,    0,   61,
   61,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  175,    0,    0,  176,    0,    0,  177,  326,    0,  170,
    0,    0,    0,    0,  178,  179,  180,  181,  182,  183,
    0,    0,    0,    0,  184,  185,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  186,  187,    0,  188,
  189,  190,  191,  192,  193,  194,  195,  196,  197,  198,
  199,  200,  201,  124,  203,  204,    0,   64,   65,   66,
   67,   68,   69,   70,   71,   72,   73,   74,   75,   76,
   77,   78,   79,   80,   81,  590,    0,   83,   84,   85,
   86,   87,   88,   89,   90,   91,   92,   93,   94,   95,
   96,   97,   98,   99,  100,  101,  102,  103,  104,  105,
  106,  107,  108,  109,  110,  111,  112,  113,  114,  115,
  116,  117,  118,  119,  120,  121,  173,  122,  123,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  174,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  175,    0,
    0,  176,    0,  326,  177,  170,    0,    0,    0,    0,
    0,    0,  178,  179,  180,  181,  182,  183,    0,    0,
    0,    0,  184,  185,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  186,  187,    0,  188,  189,  190,
  191,  192,  193,  194,  195,  196,  197,  198,  199,  200,
  201,  124,  203,  204,   64,   65,   66,   67,   68,   69,
   70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
   80,   81,  172,    0,   83,   84,   85,   86,   87,   88,
   89,   90,   91,   92,   93,   94,   95,   96,   97,   98,
   99,  100,  101,  102,  103,  104,  105,  106,  107,  108,
  109,  110,  111,  112,  113,  114,  115,  116,  117,  118,
  119,  120,  121,  173,  122,  123,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  201,    0,    0,  201,    0,
    0,  174,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  201,  201,    0,    0,    0,  201,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  326,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  178,
  179,  180,  181,  182,  183,    0,    0,  201,    0,  184,
  185,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  186,  187,    0,  188,  189,  190,  191,  192,  193,
  194,  195,  196,  197,  198,  199,  200,  201,  124,  201,
   64,   65,   66,   67,   68,   69,   70,   71,   72,   73,
   74,   75,   76,   77,   78,   79,   80,   81,  172,    0,
   83,   84,   85,   86,   87,   88,   89,   90,   91,   92,
   93,   94,   95,   96,   97,   98,   99,  100,  101,  102,
  103,  104,  105,  106,  107,  108,  109,  110,  111,  112,
  113,  114,  115,  116,  117,  118,  119,  120,  121,  173,
  122,  123,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  174,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  178,  179,  180,  181,  182,
  183,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  510,    0,    0,    0,  186,  187,    0,
  188,  189,  190,  191,  192,  193,  194,  195,  196,  197,
  198,  199,  200,  201,  124,   64,   65,   66,   67,   68,
   69,   70,   71,   72,   73,   74,   75,   76,   77,   78,
   79,   80,   81,    0,    0,   83,   84,   85,   86,   87,
   88,   89,   90,   91,   92,   93,   94,   95,   96,   97,
   98,   99,  100,  101,  102,  103,  104,  105,  106,  107,
  108,  109,  110,  111,  112,  113,  114,  115,  116,  117,
  118,  119,  120,  121,  173,  122,  123,  249,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  201,  201,    0,    0,  249,  201,  201,  201,    0,    0,
    0,  108,  174,    0,    0,  108,  108,  108,  108,  108,
    0,  108,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  108,  108,  108,  108,  108,  108,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  178,  179,  180,  181,  182,  183,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  108,  249,    0,
  249,    0,  186,  187,    0,  188,  189,  190,  191,  192,
  193,  194,  195,  196,  197,  198,  199,  200,  201,  124,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  108,
    0,   64,   65,   66,   67,   68,   69,   70,   71,   72,
   73,   74,   75,   76,   77,   78,   79,   80,   81,   82,
    0,   83,   84,   85,   86,   87,   88,   89,   90,   91,
   92,   93,   94,   95,   96,   97,   98,   99,  100,  101,
  102,  103,  104,  105,  106,  107,  108,  109,  110,  111,
  112,  113,  114,  115,  116,  117,  118,  119,  120,  121,
    0,  122,  123,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  279,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  279,    0,  249,  249,  249,  249,  249,  249,  249,  249,
  249,  249,  249,  249,  249,  249,  249,  249,  249,  249,
  249,    0,  249,  249,  249,  249,  249,  249,  249,  249,
  249,  249,  249,  249,  249,  249,  249,  249,  249,  249,
  249,  249,  249,  249,  249,  249,  249,  249,  249,  249,
  249,  249,  249,  249,  249,  249,  249,  249,  249,  249,
  249,    0,  249,  249,  279,  124,  279,    0,    0,    0,
    0,    0,  249,  249,  249,  249,  249,    0,  249,  249,
  249,  249,  249,  249,    0,  249,  249,  249,  249,  249,
  249,  249,  249,  249,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  249,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  249,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  249,  249,    0,
  108,  108,    0,  108,  108,  108,  108,  108,    0,  108,
  108,  108,  108,  108,  108,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  249,    0,  108,  108,
  108,  108,  108,  108,  108,  108,  108,  108,    0,  243,
  108,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  245,    0,  279,  279,
  279,  279,  279,  279,  279,  279,  279,  279,  279,  279,
  279,  279,  279,  279,  279,  279,  279,    0,  279,  279,
  279,  279,  279,  279,  279,  279,  279,  279,  279,  279,
  279,  279,  279,  279,  279,  279,  279,  279,  279,  279,
  279,  279,  279,  279,  279,  279,  279,  279,  279,  279,
  279,  279,  279,  279,  279,  279,  279,    0,  279,  279,
   61,    0,  244,    0,    0,    0,    0,    0,  279,  279,
    0,  279,  279,  279,    0,  279,  279,  279,  279,  279,
    0,  279,  279,  279,  279,  279,  279,  279,  279,  279,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  279,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  279,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  279,  279,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  279,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  243,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  245,    0,   64,   65,   66,   67,   68,   69,
   70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
   80,   81,   82,    0,   83,   84,   85,   86,   87,   88,
   89,   90,   91,   92,   93,   94,   95,   96,   97,   98,
   99,  100,  101,  102,  103,  104,  105,  106,  107,  108,
  109,  110,  111,  112,  113,  114,  115,  116,  117,  118,
  119,  120,  121,    0,  122,  123,   61,    0,  631,    0,
    0,    0,    0,    0,  246,    0,    0,  247,  248,    0,
    0,  249,  250,  251,  252,  253,    0,  254,  255,  256,
  257,    0,  258,  259,  260,  261,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  262,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  178,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  184,
  185,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  263,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  243,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  245,    0,    0,    0,    0,
   64,   65,   66,   67,   68,   69,   70,   71,   72,   73,
   74,   75,   76,   77,   78,   79,   80,   81,   82,    0,
   83,   84,   85,   86,   87,   88,   89,   90,   91,   92,
   93,   94,   95,   96,   97,   98,   99,  100,  101,  102,
  103,  104,  105,  106,  107,  108,  109,  110,  111,  112,
  113,  114,  115,  116,  117,  118,  119,  120,  121,   61,
  122,  123,    0,    0,    0,    0,    0,    0,    0,    0,
  246,    0,    0,  247,  248,    0,    0,  249,  250,  251,
  252,  253,    0,  254,  255,  256,  257,    0,  258,  259,
  260,  261,  123,    0,    0,  123,  123,  123,  123,  123,
  123,  123,  123,    0,  262,    0,    0,    0,    0,    0,
    0,    0,    0,  123,  123,  123,  123,  123,  123,    0,
    0,    0,    0,    0,    0,  178,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  184,  185,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  123,    0,  123,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  263,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  123,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   64,   65,   66,   67,   68,   69,   70,
   71,   72,   73,   74,   75,   76,   77,   78,   79,   80,
   81,   82,    0,   83,   84,   85,   86,   87,   88,   89,
   90,   91,   92,   93,   94,   95,   96,   97,   98,   99,
  100,  101,  102,  103,  104,  105,  106,  107,  108,  109,
  110,  111,  112,  113,  114,  115,  116,  117,  118,  119,
  120,  121,    0,  122,  123,    0,    0,    0,    0,    0,
    0,    0,    0,  246,    0,    0,  247,  248,    0,    0,
  249,  250,  251,  252,  253,    0,  254,  255,  256,  257,
    0,  258,  259,  260,  261,  141,    0,    0,  141,  141,
  141,  141,  141,  141,    0,  141,    0,  262,    0,    0,
    0,    0,    0,    0,    0,    0,  141,  141,  141,  141,
  141,  141,    0,    0,    0,    0,    0,    0,  178,    0,
    0,    0,    0,    0,    0,    0,  109,    0,  184,  185,
  109,  109,  109,  109,  109,    0,  109,    0,    0,  141,
    0,  141,    0,    0,    0,    0,    0,  109,  109,  109,
  109,  109,  109,    0,    0,    0,    0,  263,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  141,    0,    0,    0,    0,    0,    0,
    0,  179,  109,    0,    0,  179,    0,    0,  179,    0,
    0,  179,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  179,  179,  179,    0,  179,  179,    0,    0,
    0,    0,    0,    0,  109,    0,    0,    0,    0,    0,
    0,  123,  123,    0,  123,  123,  123,  123,  123,    0,
  123,  123,  123,  123,  123,  123,  123,  179,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  123,  123,  123,
  123,  123,  123,  123,  123,  123,  123,  123,  123,    0,
    0,  123,    0,    0,  126,    0,    0,    0,  126,  179,
  126,  126,  126,  126,  126,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  126,  126,  126,  126,  126,
  126,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  129,    0,    0,    0,  129,    0,  129,  129,
  129,  129,  129,    0,    0,    0,    0,    0,    0,    0,
  126,    0,    0,  129,  129,  129,  129,  129,  129,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  170,    0,    0,    0,  170,  170,  170,  170,  170,
    0,  170,  126,    0,    0,    0,    0,    0,  129,    0,
    0,    0,  170,  170,  170,    0,  170,  170,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  188,    0,    0,  188,
  129,    0,    0,    0,    0,    0,    0,  170,    0,    0,
    0,    0,    0,  188,  188,  188,    0,  188,  188,    0,
    0,    0,    0,    0,  141,  141,    0,  141,  141,  141,
  141,  141,    0,  141,  141,  141,  141,  141,  141,  170,
    0,    0,    0,    0,    0,    0,    0,    0,  188,    0,
  141,  141,  141,  141,  141,  141,  141,  141,  141,  141,
  141,  141,    0,    0,  141,  109,  109,    0,  109,  109,
  109,  109,  109,    0,  109,  109,  109,  109,  109,  109,
  188,    0,    0,    0,    0,    0,    0,    0,    0,  189,
    0,    0,  189,  109,  109,  109,  109,  109,  109,  109,
  109,  109,  109,    0,    0,  109,  189,  189,  189,    0,
  189,  189,    0,    0,    0,    0,    0,    0,    0,    0,
  179,  179,    0,  179,  179,    0,  179,  179,    0,  179,
  179,  179,  179,  179,  179,  185,    0,  185,  185,  185,
    0,  189,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  185,  185,  185,    0,  185,  185,    0,  186,
  179,  186,  186,  186,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  189,    0,    0,  186,  186,  186,    0,
  186,  186,    0,    0,    0,    0,    0,  185,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  126,  126,    0,  126,  126,  126,  126,
  126,  186,  126,  126,  126,  126,  126,  126,  126,  185,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  126,
  126,  126,  126,  126,  126,  126,  126,  126,  126,  126,
  126,  129,  129,  186,  129,  129,  129,  129,  129,    0,
  129,  129,  129,  129,  129,  129,  129,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  129,  129,  129,
  129,  129,  129,  129,  129,  129,  129,  129,  129,    0,
  170,  170,    0,  170,  170,  170,  170,  170,    0,  170,
  170,  170,  170,  170,  170,  187,    0,  187,  187,  187,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  187,  187,  187,    0,  187,  187,    0,    0,
  190,  188,  188,  190,  188,  188,  188,  188,  188,    0,
  188,  188,  188,  188,  188,  188,    0,  190,  190,  190,
    0,  190,  190,    0,    0,  193,    0,  187,  193,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  193,  193,  193,    0,  193,  193,    0,    0,
  194,    0,  190,  194,    0,    0,    0,    0,    0,  187,
    0,    0,    0,    0,    0,    0,    0,  194,  194,  194,
    0,  194,  194,    0,    0,    0,    0,  193,    0,    0,
    0,    0,    0,    0,  190,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  189,  189,    0,  189,  189,  189,
  189,  189,  194,  189,  189,  189,  189,  189,  189,  193,
  192,    0,    0,  192,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  726,    0,    0,    0,  192,  192,  192,
    0,  192,  192,    0,  194,    0,    0,    0,    0,  195,
  185,  185,  195,  185,  185,  185,  185,  185,    0,  185,
  185,  185,  185,  185,  185,  725,  195,  195,  195,    0,
  195,  195,  192,  198,  186,  186,  198,  186,  186,  186,
  186,  186,    0,  186,  186,  186,  186,  186,  186,  199,
  198,  198,  199,    0,    0,  198,    0,  200,    0,  752,
  200,  195,    0,    0,  192,    0,  199,  199,    0,    0,
  202,  199,    0,  202,  200,  200,    0,    0,  203,  200,
    0,  203,    0,    0,    0,  198,    0,  202,  202,    0,
    0,    0,  202,  195,  204,  203,  203,  204,    0,    0,
  203,  199,    0,    0,    0,    0,    0,    0,    0,  200,
    0,  204,  204,    0,    0,    0,  204,  198,    0,    0,
    0,    0,  202,    0,    0,    0,    0,    0,    0,    0,
  203,    0,    0,  199,    0,    0,    0,    0,    0,    0,
    0,  200,    0,    0,    0,    0,  204,    0,    0,    0,
    0,    0,    0,    5,  202,    0,    0,    0,    0,    0,
    0,    0,  203,    0,    0,    0,    0,    0,    0,    0,
  187,  187,    0,  187,  187,  187,  187,  187,  204,  187,
  187,  187,  187,  187,  187,    4,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  190,  190,    0,  190,  190,
  190,  190,  190,    0,  190,  190,  190,  190,  190,  190,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  378,
  193,  193,    0,    0,    0,  193,  193,  193,    0,  193,
  193,  193,  193,  193,  193,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  194,  194,    0,    0,    0,
  194,  194,  194,    0,  194,  194,  194,  194,  194,  194,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    8,    9,   10,   11,   12,   13,   14,
   15,   16,   17,   18,   19,   20,   21,   22,   23,   24,
    0,    0,    0,    0,    0,  192,  192,    0,  727,    0,
  192,  192,  192,    0,  192,  192,  192,  192,  192,  192,
    0,    0,    0,    0,    0,    0,    0,    0,  744,    0,
    0,    0,    0,    0,  195,  195,    0,    0,    0,  195,
  195,  195,    0,  195,  195,  195,  195,  195,  195,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  198,  198,
    0,    0,    0,  198,  198,  198,    0,    0,    0,  198,
  198,    0,    0,    0,  199,  199,    0,    0,    0,  199,
  199,  199,  200,  200,    0,  199,  199,  200,  200,  200,
    0,    0,    0,  200,  200,  202,  202,    0,    0,    0,
  202,  202,  202,  203,  203,    0,    0,    0,  203,  203,
  203,    0,    0,    0,    0,    0,    0,    6,    0,  204,
  204,    0,    0,    0,  204,  204,  204,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    7,
    0,    0,    0,    8,    9,   10,   11,   12,   13,   14,
   15,   16,   17,   18,   19,   20,   21,   22,   23,   24,
   64,   65,   66,   67,   68,   69,   70,   71,   72,   73,
   74,   75,   76,   77,   78,   79,   80,   81,   82,    0,
   83,   84,   85,   86,   87,   88,   89,   90,   91,   92,
   93,   94,   95,   96,   97,   98,   99,  100,  101,  102,
  103,  104,  105,  106,  107,  108,  109,  110,  111,  112,
  113,  114,  115,  116,  117,  118,  119,  120,  121,    0,
  122,  123,    0,    0,  745,   64,   65,   66,   67,   68,
   69,   70,   71,   72,   73,   74,   75,   76,   77,   78,
   79,   80,   81,   82,    0,   83,   84,   85,   86,   87,
   88,   89,   90,   91,   92,   93,   94,   95,   96,   97,
   98,   99,  100,  101,  102,  103,  104,  105,  106,  107,
  108,  109,  110,  111,  112,  113,  114,  115,  116,  117,
  118,  119,  120,  121,    0,  122,  123,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  124,    0,    0,    0,  552,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  124,
  553,  554,  555,  395,  395,  395,  395,  395,  395,  395,
  395,  395,  395,  395,  395,  395,  395,  395,  395,  395,
  395,  395,  395,  395,  395,  395,  395,  395,  395,  395,
  395,  395,  395,  395,  395,  395,  395,  395,  395,  395,
  395,  395,  395,  395,  395,  395,  395,  395,  395,  395,
  395,  395,  395,  395,  395,  395,  395,  395,  395,  395,
  395,  395,    0,  395,  395,  395,  395,  395,  395,    0,
  395,  395,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  395,  395,    0,  395,  395,
  395,  395,  395,  395,  395,  395,  395,  395,  395,  395,
  395,  395,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  395,  421,  421,
  421,  421,  421,  421,  421,  421,  421,  421,  421,  421,
  421,  421,  421,  421,  421,  421,  421,  421,  421,  421,
  421,  421,  421,  421,  421,  421,  421,  421,  421,  421,
  421,  421,  421,  421,  421,  421,  421,  421,  421,  421,
  421,  421,  421,  421,  421,  421,  421,  421,  421,  421,
  421,  421,  421,  421,  421,  421,  421,    0,  421,  421,
  421,  421,  421,  421,    0,  421,  421,  422,  422,  422,
  422,  422,  422,  422,  422,  422,  422,  422,  422,  422,
  422,  422,  422,  422,  422,  422,  422,  422,  422,  422,
  422,  422,  422,  422,  422,  422,  422,  422,  422,  422,
  422,  422,  422,  422,  422,  422,  422,  422,  422,  422,
  422,  422,  422,  422,  422,  422,  422,  422,  422,  422,
  422,  422,  422,  422,  422,  422,    0,  422,  422,  422,
  422,  422,  422,    0,  422,  422,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  421,    0,    0,  423,  423,  423,  423,  423,
  423,  423,  423,  423,  423,  423,  423,  423,  423,  423,
  423,  423,  423,  423,  424,  423,  423,  423,  423,  423,
  423,  423,  423,  423,  423,  423,  423,  423,  423,  423,
  423,  423,  423,  423,  423,  423,  423,  423,  423,  423,
  423,  423,  423,  423,  423,  423,  423,  423,  423,  423,
  423,  423,  423,  423,    0,  423,  423,  424,  424,    0,
  424,  422,  424,  424,   64,   65,   66,   67,   68,   69,
   70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
   80,   81,   82,    0,   83,   84,   85,   86,   87,   88,
   89,   90,   91,   92,   93,   94,   95,   96,   97,   98,
   99,  100,  101,  102,  103,  104,  105,  106,  107,  108,
  109,  110,  111,  112,  113,  114,  115,  116,  117,  118,
  119,  120,  121,    0,  122,  123,    0,    0,    0,    0,
    0,  777,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  423,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  778,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   64,
   65,   66,   67,   68,   69,   70,   71,   72,   73,   74,
   75,   76,   77,   78,   79,   80,   81,   82,  124,   83,
   84,   85,   86,   87,   88,   89,   90,   91,   92,   93,
   94,   95,   96,   97,   98,   99,  100,  101,  102,  103,
  104,  105,  106,  107,  108,  109,  110,  111,  112,  113,
  114,  115,  116,  117,  118,  119,  120,  121,    0,  122,
  123,    0,    0,  138,   64,   65,   66,   67,   68,   69,
   70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
   80,   81,   82,    0,   83,   84,   85,   86,   87,   88,
   89,   90,   91,   92,   93,   94,   95,   96,   97,   98,
   99,  100,  101,  102,  103,  104,  105,  106,  107,  108,
  109,  110,  111,  112,  113,  114,  115,  116,  117,  118,
  119,  120,  121,    0,  122,  123,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   64,   65,   66,   67,   68,   69,
   70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
   80,   81,    0,  775,   83,   84,   85,   86,   87,   88,
   89,   90,   91,   92,   93,   94,   95,   96,   97,   98,
   99,  100,  101,  102,  103,  104,  105,  106,  107,  108,
  109,  110,  111,  112,  113,  114,  115,  116,  117,  118,
  119,  120,  121,    0,  122,  123,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  124,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  124,
  };
  protected static readonly short [] yyCheck = {            29,
   39,  159,    2,   41,   59,  148,  172,   41,  408,  403,
   44,  161,  462,  163,  164,   39,   42,  458,  459,   59,
   61,   41,  248,   41,   44,   40,   44,   42,   42,  155,
   58,   44,   46,  123,   44,  261,   40,   44,   42,   41,
   42,  361,   58,   82,   41,   42,   40,  580,   42,   41,
   42,   51,   63,  123,  123,  123,  361,    7,   40,   44,
  123,  359,  123,   41,   46,   44,   44,  318,  123,  318,
  470,  125,  472,   46,  474,  141,   91,   91,  850,   44,
   58,   59,   40,  318,  708,   63,   44,   91,   46,   44,
   59,  326,   46,  132,  697,  134,  125,   91,  701,  138,
  243,   59,   44,   44,  125,  248,  421,  123,  132,  148,
  134,   59,  148,   46,  138,   93,   61,  697,  261,  158,
  159,  701,  817,  697,  329,  697,  148,  701,  333,  701,
  754,  903,   44,   91,  158,  159,   44,  587,   93,  178,
  179,  180,  142,  838,  697,  328,  125,  125,  701,  469,
  123,   59,  697,  697,  123,  697,  701,  701,   91,  701,
  125,  344,  421,   44,  469,  123,   41,   40,  318,   44,
  421,   59,  421,   46,   59,  123,   59,   41,   59,   59,
   44,   93,  153,  125,  125,   41,  421,   44,   44,  421,
  123,   44,  795,  321,   58,   59,  146,  351,   59,   63,
  171,  642,   58,   59,  243,   44,   59,   63,   59,  248,
   44,   59,  248,   59,   41,  795,   41,   44,   91,   44,
  421,  795,  261,  795,   91,  261,  248,   41,  421,   93,
   44,   58,   59,   58,   59,  123,   93,   93,  123,  261,
  123,  241,  795,  123,   58,   59,   60,  421,   62,   63,
  795,  795,  421,  795,   93,   60,  125,   62,  699,   93,
   41,  125,  123,   44,  797,   44,   93,   44,   93,  125,
   41,  388,  123,   44,  328,  123,   44,  123,   59,   93,
  346,   43,  321,   45,  255,   44,  257,   58,   59,  260,
  344,   44,   63,  332,   44,  275,  125,  321,  125,  328,
  125,  527,  341,   44,  318,  344,  345,  328,  332,  607,
  376,  125,  326,   44,   93,  344,   93,  341,   59,  852,
  344,  345,   93,  344,   44,   93,  297,  388,  421,  368,
  369,  421,  865,  866,   93,  178,  179,  180,   40,  367,
   93,  467,   44,   93,  368,  369,  421,  388,  387,  388,
  389,  421,  421,  421,  125,  326,  367,   59,  421,  809,
  399,  328,   93,  421,  388,  389,  421,  381,  339,  340,
  421,   44,  766,   93,  388,  399,  390,  344,  391,  389,
  406,  421,  389,  421,  527,  125,   59,  421,  531,  367,
  361,  406,  406,  432,  844,  621,  622,  623,  792,  381,
  850,  421,  406,  421,  406,  421,  391,  421,  432,  406,
   46,  450,  406,  452,  406,  421,  455,   41,  868,  390,
   44,  392,  393,   59,  590,  396,  450,  381,  452,  400,
  388,  455,  390,  404,  388,  406,  390,  125,  377,  378,
  890,  412,  413,  414,  415,  416,  417,  418,  419,  420,
  421,   41,  893,  903,   44,  388,  682,  390,   41,  900,
  901,   44,  688,   58,  435,  436,  437,  438,  439,  440,
  441,  442,  443,  444,  445,  421,  447,  421,  621,  622,
  623,  350,  351,  352,  353,  711,  125,  861,  527,  421,
  716,  527,  531,  388,  896,  531,  722,  899,  469,   46,
  471,   41,  473,  367,   44,  527,  906,  907,  381,  531,
  366,  367,   59,  552,  553,  554,  555,  125,   40,  392,
  393,  350,  351,  352,  353,  125,   40,   61,  552,  553,
  554,  555,   46,   46,  908,  909,   40,  763,   37,  682,
  511,  580,   46,   42,  770,  688,   59,   61,   47,   40,
  521,   41,  366,  367,   44,   40,  580,  371,  372,  373,
   61,  375,  376,  377,  378,  379,  380,    0,  711,   41,
  375,  376,   44,  716,  379,  380,  547,   41,   41,  722,
   44,   44,  621,  622,  623,  621,  622,  623,  627,   41,
  276,   41,   44,  615,   44,  366,  367,  369,  370,  621,
  622,  623,   40,  627,  643,  354,  317,  318,  647,  648,
  350,  351,  352,  353,  363,  364,  365,   44,   41,  643,
  763,   44,  661,  647,  648,  276,   59,  770,   41,   41,
  660,   44,   44,  319,  320,  785,  322,  661,  324,  325,
  611,  612,   41,  682,   41,   44,  682,   44,   41,  688,
  793,   44,  688,  803,  804,   41,   41,   40,   91,   44,
  682,  700,  350,  351,  352,  353,  688,  366,  319,  320,
  813,  322,  711,  324,  325,  711,  700,  716,   59,   61,
  716,  839,   41,  722,  655,   44,  722,  366,  367,  711,
   41,  370,  371,   44,  716,   59,   41,  697,  669,   44,
  722,  701,  741,  742,  123,   41,  745,  737,   44,  680,
  681,  350,  351,  352,  353,  374,  375,  741,  742,  352,
  353,  745,  372,  373,  763,  319,  320,  763,  371,  372,
  373,  770,   40,   40,  770,   40,  707,  708,  349,  350,
  351,  763,  350,  351,  352,  353,   40,  718,  770,   40,
  350,  351,  352,  353,  793,   40,   40,  793,  797,  350,
  351,  352,  353,   40,  341,  421,   59,  806,  421,  808,
   59,  793,   59,  797,  813,   40,  141,  813,  817,   59,
  421,   40,  806,  754,  808,   59,   58,   41,  123,  421,
  421,  813,  342,  817,   93,  795,   59,   59,   59,  838,
  839,   59,  421,  168,  169,  170,  171,  421,   61,   61,
  175,  176,  177,  852,  838,  839,   40,  421,  789,  184,
  185,   41,   41,   41,  389,   41,  865,  866,  852,   41,
   40,   59,  421,  872,  421,   41,   41,   41,  203,  204,
   41,  865,  866,   40,   59,   36,  421,  421,  872,   36,
   37,   44,  882,   40,   41,   42,   43,   44,   45,   46,
   47,   61,  833,  421,   40,   40,   93,  381,   93,   59,
  339,   58,   59,   60,   61,   62,   63,   61,  392,  393,
  394,  395,  396,  397,  398,  399,  400,  401,  402,  403,
  323,  392,  393,  394,  395,  396,  397,  398,  399,  400,
  401,  402,  403,   59,   91,   36,   93,  421,   36,  123,
  421,  367,  345,  421,  123,  123,  349,  350,  351,  352,
  353,  354,  355,  356,  357,  358,  359,  360,  361,  362,
  363,  364,  365,   41,   41,   58,  123,  332,  125,   41,
   36,   37,   41,   58,   40,   41,   42,   43,   44,   45,
   41,   47,  125,  421,   59,   44,  125,   58,   40,   40,
   40,   44,   58,   59,   60,   61,   62,   63,  421,   91,
  125,   41,   91,   61,   41,   40,  421,   40,   41,   40,
   40,  346,  421,   46,  349,  350,  351,  352,  353,  354,
  355,  356,  357,  358,  359,   91,  421,   93,   61,   58,
  421,  366,  367,  421,   59,  370,  371,  372,  373,  374,
  375,  376,  394,  395,  396,  397,  398,  399,  400,  401,
  402,  403,   59,  123,   40,  123,  123,  123,   91,  125,
   36,   37,   40,   40,   40,   41,   42,   43,   44,   45,
   41,   47,   41,   40,  123,  421,   41,  125,  125,  125,
   59,  789,   58,   59,   60,   61,   62,   63,  615,  357,
  615,  358,  243,  243,  661,  647,  615,  346,   36,  143,
  376,  725,   40,   41,  529,  654,   44,  795,   46,  657,
  529,  683,  643,  835,  587,   91,  868,   93,  301,  890,
   58,   59,   60,   61,   62,   63,  872,  387,  241,  142,
  465,  466,  763,  721,  551,  549,  523,   40,  806,   42,
  467,   51,   -1,   46,   -1,   -1,   -1,  123,   -1,  125,
   -1,   -1,   -1,   91,   -1,   93,   -1,   -1,   61,   -1,
   -1,  318,   -1,   -1,   -1,   -1,   -1,   -1,   40,  326,
   -1,   -1,   37,   -1,   46,   -1,   41,   42,   43,   -1,
   45,   -1,   47,   -1,   -1,  123,   -1,  125,   91,   61,
   -1,   -1,   -1,   -1,   -1,   60,   -1,   62,   63,   -1,
   -1,   -1,   -1,   -1,   -1,   40,   41,   -1,   -1,  366,
  367,   46,  369,  370,  371,  372,  373,   -1,  375,  376,
  377,  378,  379,  380,  381,   -1,   61,   -1,   -1,   -1,
   -1,   -1,  389,   -1,  391,  392,  393,  394,  395,  396,
  397,  398,  399,  400,  401,  402,  403,   -1,   -1,  406,
   -1,   -1,  318,   -1,   -1,   -1,   91,   -1,   -1,   -1,
  326,   -1,   36,   -1,  421,   -1,   -1,   41,   -1,   -1,
   44,   -1,  607,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  615,   59,   -1,   -1,   58,   59,   60,   -1,   62,   63,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  366,  367,   -1,  369,  370,  371,  372,  373,   -1,  375,
  376,  377,  378,  379,  380,   -1,   -1,   -1,   -1,   93,
   -1,   -1,   -1,  389,   -1,  391,  392,  393,  394,  395,
  396,  397,  398,  399,  400,  401,  402,  403,   -1,   -1,
  406,   -1,  318,   -1,   -1,   -1,   -1,   -1,  381,  123,
  326,  125,   -1,   -1,   -1,  421,   -1,   -1,   -1,  392,
  393,  394,  395,  396,  397,  398,  399,  400,  401,  402,
  403,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  318,   -1,   -1,   -1,   -1,   -1,   40,   -1,  326,   -1,
  366,  367,   46,  369,  370,  371,  372,  373,   -1,  375,
  376,  377,  378,  379,  380,   -1,   -1,   61,   -1,   -1,
   -1,   -1,   -1,  389,   -1,  391,  392,  393,  394,  395,
  396,  397,  398,  399,  400,  401,  402,  403,  366,  367,
  406,   -1,   -1,  371,  372,  373,   -1,  375,  376,  377,
  378,  379,  380,  381,   -1,  421,   -1,   40,   -1,   42,
   43,  389,   45,  391,  392,  393,  394,  395,  396,  397,
  398,  399,  400,  401,  402,  403,   59,   59,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  381,   -1,
   -1,   -1,   -1,  421,   -1,  388,   -1,  390,   -1,  392,
  393,  394,  395,  396,  397,  398,  399,  400,  401,  402,
  403,  366,  367,  406,  369,  370,  371,  372,  373,  381,
  375,  376,  377,  378,  379,  380,   -1,   -1,  421,   -1,
  392,  393,  394,  395,  396,  397,  398,  399,  400,  401,
  402,  403,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  406,   -1,   -1,  318,   -1,  381,   -1,   -1,   -1,
   -1,   -1,  326,   -1,   -1,   -1,   -1,  392,  393,  394,
  395,  396,  397,  398,  399,  400,  401,  402,  403,   -1,
   -1,  349,  350,  351,  352,  353,  354,  355,  356,  357,
  358,  359,  360,  361,  362,  363,  364,  365,   40,   -1,
   -1,   -1,  366,  367,   46,   -1,  374,  371,  372,  373,
   -1,  375,  376,  377,  378,  379,  380,   -1,   -1,   61,
   -1,   -1,   -1,   -1,   -1,  389,   -1,  391,   37,   -1,
   -1,   -1,   -1,   42,   43,   -1,   45,   -1,   47,   -1,
   -1,   -1,   40,   -1,   42,   43,   -1,   45,   -1,   -1,
   -1,   60,   -1,   62,   -1,   -1,   -1,  421,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  257,  258,  259,  260,  261,  262,
  263,  264,  265,  266,  267,  268,  269,  270,  271,  272,
  273,  274,  275,   91,  277,  278,  279,  280,  281,  282,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,  316,  317,  318,   -1,  381,   -1,   -1,
   -1,  323,   -1,   -1,   -1,   -1,   -1,   -1,  392,  393,
  394,  395,  396,  397,  398,  399,  400,  401,  402,  403,
   -1,  344,   -1,  345,   -1,   -1,   -1,  349,  350,  351,
  352,  353,  354,  355,  356,  357,  358,  359,  360,  361,
  362,  363,  364,  365,   -1,  368,   -1,   -1,  371,   -1,
   -1,  374,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  382,
  383,  384,  385,  386,  387,   -1,   -1,   -1,   -1,  392,
  393,  168,  169,   -1,   -1,   -1,   -1,   -1,  175,  176,
  177,  404,  405,   42,  407,  408,  409,  410,  411,  412,
  413,  414,  415,  416,  417,  418,  419,  420,  421,  422,
  423,   -1,   -1,   -1,   -1,   -1,  203,  204,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,
  258,  259,  260,  261,  262,  263,  264,  265,  266,  267,
  268,  269,  270,  271,  272,  273,  274,  275,   -1,  277,
  278,  279,  280,  281,  282,  283,  284,  285,  286,  287,
  288,  289,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,  300,  301,  302,  303,  304,  305,  306,  307,
  308,  309,  310,  311,  312,  313,  314,  315,  316,  317,
  318,   40,   -1,   42,   43,   -1,   45,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  381,
   59,   -1,   59,   -1,   -1,   -1,  344,   -1,   -1,   -1,
  392,  393,  394,  395,  396,  397,  398,  399,  400,  401,
  402,  403,  371,  372,  373,  374,  375,  376,  377,  378,
  368,   -1,   -1,  371,   91,   -1,  374,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  382,  383,  384,  385,  386,  387,
   -1,   -1,   -1,   -1,  392,  393,   -1,  354,  355,  356,
   -1,   -1,   -1,   -1,   -1,   -1,  404,  405,  125,  407,
  408,  409,  410,  411,  412,  413,  414,  415,  416,  417,
  418,  419,  420,  421,  422,  423,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,
  259,  260,  261,  262,  263,  264,  265,  266,  267,  268,
  269,  270,  271,  272,  273,  274,  275,   -1,  277,  278,
  279,  280,  281,  282,  283,  284,  285,  286,  287,  288,
  289,  290,  291,  292,  293,  294,  295,  296,  297,  298,
  299,  300,  301,  302,  303,  304,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,   -1,  317,  318,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  465,  466,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   40,   -1,   42,
   43,   -1,   45,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,
  259,  260,  261,  262,  263,  264,  265,  266,  267,  268,
  269,  270,  271,  272,  273,  274,  275,   -1,  277,  278,
  279,  280,  281,  282,  283,  284,  285,  286,  287,  288,
  289,  290,  291,  292,  293,  294,  295,  296,  297,  298,
  299,  300,  301,  302,  303,  304,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,  316,  317,  318,
  123,   -1,  421,  349,  350,  351,  352,  353,  354,  355,
  356,  357,  358,  359,  360,  361,  362,  363,  364,  365,
   -1,   -1,   -1,   -1,   -1,  344,   -1,   -1,   -1,   -1,
   -1,   -1,  349,  350,  351,  352,  353,  354,  355,  356,
  357,  358,  359,  360,  361,  362,  363,  364,  365,  368,
   -1,   -1,  371,   -1,   -1,  374,   -1,  374,   -1,   -1,
   -1,   -1,   -1,  382,  383,  384,  385,  386,  387,   -1,
   -1,   -1,   -1,  392,  393,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  404,  405,   -1,  407,  408,
  409,  410,  411,  412,  413,  414,  415,  416,  417,  418,
  419,  420,  421,  422,  423,   40,   -1,   42,   43,   -1,
   45,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  257,  258,  259,  260,  261,  262,
  263,  264,  265,  266,  267,  268,  269,  270,  271,  272,
  273,  274,  275,   -1,  277,  278,  279,  280,  281,  282,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,  316,  317,  318,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  344,   -1,   -1,   37,   -1,   -1,   40,   41,   42,
   43,   44,   45,   46,   47,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  368,   -1,   60,  371,   62,
   63,  374,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  382,
  383,  384,  385,  386,  387,   -1,   -1,   -1,   -1,  392,
  393,   -1,   40,   -1,   42,   43,   -1,   45,   91,   -1,
   -1,  404,  405,   -1,  407,  408,  409,  410,  411,  412,
  413,  414,  415,  416,  417,  418,  419,  420,  421,  422,
  423,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  257,  258,  259,  260,  261,  262,  263,  264,
  265,  266,  267,  268,  269,  270,  271,  272,  273,  274,
  275,   -1,  277,  278,  279,  280,  281,  282,  283,  284,
  285,  286,  287,  288,  289,  290,  291,  292,  293,  294,
  295,  296,  297,  298,  299,  300,  301,  302,  303,  304,
  305,  306,  307,  308,  309,  310,  311,  312,  313,  314,
  315,  316,  317,  318,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  344,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  368,   -1,   -1,  371,   -1,   -1,  374,
   40,   -1,   -1,   43,   -1,   45,   -1,  382,  383,  384,
  385,  386,  387,   -1,   -1,   -1,   -1,  392,  393,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  404,
  405,   -1,  407,  408,  409,  410,  411,  412,  413,  414,
  415,  416,  417,  418,  419,  420,  421,  422,  423,  257,
  258,  259,  260,  261,  262,  263,  264,  265,  266,  267,
  268,  269,  270,  271,  272,  273,  274,  275,   -1,  277,
  278,  279,  280,  281,  282,  283,  284,  285,  286,  287,
  288,  289,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,  300,  301,  302,  303,  304,  305,  306,  307,
  308,  309,  310,  311,  312,  313,  314,  315,  316,  317,
  318,   -1,   -1,  366,  367,   -1,  369,  370,  371,  372,
  373,   -1,  375,  376,  377,  378,  379,  380,  381,   -1,
   -1,   -1,   -1,   -1,   -1,  388,  344,  390,   -1,  392,
  393,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  368,   -1,   -1,  371,   -1,   -1,  374,   40,   -1,   42,
   -1,   -1,   -1,   -1,  382,  383,  384,  385,  386,  387,
   -1,   -1,   -1,   -1,  392,  393,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  404,  405,   -1,  407,
  408,  409,  410,  411,  412,  413,  414,  415,  416,  417,
  418,  419,  420,  421,  422,  423,   -1,  257,  258,  259,
  260,  261,  262,  263,  264,  265,  266,  267,  268,  269,
  270,  271,  272,  273,  274,  275,   -1,  277,  278,  279,
  280,  281,  282,  283,  284,  285,  286,  287,  288,  289,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
  300,  301,  302,  303,  304,  305,  306,  307,  308,  309,
  310,  311,  312,  313,  314,  315,  316,  317,  318,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  344,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  368,   -1,
   -1,  371,   -1,   40,  374,   42,   -1,   -1,   -1,   -1,
   -1,   -1,  382,  383,  384,  385,  386,  387,   -1,   -1,
   -1,   -1,  392,  393,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  404,  405,   -1,  407,  408,  409,
  410,  411,  412,  413,  414,  415,  416,  417,  418,  419,
  420,  421,  422,  423,  257,  258,  259,  260,  261,  262,
  263,  264,  265,  266,  267,  268,  269,  270,  271,  272,
  273,  274,  275,   -1,  277,  278,  279,  280,  281,  282,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,  316,  317,  318,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   41,   -1,   -1,   44,   -1,
   -1,  344,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   58,   59,   -1,   -1,   -1,   63,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   40,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  382,
  383,  384,  385,  386,  387,   -1,   -1,   93,   -1,  392,
  393,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  404,  405,   -1,  407,  408,  409,  410,  411,  412,
  413,  414,  415,  416,  417,  418,  419,  420,  421,  125,
  257,  258,  259,  260,  261,  262,  263,  264,  265,  266,
  267,  268,  269,  270,  271,  272,  273,  274,  275,   -1,
  277,  278,  279,  280,  281,  282,  283,  284,  285,  286,
  287,  288,  289,  290,  291,  292,  293,  294,  295,  296,
  297,  298,  299,  300,  301,  302,  303,  304,  305,  306,
  307,  308,  309,  310,  311,  312,  313,  314,  315,  316,
  317,  318,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  344,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  382,  383,  384,  385,  386,
  387,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   59,   -1,   -1,   -1,  404,  405,   -1,
  407,  408,  409,  410,  411,  412,  413,  414,  415,  416,
  417,  418,  419,  420,  421,  257,  258,  259,  260,  261,
  262,  263,  264,  265,  266,  267,  268,  269,  270,  271,
  272,  273,  274,   -1,   -1,  277,  278,  279,  280,  281,
  282,  283,  284,  285,  286,  287,  288,  289,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,  300,  301,
  302,  303,  304,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,  316,  317,  318,   42,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  366,  367,   -1,   -1,   59,  371,  372,  373,   -1,   -1,
   -1,   37,  344,   -1,   -1,   41,   42,   43,   44,   45,
   -1,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   58,   59,   60,   61,   62,   63,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  382,  383,  384,  385,  386,  387,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,  123,   -1,
  125,   -1,  404,  405,   -1,  407,  408,  409,  410,  411,
  412,  413,  414,  415,  416,  417,  418,  419,  420,  421,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  125,
   -1,  257,  258,  259,  260,  261,  262,  263,  264,  265,
  266,  267,  268,  269,  270,  271,  272,  273,  274,  275,
   -1,  277,  278,  279,  280,  281,  282,  283,  284,  285,
  286,  287,  288,  289,  290,  291,  292,  293,  294,  295,
  296,  297,  298,  299,  300,  301,  302,  303,  304,  305,
  306,  307,  308,  309,  310,  311,  312,  313,  314,  315,
   -1,  317,  318,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   42,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   59,   -1,  257,  258,  259,  260,  261,  262,  263,  264,
  265,  266,  267,  268,  269,  270,  271,  272,  273,  274,
  275,   -1,  277,  278,  279,  280,  281,  282,  283,  284,
  285,  286,  287,  288,  289,  290,  291,  292,  293,  294,
  295,  296,  297,  298,  299,  300,  301,  302,  303,  304,
  305,  306,  307,  308,  309,  310,  311,  312,  313,  314,
  315,   -1,  317,  318,  123,  421,  125,   -1,   -1,   -1,
   -1,   -1,  327,  328,  329,  330,  331,   -1,  333,  334,
  335,  336,  337,  338,   -1,  340,  341,  342,  343,  344,
  345,  346,  347,  348,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  361,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  382,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  392,  393,   -1,
  366,  367,   -1,  369,  370,  371,  372,  373,   -1,  375,
  376,  377,  378,  379,  380,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  421,   -1,  394,  395,
  396,  397,  398,  399,  400,  401,  402,  403,   -1,   42,
  406,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   59,   -1,  257,  258,
  259,  260,  261,  262,  263,  264,  265,  266,  267,  268,
  269,  270,  271,  272,  273,  274,  275,   -1,  277,  278,
  279,  280,  281,  282,  283,  284,  285,  286,  287,  288,
  289,  290,  291,  292,  293,  294,  295,  296,  297,  298,
  299,  300,  301,  302,  303,  304,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,   -1,  317,  318,
  123,   -1,  125,   -1,   -1,   -1,   -1,   -1,  327,  328,
   -1,  330,  331,  332,   -1,  334,  335,  336,  337,  338,
   -1,  340,  341,  342,  343,  344,  345,  346,  347,  348,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  361,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  382,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  392,  393,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  421,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   42,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   59,   -1,  257,  258,  259,  260,  261,  262,
  263,  264,  265,  266,  267,  268,  269,  270,  271,  272,
  273,  274,  275,   -1,  277,  278,  279,  280,  281,  282,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,   -1,  317,  318,  123,   -1,  125,   -1,
   -1,   -1,   -1,   -1,  327,   -1,   -1,  330,  331,   -1,
   -1,  334,  335,  336,  337,  338,   -1,  340,  341,  342,
  343,   -1,  345,  346,  347,  348,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  361,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  382,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  392,
  393,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  421,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   42,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   59,   -1,   -1,   -1,   -1,
  257,  258,  259,  260,  261,  262,  263,  264,  265,  266,
  267,  268,  269,  270,  271,  272,  273,  274,  275,   -1,
  277,  278,  279,  280,  281,  282,  283,  284,  285,  286,
  287,  288,  289,  290,  291,  292,  293,  294,  295,  296,
  297,  298,  299,  300,  301,  302,  303,  304,  305,  306,
  307,  308,  309,  310,  311,  312,  313,  314,  315,  123,
  317,  318,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  327,   -1,   -1,  330,  331,   -1,   -1,  334,  335,  336,
  337,  338,   -1,  340,  341,  342,  343,   -1,  345,  346,
  347,  348,   37,   -1,   -1,   40,   41,   42,   43,   44,
   45,   46,   47,   -1,  361,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   58,   59,   60,   61,   62,   63,   -1,
   -1,   -1,   -1,   -1,   -1,  382,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  392,  393,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   91,   -1,   93,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  421,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  257,  258,  259,  260,  261,  262,  263,
  264,  265,  266,  267,  268,  269,  270,  271,  272,  273,
  274,  275,   -1,  277,  278,  279,  280,  281,  282,  283,
  284,  285,  286,  287,  288,  289,  290,  291,  292,  293,
  294,  295,  296,  297,  298,  299,  300,  301,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,   -1,  317,  318,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  327,   -1,   -1,  330,  331,   -1,   -1,
  334,  335,  336,  337,  338,   -1,  340,  341,  342,  343,
   -1,  345,  346,  347,  348,   37,   -1,   -1,   40,   41,
   42,   43,   44,   45,   -1,   47,   -1,  361,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,   60,   61,
   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,  382,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   37,   -1,  392,  393,
   41,   42,   43,   44,   45,   -1,   47,   -1,   -1,   91,
   -1,   93,   -1,   -1,   -1,   -1,   -1,   58,   59,   60,
   61,   62,   63,   -1,   -1,   -1,   -1,  421,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  125,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   37,   93,   -1,   -1,   41,   -1,   -1,   44,   -1,
   -1,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,
   -1,   -1,   -1,   -1,  125,   -1,   -1,   -1,   -1,   -1,
   -1,  366,  367,   -1,  369,  370,  371,  372,  373,   -1,
  375,  376,  377,  378,  379,  380,  381,   93,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  392,  393,  394,
  395,  396,  397,  398,  399,  400,  401,  402,  403,   -1,
   -1,  406,   -1,   -1,   37,   -1,   -1,   -1,   41,  125,
   43,   44,   45,   46,   47,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   58,   59,   60,   61,   62,
   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   37,   -1,   -1,   -1,   41,   -1,   43,   44,
   45,   46,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   93,   -1,   -1,   58,   59,   60,   61,   62,   63,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   37,   -1,   -1,   -1,   41,   42,   43,   44,   45,
   -1,   47,  125,   -1,   -1,   -1,   -1,   -1,   93,   -1,
   -1,   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,   -1,   44,
  125,   -1,   -1,   -1,   -1,   -1,   -1,   93,   -1,   -1,
   -1,   -1,   -1,   58,   59,   60,   -1,   62,   63,   -1,
   -1,   -1,   -1,   -1,  366,  367,   -1,  369,  370,  371,
  372,  373,   -1,  375,  376,  377,  378,  379,  380,  125,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,   -1,
  392,  393,  394,  395,  396,  397,  398,  399,  400,  401,
  402,  403,   -1,   -1,  406,  366,  367,   -1,  369,  370,
  371,  372,  373,   -1,  375,  376,  377,  378,  379,  380,
  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,
   -1,   -1,   44,  394,  395,  396,  397,  398,  399,  400,
  401,  402,  403,   -1,   -1,  406,   58,   59,   60,   -1,
   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  366,  367,   -1,  369,  370,   -1,  372,  373,   -1,  375,
  376,  377,  378,  379,  380,   41,   -1,   43,   44,   45,
   -1,   93,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   58,   59,   60,   -1,   62,   63,   -1,   41,
  406,   43,   44,   45,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  125,   -1,   -1,   58,   59,   60,   -1,
   62,   63,   -1,   -1,   -1,   -1,   -1,   93,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  366,  367,   -1,  369,  370,  371,  372,
  373,   93,  375,  376,  377,  378,  379,  380,  381,  125,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  392,
  393,  394,  395,  396,  397,  398,  399,  400,  401,  402,
  403,  366,  367,  125,  369,  370,  371,  372,  373,   -1,
  375,  376,  377,  378,  379,  380,  381,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  392,  393,  394,
  395,  396,  397,  398,  399,  400,  401,  402,  403,   -1,
  366,  367,   -1,  369,  370,  371,  372,  373,   -1,  375,
  376,  377,  378,  379,  380,   41,   -1,   43,   44,   45,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,
   41,  366,  367,   44,  369,  370,  371,  372,  373,   -1,
  375,  376,  377,  378,  379,  380,   -1,   58,   59,   60,
   -1,   62,   63,   -1,   -1,   41,   -1,   93,   44,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,
   41,   -1,   93,   44,   -1,   -1,   -1,   -1,   -1,  125,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,   60,
   -1,   62,   63,   -1,   -1,   -1,   -1,   93,   -1,   -1,
   -1,   -1,   -1,   -1,  125,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  366,  367,   -1,  369,  370,  371,
  372,  373,   93,  375,  376,  377,  378,  379,  380,  125,
   41,   -1,   -1,   44,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   59,   -1,   -1,   -1,   58,   59,   60,
   -1,   62,   63,   -1,  125,   -1,   -1,   -1,   -1,   41,
  366,  367,   44,  369,  370,  371,  372,  373,   -1,  375,
  376,  377,  378,  379,  380,   91,   58,   59,   60,   -1,
   62,   63,   93,   41,  366,  367,   44,  369,  370,  371,
  372,  373,   -1,  375,  376,  377,  378,  379,  380,   41,
   58,   59,   44,   -1,   -1,   63,   -1,   41,   -1,  125,
   44,   93,   -1,   -1,  125,   -1,   58,   59,   -1,   -1,
   41,   63,   -1,   44,   58,   59,   -1,   -1,   41,   63,
   -1,   44,   -1,   -1,   -1,   93,   -1,   58,   59,   -1,
   -1,   -1,   63,  125,   41,   58,   59,   44,   -1,   -1,
   63,   93,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,
   -1,   58,   59,   -1,   -1,   -1,   63,  125,   -1,   -1,
   -1,   -1,   93,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   93,   -1,   -1,  125,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  125,   -1,   -1,   -1,   -1,   93,   -1,   -1,   -1,
   -1,   -1,   -1,   59,  125,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  366,  367,   -1,  369,  370,  371,  372,  373,  125,  375,
  376,  377,  378,  379,  380,   91,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  366,  367,   -1,  369,  370,
  371,  372,  373,   -1,  375,  376,  377,  378,  379,  380,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  125,
  366,  367,   -1,   -1,   -1,  371,  372,  373,   -1,  375,
  376,  377,  378,  379,  380,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  366,  367,   -1,   -1,   -1,
  371,  372,  373,   -1,  375,  376,  377,  378,  379,  380,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  349,  350,  351,  352,  353,  354,  355,
  356,  357,  358,  359,  360,  361,  362,  363,  364,  365,
   -1,   -1,   -1,   -1,   -1,  366,  367,   -1,  374,   -1,
  371,  372,  373,   -1,  375,  376,  377,  378,  379,  380,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  125,   -1,
   -1,   -1,   -1,   -1,  366,  367,   -1,   -1,   -1,  371,
  372,  373,   -1,  375,  376,  377,  378,  379,  380,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  366,  367,
   -1,   -1,   -1,  371,  372,  373,   -1,   -1,   -1,  377,
  378,   -1,   -1,   -1,  366,  367,   -1,   -1,   -1,  371,
  372,  373,  366,  367,   -1,  377,  378,  371,  372,  373,
   -1,   -1,   -1,  377,  378,  366,  367,   -1,   -1,   -1,
  371,  372,  373,  366,  367,   -1,   -1,   -1,  371,  372,
  373,   -1,   -1,   -1,   -1,   -1,   -1,  323,   -1,  366,
  367,   -1,   -1,   -1,  371,  372,  373,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  345,
   -1,   -1,   -1,  349,  350,  351,  352,  353,  354,  355,
  356,  357,  358,  359,  360,  361,  362,  363,  364,  365,
  257,  258,  259,  260,  261,  262,  263,  264,  265,  266,
  267,  268,  269,  270,  271,  272,  273,  274,  275,   -1,
  277,  278,  279,  280,  281,  282,  283,  284,  285,  286,
  287,  288,  289,  290,  291,  292,  293,  294,  295,  296,
  297,  298,  299,  300,  301,  302,  303,  304,  305,  306,
  307,  308,  309,  310,  311,  312,  313,  314,  315,   -1,
  317,  318,   -1,   -1,  321,  257,  258,  259,  260,  261,
  262,  263,  264,  265,  266,  267,  268,  269,  270,  271,
  272,  273,  274,  275,   -1,  277,  278,  279,  280,  281,
  282,  283,  284,  285,  286,  287,  288,  289,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,  300,  301,
  302,  303,  304,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,   -1,  317,  318,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  421,   -1,   -1,   -1,  360,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  421,
  422,  423,  424,  257,  258,  259,  260,  261,  262,  263,
  264,  265,  266,  267,  268,  269,  270,  271,  272,  273,
  274,  275,  276,  277,  278,  279,  280,  281,  282,  283,
  284,  285,  286,  287,  288,  289,  290,  291,  292,  293,
  294,  295,  296,  297,  298,  299,  300,  301,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,   -1,  317,  318,  319,  320,  321,  322,   -1,
  324,  325,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  349,  350,   -1,  352,  353,
  354,  355,  356,  357,  358,  359,  360,  361,  362,  363,
  364,  365,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  421,  257,  258,
  259,  260,  261,  262,  263,  264,  265,  266,  267,  268,
  269,  270,  271,  272,  273,  274,  275,  276,  277,  278,
  279,  280,  281,  282,  283,  284,  285,  286,  287,  288,
  289,  290,  291,  292,  293,  294,  295,  296,  297,  298,
  299,  300,  301,  302,  303,  304,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,   -1,  317,  318,
  319,  320,  321,  322,   -1,  324,  325,  257,  258,  259,
  260,  261,  262,  263,  264,  265,  266,  267,  268,  269,
  270,  271,  272,  273,  274,  275,  276,  277,  278,  279,
  280,  281,  282,  283,  284,  285,  286,  287,  288,  289,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
  300,  301,  302,  303,  304,  305,  306,  307,  308,  309,
  310,  311,  312,  313,  314,  315,   -1,  317,  318,  319,
  320,  321,  322,   -1,  324,  325,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  421,   -1,   -1,  257,  258,  259,  260,  261,
  262,  263,  264,  265,  266,  267,  268,  269,  270,  271,
  272,  273,  274,  275,  276,  277,  278,  279,  280,  281,
  282,  283,  284,  285,  286,  287,  288,  289,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,  300,  301,
  302,  303,  304,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,   -1,  317,  318,  319,  320,   -1,
  322,  421,  324,  325,  257,  258,  259,  260,  261,  262,
  263,  264,  265,  266,  267,  268,  269,  270,  271,  272,
  273,  274,  275,   -1,  277,  278,  279,  280,  281,  282,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,   -1,  317,  318,   -1,   -1,   -1,   -1,
   -1,  324,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  421,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  382,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,
  258,  259,  260,  261,  262,  263,  264,  265,  266,  267,
  268,  269,  270,  271,  272,  273,  274,  275,  421,  277,
  278,  279,  280,  281,  282,  283,  284,  285,  286,  287,
  288,  289,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,  300,  301,  302,  303,  304,  305,  306,  307,
  308,  309,  310,  311,  312,  313,  314,  315,   -1,  317,
  318,   -1,   -1,  321,  257,  258,  259,  260,  261,  262,
  263,  264,  265,  266,  267,  268,  269,  270,  271,  272,
  273,  274,  275,   -1,  277,  278,  279,  280,  281,  282,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,   -1,  317,  318,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  257,  258,  259,  260,  261,  262,
  263,  264,  265,  266,  267,  268,  269,  270,  271,  272,
  273,  274,   -1,  421,  277,  278,  279,  280,  281,  282,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,   -1,  317,  318,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  421,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  421,
  };

#line 1667 "Parser.y"

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
#line default
namespace yydebug {
        using System;
	 internal interface yyDebug {
		 void push (int state, Object value);
		 void lex (int state, int token, string name, Object value);
		 void shift (int from, int to, int errorFlag);
		 void pop (int state);
		 void discard (int state, int token, string name, Object value);
		 void reduce (int from, int to, int rule, string text, int len);
		 void shift (int from, int to);
		 void accept (Object value);
		 void error (string message);
		 void reject ();
	 }
	 
	 class yyDebugSimple : yyDebug {
		 void println (string s){
			 Console.Error.WriteLine (s);
		 }
		 
		 public void push (int state, Object value) {
			 println ("push\tstate "+state+"\tvalue "+value);
		 }
		 
		 public void lex (int state, int token, string name, Object value) {
			 println("lex\tstate "+state+"\treading "+name+"\tvalue "+value);
		 }
		 
		 public void shift (int from, int to, int errorFlag) {
			 switch (errorFlag) {
			 default:				// normally
				 println("shift\tfrom state "+from+" to "+to);
				 break;
			 case 0: case 1: case 2:		// in error recovery
				 println("shift\tfrom state "+from+" to "+to
					     +"\t"+errorFlag+" left to recover");
				 break;
			 case 3:				// normally
				 println("shift\tfrom state "+from+" to "+to+"\ton error");
				 break;
			 }
		 }
		 
		 public void pop (int state) {
			 println("pop\tstate "+state+"\ton error");
		 }
		 
		 public void discard (int state, int token, string name, Object value) {
			 println("discard\tstate "+state+"\ttoken "+name+"\tvalue "+value);
		 }
		 
		 public void reduce (int from, int to, int rule, string text, int len) {
			 println("reduce\tstate "+from+"\tuncover "+to
				     +"\trule ("+rule+") "+text);
		 }
		 
		 public void shift (int from, int to) {
			 println("goto\tfrom state "+from+" to "+to);
		 }
		 
		 public void accept (Object value) {
			 println("accept\tvalue "+value);
		 }
		 
		 public void error (string message) {
			 println("error\t"+message);
		 }
		 
		 public void reject () {
			 println("reject");
		 }
		 
	 }
}
// %token constants
 class Token {
  public const int EOF = 0;
  public const int KBOOL = 257;
  public const int KBYTE = 258;
  public const int KSBYTE = 259;
  public const int KCHAR = 260;
  public const int KUCHAR = 261;
  public const int KDOUBLE = 262;
  public const int KFLOAT = 263;
  public const int KINT = 264;
  public const int KUINT = 265;
  public const int KLONG = 266;
  public const int KULONG = 267;
  public const int KSHORT = 268;
  public const int KUSHORT = 269;
  public const int KOBJECT = 270;
  public const int KSTRING = 271;
  public const int KSIZE_T = 272;
  public const int KPTRDIFF_T = 273;
  public const int KVOID = 274;
  public const int KCONST = 275;
  public const int KTYPEDEF = 276;
  public const int KVEC2 = 277;
  public const int KVEC3 = 278;
  public const int KVEC4 = 279;
  public const int KDVEC2 = 280;
  public const int KDVEC3 = 281;
  public const int KDVEC4 = 282;
  public const int KIVEC2 = 283;
  public const int KIVEC3 = 284;
  public const int KIVEC4 = 285;
  public const int KBVEC2 = 286;
  public const int KBVEC3 = 287;
  public const int KBVEC4 = 288;
  public const int KMAT2 = 289;
  public const int KMAT2x3 = 290;
  public const int KMAT2x4 = 291;
  public const int KMAT3x2 = 292;
  public const int KMAT3 = 293;
  public const int KMAT3x4 = 294;
  public const int KMAT4x2 = 295;
  public const int KMAT4x3 = 296;
  public const int KMAT4 = 297;
  public const int KDMAT2 = 298;
  public const int KDMAT2x3 = 299;
  public const int KDMAT2x4 = 300;
  public const int KDMAT3x2 = 301;
  public const int KDMAT3 = 302;
  public const int KDMAT3x4 = 303;
  public const int KDMAT4x2 = 304;
  public const int KDMAT4x3 = 305;
  public const int KDMAT4 = 306;
  public const int KIMAT2 = 307;
  public const int KIMAT2x3 = 308;
  public const int KIMAT2x4 = 309;
  public const int KIMAT3x2 = 310;
  public const int KIMAT3 = 311;
  public const int KIMAT3x4 = 312;
  public const int KIMAT4x2 = 313;
  public const int KIMAT4x3 = 314;
  public const int KIMAT4 = 315;
  public const int KNULL = 316;
  public const int KBASE = 317;
  public const int KTHIS = 318;
  public const int KCLASS = 319;
  public const int KDELEGATE = 320;
  public const int KEVENT = 321;
  public const int KINTERFACE = 322;
  public const int KNAMESPACE = 323;
  public const int KSTRUCT = 324;
  public const int KENUM = 325;
  public const int KOPERATOR = 326;
  public const int KBREAK = 327;
  public const int KCASE = 328;
  public const int KCATCH = 329;
  public const int KCONTINUE = 330;
  public const int KDO = 331;
  public const int KELSE = 332;
  public const int KFINALLY = 333;
  public const int KFOR = 334;
  public const int KFOREACH = 335;
  public const int KFIXED = 336;
  public const int KGOTO = 337;
  public const int KIF = 338;
  public const int KIN = 339;
  public const int KLOCK = 340;
  public const int KRETURN = 341;
  public const int KWHILE = 342;
  public const int KDELETE = 343;
  public const int KDEFAULT = 344;
  public const int KUSING = 345;
  public const int KSWITCH = 346;
  public const int KTHROW = 347;
  public const int KTRY = 348;
  public const int KEXTERN = 349;
  public const int KPUBLIC = 350;
  public const int KINTERNAL = 351;
  public const int KPROTECTED = 352;
  public const int KPRIVATE = 353;
  public const int KKERNEL = 354;
  public const int KSTATIC = 355;
  public const int KVIRTUAL = 356;
  public const int KOVERRIDE = 357;
  public const int KABSTRACT = 358;
  public const int KSEALED = 359;
  public const int KREADONLY = 360;
  public const int KUNSAFE = 361;
  public const int KPARTIAL = 362;
  public const int KCDECL = 363;
  public const int KSTDCALL = 364;
  public const int KAPICALL = 365;
  public const int LAND = 366;
  public const int LOR = 367;
  public const int LNOT = 368;
  public const int LBITSHIFT = 369;
  public const int RBITSHIFT = 370;
  public const int BITAND = 371;
  public const int BITOR = 372;
  public const int BITXOR = 373;
  public const int BITNOT = 374;
  public const int LEQ = 375;
  public const int GEQ = 376;
  public const int EQ = 377;
  public const int NEQ = 378;
  public const int IS = 379;
  public const int AS = 380;
  public const int ARROW = 381;
  public const int NEW = 382;
  public const int HEAPALLOC = 383;
  public const int STACKALLOC = 384;
  public const int CHECKED = 385;
  public const int UNCHECKED = 386;
  public const int REINTERPRET_CAST = 387;
  public const int GENERIC_START = 388;
  public const int GENERIC_END = 389;
  public const int GENERIC_START_DOT = 390;
  public const int GENERIC_END_DOT = 391;
  public const int INCR = 392;
  public const int DECR = 393;
  public const int ADD_SET = 394;
  public const int SUB_SET = 395;
  public const int MUL_SET = 396;
  public const int DIV_SET = 397;
  public const int MOD_SET = 398;
  public const int OR_SET = 399;
  public const int XOR_SET = 400;
  public const int AND_SET = 401;
  public const int LSHIFT_SET = 402;
  public const int RSHIFT_SET = 403;
  public const int SIZEOF = 404;
  public const int TYPEOF = 405;
  public const int FUNC_PTR = 406;
  public const int BYTE = 407;
  public const int SBYTE = 408;
  public const int SHORT = 409;
  public const int USHORT = 410;
  public const int INTEGER = 411;
  public const int UINTEGER = 412;
  public const int LONG = 413;
  public const int ULONG = 414;
  public const int BOOL = 415;
  public const int FLOAT = 416;
  public const int DOUBLE = 417;
  public const int CHARACTER = 418;
  public const int CSTRING = 419;
  public const int STRING = 420;
  public const int IDENTIFIER = 421;
  public const int KREF = 422;
  public const int KOUT = 423;
  public const int KPARAMS = 424;
  public const int yyErrorCode = 256;
 }
 namespace yyParser {
  using System;
  /** thrown for irrecoverable syntax errors and stack overflow.
    */
  internal class yyException : System.Exception {
    public yyException (string message) : base (message) {
    }
  }
  internal class yyUnexpectedEof : yyException {
    public yyUnexpectedEof (string message) : base (message) {
    }
    public yyUnexpectedEof () : base ("") {
    }
  }

  /** must be implemented by a scanner object to supply input to the parser.
    */
  internal interface yyInput {
    /** move on to next token.
        @return false if positioned beyond tokens.
        @throws IOException on input error.
      */
    bool advance (); // throws java.io.IOException;
    /** classifies current token.
        Should not be called if advance() returned false.
        @return current %token or single character.
      */
    int token ();
    /** associated with current token.
        Should not be called if advance() returned false.
        @return value for token().
      */
    Object value ();
  }
 }
} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
