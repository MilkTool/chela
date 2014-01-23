#ifndef CHELA_DEBUG_INFORMATION_HPP
#define CHELA_DEBUG_INFORMATION_HPP

#include "Module.hpp"
#include "ModuleReader.hpp"
#include "llvm/Metadata.h"
#include "llvm/Support/IRBuilder.h"
#include "llvm/Analysis/DebugInfo.h"
#include "llvm/Analysis/DIBuilder.h"

namespace ChelaVm
{
    // Local variable type.
    enum LocalType
    {
        LT_Normal = 0,
        LT_Argument,
        LT_Generated,
        LT_Return,
    };

    // Source position data.
    class DebugInformation;
    class SourcePosition
    {
    public:
        SourcePosition(DebugInformation *debugInfo);

        const std::string &GetFileName() const;

        inline unsigned int GetLine() const
        {   return line; }

        inline unsigned int GetColumn() const
        {   return column; }

        void Read(ModuleReader &reader);

    private:
        DebugInformation *debugInfo;
        uint32_t fileName;
        unsigned int line;
        unsigned int column;
    };

    // Member debug information.
    class MemberDebugInfo
    {
    public:
        MemberDebugInfo(DebugInformation *debugInfo, uint32_t memberId);
        ~MemberDebugInfo();

        virtual bool IsFunction() const;
        virtual bool IsStructure() const;
        virtual bool IsField() const;

        Member *GetMember();

    protected:
        Module *module;
        DebugInformation *debugInfo;
        uint32_t memberId;
    };

    // Local variable debug information.
    class FunctionLocal;
    class LocalDebugInfo
    {
    public:
        LocalDebugInfo(DebugInformation *debugInfo);
        ~LocalDebugInfo();

        const std::string &GetName() const;
        size_t GetScopeIndex() const;
        LocalType GetLocalType() const;
        int GetArgumentIndex() const;

        const SourcePosition &GetPosition() const;

        llvm::DIVariable CreateDescriptor(FunctionLocal *local, llvm::DIDescriptor parentScope);
        void Read(ModuleReader &reader);

    private:
        DebugInformation *debugInfo;
        uint32_t localName;
        uint8_t scopeIndex;
        LocalType localType;
        uint8_t argumentIndex;
        SourcePosition position;
    };

    // BasicBlock subrange position.
    class BasicBlockRangePosition
    {
    public:
        BasicBlockRangePosition(DebugInformation *debugInfo);
        ~BasicBlockRangePosition();

        inline unsigned int GetStart() const
        {   return start; }

        inline unsigned int GetEnd() const
        {   return end; }

        inline const SourcePosition &GetPosition() const
        {   return position; }

        void Read(ModuleReader &reader);

    private:
        uint16_t start;
        uint16_t end;
        SourcePosition position;
    };

    // BasicBlock debug information.
    class BasicBlockDebugInfo
    {
    public:
        BasicBlockDebugInfo(DebugInformation *debugInfo);
        ~BasicBlockDebugInfo();

        size_t GetRangeCount() const;
        const BasicBlockRangePosition &GetRange(size_t index) const;

        void Read(ModuleReader &reader);

    private:
        DebugInformation *debugInfo;
        std::vector<BasicBlockRangePosition> subBlocks;
    };

    // Lexical scope debug info.
    class FunctionDebugInfo;
    class LexicalScopeDebugInfo
    {
    public:
        LexicalScopeDebugInfo(DebugInformation *debugInfo, FunctionDebugInfo *functionInfo);
        ~LexicalScopeDebugInfo();

        LexicalScopeDebugInfo *GetParentScope();
        const SourcePosition &GetPosition();

        llvm::DILexicalBlock GetDescriptor();

        void Read(ModuleReader &reader);

    private:
        DebugInformation *debugInfo;
        FunctionDebugInfo *functionInfo;
        uint8_t parentScope;
        SourcePosition position;
        llvm::DILexicalBlock descriptor;
    };

    // Function debug information.
    class FunctionDebugInfo: public MemberDebugInfo
    {
    public:
        FunctionDebugInfo(DebugInformation *debugInfo, uint32_t memberId);
        ~FunctionDebugInfo();

        virtual bool IsFunction() const;

        const SourcePosition &GetPosition();

        size_t GetLexicalScopeCount() const;
        LexicalScopeDebugInfo *GetLexicalScope(size_t index) const;

        LocalDebugInfo *GetLocal(size_t index) const;

        BasicBlockDebugInfo *GetBlock(size_t index) const;

        llvm::DISubprogram CreateDescriptor(Function *function);
        llvm::DIDescriptor GetDescriptor();

        void Read(ModuleReader &reader);

    private:
        SourcePosition position;
        std::vector<uint32_t> argNames;
        std::vector<LexicalScopeDebugInfo*> lexicalScopes;
        std::vector<LocalDebugInfo*> locals;
        std::vector<BasicBlockDebugInfo*> basicBlocks;
        llvm::DISubprogram descriptor;
    };

    // Structure debug information.
    class StructureDebugInfo: public MemberDebugInfo
    {
    public:
        StructureDebugInfo(DebugInformation *debugInfo, uint32_t memberId);
        ~StructureDebugInfo();

        virtual bool IsStructure() const;

        const SourcePosition &GetPosition();

        void Read(ModuleReader &reader);

    private:
        SourcePosition position;
    };

    // Field debug information.
    class FieldDebugInfo: public MemberDebugInfo
    {
    public:
        FieldDebugInfo(DebugInformation *debugInfo, uint32_t memberId);
        ~FieldDebugInfo();

        virtual bool IsField() const;

        const SourcePosition &GetPosition();

        void Read(ModuleReader &reader);

    private:
        SourcePosition position;
    };

    // Debug information root class
    class DebugInformation
    {
    public:
        DebugInformation(Module *module);
        ~DebugInformation();

        Module *GetModule();
        const std::string &GetString(uint32_t id) const;
        MemberDebugInfo *GetDebugMember(uint32_t id) const;

        FunctionDebugInfo *GetFunctionDebug(uint32_t id) const;
        StructureDebugInfo *GetStructureDebug(uint32_t id) const;
        FieldDebugInfo *GetFieldDebug(uint32_t id) const;

        void Read(ModuleReader &reader, size_t infoSize);

        llvm::DIFile GetFileDescriptor(const std::string &fileName);
        llvm::DIFile GetModuleFileDescriptor();

        void RegisterMemberNode(const Member *member, llvm::DIDescriptor desc);
        llvm::DIDescriptor GetMemberNode(const Member *member);

        llvm::DIType GetCachedDebugType(const ChelaType *type);
        void RegisterDebugType(const ChelaType *type, llvm::DIType debugType);
        void ReplaceDebugType(const ChelaType *type, llvm::DIType debugType);

        llvm::IRBuilder<> &GetBuilder();
        llvm::DIBuilder &GetDebugBuilder();

        llvm::DISubprogram DeclareFunction(Function *function);
        llvm::DILexicalBlock GetTopLexicalScope(Function *function);
        void FinishDebug();

    private:
        typedef std::map<std::string, llvm::DIFile> FileDescriptors;
        typedef std::map<uint32_t, MemberDebugInfo*> MemberDebugInfoMap;
        typedef std::map<const ChelaType*, llvm::DIType> DebugTypes;
        typedef std::map<const Member*, llvm::DIDescriptor> MemberNodes;

        void CreateCompileUnit();
        llvm::DISubprogram CreateDummySubprogram(Function *function);
        void ReadFunctionDebugInfo(ModuleReader &reader);
        void ReadStructureDebugInfo(ModuleReader &reader);
        void ReadFieldDebugInfo(ModuleReader &reader);
        SourcePosition ReadSourcePosition(ModuleReader &reader);

        Module *module;
        std::vector<std::string> stringTable;
        MemberDebugInfoMap memberDebugInfoMap;

        // Module data.
        uint32_t fileName;
        uint32_t workDirectory;

        // Metadata nodes.
        MemberNodes memberNodes;
        DebugTypes debugTypes;
        FileDescriptors fileDescriptors;
        llvm::DIFile moduleFile;

        // Intermediate builder.
        llvm::IRBuilder<> builder;
        llvm::DIBuilder diBuilder;
    };
}

#endif //CHELA_DEBUG_INFORMATION_HPP
