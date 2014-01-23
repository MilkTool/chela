# Chela compiler flags.
set(CMAKE_CHELA_FLAGS_DEBUG -g CACHE STRING "Chela debug build flags")
set(CMAKE_CHELA_FLAGS_RELEASE -O2 CACHE STRING "Chela release build flags")
set(CMAKE_CHELA_FLAGS_RELWITHDEBINFO -g -O2 CACHE STRING "Chela release build flags")
set(CMAKE_CHELA_FLAGS ${CMAKE_CHELA_FLAGS} CACHE STRING "Chela common build flags")
    
# Chela compiler
set(ChelaC "${EXECUTABLE_OUTPUT_PATH}/chlc.exe")
set(ChelaC_CMD ${MONO_DBG} ${ChelaC})

# Chela library prefix and suffix.
# Don't use mono win windows.
if(${CMAKE_HOST_WIN32})
	set(CMAKE_CHELA_LIBRARY_PREFIX "")
    set(CMAKE_CHELA_LIBRARY_SUFFIX ".dll")
else()
	set(CMAKE_CHELA_LIBRARY_PREFIX ${CMAKE_SHARED_LIBRARY_PREFIX})
    set(CMAKE_CHELA_LIBRARY_SUFFIX ${CMAKE_SHARED_LIBRARY_SUFFIX})
endif()
# Prefix parameters macro.
macro(PREFIX_LIST VAR LIST PREFIX)
    set(${VAR})
    foreach(ARG ${${LIST}})
        list(APPEND ${VAR} "${PREFIX}${ARG}")
    endforeach()
endmacro()

function(CHELA_MODULE Target)
    # Get the build type.
    string(TOUPPER "${CMAKE_BUILD_TYPE}" BuildType)

    # Initialize target variables.
    set(References)
    set(RefDeps)
    set(Libraries)
    set(SourceFiles)
    set(Resources)
    set(Flags ${CMAKE_CHELA_FLAGS} ${CMAKE_CHELA_FLAGS_${BuildType}})
    set(TargetType exe)

    # Parse the arguments.
    set(I 1)
    set(STATE SRC)
    while(I LESS ARGC)
        # Store the argument.
        set(ARG ${ARGV${I}})
        if(ARG STREQUAL REFS OR ARG STREQUAL LIBS OR ARG STREQUAL SRC OR ARG STREQUAL FLAGS
            OR ARG STREQUAL RESOURCES )
            # Change the argument state.
            set(STATE ${ARG})
        elseif(ARG STREQUAL LIBRARY)
            # The module is a library.
            set(TargetType lib)
        elseif(ARG STREQUAL EXECUTABLE)
            # The module is a executable.
            set(TargetType exe)
        else()
            # Store the parameter in the correct list.
            if(STATE STREQUAL REFS)
                set(References ${References} ${ARG})
                # If the reference is a target, add a dependency to it.
                if(TARGET ${ARG}_dest_)
                    set(RefDeps ${RefDeps} ${ARG}_dest_)
                endif()
            elseif(STATE STREQUAL LIBS)
                set(Libraries ${Libraries} ${ARG})
            elseif(STATE STREQUAL FLAGS)
                set(Flags ${Flags} ${ARG})
            elseif(STATE STREQUAL SRC)
                set(SourceFiles ${SourceFiles} ${ARG})
            elseif(STATE STREQUAL RESOURCES)
                set(Resources ${Resources} ${ARG})
            endif()
        endif()

        # Read the next argument.
        math(EXPR I "${I}+1")
    endwhile()

    # Set the target name.
    if(TargetType STREQUAL lib)
        set(${Target}_dest_ "${LIBRARY_OUTPUT_PATH}/${CMAKE_CHELA_LIBRARY_PREFIX}${Target}${CMAKE_CHELA_LIBRARY_SUFFIX}" CACHE INTERNAL "output file" FORCE)
    elseif(TargetType STREQUAL exe)
        set(${Target}_dest_ "${EXECUTABLE_OUTPUT_PATH}/${Target}${CMAKE_EXECUTABLE_SUFFIX}" CACHE INTERNAL "output file" FORCE)
    else()
        set(${Target}_dest_ "${LIBRARY_OUTPUT_PATH}/${Target}.cbm" CACHE INTERNAL "output file" FORCE)
    endif()

    # Add prefixes to the libs and references.
    PREFIX_LIST(LibArgs Libraries "-l")
    PREFIX_LIST(RefArgs References "-r:")
    PREFIX_LIST(ResourcesArgs Resources "-resource:")

    # Used for debugging.
    set(Verbose)
    #set(Verbose -v > "${${Target}_dest_}.chsm")

    # Build the command line.
    set(CommandLine ${ChelaC_CMD} -o "${${Target}_dest_}"
                    -t:${TargetType} ${Flags}
                    ${LibArgs} "-L${LIBRARY_OUTPUT_PATH}"
                    ${RefArgs} ${SourceFiles} ${ResourcesArgs} ${Verbose})

    #message("command ${CommandLine}")

    # Build the module.
    add_custom_command(OUTPUT ${${Target}_dest_}
        COMMAND ${CommandLine}
        DEPENDS ${ChelaC} chlc-vm ${SourceFiles} ${Resources} ${RefDeps} ${Libraries}
        WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
    add_custom_target(${Target} ALL DEPENDS ${${Target}_dest_})
endfunction()

macro(CHELA_EXECUTABLE Target)
    CHELA_MODULE(${Target} EXECUTABLE SRC ${ARGN})
endmacro()

macro(CHELA_LIBRARY Target)
    CHELA_MODULE(${Target} LIBRARY SRC ${ARGN})
endmacro()

