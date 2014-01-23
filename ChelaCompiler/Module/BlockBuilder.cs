using System.Collections.Generic;
using Chela.Compiler.Ast;

namespace Chela.Compiler.Module
{
    public class BlockBuilder
    {
        private BasicBlock currentBlock;
        private List<AstNode> nodeList;
        private AstNode currentNode;
     
        public BlockBuilder()
        {
            this.currentBlock = null;
            this.nodeList = new List<AstNode> ();
            this.currentNode = null;
        }
     
        public BasicBlock GetBlock()
        {
            return this.currentBlock;
        }
     
        public void SetBlock(BasicBlock block)
        {
            // Set the block.
            this.currentBlock = block;
        }

        // Used by debugging information
        public void BeginNode(AstNode node)
        {
            nodeList.Add(currentNode);
            currentNode = node;
        }

        public AstNode EndNode()
        {
            // Return the current node.
            AstNode ret = currentNode;

            // Retrieve the last node in the list.
            int lastIndex = nodeList.Count-1;
            currentNode = nodeList[lastIndex];

            // Remove the last node from the list.
            nodeList.RemoveAt(lastIndex);

            return ret;
        }
     
        public void Append(Instruction instruction)
        {
            if(currentNode != null)
                instruction.SetPosition(currentNode.GetPosition());
            currentBlock.Append(instruction);
        }

        public void AppendInst0(OpCode op)
        {
            Append(new Instruction(op, null));
        }
     
        public void AppendInst1(OpCode op, object a1)
        {
            Append(new Instruction(op, new object[1] {a1}));
        }

        public void AppendInst2(OpCode op, object a1, object a2)
        {
            Append(new Instruction(op, new object[2] {a1, a2}));
        }
     
        public void AppendInst3(OpCode op, object a1, object a2, object a3)
        {
            Append(new Instruction(op, new object[3] {a1, a2, a3}));
        }
     
        public Instruction GetLastInstruction()
        {
            return currentBlock.GetLast();
        }
     
        public bool IsLastTerminator()
        {
            Instruction last = GetLastInstruction();
            return last != null && last.IsTerminator();
        }
     
        // Nop
        public void CreateNop()
        {
            AppendInst0(OpCode.Nop);
        }
     
        // Variable load/store
        public void CreateLoadArg(byte arg)
        {
            AppendInst1(OpCode.LoadArg, arg);
        }

        public void CreateLoadLocal(byte arg)
        {
            AppendInst1(OpCode.LoadLocal, arg);
        }

        public void CreateLoadLocalS(ushort arg)
        {
            AppendInst1(OpCode.LoadLocalS, arg);
        }

        public void CreateStoreLocal(byte arg)
        {
            AppendInst1(OpCode.StoreLocal, arg);
        }

        public void CreateStoreLocalS(ushort arg)
        {
            AppendInst1(OpCode.StoreLocalS, arg);
        }
     
        public void CreateLoadField(FieldVariable field)
        {
            AppendInst1(OpCode.LoadField, field);
        }

        public void CreateStoreField(FieldVariable field)
        {
            AppendInst1(OpCode.StoreField, field);
        }

        public void CreateLoadGlobal(Variable globalVar)
        {
            AppendInst1(OpCode.LoadGlobal, globalVar);
        }

        public void CreateStoreGlobal(Variable globalVar)
        {
            AppendInst1(OpCode.StoreGlobal, globalVar);
        }

        public void CreateLoadLocal(LocalVariable variable)
        {
            if(variable.IsPseudoLocal)
                throw new ModuleException("Cannot use pseudo variable directly.");

            uint index = (uint)variable.GetLocalIndex();
            if(index < 256)
                CreateLoadLocal((byte)index);
            else
                CreateLoadLocalS((ushort)index);
        }
     
        public void CreateStoreLocal(LocalVariable variable)
        {
            if(variable.IsPseudoLocal)
                throw new ModuleException("Cannot use pseudo variable directly.");

            uint index = (uint)variable.GetLocalIndex();
            if(index < 256)
                CreateStoreLocal((byte)index);
            else
                CreateStoreLocalS((ushort)index);
        }
     
        public void CreateLoadArraySlot(IChelaType arrayType)
        {
            AppendInst1(OpCode.LoadArraySlot, arrayType);
        }
     
        public void CreateStoreArraySlot(IChelaType arrayType)
        {
            AppendInst1(OpCode.StoreArraySlot, arrayType);
        }

        public void CreateLoadValue()
        {
            AppendInst0(OpCode.LoadValue);
        }

        public void CreateStoreValue()
        {
            AppendInst0(OpCode.StoreValue);
        }

        public void CreateLoadSwizzle(int components, int mask)
        {
            AppendInst2(OpCode.LoadSwizzle, (byte)components, (byte)mask);
        }

        public void CreateStoreSwizzle(int components, int mask)
        {
            AppendInst2(OpCode.StoreSwizzle, (byte)components, (byte)mask);
        }

        // Variable address.
        public void CreateLoadLocalAddr(ushort index)
        {
            AppendInst1(OpCode.LoadLocalAddr, index);
        }

        public void CreateLoadLocalAddr(LocalVariable local)
        {
            if(local.IsPseudoLocal)
                throw new ModuleException("Cannot use pseudo variable directly.");

            CreateLoadLocalAddr((ushort)local.GetLocalIndex());
        }

        public void CreateLoadLocalRef(ushort index)
        {
            AppendInst1(OpCode.LoadLocalRef, index);
        }

        public void CreateLoadLocalRef(LocalVariable local)
        {
            CreateLoadLocalRef((ushort)local.GetLocalIndex());
        }

        public void CreateLoadFieldAddr(FieldVariable field)
        {
            AppendInst1(OpCode.LoadFieldAddr, field);
        }

        public void CreateLoadFieldRef(FieldVariable field)
        {
            AppendInst1(OpCode.LoadFieldRef, field);
        }

        public void CreateLoadGlobalAddr(Variable globalVar)
        {
            AppendInst1(OpCode.LoadGlobalAddr, globalVar);
        }

        public void CreateLoadGlobalRef(Variable globalVar)
        {
            AppendInst1(OpCode.LoadGlobalRef, globalVar);
        }

        public void CreateLoadArraySlotAddr(IChelaType arrayType)
        {
            AppendInst1(OpCode.LoadArraySlotAddr, arrayType);
        }

        public void CreateLoadArraySlotRef(IChelaType arrayType)
        {
            AppendInst1(OpCode.LoadArraySlotRef, arrayType);
        }

        public void CreateLoadFunctionAddr(Function function)
        {
            AppendInst1(OpCode.LoadFunctionAddr, function);
        }
     
        // Constant loading
        public void CreateLoadBool(bool constant)
        {
            if(constant)
                AppendInst1(OpCode.LoadBool, (byte)1);
            else
                AppendInst1(OpCode.LoadBool, (byte)0);
        }

        public void CreateLoadChar(char constant)
        {
            AppendInst1(OpCode.LoadChar, (ushort)constant);
        }

        public void CreateLoadInt8(sbyte constant)
        {
            AppendInst1(OpCode.LoadInt8, constant);
        }

        public void CreateLoadUInt8(byte constant)
        {
            AppendInst1(OpCode.LoadUInt8, constant);
        }

        public void CreateLoadInt16(short constant)
        {
            AppendInst1(OpCode.LoadInt16, constant);
        }

        public void CreateLoadUInt16(ushort constant)
        {
            AppendInst1(OpCode.LoadUInt16, constant);
        }

        public void CreateLoadInt32(int constant)
        {
            AppendInst1(OpCode.LoadInt32, constant);
        }
     
        public void CreateLoadUInt32(uint constant)
        {
            AppendInst1(OpCode.LoadUInt32, constant);
        }

        public void CreateLoadInt64(long constant)
        {
            AppendInst1(OpCode.LoadInt64, constant);
        }

        public void CreateLoadUInt64(ulong constant)
        {
            AppendInst1(OpCode.LoadUInt64, constant);
        }

        public void CreateLoadFp32(float constant)
        {
            AppendInst1(OpCode.LoadFp32, constant);
        }
     
        public void CreateLoadFp64(double constant)
        {
            AppendInst1(OpCode.LoadFp64, constant);
        }

        public void CreateLoadCString(string value)
        {
            AppendInst1(OpCode.LoadCString, value);
        }
     
        public void CreateLoadString(string value)
        {
            AppendInst1(OpCode.LoadString, value);
        }
     
        public void CreateLoadNull()
        {
            AppendInst0(OpCode.LoadNull);
        }

        public void CreateLoadDefault(IChelaType type)
        {
            AppendInst1(OpCode.LoadDefault, type);
        }
     
        // Arithmetic
        public void CreateAdd()
        {
            AppendInst0(OpCode.Add);
        }
     
        public void CreateSub()
        {
            AppendInst0(OpCode.Sub);
        }

        public void CreateMatMul()
        {
            AppendInst0(OpCode.MatMul);
        }

        public void CreateMul()
        {
            AppendInst0(OpCode.Mul);
        }
     
        public void CreateDiv()
        {
            AppendInst0(OpCode.Div);
        }
     
        public void CreateMod()
        {
            AppendInst0(OpCode.Mod);
        }
     
        public void CreateNeg()
        {
            AppendInst0(OpCode.Neg);
        }
     
        // Bitwise.
        public void CreateNot()
        {
            AppendInst0(OpCode.Not);
        }
     
        public void CreateAnd()
        {
            AppendInst0(OpCode.And);
        }
     
        public void CreateOr()
        {
            AppendInst0(OpCode.Or);
        }
     
        public void CreateXor()
        {
            AppendInst0(OpCode.Xor);
        }
     
        public void CreateShLeft()
        {
            AppendInst0(OpCode.ShLeft);
        }
     
        public void CreateShRight()
        {
            AppendInst0(OpCode.ShRight);
        }
     
        // Comparison
        public void CreateCmpZ()
        {
            AppendInst0(OpCode.CmpZ);
        }
     
        public void CreateCmpNZ()
        {
            AppendInst0(OpCode.CmpNZ);
        }
     
        public void CreateCmpEQ()
        {
            AppendInst0(OpCode.CmpEQ);
        }
     
        public void CreateCmpNE()
        {
            AppendInst0(OpCode.CmpNE);
        }
     
        public void CreateCmpLT()
        {
            AppendInst0(OpCode.CmpLT);
        }
     
        public void CreateCmpLE()
        {
            AppendInst0(OpCode.CmpLE);
        }
     
        public void CreateCmpGT()
        {
            AppendInst0(OpCode.CmpGT);
        }
     
        public void CreateCmpGE()
        {
            AppendInst0(OpCode.CmpGE);
        }
     
        // Casting
        public void CreateCast(IChelaType targetType)
        {
            AppendInst1(OpCode.Cast, targetType);
        }

        public void CreateGCast(IChelaType targetType)
        {
            AppendInst1(OpCode.GCast, targetType);
        }

        public void CreateIsA(IChelaType cmp)
        {
            AppendInst1(OpCode.IsA, cmp);
        }

        public void CreateBitCast(IChelaType targetType)
        {
            AppendInst1(OpCode.BitCast, targetType);
        }
     
        // Object management.
        public void CreateNewObject(IChelaType type, Function constructor, uint numargs)
        {
            AppendInst3(OpCode.NewObject, type, constructor, (byte)numargs);
        }

        public void CreateNewStruct(IChelaType type, Function constructor, uint numargs)
        {
            AppendInst3(OpCode.NewStruct, type, constructor, (byte)numargs);
        }

        public void CreateNewVector(IChelaType type)
        {
            AppendInst1(OpCode.NewVector, type);
        }

        public void CreateNewMatrix(IChelaType type)
        {
            AppendInst1(OpCode.NewMatrix, type);
        }

        public void CreateNewDelegate(IChelaType type, Function invoked)
        {
            AppendInst2(OpCode.NewDelegate, type, invoked);
        }

        public void CreateNewArray(IChelaType type)
        {
            AppendInst1(OpCode.NewArray, type);
        }

        public void CreateNewStackObject(IChelaType type, Function constructor, uint numargs)
        {
            AppendInst3(OpCode.NewStackObject, type, constructor, (byte)numargs);
        }

        public void CreateNewStackArray(IChelaType type)
        {
            AppendInst1(OpCode.NewStackArray, type);
        }

        public void CreateNewRawObject(IChelaType type, Function constructor, uint numargs)
        {
            AppendInst3(OpCode.NewRawObject, type, constructor, (byte)numargs);
        }

        public void CreateNewRawArray(IChelaType type)
        {
            AppendInst1(OpCode.NewRawArray, type);
        }

        public void CreateBox(IChelaType type)
        {
            AppendInst1(OpCode.Box, type);
        }

        public void CreateUnbox(IChelaType type)
        {
            AppendInst1(OpCode.Unbox, type);
        }

        public void CreateExtractPrim()
        {
            AppendInst0(OpCode.ExtractPrim);
        }

        public void CreateDeleteObject()
        {
            AppendInst0(OpCode.DeleteObject);
        }

        public void CreateDeleteRawArray()
        {
            AppendInst0(OpCode.DeleteRawArray);
        }
        
        public void CreatePrimBox(IChelaType type)
        {
            AppendInst1(OpCode.PrimBox, type);
        }

        public void CreateSizeOf(IChelaType type)
        {
            AppendInst1(OpCode.SizeOf, type);
        }

        public void CreateTypeOf(IChelaType type)
        {
            AppendInst1(OpCode.TypeOf, type);
        }

        // Exception handling.
        public void CreateThrow()
        {
            AppendInst0(OpCode.Throw);
        }
     
        // Branching.
        public void CreateJmp(BasicBlock dest)
        {
            AppendInst1(OpCode.Jmp, dest);
            currentBlock.AddSuccessor(dest);
        }
     
        public void CreateBr(BasicBlock trueDest, BasicBlock falseDest)
        {
            AppendInst2(OpCode.Br, trueDest, falseDest);
            currentBlock.AddSuccessor(trueDest);
            currentBlock.AddSuccessor(falseDest);
        }

        public void CreateJumpResume(BasicBlock dest)
        {
            AppendInst1(OpCode.JumpResume, dest);
            currentBlock.AddSuccessor(dest);
        }

        public void CreateSwitch(int[] constants, BasicBlock[] blocks)
        {
            object[] args = new object[blocks.Length * 2 + 1];
            args[0] = (ushort)blocks.Length;
            for(int i = 0; i < blocks.Length; i++)
            {
                args[i * 2 + 1] = constants[i];
                args[i * 2 + 2] = blocks[i];
                currentBlock.AddSuccessor(blocks[i]);
            }

            Append(new Instruction(OpCode.Switch, args));
        }

        // Call/Return
        public void CreateRet()
        {
            AppendInst0(OpCode.Ret);
        }
     
        public void CreateRetVoid()
        {
            AppendInst0(OpCode.RetVoid);
        }
     
        public void CreateCall(Function function, uint numargs)
        {
            AppendInst2(OpCode.Call, function, (byte)numargs);
        }
     
        public void CreateCallVirtual(Function function, uint numargs)
        {
            AppendInst2(OpCode.CallVirtual, function, (byte)numargs);
        }
     
        public void CreateCallIndirect(uint numargs)
        {
            AppendInst1(OpCode.CallIndirect, (byte)numargs);
        }
     
        public void CreateCallDynamic(uint numargs)
        {
            AppendInst1(OpCode.CallDynamic, (byte)numargs);
        }

        public void CreateBindKernel(IChelaType delegateType, Function kernel)
        {
            AppendInst2(OpCode.BindKernel, delegateType, kernel);
        }

        // Instruction prefixes.
        public void CreateChecked()
        {
            AppendInst0(OpCode.Checked);
        }
     
        // Stack tiding.
        public void CreatePush(uint offset)
        {
            AppendInst1(OpCode.Push, (byte)offset);
        }

        public void CreatePop()
        {
            AppendInst0(OpCode.Pop);
        }

        public void CreateDup(uint amount)
        {
            AppendInst1(OpCode.Dup, (byte)amount);
        }
     
        public void CreateDup1()
        {
            AppendInst0(OpCode.Dup1);
        }

        public void CreateDup2()
        {
            AppendInst0(OpCode.Dup2);
        }

        public void CreateRemove(uint offset)
        {
            AppendInst1(OpCode.Remove, (byte)offset);
        }
    }
}

