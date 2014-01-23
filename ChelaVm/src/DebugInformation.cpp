#include "ChelaVm/DebugInformation.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/FunctionLocal.hpp"
#include "llvm/Support/Dwarf.h"

using namespace llvm::dwarf;
using llvm::LLVMDebugVersion;

namespace ChelaVm
{
    inline std::string GetFileDirectory(const std::string &fileName)
    {
        return fileName.substr(0, fileName.rfind('/'));
    }

    inline std::string GetFileName(const std::string &fileName)
    {
        return fileName.substr(fileName.rfind('/') + 1, std::string::npos);
    }

    inline std::string JoinPath(const std::string &left, const std::string &right)
    {
        return left + "/" + right;
    }

    // TODO: Standardize language and language id.
    const int DW_LANG_Chela = DW_LANG_C_plus_plus;//DW_LANG_lo_user + 123;
    const char *DebugProducerString = "0.1 ChelaVM";

    // Source position.
    SourcePosition::SourcePosition(DebugInformation *debugInfo)
        : debugInfo(debugInfo)
    {
    }

    const std::string &SourcePosition::GetFileName() const
    {
        return debugInfo->GetString(fileName);
    }

    void SourcePosition::Read(ModuleReader &reader)
    {
        reader >> fileName >> line >> column;
    }

    // Member debug information.
    MemberDebugInfo::MemberDebugInfo(DebugInformation *debugInfo, uint32_t memberId)
        : debugInfo(debugInfo), memberId(memberId)
    {
        module = debugInfo->GetModule();
    }

    MemberDebugInfo::~MemberDebugInfo()
    {
    }

    bool MemberDebugInfo::IsFunction() const
    {
        return false;
    }

    bool MemberDebugInfo::IsStructure() const
    {
        return false;
    }

    bool MemberDebugInfo::IsField() const
    {
        return false;
    }

    Member *MemberDebugInfo::GetMember()
    {
        return module->GetMember(memberId);
    }

    // Local variable debug information.
    LocalDebugInfo::LocalDebugInfo(DebugInformation *debugInfo)
        : debugInfo(debugInfo), position(debugInfo)
    {
        scopeIndex = 0;
        localName = 0;
    }

    LocalDebugInfo::~LocalDebugInfo()
    {
    }

    const std::string &LocalDebugInfo::GetName() const
    {
        return debugInfo->GetString(localName);
    }

    size_t LocalDebugInfo::GetScopeIndex() const
    {
        return scopeIndex;
    }

    LocalType LocalDebugInfo::GetLocalType() const
    {
        return localType;
    }

    int LocalDebugInfo::GetArgumentIndex() const
    {
        return argumentIndex;
    }

    const SourcePosition &LocalDebugInfo::GetPosition() const
    {
        return position;
    }

    llvm::DIVariable LocalDebugInfo::CreateDescriptor(FunctionLocal *local, llvm::DIDescriptor parentScope)
    {
        // Select the local tag.
        int tag;
        switch(localType)
        {
        case LT_Argument:
            tag = DW_TAG_arg_variable;
            break;
        case LT_Return:
            tag = DW_TAG_return_variable;
            break;
        case LT_Generated:
        case LT_Normal:
        default:
            tag = DW_TAG_auto_variable;
            break;
        }

        llvm::DIType debugType = local->GetType()->GetDebugType(debugInfo);
        if(!debugType.Verify())
            throw ModuleException("Failed to get debug type for " + local->GetType()->GetFullName());
        return debugInfo->GetDebugBuilder()
            .createLocalVariable(tag, parentScope, GetName(),
                debugInfo->GetFileDescriptor(position.GetFileName()),
                position.GetLine(), debugType,
                true, 0, argumentIndex);
    }

    void LocalDebugInfo::Read(ModuleReader &reader)
    {
        // Read the local name, type and the argument index.
        uint8_t type;
        reader >> localName >> scopeIndex >> type >> argumentIndex;
        localType = (LocalType)type;

        // Read the position.
        position.Read(reader);
    }

    // Basic block instruction range positions.
    BasicBlockRangePosition::BasicBlockRangePosition(DebugInformation *debugInfo)
        : position(debugInfo)
    {
    }

    BasicBlockRangePosition::~BasicBlockRangePosition()
    {
    }

    void BasicBlockRangePosition::Read(ModuleReader &reader)
    {
        reader >> start >> end;
        position.Read(reader);
    }

    // Basic block debug information.
    BasicBlockDebugInfo::BasicBlockDebugInfo(DebugInformation *debugInfo)
        : debugInfo(debugInfo)
    {
    }

    BasicBlockDebugInfo::~BasicBlockDebugInfo()
    {
    }

    size_t BasicBlockDebugInfo::GetRangeCount() const
    {
        return subBlocks.size();
    }

    const BasicBlockRangePosition &BasicBlockDebugInfo::GetRange(size_t index) const
    {
        if(index >= subBlocks.size())
            throw ModuleException("Invalid block range description.");
        return subBlocks[index];
    }

    void BasicBlockDebugInfo::Read(ModuleReader &reader)
    {
        // Read the sub-blocks.
        uint16_t numsubs;
        reader >> numsubs;
        subBlocks.resize(numsubs, BasicBlockRangePosition(debugInfo));
        for(size_t i = 0; i < numsubs; ++i)
            subBlocks[i].Read(reader);
    }

    // Lexical scope debug information
    LexicalScopeDebugInfo::LexicalScopeDebugInfo(DebugInformation *debugInfo, FunctionDebugInfo *functionInfo)
        : debugInfo(debugInfo), functionInfo(functionInfo), parentScope(0), position(debugInfo)
    {
    }

    LexicalScopeDebugInfo::~LexicalScopeDebugInfo()
    {
    }

    LexicalScopeDebugInfo *LexicalScopeDebugInfo::GetParentScope()
    {
        return functionInfo->GetLexicalScope(parentScope);
    }

    const SourcePosition &LexicalScopeDebugInfo::GetPosition()
    {
        return position;
    }

    llvm::DILexicalBlock LexicalScopeDebugInfo::GetDescriptor()
    {
        // Return the already created descriptor.
        if(descriptor)
            return descriptor;

        // Get my parent descriptor.
        LexicalScopeDebugInfo *parent = GetParentScope();
        llvm::DIDescriptor parentDesc;
        if(parent == this)
            parentDesc = functionInfo->GetDescriptor();
        else
            parentDesc = parent->GetDescriptor();

        // Create my descriptor.
        descriptor = debugInfo->GetDebugBuilder()
            .createLexicalBlock(parentDesc, debugInfo->GetFileDescriptor(position.GetFileName()),
                position.GetLine(), position.GetColumn());

        // Return the created descriptor.
        return descriptor;
    }

    void LexicalScopeDebugInfo::Read(ModuleReader &reader)
    {
        // Read the parent scope.
        reader >> parentScope;

        // Read the scope position.
        position.Read(reader);
    }

    // Function debug information.
    FunctionDebugInfo::FunctionDebugInfo(DebugInformation *debugInfo, uint32_t memberId)
        : MemberDebugInfo(debugInfo, memberId), position(debugInfo)
    {
    }

    FunctionDebugInfo::~FunctionDebugInfo()
    {
        for(size_t i = 0; i < lexicalScopes.size(); ++i)
            delete lexicalScopes[i];

        for(size_t i = 0; i < locals.size(); ++i)
            delete locals[i];

        for(size_t i = 0; i < basicBlocks.size(); ++i)
            delete basicBlocks[i];
    }

    bool FunctionDebugInfo::IsFunction() const
    {
        return true;
    }

    const SourcePosition &FunctionDebugInfo::GetPosition()
    {
        return position;
    }

    size_t FunctionDebugInfo::GetLexicalScopeCount() const
    {
        return lexicalScopes.size();
    }

    LexicalScopeDebugInfo *FunctionDebugInfo::GetLexicalScope(size_t index) const
    {
        if(index >= lexicalScopes.size())
            throw ModuleException("invalid debug lexical scope index.");
        return lexicalScopes[index];
    }

    LocalDebugInfo *FunctionDebugInfo::GetLocal(size_t index) const
    {
        if(index >= locals.size())
            throw ModuleException("invalid debug local index.");
        return locals[index];
    }

    BasicBlockDebugInfo *FunctionDebugInfo::GetBlock(size_t index) const
    {
        if(index >= basicBlocks.size())
            throw ModuleException("invalid debug block index.");
        return basicBlocks[index];
    }

    llvm::DISubprogram FunctionDebugInfo::CreateDescriptor(Function *function)
    {
        // Get the function type.
        const FunctionType *functionType = function->GetFunctionType();

        // Create the descriptor.
        descriptor = debugInfo->GetDebugBuilder()
            .createFunction(function->GetParent()->GetDebugNode(debugInfo),
                            function->GetName(), function->GetMangledName(),
                            debugInfo->GetFileDescriptor(position.GetFileName()),
                            position.GetLine(), functionType->GetDebugType(debugInfo),
                            !function->IsPublic(), !function->IsExtern(), 0,
                            true, function->GetTarget());

        // TODO: Add virtuality support.

        return descriptor;
    }

    llvm::DIDescriptor FunctionDebugInfo::GetDescriptor()
    {
        return descriptor;
    }

    void FunctionDebugInfo::Read(ModuleReader &reader)
    {
        // Read the position.
        position.Read(reader);

        // Read the lexical scopes.
        uint8_t lexicalCount;
        reader >> lexicalCount;
        lexicalScopes.reserve(lexicalCount);
        for(size_t i = 0; i < lexicalCount; ++i)
        {
            LexicalScopeDebugInfo *scope = new LexicalScopeDebugInfo(debugInfo, this);
            scope->Read(reader);
            lexicalScopes.push_back(scope);
        }

        // Read the local data.
        uint16_t localCount;
        reader >> localCount;
        locals.reserve(localCount);
        for(size_t i = 0; i < localCount; ++i)
        {
            LocalDebugInfo *local = new LocalDebugInfo(debugInfo);
            local->Read(reader);
            locals.push_back(local);
        }

        // Read the basic blocks.
        uint16_t blockCount;
        reader >> blockCount;
        basicBlocks.reserve(blockCount);
        for(size_t i = 0; i < blockCount; ++i)
        {
            BasicBlockDebugInfo *block = new BasicBlockDebugInfo(debugInfo);
            block->Read(reader);
            basicBlocks.push_back(block);
        }
    }

    // Structure debug info.
    StructureDebugInfo::StructureDebugInfo(DebugInformation *debugInfo, uint32_t memberId)
        : MemberDebugInfo(debugInfo, memberId), position(debugInfo)
    {
    }

    StructureDebugInfo::~StructureDebugInfo()
    {
    }

    bool StructureDebugInfo::IsStructure() const
    {
        return true;
    }

    const SourcePosition &StructureDebugInfo::GetPosition()
    {
        return position;
    }

    void StructureDebugInfo::Read(ModuleReader &reader)
    {
        // Read the position.
        position.Read(reader);
    }

    // Field debug info.
    FieldDebugInfo::FieldDebugInfo(DebugInformation *debugInfo, uint32_t memberId)
        : MemberDebugInfo(debugInfo, memberId), position(debugInfo)
    {
    }

    FieldDebugInfo::~FieldDebugInfo()
    {
    }

    bool FieldDebugInfo::IsField() const
    {
        return true;
    }

    const SourcePosition &FieldDebugInfo::GetPosition()
    {
        return position;
    }

    void FieldDebugInfo::Read(ModuleReader &reader)
    {
        // Read the position.
        position.Read(reader);
    }

    // Debug information
    DebugInformation::DebugInformation(Module *module)
        : module(module), builder(module->GetLlvmContext()), diBuilder(*module->GetTargetModule())
    {
    }

    DebugInformation::~DebugInformation()
    {
        MemberDebugInfoMap::iterator it = memberDebugInfoMap.begin();
        for(; it != memberDebugInfoMap.end(); ++it)
            delete it->second;
    }

    Module *DebugInformation::GetModule()
    {
        return module;
    }

    const std::string &DebugInformation::GetString(uint32_t id) const
    {
        static std::string emptyStr = "";
        if(id == 0)
            return emptyStr;
        else if((id-1) >= stringTable.size())
            throw ModuleException("invalid debug string id");
        return stringTable[id-1];
    }

    MemberDebugInfo *DebugInformation::GetDebugMember(uint32_t id) const
    {
        MemberDebugInfoMap::const_iterator it = memberDebugInfoMap.find(id);
        if(it != memberDebugInfoMap.end())
            return it->second;
        return NULL;
    }

    FunctionDebugInfo *DebugInformation::GetFunctionDebug(uint32_t id) const
    {
        MemberDebugInfo *debugInfo = GetDebugMember(id);
        if(!debugInfo)
            return NULL;
        if(!debugInfo->IsFunction())
            throw ModuleException("Expected function debug info.");
        return static_cast<FunctionDebugInfo*> (debugInfo);
    }

    StructureDebugInfo *DebugInformation::GetStructureDebug(uint32_t id) const
    {
        MemberDebugInfo *debugInfo = GetDebugMember(id);
        if(!debugInfo)
            return NULL;
        if(!debugInfo->IsStructure())
            throw ModuleException("Expected structure debug info.");
        return static_cast<StructureDebugInfo*> (debugInfo);
    }

    FieldDebugInfo *DebugInformation::GetFieldDebug(uint32_t id) const
    {
        MemberDebugInfo *debugInfo = GetDebugMember(id);
        if(!debugInfo)
            return NULL;
        if(!debugInfo->IsField())
            throw ModuleException("Expected field debug info.");
        return static_cast<FieldDebugInfo*> (debugInfo);
    }

    void DebugInformation::Read(ModuleReader &reader, size_t infoSize)
    {
        // Compute the end offset
        size_t endOffset = reader.GetPosition() + infoSize;

        // Read the debug string table.
        uint32_t numStringTableRecords;
        reader >> numStringTableRecords;
        stringTable.resize(numStringTableRecords);
        for(size_t i = 0; i < numStringTableRecords; ++i)
            reader >> stringTable[i];

        // Read the module file name and work directory.
        reader >> fileName >> workDirectory;

        // Read the functions debug information.
        uint32_t numFunctions;
        reader >> numFunctions;
        for(uint32_t i = 0; i < numFunctions; ++i)
            ReadFunctionDebugInfo(reader);

        // Read the structures debug information.
        uint32_t numStructures;
        reader >> numStructures;
        for(uint32_t i = 0; i < numStructures; ++i)
            ReadStructureDebugInfo(reader);

        // Read the fields debug information.
        uint32_t numFields;
        reader >> numFields;
        for(uint32_t i = 0; i < numFields; ++i)
            ReadFieldDebugInfo(reader);

        // Check all of the information has been readed.
        if(endOffset != reader.GetPosition())
            printf("Warning, incomplete debug info readed\n");

        // Create the compile unit.
        CreateCompileUnit();
    }

    void DebugInformation::ReadFunctionDebugInfo(ModuleReader &reader)
    {
        // Read the function id.
        uint32_t functionId;
        reader >> functionId;

        // Create an object to hold the debug information.
        FunctionDebugInfo *functionDebug = new FunctionDebugInfo(this, functionId);
        memberDebugInfoMap.insert(std::make_pair(functionId, functionDebug));

        // Read the function data.
        functionDebug->Read(reader);
    }

    void DebugInformation::ReadStructureDebugInfo(ModuleReader &reader)
    {
        // Read the structure id.
        uint32_t structureId;
        reader >> structureId;

        // Create an object to hold the debug information.
        StructureDebugInfo *structDebug = new StructureDebugInfo(this, structureId);
        memberDebugInfoMap.insert(std::make_pair(structureId, structDebug));

        // Read the structure data.
        structDebug->Read(reader);
    }

    void DebugInformation::ReadFieldDebugInfo(ModuleReader &reader)
    {
        // Read the field id.
        uint32_t fieldId;
        reader >> fieldId;

        // Create an object to hold the debug information.
        FieldDebugInfo *fieldDebug = new FieldDebugInfo(this, fieldId);
        memberDebugInfoMap.insert(std::make_pair(fieldId, fieldDebug));

        // Read the field data.
        fieldDebug->Read(reader);
    }

    SourcePosition DebugInformation::ReadSourcePosition(ModuleReader &reader)
    {
        SourcePosition res(this);
        res.Read(reader);
        return res;
    }

    // Create the compile unit data.
    void DebugInformation::CreateCompileUnit()
    {
        diBuilder.createCompileUnit(DW_LANG_Chela,
            GetString(fileName),
            GetString(workDirectory),
            DebugProducerString,
            true, // TODO: Add support for unoptimized case.
            "", // TODO: Get the flags string
            0); // TODO: Get the runtime version.
    }

    llvm::DISubprogram DebugInformation::CreateDummySubprogram(Function *function)
    {
        return diBuilder.createFunction(function->GetParent()->GetDebugNode(this),
            function->GetName(), function->GetMangledName(),
            GetModuleFileDescriptor(), 0,
            llvm::DIType()/*function->GetFunctionType()->GetDebugType(this)*/, !function->IsPublic(),
            false, /*ScopeLine*/ 0);
    }

    llvm::DIFile DebugInformation::GetFileDescriptor(const std::string &fileName)
    {
        // Find the file descriptor.
        FileDescriptors::iterator it = fileDescriptors.find(fileName);
        if(it != fileDescriptors.end())
            return it->second;

        // Cache the file descriptor.
        llvm::DIFile ret = diBuilder.createFile(fileName, GetString(workDirectory));
        fileDescriptors.insert(std::make_pair(fileName, ret));
        return ret;
    }

    llvm::DIFile DebugInformation::GetModuleFileDescriptor()
    {
        if(moduleFile)
            return moduleFile;

        // Create the module file.
        moduleFile = diBuilder.createFile(GetString(fileName), GetString(workDirectory));
        return moduleFile;
    }

    llvm::DISubprogram DebugInformation::DeclareFunction(Function *function)
    {
        // Only declare my functions.
        if(function->GetDeclaringModule() != module)
            return CreateDummySubprogram(function);

        // Get the function debug info.
        FunctionDebugInfo *functionDebug = GetFunctionDebug(function->GetMemberId());
        if(functionDebug == NULL)
            return CreateDummySubprogram(function);

        // TODO: Support virtual/abstract functions
        if(function->IsAbstract() || function->IsContract())
            return llvm::DISubprogram();

        // Create the descriptor.
        llvm::DISubprogram desc = functionDebug->CreateDescriptor(function);
        RegisterMemberNode(function, desc);
        return desc;
    }

    llvm::DILexicalBlock DebugInformation::GetTopLexicalScope(Function *function)
    {
        // Only declare my functions.
        if(function->GetDeclaringModule() != module)
            return llvm::DILexicalBlock();

        // Get the function debug info.
        FunctionDebugInfo *functionDebug = GetFunctionDebug(function->GetMemberId());
        if(functionDebug == NULL)
            return llvm::DILexicalBlock();

        // TODO: Support virtual/abstract functions
        if(function->IsAbstract() || function->IsContract())
            return llvm::DILexicalBlock();

        // TODO: Deprecate this.
        if(functionDebug->GetLexicalScopeCount() == 0)
            return llvm::DILexicalBlock();
        return functionDebug->GetLexicalScope(0)->GetDescriptor();
    }

    void DebugInformation::FinishDebug()
    {
        // Make opaque types concrete.
        DebugTypes::iterator it = debugTypes.begin();
        for(; it != debugTypes.end(); ++it)
        {
            //printf("Finish %s\n", it->first->GetFullName().c_str());
            //it->second.dump();
            if(!it->second || !it->second.getContext())
                it->first->GetDebugType(this);
        }

        // Write deferred data.
        diBuilder.finalize();
    }

    void DebugInformation::RegisterMemberNode(const Member *member, llvm::DIDescriptor desc)
    {
        memberNodes.insert(std::make_pair(member, desc));
    }

    llvm::DIDescriptor DebugInformation::GetMemberNode(const Member *member)
    {
        MemberNodes::iterator it = memberNodes.find(member);
        if(it != memberNodes.end())
            return it->second;
        return llvm::DIDescriptor();
    }

    llvm::DIType DebugInformation::GetCachedDebugType(const ChelaType *type)
    {
        DebugTypes::iterator it = debugTypes.find(type);
        if(it != debugTypes.end())
            return it->second;
        return llvm::DIType();
    }

    void DebugInformation::RegisterDebugType(const ChelaType *type, llvm::DIType debugType)
    {
        debugTypes.insert(std::make_pair(type, debugType));
    }

    void DebugInformation::ReplaceDebugType(const ChelaType *type, llvm::DIType debugType)
    {
        // Find the old definition.
        DebugTypes::iterator it = debugTypes.find(type);
        if(it != debugTypes.end())
        {
            llvm::DIType oldType = it->second;
            it->second = debugType;
            if(oldType != debugType)
                oldType->replaceAllUsesWith(debugType);
        }
        else
        {
            RegisterDebugType(type, debugType);
        }
    }

    llvm::IRBuilder<> &DebugInformation::GetBuilder()
    {
        return builder;
    }

    llvm::DIBuilder &DebugInformation::GetDebugBuilder()
    {
        return diBuilder;
    }
}
