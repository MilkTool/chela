using System;
using Chela.Compiler.Ast;

namespace Chela.Compiler.Module
{
    public class ConstantValue: IEquatable<ConstantValue>
    {
        public enum ValueType
        {
            Byte = 0,
            SByte,
            Char,
            Short,
            UShort,
            Int,
            UInt,
            Long,
            ULong,
            Float,
            Double,
            Bool,
            Size,
            String,
            Null,
        };

        private ValueType type;
        private byte vbyte;
        private sbyte vsbyte;
        private char vchar;
        private short vshort;
        private ushort vushort;
        private int vint;
        private uint vuint;
        private long vlong;
        private ulong vulong;
        private float vfloat;
        private double vdouble;
        private bool vbool;
        private string vstring;

        private ConstantValue()
        {
        }

        public ConstantValue (byte value)
        {
            type = ValueType.Byte;
            vbyte = value;
        }

        public ConstantValue (sbyte value)
        {
            type = ValueType.SByte;
            vsbyte = value;
        }

        public ConstantValue (char value)
        {
            type = ValueType.Char;
            vchar = value;
        }

        public ConstantValue (short value)
        {
            type = ValueType.Short;
            vshort = value;
        }

        public ConstantValue (ushort value)
        {
            type = ValueType.UShort;
            vushort = value;
        }

        public ConstantValue (int value)
        {
            type = ValueType.Int;
            vint = value;
        }

        public ConstantValue (uint value)
        {
            type = ValueType.UInt;
            vuint = value;
        }

        public ConstantValue (long value)
        {
            type = ValueType.Long;
            vlong = value;
        }

        public ConstantValue (ulong value)
        {
            type = ValueType.ULong;
            vulong = value;
        }

        public ConstantValue (float value)
        {
            type = ValueType.Float;
            vfloat = value;
        }

        public ConstantValue (double value)
        {
            type = ValueType.Double;
            vdouble = value;
        }

        public ConstantValue (bool value)
        {
            type = ValueType.Bool;
            vbool = value;
        }

        public ConstantValue (string value)
        {
            type = ValueType.String;
            vstring = value;
        }

        public static ConstantValue CreateNull()
        {
            ConstantValue ret = new ConstantValue();
            ret.type = ValueType.Null;
            return ret;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="Chela.Compiler.Module.ConstantValue"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.
        /// </returns>
        public override int GetHashCode()
        {
            int res = type.GetHashCode();
            switch(type)
            {
            case ValueType.Byte:
                res ^= vbyte.GetHashCode();
                break;
            case ValueType.SByte:
                res ^= vsbyte.GetHashCode();
                break;
            case ValueType.Char:
                res ^= vchar.GetHashCode();
                break;
            case ValueType.Short:
                res ^= vshort.GetHashCode();
                break;
            case ValueType.UShort:
                res ^= vushort.GetHashCode();
                break;
            case ValueType.Int:
                res ^= vint.GetHashCode();
                break;
            case ValueType.UInt:
                res ^= vuint.GetHashCode();
                break;
            case ValueType.Long:
                res ^= vlong.GetHashCode();
                break;
            case ValueType.ULong:
            case ValueType.Size:
                res ^= vulong.GetHashCode();
                break;
            case ValueType.Float:
                res ^= vfloat.GetHashCode();
                break;
            case ValueType.Double:
                res ^= vdouble.GetHashCode();
                break;
            case ValueType.Bool:
                res ^= vbool.GetHashCode();
                break;
            case ValueType.String:
                res ^= vstring.GetHashCode();
                break;
            case ValueType.Null:
                break;
            }
            return res;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Chela.Compiler.Module.ConstantValue"/>.
        /// </summary>
        /// <param name='obj'>
        /// The <see cref="System.Object"/> to compare with the current <see cref="Chela.Compiler.Module.ConstantValue"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Chela.Compiler.Module.ConstantValue"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            // Make sure the object its a constant.
            ConstantValue cv = obj as ConstantValue;
            if(cv == null)
                return false;

            // Compare the constant type.
            if(type != cv.type)
                return false;

            // Compare the constant value.
            switch(type)
            {
            case ValueType.Byte:
                return vbyte == cv.vbyte;
            case ValueType.SByte:
                return vsbyte == cv.vsbyte;
            case ValueType.Char:
                return vchar == cv.vchar;
            case ValueType.Short:
                return vshort == cv.vshort;
            case ValueType.UShort:
                return vushort == cv.vushort;
            case ValueType.Int:
                return vint == cv.vint;
            case ValueType.UInt:
                return vuint == cv.vuint;
            case ValueType.Long:
                return vlong == cv.vlong;
            case ValueType.ULong:
            case ValueType.Size:
                return vulong == cv.vulong;
            case ValueType.Float:
                return vfloat == cv.vfloat;
            case ValueType.Double:
                return vdouble == cv.vdouble;
            case ValueType.Bool:
                return vbool == cv.vbool;
            case ValueType.String:
                return vstring == cv.vstring;
            case ValueType.Null:
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Chela.Compiler.Module.ConstantValue"/>.
        /// </summary>
        /// <param name='obj'>
        /// The <see cref="System.Object"/> to compare with the current <see cref="Chela.Compiler.Module.ConstantValue"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Chela.Compiler.Module.ConstantValue"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ConstantValue cv)
        {
            // Compare the constant type.
            if(type != cv.type)
                return false;

            // Compare the constant value.
            switch(type)
            {
            case ValueType.Byte:
                return vbyte == cv.vbyte;
            case ValueType.SByte:
                return vsbyte == cv.vsbyte;
            case ValueType.Char:
                return vchar == cv.vchar;
            case ValueType.Short:
                return vshort == cv.vshort;
            case ValueType.UShort:
                return vushort == cv.vushort;
            case ValueType.Int:
                return vint == cv.vint;
            case ValueType.UInt:
                return vuint == cv.vuint;
            case ValueType.Long:
                return vlong == cv.vlong;
            case ValueType.ULong:
            case ValueType.Size:
                return vulong == cv.vulong;
            case ValueType.Float:
                return vfloat == cv.vfloat;
            case ValueType.Double:
                return vdouble == cv.vdouble;
            case ValueType.Bool:
                return vbool == cv.vbool;
            case ValueType.String:
                return vstring == cv.vstring;
            case ValueType.Null:
                return true;
            }

            return false;
        }

        public IChelaType GetConstantType()
        {
            switch(type)
            {
            case ValueType.Byte:
                return ChelaType.GetByteType();
            case ValueType.SByte:
                return ChelaType.GetSByteType();
            case ValueType.Char:
                return ChelaType.GetCharType();
            case ValueType.Short:
                return ChelaType.GetShortType();
            case ValueType.UShort:
                return ChelaType.GetUShortType();
            case ValueType.Int:
                return ChelaType.GetIntType();
            case ValueType.UInt:
                return ChelaType.GetUIntType();
            case ValueType.Long:
                return ChelaType.GetLongType();
            case ValueType.ULong:
                return ChelaType.GetULongType();
            case ValueType.Size:
                return ChelaType.GetSizeType();
            case ValueType.Float:
                return ChelaType.GetFloatType();
            case ValueType.Double:
                return ChelaType.GetDoubleType();
            case ValueType.Bool:
                return ChelaType.GetBoolType();
            case ValueType.Null:
                return ChelaType.GetNullType();
            default:
                return null;
            }
        }

        public static bool IsSignedType(ValueType type)
        {
            switch(type)
            {
            case ValueType.Byte: return false;
            case ValueType.SByte: return true;
            case ValueType.Char: return false;
            case ValueType.Short: return true;
            case ValueType.UShort: return false;
            case ValueType.Int: return true;
            case ValueType.UInt: return false;
            case ValueType.Long: return true;
            case ValueType.ULong: return false;
            case ValueType.Size: return false;
            case ValueType.Float: return false;
            case ValueType.Double: return false;
            case ValueType.Bool: return false;
            case ValueType.Null: return false;
            default: return false;
            }
        }

        public static bool IsUnsignedType(ValueType type)
        {
            switch(type)
            {
            case ValueType.Byte: return true;
            case ValueType.SByte: return false;
            case ValueType.Char: return true;
            case ValueType.Short: return false;
            case ValueType.UShort: return true;
            case ValueType.Int: return false;
            case ValueType.UInt: return true;
            case ValueType.Long: return false;
            case ValueType.ULong: return true;
            case ValueType.Size: return true;
            case ValueType.Float: return false;
            case ValueType.Double: return false;
            case ValueType.Bool: return false;
            case ValueType.Null: return false;
            default: return false;
            }
        }

        public static bool IsFloatingType(ValueType type)
        {
            switch(type)
            {
            case ValueType.Byte: return false;
            case ValueType.SByte: return false;
            case ValueType.Char: return false;
            case ValueType.Short: return false;
            case ValueType.UShort: return false;
            case ValueType.Int: return false;
            case ValueType.UInt: return false;
            case ValueType.Long: return false;
            case ValueType.ULong: return false;
            case ValueType.Size: return false;
            case ValueType.Float: return true;
            case ValueType.Double: return true;
            case ValueType.Bool: return false;
            case ValueType.Null: return false;
            default: return false;
            }
        }

        public bool IsPositive()
        {
            switch(type)
            {
            case ValueType.Byte: return true;
            case ValueType.SByte: return vsbyte >= 0;
            case ValueType.Char: return true;
            case ValueType.Short: return vshort >= 0;
            case ValueType.UShort: return true;
            case ValueType.Int: return vint >= 0;
            case ValueType.UInt: return true;
            case ValueType.Long: return vlong >= 0;
            case ValueType.ULong: return true;
            case ValueType.Size: return true;
            case ValueType.Float: return false;
            case ValueType.Double: return false;
            case ValueType.Bool: return false;
            case ValueType.Null: return false;
            default: return false;
            }
        }

        public ConstantValue Cast(ValueType target)
        {
            // Avoid unnecessary casting.
            if(type == target)
                return this;

            // First cast to the higher of the kind.
            long signed = 0;
            ulong unsigned = 0;
            double floating = 0;
            switch(type)
            {
            case ValueType.Byte:
                unsigned = vbyte;
                break;
            case ValueType.Char:
                unsigned = vchar;
                break;
            case ValueType.UShort:
                unsigned = vushort;
                break;
            case ValueType.UInt:
                unsigned = vuint;
                break;
            case ValueType.ULong:
            case ValueType.Size:
                unsigned = vulong;
                break;
            case ValueType.SByte:
                signed = vsbyte;
                break;
            case ValueType.Short:
                signed = vshort;
                break;
            case ValueType.Int:
                signed = vint;
                break;
            case ValueType.Long:
                signed = vlong;
                break;
            case ValueType.Float:
                floating = vfloat;
                break;
            case ValueType.Double:
                floating = vdouble;
                break;
            case ValueType.Null:
            case ValueType.Bool:
                throw new ModuleException("Invalid constant cast from " + type + " to " + target);
            }

            // Now create the target constant.
            ConstantValue ret = new ConstantValue();
            ret.type = target;

            // Adjust the upper category.
            if(IsSignedType(target))
            {
                if(IsUnsignedType(type))
                    signed = (long)unsigned;
                else if(IsFloatingType(type))
                    signed = (long)floating;
            }
            else if(IsUnsignedType(target))
            {
                if(IsSignedType(type))
                    unsigned = (ulong)signed;
                else if(IsFloatingType(type))
                    unsigned = (ulong)floating;
            }
            else if(IsFloatingType(target))
            {
                if(IsUnsignedType(type))
                    floating = (double)signed;
                else if(IsSignedType(type))
                    floating = (double)floating;
            }

            // Perform the casting.
            switch(target)
            {
            case ValueType.Byte:
                ret.vbyte = (byte)unsigned;
                break;
            case ValueType.Char:
                ret.vchar = (char)unsigned;
                break;
            case ValueType.UShort:
                ret.vushort = (ushort)unsigned;
                break;
            case ValueType.UInt:
                ret.vuint = (uint)unsigned;
                break;
            case ValueType.ULong:
            case ValueType.Size:
                ret.vulong = unsigned;
                break;
            case ValueType.SByte:
                ret.vsbyte = (sbyte)signed;
                break;
            case ValueType.Short:
                ret.vshort = (short)signed;
                break;
            case ValueType.Int:
                ret.vint = (int)signed;
                break;
            case ValueType.Long:
                ret.vlong = signed;
                break;
            case ValueType.Float:
                ret.vfloat = (float)floating;
                break;
            case ValueType.Double:
                ret.vdouble = floating;
                break;
            }

            // Return the casted constant.
            return ret;
        }

        public static ConstantValue CreateSize(ulong size)
        {
            ConstantValue ret = new ConstantValue();
            ret.type = ConstantValue.ValueType.Size;
            ret.vulong = size;
            return ret;
        }

        public ConstantValue Cast(IChelaType target)
        {
            // De-const the type.
            if(target.IsConstant())
            {
                ConstantType constantType = (ConstantType)target;
                target = constantType.GetValueType();
            }

            // Discriminate the target type.
            if(target.IsInteger())
            {
                if(target.IsUnsigned())
                {
                    if(target == ChelaType.GetByteType())
                        return Cast(ValueType.Byte);
                    else if(target == ChelaType.GetUShortType())
                        return Cast(ValueType.UShort);
                    else if(target == ChelaType.GetCharType())
                        return Cast(ValueType.Char);
                    else if(target == ChelaType.GetUIntType())
                        return Cast(ValueType.UInt);
                    else if(target == ChelaType.GetULongType())
                        return Cast(ValueType.ULong);
                    else if(target == ChelaType.GetSizeType())
                        return Cast(ValueType.Size);
                    else if(target == ChelaType.GetBoolType())
                        return Cast(ValueType.Bool);
                }
                else
                {
                    if(target == ChelaType.GetSByteType())
                        return Cast(ValueType.SByte);
                    else if(target == ChelaType.GetShortType())
                        return Cast(ValueType.Short);
                    else if(target == ChelaType.GetIntType())
                        return Cast(ValueType.Int);
                    else if(target == ChelaType.GetLongType())
                        return Cast(ValueType.Long);
                }
            }
            else if(target == ChelaType.GetFloatType())
            {
                return Cast(ValueType.Float);
            }
            else if(target == ChelaType.GetDoubleType())
            {
                return Cast(ValueType.Double);
            }
            else if(target == ChelaType.GetNullType())
            {
                return Cast(ValueType.Null);
            }
            else if(target.IsStructure())
            {
                // It could be boxing.
                Structure building = (Structure)target;
                FieldVariable valueField = building.FindMember("__value") as FieldVariable;
                if(valueField == null)
                    valueField = building.FindMember("m_value") as FieldVariable;
                // Try with boxing.
                if(valueField != null && building.GetSlotCount() == 1)
                    return Cast(valueField.GetVariableType());
            }

            throw new ModuleException("Unsupported target type " + target.GetDisplayName());
        }

        public AstNode ToAstNode(TokenPosition position)
        {
            AstNode ret;
            switch(type)
            {
            case ValueType.Byte:
                ret = new ByteConstant(vbyte, position);
                break;
            case ValueType.SByte:
                ret = new SByteConstant(vsbyte, position);
                break;
            case ValueType.Char:
                ret = new CharacterConstant(vchar, position);
                break;
            case ValueType.Short:
                ret = new ShortConstant(vshort, position);
                break;
            case ValueType.UShort:
                ret = new UShortConstant(vushort, position);
                break;
            case ValueType.Int:
                ret = new IntegerConstant(vint, position);
                break;
            case ValueType.UInt:
                ret = new UIntegerConstant(vuint, position);
                break;
            case ValueType.Long:
                ret = new LongConstant(vlong, position);
                break;
            case ValueType.ULong:
                ret = new ULongConstant(vulong, position);
                break;
            case ValueType.Float:
                ret = new FloatConstant(vfloat, position);
                break;
            case ValueType.Double:
                ret = new DoubleConstant(vdouble, position);
                break;
            case ValueType.Bool:
                ret = new BoolConstant(vbool, position);
                break;
            case ValueType.Null:
                ret = new NullConstant(position);
                break;
            case ValueType.String:
                ret = new StringConstant(vstring, position);
                break;
            default:
                ret = null;
                break;
            }

            if(ret != null)
                ret.SetNodeValue(this);
            return ret;
        }

        public override string ToString ()
        {
            switch(type)
            {
            case ValueType.Byte:
                return vbyte.ToString();
            case ValueType.SByte:
                return vsbyte.ToString();
            case ValueType.Char:
                return vchar.ToString();
            case ValueType.Short:
                return vshort.ToString();
            case ValueType.UShort:
                return vushort.ToString();
            case ValueType.Int:
                return vint.ToString();
            case ValueType.UInt:
                return vuint.ToString();
            case ValueType.Long:
                return vlong.ToString();
            case ValueType.Size:
            case ValueType.ULong:
                return vulong.ToString();
            case ValueType.Float:
                return vfloat.ToString();
            case ValueType.Double:
                return vdouble.ToString();
            case ValueType.Bool:
                return vbool.ToString();
            case ValueType.Null:
                return "null";
            default:
                return "(unknown)";
            }
        }

        public static int GetRawSize(ValueType type)
        {
            switch(type)
            {
            case ValueType.Byte:
            case ValueType.SByte:
                return 1;
            case ValueType.Char:
            case ValueType.Short:
            case ValueType.UShort:
                return 2;
            case ValueType.Int:
            case ValueType.UInt:
                return 4;
            case ValueType.Long:
            case ValueType.ULong:
            case ValueType.Size:
                return 8;
            case ValueType.Float:
                return 4;
            case ValueType.Double:
                return 8;
            case ValueType.Bool:
                return 1;
            case ValueType.String:
                return 4;
            case ValueType.Null:
            default:
                return 0;
            }
        }

        public int GetRawSize()
        {
            return GetRawSize(type);
        }

        public int GetQualifiedSize()
        {
            return GetRawSize() + 1;
        }

        public void Write(ChelaModule module, ModuleWriter writer)
        {
            switch(type)
            {
            case ValueType.Byte:
                writer.Write(vbyte);
                break;
            case ValueType.SByte:
                writer.Write(vsbyte);
                break;
            case ValueType.Char:
                writer.Write((ushort)vchar);
                break;
            case ValueType.Short:
                writer.Write(vshort);
                break;
            case ValueType.UShort:
                writer.Write(vushort);
                break;
            case ValueType.Int:
                writer.Write(vint);
                break;
            case ValueType.UInt:
                writer.Write(vuint);
                break;
            case ValueType.Long:
                writer.Write(vlong);
                break;
            case ValueType.Size:
            case ValueType.ULong:
                writer.Write(vulong);
                break;
            case ValueType.Float:
                writer.Write(vfloat);
                break;
            case ValueType.Double:
                writer.Write(vdouble);
                break;
            case ValueType.Bool:
                {
                    byte v = vbool ? (byte)1 : (byte)0;
                    writer.Write(v);
                }
                break;
            case ValueType.String:
                writer.Write(module.RegisterString(vstring));
                break;
            case ValueType.Null:
            default:
                break;
            }
        }

        public void WriteQualified(ChelaModule module, ModuleWriter writer)
        {
            byte typeId = (byte)type;
            writer.Write(typeId);
            Write(module, writer);
        }

        public static ConstantValue Read(ChelaModule module, ModuleReader reader, ValueType type)
        {
            ConstantValue res = null;
            switch(type)
            {
            case ValueType.Byte:
                return new ConstantValue(reader.ReadByte());
            case ValueType.SByte:
                return new ConstantValue(reader.ReadSByte());
            case ValueType.Char:
                return new ConstantValue((char)reader.ReadUShort());
            case ValueType.Short:
                return new ConstantValue(reader.ReadShort());
            case ValueType.UShort:
                return new ConstantValue(reader.ReadUShort());
            case ValueType.Int:
                return new ConstantValue(reader.ReadInt());
            case ValueType.UInt:
                return new ConstantValue(reader.ReadUInt());
            case ValueType.Long:
                return new ConstantValue(reader.ReadLong());
            case ValueType.Size:
                res = new ConstantValue(reader.ReadULong());
                res.type = ConstantValue.ValueType.Size;
                return res;
            case ValueType.ULong:
                return new ConstantValue(reader.ReadULong());
            case ValueType.Float:
                return new ConstantValue(reader.ReadFloat());
            case ValueType.Double:
                return new ConstantValue(reader.ReadDouble());
            case ValueType.Bool:
                return new ConstantValue(reader.ReadByte() != 0);
            case ValueType.String:
                return new ConstantValue(module.GetString(reader.ReadUInt()));
            case ValueType.Null:
                return new ConstantValue();
            default:
                return null;
            }
        }

        public static ConstantValue ReadQualified(ChelaModule module, ModuleReader reader)
        {
            byte typeId = reader.ReadByte();
            ValueType type = (ValueType)typeId;
            return Read(module, reader, type);
        }

        public static void SkipQualified(ModuleReader reader)
        {
            ValueType type = (ValueType)reader.ReadByte();
            reader.Skip(GetRawSize(type));
        }

        public bool GetBoolValue()
        {
            if(type == ValueType.Bool)
            {
                return vbool;
            }
            else
            {
                ConstantValue casted = Cast(ValueType.Bool);
                return casted.vbool;
            }
        }

        public int GetIntValue()
        {
            if(type == ValueType.Int)
            {
                return vint;
            }
            else
            {
                ConstantValue casted = Cast(ValueType.Int);
                return casted.vint;
            }
        }

        public long GetLongValue()
        {
            if(type == ValueType.Long)
            {
                return vlong;
            }
            else
            {
                ConstantValue casted = Cast(ValueType.Long);
                return casted.vlong;
            }
        }

        public uint GetUIntValue()
        {
            if(type == ValueType.UInt)
            {
                return vuint;
            }
            else
            {
                ConstantValue casted = Cast(ValueType.UInt);
                return casted.vuint;
            }
        }

        public ulong GetULongValue()
        {
            if(type == ValueType.ULong)
            {
                return vulong;
            }
            else
            {
                ConstantValue casted = Cast(ValueType.ULong);
                return casted.vulong;
            }
        }

        private static void CheckTypes(ConstantValue a, ConstantValue b)
        {
            if(a.type != b.type)
                throw new ModuleException("only costants with the same types can be operated.");
        }

        public static ConstantValue operator+(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte + b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(a.vsbyte + b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort + b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort + b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint + b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint + b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong + b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong + b.vulong;
                break;
            case ValueType.Float:
                res.vfloat = a.vfloat + b.vfloat;
                break;
            case ValueType.Double:
                res.vdouble = a.vdouble + b.vdouble;
                break;
            case ValueType.String:
                res.vstring = a.vstring + b.vstring;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator-(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte - b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(a.vsbyte - b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort - b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort - b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint - b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint - b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong - b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong - b.vulong;
                break;
            case ValueType.Float:
                res.vfloat = a.vfloat - b.vfloat;
                break;
            case ValueType.Double:
                res.vdouble = a.vdouble - b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator*(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte * b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(a.vsbyte * b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort * b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort * b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint * b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint * b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong * b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong * b.vulong;
                break;
            case ValueType.Float:
                res.vfloat = a.vfloat * b.vfloat;
                break;
            case ValueType.Double:
                res.vdouble = a.vdouble * b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator/(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte / b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(a.vsbyte / b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort / b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort / b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint / b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint / b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong / b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong / b.vulong;
                break;
            case ValueType.Float:
                res.vfloat = a.vfloat / b.vfloat;
                break;
            case ValueType.Double:
                res.vdouble = a.vdouble / b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator%(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte % b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(a.vsbyte % b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort % b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort % b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint % b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint % b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong % b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong % b.vulong;
                break;
            case ValueType.Float:
                res.vfloat = a.vfloat % b.vfloat;
                break;
            case ValueType.Double:
                res.vdouble = a.vdouble % b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator|(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte | b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)((byte)a.vsbyte | (byte)b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort | b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort | b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint | b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint | b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong | b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong | b.vulong;
                break;
            case ValueType.Float:
            case ValueType.Double:
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator&(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte & b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(a.vsbyte & b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort & b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort & b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint & b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint & b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong & b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong & b.vulong;
                break;
            case ValueType.Float:
            case ValueType.Double:
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator^(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte ^ b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(a.vsbyte ^ b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort ^ b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort ^ b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint ^ b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint ^ b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong ^ b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong ^ b.vulong;
                break;
            case ValueType.Float:
            case ValueType.Double:
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue BitLeft(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte << b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(a.vsbyte << b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort << b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort << b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint << b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint << (int)b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong << (int)b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong << (int)b.vulong;
                break;
            case ValueType.Float:
            case ValueType.Double:
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue BitRight(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = a.type;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(a.vbyte >> b.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(a.vsbyte >> b.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(a.vshort >> b.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(a.vushort >> b.vushort);
                break;
            case ValueType.Int:
                res.vint = a.vint >> b.vint;
                break;
            case ValueType.UInt:
                res.vuint = a.vuint >> (int)b.vuint;
                break;
            case ValueType.Long:
                res.vlong = a.vlong >> (int)b.vlong;
                break;
            case ValueType.ULong:
                res.vulong = a.vulong >> (int)b.vulong;
                break;
            case ValueType.Float:
            case ValueType.Double:
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator<(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = ValueType.Bool;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbool = a.vbyte < b.vbyte;
                break;
            case ValueType.SByte:
                res.vbool = a.vsbyte < b.vsbyte;
                break;
            case ValueType.Short:
                res.vbool = a.vshort < b.vshort;
                break;
            case ValueType.UShort:
                res.vbool = a.vushort < b.vushort;
                break;
            case ValueType.Int:
                res.vbool = a.vint < b.vint;
                break;
            case ValueType.UInt:
                res.vbool = a.vuint < b.vuint;
                break;
            case ValueType.Long:
                res.vbool = a.vlong < b.vlong;
                break;
            case ValueType.ULong:
                res.vbool = a.vulong < b.vulong;
                break;
            case ValueType.Float:
                res.vbool = a.vfloat < b.vfloat;
                break;
            case ValueType.Double:
                res.vbool = a.vdouble < b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator>(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = ValueType.Bool;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbool = a.vbyte > b.vbyte;
                break;
            case ValueType.SByte:
                res.vbool = a.vsbyte > b.vsbyte;
                break;
            case ValueType.Short:
                res.vbool = a.vshort > b.vshort;
                break;
            case ValueType.UShort:
                res.vbool = a.vushort > b.vushort;
                break;
            case ValueType.Int:
                res.vbool = a.vint > b.vint;
                break;
            case ValueType.UInt:
                res.vbool = a.vuint > b.vuint;
                break;
            case ValueType.Long:
                res.vbool = a.vlong > b.vlong;
                break;
            case ValueType.ULong:
                res.vbool = a.vulong > b.vulong;
                break;
            case ValueType.Float:
                res.vbool = a.vfloat > b.vfloat;
                break;
            case ValueType.Double:
                res.vbool = a.vdouble > b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator<=(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = ValueType.Bool;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbool = a.vbyte <= b.vbyte;
                break;
            case ValueType.SByte:
                res.vbool = a.vsbyte <= b.vsbyte;
                break;
            case ValueType.Short:
                res.vbool = a.vshort <= b.vshort;
                break;
            case ValueType.UShort:
                res.vbool = a.vushort <= b.vushort;
                break;
            case ValueType.Int:
                res.vbool = a.vint <= b.vint;
                break;
            case ValueType.UInt:
                res.vbool = a.vuint <= b.vuint;
                break;
            case ValueType.Long:
                res.vbool = a.vlong <= b.vlong;
                break;
            case ValueType.ULong:
                res.vbool = a.vulong <= b.vulong;
                break;
            case ValueType.Float:
                res.vbool = a.vfloat <= b.vfloat;
                break;
            case ValueType.Double:
                res.vbool = a.vdouble <= b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator>=(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = ValueType.Bool;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbool = a.vbyte >= b.vbyte;
                break;
            case ValueType.SByte:
                res.vbool = a.vsbyte >= b.vsbyte;
                break;
            case ValueType.Short:
                res.vbool = a.vshort >= b.vshort;
                break;
            case ValueType.UShort:
                res.vbool = a.vushort >= b.vushort;
                break;
            case ValueType.Int:
                res.vbool = a.vint >= b.vint;
                break;
            case ValueType.UInt:
                res.vbool = a.vuint >= b.vuint;
                break;
            case ValueType.Long:
                res.vbool = a.vlong >= b.vlong;
                break;
            case ValueType.ULong:
                res.vbool = a.vulong >= b.vulong;
                break;
            case ValueType.Float:
                res.vbool = a.vfloat >= b.vfloat;
                break;
            case ValueType.Double:
                res.vbool = a.vdouble >= b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue Equals(ConstantValue a, ConstantValue b)
        {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = ValueType.Bool;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbool = a.vbyte == b.vbyte;
                break;
            case ValueType.SByte:
                res.vbool = a.vsbyte == b.vsbyte;
                break;
            case ValueType.Short:
                res.vbool = a.vshort == b.vshort;
                break;
            case ValueType.UShort:
                res.vbool = a.vushort == b.vushort;
                break;
            case ValueType.Int:
                res.vbool = a.vint == b.vint;
                break;
            case ValueType.UInt:
                res.vbool = a.vuint == b.vuint;
                break;
            case ValueType.Long:
                res.vbool = a.vlong == b.vlong;
                break;
            case ValueType.ULong:
                res.vbool = a.vulong == b.vulong;
                break;
            case ValueType.Float:
                res.vbool = a.vfloat == b.vfloat;
                break;
            case ValueType.Double:
                res.vbool = a.vdouble == b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

       public static ConstantValue NotEquals(ConstantValue a, ConstantValue b)
       {
            // Check the types.
            CheckTypes(a, b);

            // Create the result
            ConstantValue res = new ConstantValue();
            res.type = ValueType.Bool;

            // Perform the operation.
            switch(res.type)
            {
            case ValueType.Byte:
                res.vbool = a.vbyte != b.vbyte;
                break;
            case ValueType.SByte:
                res.vbool = a.vsbyte != b.vsbyte;
                break;
            case ValueType.Short:
                res.vbool = a.vshort != b.vshort;
                break;
            case ValueType.UShort:
                res.vbool = a.vushort != b.vushort;
                break;
            case ValueType.Int:
                res.vbool = a.vint != b.vint;
                break;
            case ValueType.UInt:
                res.vbool = a.vuint != b.vuint;
                break;
            case ValueType.Long:
                res.vbool = a.vlong != b.vlong;
                break;
            case ValueType.ULong:
                res.vbool = a.vulong != b.vulong;
                break;
            case ValueType.Float:
                res.vbool = a.vfloat != b.vfloat;
                break;
            case ValueType.Double:
                res.vbool = a.vdouble != b.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            // Return the result.
            return res;
        }

        public static ConstantValue operator!(ConstantValue a)
        {
            if(a.type != ConstantValue.ValueType.Bool)
                throw new ModuleException("cannot negate constant.");
            return new ConstantValue(!a.vbool);
        }

        public static ConstantValue operator~(ConstantValue a)
        {
            ConstantValue res = new ConstantValue();
            res.type = a.type;
            switch(a.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(~a.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(~a.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(~a.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(~a.vushort);
                break;
            case ValueType.Int:
                res.vint = ~a.vint;
                break;
            case ValueType.UInt:
                res.vuint = ~a.vuint;
                break;
            case ValueType.Long:
                res.vlong = ~a.vlong;
                break;
            case ValueType.ULong:
                res.vulong = ~a.vulong;
                break;
            case ValueType.Float:
            case ValueType.Double:
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            return res;
        }

        public static ConstantValue operator-(ConstantValue a)
        {
            ConstantValue res = new ConstantValue();
            res.type = a.type;
            switch(a.type)
            {
            case ValueType.Byte:
                res.vbyte = (byte)(-a.vbyte);
                break;
            case ValueType.SByte:
                res.vsbyte = (sbyte)(-a.vsbyte);
                break;
            case ValueType.Short:
                res.vshort = (short)(-a.vshort);
                break;
            case ValueType.UShort:
                res.vushort = (ushort)(-a.vushort);
                break;
            case ValueType.Int:
                res.vint = -a.vint;
                break;
            case ValueType.UInt:
                res.vuint = (uint)(-a.vuint);
                break;
            case ValueType.Long:
                res.vlong = -a.vlong;
                break;
            case ValueType.ULong:
                res.vulong = (ulong)(-(long)a.vulong);
                break;
            case ValueType.Float:
                res.vfloat = -a.vfloat;
                break;
            case ValueType.Double:
                res.vdouble = -a.vdouble;
                break;
            case ValueType.Bool:
            case ValueType.Null:
            default:
                throw new ModuleException("cannot operate constants.");
            }

            return res;
        }
    }
}
