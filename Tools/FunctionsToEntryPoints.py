#!/usr/bin/python
import re
import sys

# Regular expression for extracting constants.
FUNCTION_RE = r'^((?:[a-zA-Z_]\w*\**\s+)+)([a-zA-Z_]\w*)\((.*)\)'
RETTYPE_RE = r'(\w+\**)'
ARGS_RE = r'(\w+)\s+(\w+)\s*,?'
NAME_RE = r'([a-z_]*)(\w+)'

# Output file header.
HEADER = \
"""///
/// THIS FILE WAS GENERATED AUTOMATICALLY - DO NOT MODIFY
///

"""

# Identation unit.
IDENT_UNIT = "    "

# Convert a string into CamelCase
def CamelCase(s, ig=0):
    ret = ""
    parts = s.split('_')
    for i in range(ig, len(parts)):
        p = parts[i]
        p = p[0] + p[1:].lower()
        ret += p
    return ret

# Entry point data
class EntryPoint:
    def __init__(self, retType, prefix, name, rawArgs, args):
        self.returnType = retType
        self.prefix = prefix
        self.name = name
        self.rawArgs = rawArgs
        self.args = args

# Extract definitions and convert them into constants.
class FunctionsToEntryPoints:
    def __init__(self):
        self.functionRe = re.compile(FUNCTION_RE, re.MULTILINE)
        self.retTypeRe = re.compile(RETTYPE_RE, re.MULTILINE)
        self.nameRe = re.compile(NAME_RE, re.MULTILINE)
        self.argsRe = re.compile(ARGS_RE, re.MULTILINE)        
        self.outputFileName = None
        self.inputFileName = None
        self.namespace = ""
        self.parentClass = ""
        self.parentVisibility = "public"
        self.visibility = "public"
        self.className = "EntryPoints"

    def Run(self):
        # Read the input file.
        inFile = open(self.inputFileName, "r")
        inData = inFile.read()
        inFile.close()
        
        # Extract the entry points.
        functionProtos = re.findall(self.functionRe, inData)
        entryPoints = []
        for proto in functionProtos:
            # Remove some potential macros from the return type part.
            retTypeComps = re.findall(self.retTypeRe, proto[0])
            retType = ""
            for comp in retTypeComps:
                if comp.endswith("API") or comp.endswith("APIENTRY"):
                    continue
                if retType != "":
                    retType += " "
                retType += comp
            
            # Extract the name and the arguments.
            (prefix, name) = re.findall(self.nameRe, proto[1])[0]
            rawArgs = proto[2].strip()
            args = re.findall(self.argsRe, rawArgs)
            
            # Remove (void)
            if rawArgs == "void":
                rawArgs = ""
                args = []                
            
            # Store the entry points
            entryPoints.append(EntryPoint(retType, prefix, name, rawArgs, args))
        
        # Open the output file.
        outFile = sys.stdout
        if self.outputFileName != None:
            outFile = open(self.outputFileName, "w")
            
        # Write the file header.
        outFile.write(HEADER)
        
        # Write the namespace.
        ident = 0
        hasNamespace = self.namespace != "" and self.namespace != None
        if hasNamespace:
            outFile.write("namespace %s\n" % self.namespace)
            outFile.write("{\n")
            ident += 1
            
        # Write the parent class
        hasParent = self.parentClass != "" and self.parentClass != None
        if hasParent:
            outFile.write("%s%s partial class %s\n" % (IDENT_UNIT*ident, self.parentVisibility, self.parentClass))
            outFile.write("%s{\n" % (IDENT_UNIT*ident))
            ident += 1
            
        # Begin the class.
        outFile.write("%s%s static class %s\n" % (IDENT_UNIT*ident, self.visibility, self.className))
        outFile.write("%s{\n" % (IDENT_UNIT*ident))
        ident += 1
        
        # Write the function pointers.
        for ep in entryPoints:
            # Write the typedef.
            outFile.write("%spublic typedef %s (*) __apicall (%s) %s;\n" \
             % (IDENT_UNIT*ident, ep.returnType, ep.rawArgs, ep.name))
             
            # Write the function pointer.
            outFile.write("%spublic static unsafe %s %s;\n" \
             % (IDENT_UNIT*ident, ep.name, ep.prefix + ep.name))
             
        outFile.write("\n");
        
        # Write the entry points loader.
        outFile.write("%spublic static unsafe void LoadEntryPoints()\n" % (IDENT_UNIT*ident))
        outFile.write("%s{\n" % (IDENT_UNIT*ident))
        ident += 1
        
        # Load each one of the entry points.
        for ep in entryPoints:
            outFile.write('%s%s = (%s)GetEntryPoint(c"%s");\n' \
                % (IDENT_UNIT*ident, ep.prefix+ep.name, ep.name, ep.prefix + ep.name));
        
        # End the entry points loader.
        ident -= 1
        outFile.write("%s}\n" % (IDENT_UNIT*ident))
            

        # End the class.
        ident -= 1
        outFile.write("%s}\n" % (IDENT_UNIT*ident))
        
        # End the parent class.
        if hasParent:
            outFile.write("%s}\n" % (IDENT_UNIT*ident))
        
        # End the namespace
        if hasNamespace:
            outFile.write("}\n")
            
        # Close the output file.
        if self.outputFileName != None:
            outFile.close();
        
        return 0
        
        
    def Main(self, args):
        # Read the arguments.
        i = 1
        while i < len(args):
            argName = args[i]
            if argName == "-o":
                i += 1
                self.outputFileName = args[i]
            elif argName == "-n":
                i += 1
                self.namespace = args[i]
            elif argName == "-p":
                i += 1
                self.parentClass = args[i]
            elif argName == "-pv":
                i += 1
                self.parentVisibility = args[i]
            elif argName == "-c":
                i += 1
                self.className = args[i]
            elif argName == "-v":
                i += 1
                self.visibility = args[i]
            else:
                self.inputFileName = args[i]
            i += 1

        # Print usage.
        if self.inputFileName == None:
            print "DefinesToEnum.py [options] input"
            print "-o\tOutput file name."
            print "-n\tNamespace."
            print "-p\tParent class name."
            print "-pv\tParent class visibility."
            print "-c\tClass name."
            print ".v\tClass visibility."
            return -1

        # Run the program.
        return self.Run()
    
# Run the program.
if __name__ == "__main__":
    prog = FunctionsToEntryPoints()
    sys.exit(prog.Main(sys.argv))
    
