#ifdef _WIN32
#include "SysLayer.hpp"
#include <windows.h>

SYSAPI FILE_HANDLE _Chela_IO_GetStdin()
{
    return (FILE_HANDLE)GetStdHandle(STD_INPUT_HANDLE);
}

SYSAPI FILE_HANDLE _Chela_IO_GetStdout()
{
    return (FILE_HANDLE)GetStdHandle(STD_OUTPUT_HANDLE);
}

SYSAPI FILE_HANDLE _Chela_IO_GetStderr()
{
    return (FILE_HANDLE)GetStdHandle(STD_ERROR_HANDLE);
}

SYSAPI size_t _Chela_IO_Read(void *ptr, size_t size, size_t count, FILE_HANDLE stream)
{
    DWORD readed;
    if(ReadFile((HANDLE)stream, ptr, size*count, &readed, NULL))
        return readed/size;
    else
        return 0;
}

SYSAPI size_t _Chela_IO_Write(const void *ptr, size_t size, size_t count, FILE_HANDLE stream)
{
    DWORD written;
    if(WriteFile((HANDLE)stream, ptr, size*count, &written, NULL))
        return written/size;
    else
        return 0;
}

SYSAPI FILE_HANDLE _Chela_IO_Open(int pathSize, const char16_t *utf16Path, int mode, int access, int share, int async)
{
    // Read the access.
    DWORD desiredAccess = 0;
    if(access == FA_ReadWrite)
        desiredAccess = GENERIC_READ | GENERIC_WRITE;
    else if(access == FA_Read)
        desiredAccess = GENERIC_READ;
    else if(access == FA_Write)
        desiredAccess = GENERIC_WRITE;
    else
        _Chela_Sys_Fatal("Invalid file access flag");

    // Read the mode
    DWORD creationDisposition = 0;
    DWORD flagsAttributes = FILE_ATTRIBUTE_NORMAL;
    switch(mode)
    {
    case FM_CreateNew:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use create new mode without write access.");
        creationDisposition = CREATE_NEW;
        break;
    case FM_Create:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use create mode without write access.");
        creationDisposition = CREATE_ALWAYS;
        break;
    case FM_Open:
        creationDisposition = OPEN_EXISTING;
        break;
    case FM_OpenOrCreate:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use open or create mode without write access.");
        creationDisposition = OPEN_ALWAYS;
        break;
    case FM_Truncate:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use truncate mode without write access.");
        creationDisposition = TRUNCATE_EXISTING;
        break;
    case FM_Append:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use append mode without write access.");
        desiredAccess = FILE_APPEND_DATA;
        creationDisposition = OPEN_ALWAYS;        
        break;
    default:
        _Chela_Sys_Fatal("Invalid file mode");
        break;
    }

    // Read the share flag.
    DWORD shareMode = 0;
    switch(share)
    {
    case FS_None:
        shareMode = 0;
        break;
    case FS_Read:
        shareMode = FILE_SHARE_READ;
        break;
    case FS_Write:
        shareMode = FILE_SHARE_WRITE;
        break;
    case FS_ReadWrite:
        shareMode = FILE_SHARE_READ | FILE_SHARE_WRITE;
        break;
    }

    // Add the async flag.
    if(async)
        flagsAttributes |= FILE_FLAG_OVERLAPPED;

    // Open the file.
    HANDLE fileHandle = CreateFileW((LPCWSTR)utf16Path, desiredAccess,
                shareMode, NULL, creationDisposition, flagsAttributes, NULL);
    if(fileHandle == NULL)
        return 0;
        
    // Move the file pointer to the end in appending mode.
    if(mode == FM_Append)
        SetFilePointer(fileHandle, 0, NULL, FILE_END);
        
    // Return the file handle.
    return (FILE_HANDLE)(size_t)fileHandle;
}

SYSAPI int _Chela_IO_Close(FILE_HANDLE file)
{
    return CloseHandle((HANDLE)file);
}

SYSAPI int _Chela_IO_Flush(FILE_HANDLE stream)
{
    return FlushFileBuffers((HANDLE)stream);
}

SYSAPI size_t _Chela_IO_Tell(FILE_HANDLE stream)
{
    return SetFilePointer((HANDLE)stream, 0, NULL, FILE_CURRENT);
}

SYSAPI int _Chela_IO_Seek(FILE_HANDLE stream, size_t offset, int origin)
{
    DWORD whence = 0;
    switch(origin)
    {
    case SO_Begin:
        whence = FILE_BEGIN;
        break;
    case SO_Current:
        whence = FILE_CURRENT;
        break;
    case SO_End:
        whence = FILE_END;
        break;
    default:
        _Chela_Sys_Fatal("Invalid seek origin value.");
        break;
    }
    return SetFilePointer((HANDLE)stream, offset, NULL, whence) != INVALID_SET_FILE_POINTER;
}

SYSAPI int _Chela_IO_SetLength(FILE_HANDLE stream, size_t length)
{
    HANDLE file = (HANDLE)stream;
    
    // Store the old position and move to the new end.
    DWORD oldPosition = SetFilePointer(file, length, NULL, FILE_BEGIN);
    bool res = SetEndOfFile(file);
    
    // Move back to the old position.
    if(oldPosition < length)
        SetFilePointer(file, length, NULL, FILE_BEGIN);
    return res;
}

SYSAPI FILE_HANDLE _Chela_IO_InvalidFile()
{
    return (FILE_HANDLE)0;
}

SYSAPI int _Chela_IO_ErrorNo()
{
    return GetLastError();
}


const char *_Chela_Win32_GetErrorMessage(int errorCode);
SYSAPI const char *_Chela_IO_ErrorStr(int error)
{
    return _Chela_Win32_GetErrorMessage(error);
}

SYSAPI int _Chela_IO_TransError(int id)
{
    switch(id)
    {
    case IOE_NameTooLong: return ERROR_BAD_PATHNAME;
    case IOE_NotFound: return ERROR_FILE_NOT_FOUND;
    case IOE_NotDir: return ERROR_PATH_NOT_FOUND;
    case IOE_Access: return ERROR_ACCESS_DENIED;
    default: return 0;
    }
}

#endif
