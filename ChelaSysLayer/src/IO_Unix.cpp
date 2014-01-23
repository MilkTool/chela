#if defined(__unix__)

#include <sys/stat.h>
#include <sys/types.h>
#include <fcntl.h>
#include <unistd.h>
#include <string.h>
#include <errno.h>
#include "SysLayer.hpp"
#include "ChelaUtf8.hpp"

SYSAPI FILE_HANDLE _Chela_IO_GetStdin()
{
    return (FILE_HANDLE)STDIN_FILENO;
}

SYSAPI FILE_HANDLE _Chela_IO_GetStdout()
{
    return (FILE_HANDLE)STDOUT_FILENO;
}

SYSAPI FILE_HANDLE _Chela_IO_GetStderr()
{
    return (FILE_HANDLE)STDERR_FILENO;
}

SYSAPI size_t _Chela_IO_Read(void *ptr, size_t size, size_t count, FILE_HANDLE stream)
{
    ssize_t ret = read((size_t)stream, ptr, size*count);
    return size != 0 ? ret/size : 0;
}

SYSAPI size_t _Chela_IO_Write(const void *ptr, size_t size, size_t count, FILE_HANDLE stream)
{
    ssize_t ret = write((size_t)stream, ptr, size*count);
    return size != 0 ? ret/size : 0;
}

SYSAPI FILE_HANDLE _Chela_IO_Open(int pathSize, const char16_t *utf16Path, int mode, int access, int share, int async)
{
    int flags = 0;

    // Encode the path in utf-8
    ChelaUtf8String path(pathSize, utf16Path);

    // Read the access.
    if(access == FA_ReadWrite)
        flags = O_RDWR;
    else if(access == FA_Read)
        flags = O_RDONLY;
    else if(access == FA_Write)
        flags = O_WRONLY;
    else
        _Chela_Sys_Fatal("Invalid file access flag");

    // Read the mode
    switch(mode)
    {
    case FM_CreateNew:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use create new mode without write access.");
        flags |= O_CREAT | O_EXCL;
        break;
    case FM_Create:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use create mode without write access.");
        flags |= O_CREAT | O_TRUNC;
        break;
    case FM_Open:
        break;
    case FM_OpenOrCreate:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use open or create mode without write access.");
        flags |= O_CREAT;
        break;
    case FM_Truncate:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use truncate mode without write access.");
        flags |= O_TRUNC;
        break;
    case FM_Append:
        if((access & FA_Write) == 0)
            _Chela_Sys_Fatal("Cannot use append mode without write access.");
        flags |= O_APPEND;
        break;
    default:
        _Chela_Sys_Fatal("Invalid file mode");
        break;
    }

    // TODO: Support sharing locking.

    // Add the async flag.
    if(async)
        flags |= O_ASYNC;

    // Open the file.
    return (FILE_HANDLE)(size_t)open(path.c_str(), flags, S_IRUSR | S_IWUSR | S_IRGRP | S_IROTH);
}

SYSAPI int _Chela_IO_Close(FILE_HANDLE file)
{
    return close((size_t)file);
}

SYSAPI int _Chela_IO_Flush(FILE_HANDLE stream)
{
    return fsync((size_t)stream);
}

SYSAPI size_t _Chela_IO_Tell(FILE_HANDLE stream)
{
    return lseek((size_t)stream, 0, SEEK_CUR);
}

SYSAPI int _Chela_IO_Seek(FILE_HANDLE stream, size_t offset, int origin)
{
    int whence = 0;
    switch(origin)
    {
    case SO_Begin:
        whence = SEEK_SET;
        break;
    case SO_Current:
        whence = SEEK_CUR;
        break;
    case SO_End:
        whence = SEEK_END;
        break;
    default:
        _Chela_Sys_Fatal("Invalid seek origin value.");
        break;
    }
    return lseek((size_t)stream, offset, whence) != off_t(-1);
}

SYSAPI int _Chela_IO_SetLength(FILE_HANDLE stream, size_t length)
{
    return ftruncate((size_t)stream, length);
}

SYSAPI FILE_HANDLE _Chela_IO_InvalidFile()
{
    return (FILE_HANDLE)(size_t)-1;
}

SYSAPI int _Chela_IO_ErrorNo()
{
    return errno;
}

SYSAPI const char *_Chela_IO_ErrorStr(int error)
{
    return strerror(errno);
}

SYSAPI int _Chela_IO_TransError(int id)
{
    switch(id)
    {
    case IOE_NameTooLong: return ENAMETOOLONG;
    case IOE_NotFound: return ENOENT;
    case IOE_NotDir: return ENOTDIR;
    case IOE_Access: return EACCES;
    default: return 0;
    }
}

#endif
