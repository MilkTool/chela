#ifndef CHELAVM_MODULE_READER_HPP
#define CHELAVM_MODULE_READER_HPP

#ifndef __STDC_CONSTANT_MACROS
#define __STDC_CONSTANT_MACROS
#endif

#include <stdexcept>
#include <string>
#include <cstdlib>
#include <cstdio>
#include <cstring>
#include <errno.h>
#include <inttypes.h>

#ifndef UINT64_C
#define UINT64_C(x) (x ## ULL)
#endif

#ifndef INT64_C
#define INT64_C(x) (x ## LL)
#endif

namespace ChelaVm
{
	class ModuleReaderException: public std::runtime_error
	{
	public:
		ModuleReaderException(const std::string &what)
			: std::runtime_error(what) {}
	};
	
	class ModuleReaderEof: public ModuleReaderException
	{
	public:
		ModuleReaderEof()
			: ModuleReaderException("end of file reached.") {}
	};
	
	class ModuleReader
	{
	public:
		ModuleReader(const std::string &name)
            : name(name)
		{
			FILE *file = fopen(name.c_str(), "rb");
			if(!file)
				throw ModuleReaderException("Failed to open file '" + name + "': " + strerror(errno));
			Open(file);
            fileBase = 0;
		}
		
		ModuleReader(FILE *input)
		{
			Open(input);
            fileBase = 0;
		}

        ModuleReader(size_t length, uint8_t *data)
            : file(NULL), length(length), position(0), data(data), fileBase(0)
        {
        }
		
		~ModuleReader()
		{
			if(file)
				fclose(file);
		}
		
		size_t GetPosition() const
		{
            if(data)
                return position - fileBase;
            else
			    return ftell(file) - fileBase;
		}
		
		size_t GetFileSize() const
		{
            if(data)
                return length - fileBase;
            else
            {
    			size_t pos = ftell(file);
    			fseek(file, 0, SEEK_END);
    			size_t ret = ftell(file);
    			fseek(file, pos, SEEK_SET);
    			return ret - fileBase;
            }
		}
		
		void Seek(size_t offset, int mode)
		{
            if(data)
            {
                // Perform seek.
                switch(mode)
                {
                case SEEK_SET:
                    position = offset + fileBase;
                    break;
                case SEEK_CUR:
                    position += offset;
                    break;
                case SEEK_END:
                    position = length + offset;
                    break;
                }

                // Make sure the position is not greater than the length.
                if(position > length)
                    position = length;
            }
            else
            {
                if(mode == SEEK_SET)
                    fseek(file, offset + fileBase, mode);
                else
    			    fseek(file, offset, mode);
            }
		}
		
		void Skip(size_t amount)
		{
			Seek(amount, SEEK_CUR);
		}

        void FixBase()
        {
            fileBase += GetPosition();
        }

		size_t Read(void *dest, size_t size)
		{
            if(data)
            {
                // Raise EOF
                if(position >= length)
                    throw ModuleReaderEof();

                // Compute the amount to read.
                size_t toread;
                if(position + size < length)
                    toread = size;
                else
                    toread = length - position;

                // Perform the read.
                memcpy(dest, &data[position], size);
                position += toread;
                return toread;
            }
            else
            {
    			if(feof(file))
    				throw ModuleReaderEof();
    			return fread(dest, 1, size, file);
            }
		}
		
		ModuleReader &operator>>(uint8_t &v)
		{
			Read(&v, 1);
			return *this;
		}

		ModuleReader &operator>>(int8_t &v)
		{
			Read(&v, 1);
			return *this;
		}

		ModuleReader &operator>>(uint16_t &v)
		{
			uint8_t data[2];
			Read(data, 2);
			v = (data[1]<<8) | (data[0]);
			return *this;
		}

		ModuleReader &operator>>(int16_t &v)
		{
			return *this >> (uint16_t&)v;
		}

		ModuleReader &operator>>(uint32_t &v)
		{
			uint8_t data[4];
			Read(data, 4);
			v = (data[3] << 24) | (data[2] << 16) | (data[1]<<8) | (data[0]);
			return *this;
		}

		ModuleReader &operator>>(int32_t &v)
		{
			return *this >> (uint32_t&)v;
		}
		
		ModuleReader &operator>>(uint64_t &v)
		{
			uint8_t data[8];
			Read(data, 8);
			v = ((uint64_t)data[7] << UINT64_C(56)) |
				((uint64_t)data[6] << UINT64_C(48)) |
				((uint64_t)data[5] << UINT64_C(40)) |
				((uint64_t)data[4] << UINT64_C(32)) |
				((uint64_t)data[3] << UINT64_C(24)) |
				((uint64_t)data[2] << UINT64_C(16)) |
				((uint64_t)data[1] << UINT64_C(8)) |
				((uint64_t)data[0] /*<< 0l*/);			
				
			return *this;
		}

		ModuleReader &operator>>(int64_t &v)
		{
			return *this >> (uint64_t&)v;
		}
		
		ModuleReader &operator>>(float &v)
		{
			uint32_t data;
			*this >> data;

            uint32_t &dref = data;
			v = reinterpret_cast<float&> (dref);
			return *this;
		}
		
		ModuleReader &operator>>(double &v)
		{
			uint64_t data;
			*this >> data;

            uint64_t &dref = data;
			v = reinterpret_cast<double&> (dref);
			return *this;
		}
		
		ModuleReader &operator>>(std::string &v)
		{
			uint16_t len;
			*this >> len;
			
			for(size_t i = 0; i < len; i++)
			{
				uint8_t c;
				*this >> c;
				
				// TODO: Decode UTF-8 characters.
				v.push_back(c);
			}
			
			return *this;
		}

        const std::string &GetFileName() const
        {
            return name;
        }
		
	private:
		void Open(FILE *f)
		{
			this->file = f;
            data = NULL;
		}

        // File stream.
		FILE *file;
        std::string name;

        // Memory stream.
        size_t length;
        size_t position;
        uint8_t *data;

        // Subfiles.
        size_t fileBase;
	};

    // Byte swapping functions.
    inline uint64_t SwapBytes(uint64_t value)
    {
        return ((value & UINT64_C(0x00000000000000FF)) << 56) |
               ((value & UINT64_C(0x000000000000FF00)) << 40) |
               ((value & UINT64_C(0x0000000000FF0000)) << 24) |
               ((value & UINT64_C(0x00000000FF000000)) <<  8) |
               ((value & UINT64_C(0x000000FF00000000)) >>  8) |
               ((value & UINT64_C(0x0000FF0000000000)) >> 24) |
               ((value & UINT64_C(0x00FF000000000000)) >> 40) |
               ((value & UINT64_C(0xFF00000000000000)) >> 56);
    }

    inline int64_t SwapBytes(int64_t value)
    {
        return ((value & INT64_C(0x00000000000000FF)) << 56) |
               ((value & INT64_C(0x000000000000FF00)) << 40) |
               ((value & INT64_C(0x0000000000FF0000)) << 24) |
               ((value & INT64_C(0x00000000FF000000)) <<  8) |
               ((value & INT64_C(0x000000FF00000000)) >>  8) |
               ((value & INT64_C(0x0000FF0000000000)) >> 24) |
               ((value & INT64_C(0x00FF000000000000)) >> 40) |
               ((value & INT64_C(0xFF00000000000000)) >> 56);
    }

    inline uint32_t SwapBytes(uint32_t value)
    {
        return ((value & 0x000000FF) << 24) |
               ((value & 0x0000FF00) <<  8) |
               ((value & 0x00FF0000) >>  8) |
               ((value & 0xFF000000) >> 24);
    }

    inline int32_t SwapBytes(int32_t value)
    {
        return ((value & 0x000000FF) << 24)|
               ((value & 0x0000FF00) << 8) |
               ((value & 0x00FF0000) >> 8) |
               ((value & 0xFF000000) >> 24);
    }

    inline uint16_t SwapBytes(uint16_t value)
    {
        return ((value & 0x00FF) << 8)|
               ((value & 0xFF00) >> 8);
    }

    inline int16_t SwapBytes(int16_t value)
    {
        return ((value & 0x00FF) << 8)|
               ((value & 0xFF00) >> 8);
    }

    template <typename T>
    void SwapBytesD(T &value)
    {
        value = SwapBytes(value);
    }

};

#endif //CHELAVM_MODULE_READER_HPP
