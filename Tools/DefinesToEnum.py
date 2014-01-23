#!/usr/bin/python
import re
import sys

# Regular expression for extracting constants.
CONSTANT_RE = r"#define[\t ]+([a-zA-Z_][a-zA-Z_0-9]*)[\t ]+((?:0x[0-9A-Fa-f]+)|(?:[0-9][0-9]*))"

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

# Extract definitions and convert them into constants.
class DefinesToEnum:
    def __init__(self):
        self.re = re.compile(CONSTANT_RE)
        self.outputFileName = None
        self.inputFileName = None
        self.namespace = ""
        self.enumName = "Constants"
        self.visibility = "public"

    def Run(self):
        # Read the input file.
        inFile = open(self.inputFileName, "r")
        inData = inFile.read()
        inFile.close()
        
        # Extract the constants.
        constants = re.findall(self.re, inData)
        
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
            
        # Begin the enumeration.
        outFile.write("%s%s enum %s\n" %(IDENT_UNIT*ident, self.visibility, self.enumName))
        outFile.write("%s{\n" % (IDENT_UNIT*ident))
        ident += 1
        
        # Write the constants.
        for const in constants:
            outFile.write("%s%s = %s,\n" % (IDENT_UNIT*ident, CamelCase(const[0],1), const[1]))
            
        # End the enumeration.
        ident -= 1
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
            elif argName == "-e":
                i += 1
                self.enumName = args[i]
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
            print "-e\tEnum name."
            print "-v\tEnum visibility."
            return -1

        # Run the program.
        return self.Run()
    
# Run the program.
if __name__ == "__main__":
    prog = DefinesToEnum()
    sys.exit(prog.Main(sys.argv))
    
