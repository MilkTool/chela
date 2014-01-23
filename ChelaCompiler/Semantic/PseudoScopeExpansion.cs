using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class PseudoScopeExpansion: ObjectDeclarator
    {
        public PseudoScopeExpansion()
        {
        }

        public override AstNode Visit (UsingStatement node)
        {
            // Visit the using member.
            Expression member = node.GetMember();
            member.Accept(this);

            // Cast the pseudo-scope.
            PseudoScope scope = (PseudoScope)node.GetScope();

            // Get the member type.
            IChelaType memberType = member.GetNodeType();
            if(memberType.IsMetaType())
            {
                // Get the actual type.
                MetaType meta = (MetaType) memberType;
                memberType = meta.GetActualType();

                // Only structure relatives.
                if(memberType.IsStructure() || memberType.IsClass() || memberType.IsInterface())
                {
                    scope.AddAlias(memberType.GetName(), (ScopeMember)memberType);
                }
                else
                {
                    Error(node, "unexpected type.");
                }
            }
            else if(memberType.IsReference())
            {
                // Only static members supported.
                ScopeMember theMember = (ScopeMember)member.GetNodeValue();
                MemberFlags instanceFlags = MemberFlags.InstanceMask & theMember.GetFlags();
                if(instanceFlags != MemberFlags.Static)
                    Error(node, "unexpected member type.");

                // Store the member.
                scope.AddAlias(theMember.GetName(), theMember);

            }
            else if(memberType.IsNamespace())
            {
                // Set the namespace chain.
                Namespace space = (Namespace)member.GetNodeValue();
                scope.SetChainNamespace(space);
            }
            else
            {
                Error(node, "unsupported object to use.");
            }

            base.Visit(node);

            return node;
        }
    }
}

