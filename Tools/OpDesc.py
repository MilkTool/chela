#!/usr/bin/python
import re
import sys

# Indent unit.
IndentUnit = "    "

# Compile the opcode description pattern.
DescriptionPattern = re.compile(r"([A-Za-z0-9]+)(?:[ \t]*=[ \t]*[0-9]+)?[ \t]*[,;]?[ \t]*//<([^>]+)>(.*)")

# Instruction argument descriptions.
argumentTypes = """
u8->UInt8,
u8v->UInt8V,
i8->Int8,
i8v->Int8V,
u16->UInt16,
u16v->UInt16V,
i16->Int16,
i16v->Int16V,
u32->UInt32,
u32v->UInt32V,
i32->Int32,
i32v->Int32V,
u64->UInt64,
u64v->UInt64V,
i64->Int64,
i64v->Int64V,
fp32->Fp32,
fp32v->Fp32V,
fp64->Fp64,
fp64v->Fp64V,
tyid->TypeID,
gblid->GlobalID,
fldid->FieldID,
fnid->FunctionID,
sid->StringID,
bblid->BasicBlockID,
jmptbl->JumpTable
""".split(",")

# Create something easier to use.
InstructionArgumentTypes = []
ArgumentTypeMap = {}

for arg in argumentTypes:
    arg = arg.strip().split("->")
    typeName = arg[0].strip()
    typeValue = arg[1].strip()
    InstructionArgumentTypes.append(typeValue)
    ArgumentTypeMap[typeName] = typeValue

#print InstructionArgumentTypes
#print ArgumentTypeMap


#-------------------------------------------------------------------------------
# Instruction class.
class Instruction:
    def __init__(self, enumName="", mnemonic="", args="", description=""):
        self.enumName = enumName
        self.mnemonic = mnemonic
        self.args = args
        self.description = description

    def __str__(self):
        return "{mnemonic='%s',args=%s,description='%s'}" % (self.mnemonic, str(self.args), self.description)

#-------------------------------------------------------------------------------
# Template class
class Template:
    def __init__(self):
        self.Globals = {"Tab": IndentUnit, "Tab2": IndentUnit*2, "Tab3": IndentUnit*3}
        self.Body = ""
        self.OpCodeRecord = None
        self.OpcodeRecordSeparator = None
        self.ArgumentTypeRecord = None
        self.ArgumentTypeRecordSeparator = None
        self.FlatArgRecord = None
        self.FlatArgRecordSeparator = None
        self.InstructionArgTypeRecord = None
        self.InstructionArgTypeRecordSeparator = None
        self.InstructionRecord = None
        self.InstructionRecordSeparator = None
       
    # Gets the named global.
    def GetGlobal(self, name):
        return self.Globals[name]
        
    # Expands a template in a string.
    def ExpandTemplate(self, body, replacements):
        # Ignore none bodies.
        if body == None: return ""
        
        ret = body
        # TODO: Use a more efficient way.
        
        # Perform local replacements.
        for key in replacements.keys():
            ret = ret.replace("@" + key + "@", replacements[key])
            
        # Perform global replacements.
        for key in self.Globals.keys():
            ret = ret.replace("@" + key + "@", self.Globals[key])
        
        return ret

    # Creates the opcode argument string.
    def CreateArgsString(self, args):
        ret = ""
        for arg in args:
            ret += ","
            ret += arg
        return ret
        
    # Creates the opcodes table.
    def CreateOpCodeTable(self, instructions):
        table = ""
        localVars = {}
        numinstructions = len(instructions)
        for i in range(numinstructions):
            inst = instructions[i]
            
            # Expand the opcode record
            localVars["EnumName"] = inst.enumName
            localVars["Mnemonic"] = inst.mnemonic
            localVars["Description"] = inst.description
            localVars["ArgTypeNames"] = self.CreateArgsString(inst.args)
            localVars["OpCode"] = str(i)
            table += self.ExpandTemplate(self.OpCodeRecord, localVars)
            
            # Append the separator.
            if i + 1 < numinstructions:
                table += self.OpCodeRecordSeparator
            
        return table

    # Creates the argument type table.
    def CreateArgumentTypeTable(self):
        table = ""
        localVars = {}
        numtypes = len(InstructionArgumentTypes)
        for i in range(numtypes):
            argType = InstructionArgumentTypes[i]

            # Expand the argument record
            localVars["Name"] = InstructionArgumentTypes[i]
            localVars["TypeID"] = str(i)
            table += self.ExpandTemplate(self.ArgumentTypeRecord, localVars)
            
            # Append the separator.
            if i + 1 < numtypes:
                table += self.ArgumentTypeRecordSeparator
        return table

    # Creates the opcodes table.
    def CreateInstructionArgTable(self, arguments):
        # Don't create when unneeded.
        if self.InstructionArgTypeRecord == None: return ""
        
        table = ""
        localVars = {}
        numargs = len(arguments)
        for i in range(numargs):
            arg = arguments[i]
        
            # Expand the argument record
            localVars["ShortType"] = arg
            localVars["Type"] = ArgumentTypeMap[arg]
            table += self.ExpandTemplate(self.InstructionArgTypeRecord, localVars)
            
            # Append the separator.
            if i + 1 < numargs:
                table += self.InstructionArgTypeRecordSeparator
        return table

    # Creates the opcodes table.
    def CreateInstructionTable(self, instructions):
        table = ""
        localVars = {}
        numinstructions = len(instructions)
        
        # Allocate space for the flat arguments
        flatArgs = None
        flatTable = None
        if self.FlatArgRecord != None:
            flatArgs = []
            flatTable = ""
            
        # Build the instruction table.
        for i in range(numinstructions):
            inst = instructions[i]
            
            # Take note of the flat args.
            flatId = -1
            if flatArgs != None:
                flatId = len(flatArgs)
                flatArgs += inst.args
            
            # Expand the instruction record.
            localVars["EnumName"] = inst.enumName
            localVars["Mnemonic"] = inst.mnemonic
            localVars["Description"] = inst.description
            localVars["OpCode"] = str(i)
            localVars["ArgTable"] = self.CreateInstructionArgTable(inst.args)            
            localVars["NumArgs"] = str(len(inst.args))
            localVars["FlatArgIndex"] = str(flatId)
            table += self.ExpandTemplate(self.InstructionRecord, localVars)
            
            # Append the separator.
            if i + 1 < numinstructions:
                table += self.InstructionRecordSeparator
                
        # Build the flat argument table.
        if flatArgs != None:
            localVars = {}
            numflats = len(flatArgs)
            for i in range(numflats):
                flat = flatArgs[i]
                
                # Expand the flat record.
                localVars["ShortType"] = flat
                localVars["Type"] = ArgumentTypeMap[flat]
                flatTable += self.ExpandTemplate(self.FlatArgRecord, localVars)
                
                # Append the separator.
                if i + 1 < numflats:
                    flatTable += self.FlatArgRecordSeparator

        return table, flatTable
        
    # Creates the templated body.
    def CreateBody(self, instructions):
        # Build the local replacements.
        localVars = {}
        
        # Create the opcodes table.
        if self.OpCodeRecord != None:
            localVars["OpCodeTable"] = self.CreateOpCodeTable(instructions)
            
        if self.ArgumentTypeRecord != None:
            localVars["ArgumentTypeTable"] = self.CreateArgumentTypeTable()
        
        # Create the instructions table.
        if self.InstructionRecord != None:
            (instructions, flatArgs) = self.CreateInstructionTable(instructions)
            localVars["InstructionTable"] = instructions
            if flatArgs != None:
                localVars["FlatArgumentTable"] = flatArgs

        # Use the locals in the full body.
        return self.ExpandTemplate(self.Body, localVars)
            
        
# OpCodes description processor.
class OpDesc:
    def __init__(self):
        self.ctables = ""
        self.cstables = ""
        self.chtables = ""
        self.opcodes = ""
        self.instructions = None

    def PrintHelp(self):
        print "opdesc.py -opcodes <OpCodes.cs> -ctables <InstructionTables.h> -cstables <InstructionTables.cs>"

    def PrintVersion(self):
        print "opdesc.py version 0.1a"

    def ParseCommandLine(self, args):
        import sys
        i = 1;
        while i < len(args):
            arg = args[i]
            if arg == "-ctables":
                i += 1
                self.ctables = args[i]
            elif arg == "-cstables":
                i += 1
                self.cstables = args[i]
            elif arg == "-chtables":
                i += 1
                self.chtables = args[i]
            elif arg == "-help":
                self.PrintHelp()
                sys.exit(0)
            elif arg == "-version":
                self.PrintVersion()
                sys.exit(0)
            elif arg == "-opcodes":
                i += 1
                self.opcodes = args[i]
            else:
                self.PrintHelp()
                sys.exit(0)
            i += 1

        # Make sure to have at least the opcodes input file.
        if self.opcodes == "":
            self.PrintHelp()
            sys.exit(0)

    def ReadOpCodes(self):
        # Create the table.
        table = []

        # Open the opcodes file.
        opcodes = open(self.opcodes, "r")

        # Read the lines for matching opcode description.
        for line in opcodes:
            # Strip the line.
            line = line.strip()

            # Match the opcode description
            match = DescriptionPattern.search(line)
            if not match: continue

            # Get the instruction group.
            rawArgs = match.group(2).split(",")

            # Remove the trailing spaces of the instruction elements.
            args = []
            for arg in rawArgs:
                args.append(arg.strip());

            # Create the instruction data.
            inst = Instruction(match.group(1).strip(), args[0], args[1:], match.group(3).strip());

            # Insert the instruction into the table.
            table.append(inst)

        # Print the instruction table
        #for inst in table:
        #    print inst

        # Store the instruction table.
        self.instructions = table

    def WriteTables(self, template, fileName):
        # Do nothing if there's not output
        if fileName == "": return
        
        # Create the tables.
        tables = template.CreateBody(self.instructions)

        # Write them.
        out = open(fileName, "w")
        out.write(tables)
        out.close()

    def Run(self, args):
        # Parse the command line.
        self.ParseCommandLine(args)

        # Read the opcodes.
        self.ReadOpCodes()

        # Write the C# tables
        self.WriteTables(CSharpTemplate, self.cstables)
        
        # Write the Chela tables.
        self.WriteTables(ChelaTemplate, self.chtables)

        # Write the C++ tables
        self.WriteTables(CppTemplate, self.ctables)

#-------------------------------------------------------------------------------
# C++ instruction table template

CppTemplate = Template()
CppTemplate.Body = \
"""// THIS CODE IS AUTOMATICALLY GENERATED, DO NOT MODIFY

#ifndef CHELA_VM_INSTRUCTION_DESCRIPTION_HPP
#define CHELA_VM_INSTRUCTION_DESCRIPTION_HPP

namespace ChelaVm
{
    struct OpCode
    {
@OpCodeTable@
    };

    struct InstructionDescription
    {
        enum ArgumentType
        {
@ArgumentTypeTable@
        };

        const char *mnemonic;
        int numargs;
        const ArgumentType *args;
        const char *description;
    };

    const InstructionDescription::ArgumentType InstructionArgumentTable[] =
    {
@FlatArgumentTable@
    };

    const InstructionDescription InstructionTable[] =
    {
@InstructionTable@
    };
}

#endif //CHELA_VM_INSTRUCTION_DESCRIPTION_HPP
"""
CppTemplate.OpCodeRecord = "@Tab2@static const int @EnumName@ = @OpCode@; //<@Mnemonic@@ArgTypeNames@> @Description@"
CppTemplate.OpCodeRecordSeparator = "\n"
CppTemplate.ArgumentTypeRecord = "@Tab3@@Name@ = @TypeID@"
CppTemplate.ArgumentTypeRecordSeparator = ",\n"
CppTemplate.InstructionRecord = \
r'@Tab2@{"@Mnemonic@", @NumArgs@, &InstructionArgumentTable[@FlatArgIndex@], "@Description@"}'
CppTemplate.InstructionRecordSeparator = ",\n"
CppTemplate.FlatArgRecord = "@Tab2@InstructionDescription::@Type@"
CppTemplate.FlatArgRecordSeparator = ",\n"

#-------------------------------------------------------------------------------
# C# instruction table template
CSharpTemplate = Template()
CSharpTemplate.Body = \
"""// THIS CODE IS AUTOMATICALLY GENERATED, DO NOT MODIFY

namespace Chela.Compiler.Module
{
    public enum InstructionArgumentType
    {
@ArgumentTypeTable@
    }

    public sealed class InstructionDescription
    {
        private string mnemonic;
        private InstructionArgumentType[] args;
        private string description;

        public InstructionDescription(string mnemonic, InstructionArgumentType[] args, string description)
        {
            this.mnemonic = mnemonic;
            this.args = args;
            this.description = description;
        }

        public string GetMnemonic()
        {
            return this.mnemonic;
        }

        public InstructionArgumentType[] GetArguments()
        {
            return this.args;
        }

        public string GetDescription()
        {
            return this.description;
        }

        private static InstructionDescription[] instructionTable = new InstructionDescription[]
        {
@InstructionTable@
        };

        public static InstructionDescription[] GetInstructionTable()
        {
            return instructionTable;
        }
    }
}
"""

CSharpTemplate.ArgumentTypeRecord = "@Tab2@@Name@ = @TypeID@"
CSharpTemplate.ArgumentTypeRecordSeparator = ",\n"
CSharpTemplate.InstructionRecord = \
r'@Tab3@new InstructionDescription("@Mnemonic@", new InstructionArgumentType[] {@ArgTable@}, "@Description@")'
CSharpTemplate.InstructionRecordSeparator = ",\n"
CSharpTemplate.InstructionArgTypeRecord = "InstructionArgumentType.@Type@"
CSharpTemplate.InstructionArgTypeRecordSeparator = ", "

#-------------------------------------------------------------------------------
# Chela instruction table template

ChelaTemplate = Template()
ChelaTemplate.Body = \
"""// THIS CODE IS AUTOMATICALLY GENERATED, DO NOT MODIFY

namespace Chela.Reflection.Emition
{
    public enum OpCode
    {
@OpCodeTable@
    }
    
    public enum InstructionArgumentType
    {
@ArgumentTypeTable@
    }

    public sealed class InstructionDescription
    {
        private string mnemonic;
        private InstructionArgumentType[] args;
        private string description;

        public InstructionDescription(string mnemonic, InstructionArgumentType[] args, string description)
        {
            this.mnemonic = mnemonic;
            this.args = args;
            this.description = description;
        }

        public string GetMnemonic()
        {
            return this.mnemonic;
        }

        public InstructionArgumentType[] GetArguments()
        {
            return this.args;
        }

        public string GetDescription()
        {
            return this.description;
        }

        private static InstructionDescription[] instructionTable = new InstructionDescription[]
        {
@InstructionTable@
        };

        public static InstructionDescription[] GetInstructionTable()
        {
            return instructionTable;
        }
    }
}
"""

ChelaTemplate.OpCodeRecord = "@Tab2@@EnumName@ = @OpCode@, //<@Mnemonic@@ArgTypeNames@> @Description@"
ChelaTemplate.OpCodeRecordSeparator = "\n"
ChelaTemplate.ArgumentTypeRecord = "@Tab2@@Name@ = @TypeID@"
ChelaTemplate.ArgumentTypeRecordSeparator = ",\n"
ChelaTemplate.InstructionRecord = \
r'@Tab3@new InstructionDescription("@Mnemonic@", new InstructionArgumentType[] {@ArgTable@}, "@Description@")'
ChelaTemplate.InstructionRecordSeparator = ",\n"
ChelaTemplate.InstructionArgTypeRecord = "InstructionArgumentType.@Type@"
ChelaTemplate.InstructionArgTypeRecordSeparator = ", "

        
# Run the program.
if __name__ == "__main__":
    prog = OpDesc()
    prog.Run(sys.argv)



