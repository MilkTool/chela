# Find programs.

# Don't use mono win windows.
if(${CMAKE_HOST_WIN32})
	set(MONO )
    set(MONO_DBG)
else()
	find_program(MONO mono DOC "path to the mono runner")
	set(MONO_DBG ${MONO} --debug)
endif()

# Find the C# compiler.
find_program(CSHARP_COMPILER NAMES csc gmcs mcs DOC "path to the csharp compiler")

macro(PREFIX_LIST VAR LIST PREFIX)
    set(${VAR})
    foreach(ARG ${${LIST}})
        list(APPEND ${VAR} "${PREFIX}${ARG}")
    endforeach()
endmacro()

# Macro to build a library.
macro(CSHARP_LIBRARY Target)
    set(${Target}_dest_ "${LIBRARY_OUTPUT_PATH}/${Target}.dll" CACHE INTERNAL "output file" FORCE)
    add_custom_command(OUTPUT ${${Target}_dest_}
        COMMAND ${CSHARP_COMPILER}
        ARGS -target:library "-out:${${Target}_dest_}" ${CSHARP_FLAGS} ${ARGN}
        DEPENDS ${ARGN}
        WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
    add_custom_target(${Target} ALL DEPENDS ${${Target}_dest_})
endmacro()

# Macro to build a program.
macro(CSHARP_PROGRAM Target Ref)
    set(${Target}_dest_ "${EXECUTABLE_OUTPUT_PATH}/${Target}.exe" CACHE INTERNAL "output file" FORCE)
    #message("prog ${${Target}_dest_}")
    add_custom_command(OUTPUT ${${Target}_dest_}
        COMMAND ${CSHARP_COMPILER}
        ARGS -target:exe "-r:${${Ref}_dest_}" "-out:${${Target}_dest_}" ${CSHARP_FLAGS} ${ARGN}
        DEPENDS ${${Ref}_dest_} ${ARGN}
        WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
    add_custom_target(${Target} ALL DEPENDS ${${Target}_dest_})
endmacro()

# Macro to build a c# module.
function(CSHARP_MODULE Target)
    # Initialize target variables.
    set(References)
    set(RefDeps)
    set(SourceFiles)
    set(Flags)
    set(TargetType exe)

    # Parse the arguments.
    set(I 1)
    set(STATE SRC)
    while(I LESS ARGC)
        # Store the argument.
        set(ARG ${ARGV${I}})
        if(ARG STREQUAL REFS OR ARG STREQUAL SRC OR ARG STREQUAL FLAGS)
            # Change the argument state.
            set(STATE ${ARG})
        elseif(ARG STREQUAL LIBRARY OR ARG STREQUAL LIB)
            # The module is a library.
            set(TargetType lib)
        elseif(ARG STREQUAL EXECUTABLE OR ARG STREQUAL EXE)
            # The module is a executable.
            set(TargetType exe)
        else()
            # Store the parameter in the correct list.
            if(STATE STREQUAL REFS)
                set(References ${References} ${ARG})
                # If the reference is a target, add a dependency to it.
                if(TARGET ${ARG})
                    set(RefDeps ${RefDeps} ${ARG})
                endif()
            elseif(STATE STREQUAL FLAGS)
                set(Flags ${Flags} ${ARG})
            elseif(STATE STREQUAL SRC)
                set(SourceFiles ${SourceFiles} ${ARG})
            endif()
        endif()

        # Read the next argument.
        math(EXPR I "${I}+1")
    endwhile()

    # Set the target name.
    if(TargetType STREQUAL lib)
        set(TargetType library)
        set(${Target}_dest_ "${LIBRARY_OUTPUT_PATH}/${Target}.dll" CACHE INTERNAL "output file" FORCE)
    elseif(TargetType STREQUAL exe)
        set(${Target}_dest_ "${EXECUTABLE_OUTPUT_PATH}/${Target}.exe" CACHE INTERNAL "output file" FORCE)
    endif()

    # Add prefixes to the libs and references.
    PREFIX_LIST(RefArgs References "-r:")

    # Build the command line.
    set(CommandLine ${CSHARP_COMPILER} "-out:${${Target}_dest_}"
                    -target:${TargetType} ${Flags}
                    ${RefArgs} ${SourceFiles})

    # Build the module.
    add_custom_command(OUTPUT ${${Target}_dest_}
        COMMAND ${CommandLine}
        DEPENDS ${SourceFiles} ${RefDeps}
        WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
    add_custom_target(${Target} ALL DEPENDS ${${Target}_dest_})
endfunction()
