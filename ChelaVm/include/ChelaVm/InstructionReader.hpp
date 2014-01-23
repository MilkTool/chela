#ifndef CHELAVM_INSTRUCTION_READER_HPP
#define CHELAVM_INSTRUCTION_READER_HPP

#include "InstructionDescription.hpp"

namespace ChelaVm
{
    class InstructionReader
    {
    public:
        InstructionReader(const uint8_t *instructions, uint16_t blockSize)
        {
            pc = instructions;
            end = pc + blockSize;
        }
        
        ~InstructionReader()
        {
        }
        
        bool End() const
        {
            return pc >= end;
        }
        
        int ReadOpCode()
        {
            if(pc >= end)
                lastOpcode = OpCode::Invalid;
            else
                lastOpcode = *pc++;
            return lastOpcode;
        }
        
        void Next()
        {
            if(lastOpcode >= OpCode::Invalid)
                return;
                
            const InstructionDescription &desc = InstructionTable[lastOpcode];
            for(int i = 0; i < desc.numargs && pc < end; i++)
            {
                switch(desc.args[i])
                {
                case InstructionDescription::UInt8:
                case InstructionDescription::Int8:
                    pc++;
                    break;
                case InstructionDescription::UInt16:
                case InstructionDescription::Int16:
                    pc += 2;
                    break;
                case InstructionDescription::UInt32:
                case InstructionDescription::Int32:
                    pc += 4;
                    break;
                case InstructionDescription::UInt64:
                case InstructionDescription::Int64:
                    pc += 8;
                    break;
                case InstructionDescription::Fp32:
                    pc += 4;
                    break;
                case InstructionDescription::Fp64:
                    pc += 8;
                    break;                
                case InstructionDescription::Int8V:
                case InstructionDescription::UInt8V:
                    {
                        int n = *pc++;
                        pc += n;
                    }
                    break;
                case InstructionDescription::UInt16V:
                case InstructionDescription::Int16V:
                    {
                        int n = *pc++;
                        pc += n*2;
                    }
                    break;
                case InstructionDescription::UInt32V:
                case InstructionDescription::Int32V:
                    {
                        int n = *pc++;
                        pc += n*4;
                    }
                    break;
                case InstructionDescription::UInt64V:
                case InstructionDescription::Int64V:
                    {
                        int n = *pc++;
                        pc += n*8;
                    }
                    break;
                case InstructionDescription::Fp32V:
                    {
                        int n = *pc++;
                        pc += 4*n;
                    }
                    break;
                case InstructionDescription::Fp64V:
                    {
                        int n = *pc++;
                        pc += 8*n;
                    }
                    break;                
                case InstructionDescription::TypeID:
                    pc += 4;
                    break;
                case InstructionDescription::GlobalID:
                    pc += 4;
                    break;
                case InstructionDescription::FieldID:
                    pc += 4;
                    break;
                case InstructionDescription::FunctionID:
                    pc += 4;
                    break;
                case InstructionDescription::StringID:
                    pc += 4;
                    break;
                case InstructionDescription::BasicBlockID:
                    pc += 2;
                    break;
                case InstructionDescription::JumpTable:
                    {
                        int n = ReadUI16();
                        pc += 6*n;
                    }
                    break;
                }
            }
            
            // Adjust the pc.
            if(pc > end)
                pc = end;
        }
        
        void IncreasePc(uint32_t amount)
        {
            pc += amount;
            if(pc > end)
                pc = end;
        }

        uint8_t ReadUI8()
        {
            return *pc++;
        }
        
        int8_t ReadI8()
        {
            return *pc++;
        }
        
        uint16_t ReadUI16()
        {
            uint16_t ret = *pc++;
            ret |= (*pc++)<<8;
            return ret;
        }        
        
        int16_t ReadI16()
        {
            return ReadUI16();
        }
        
        uint32_t ReadUI32()
        {
            uint32_t ret = *pc++;
            ret |= (*pc++)<<8;
            ret |= (*pc++)<<16;
            ret |= (*pc++)<<24;
            return ret;
        }

        int32_t ReadI32()
        {
            return ReadUI32();
        }

        uint64_t ReadUI64()
        {
            uint64_t ret = *pc++;
            ret |= uint64_t(*pc++)<<8l;
            ret |= uint64_t(*pc++)<<16l;
            ret |= uint64_t(*pc++)<<24l;
            ret |= uint64_t(*pc++)<<32l;
            ret |= uint64_t(*pc++)<<40l;
            ret |= uint64_t(*pc++)<<48l;
            ret |= uint64_t(*pc++)<<56l;
            return ret;
        }

        int64_t ReadI64()
        {
            return ReadUI64();
        }
        
        float ReadFP32()
        {
            uint32_t ret = ReadUI32();
            uint32_t &ref = ret;
            return reinterpret_cast<const float&> (ref);
        }
        
        double ReadFP64()
        {
            uint64_t ret = ReadUI64();
            uint64_t &ref = ret;
            return reinterpret_cast<const double&> (ref);
        }
        
        uint32_t ReadStringId()
        {
            return ReadUI32();
        }

        uint32_t ReadFieldId()
        {
            return ReadUI32();
        }

        uint32_t ReadFunctionId()
        {
            return ReadUI32();
        }

        uint32_t ReadTypeId()
        {
            return ReadUI32();
        }
        
        uint16_t ReadBlock()
        {
            return ReadUI16();
        }        
        
    private:
        const uint8_t *pc, *end;
        int lastOpcode;
    };
};

#endif //CHELAVM_INSTRUCTION_READER_HPP
