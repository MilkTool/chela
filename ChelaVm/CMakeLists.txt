# Platform specific libraries.
if(${CMAKE_HOST_WIN32})
    set(HOST_LIBS psapi imagehlp)
elseif(${CMAKE_HOST_UNIX})
    set(HOST_LIBS pthread m)
    if(LIB_DL)
        set(HOST_LIBS ${HOST_LIBS} dl)
    endif()
endif()

# Use the includes.
include_directories(
    include)

# ChelaVm source files.
set(ChelaVm_SRC
    src/ArgumentData.cpp
    src/AttributeConstant.cpp
    src/Class.cpp
    src/Closure.cpp
    src/ConstantValue.cpp
    src/DebugInformation.cpp
    src/ELFFormat.cpp
    src/Field.cpp
    src/FileSystem.cpp
    src/Function.cpp
    src/FunctionArgument.cpp
    src/FunctionBlock.cpp
    src/FunctionGroup.cpp
    src/FunctionInstance.cpp
    src/FunctionLocal.cpp
    src/GenericInstance.cpp
    src/GenericPrototype.cpp
    src/Interface.cpp
    src/main.cpp
    src/Member.cpp
    src/MemberInstance.cpp
    src/Module.cpp
    src/ModuleFile.cpp
    src/Namespace.cpp
    src/PEFormat.cpp
    src/PlaceHolderType.cpp
    src/Property.cpp
    #src/RefCountPlugin.cpp
    src/SpecialFunctions.cpp
    src/Structure.cpp
    src/System_Unix.cpp
    src/System_Win32.cpp
    src/Types.cpp
    src/TypeGroup.cpp
    src/TypeInstance.cpp
    src/VirtualMachine.cpp
)

# Use llvm flags.
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${LLVM_CFLAGS} ")
set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} ${LLVM_LFLAGS} ")

# Create the virtual machine.
add_executable(chlc-vm ${ChelaVm_SRC})
target_link_libraries(chlc-vm ${LLVM_JIT_LIBS} ${HOST_LIBS})

