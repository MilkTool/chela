#ifdef _WIN32
#include "SysLayer.hpp"
#include <windows.h>

SYSAPI uint64_t _Chela_Date_UtcNow()
{
    FILETIME ft;
    GetSystemTimeAsFileTime(&ft);
    return (uint64_t)ft.dwLowDateTime |
           (((uint64_t)ft.dwHighDateTime) << UINT64_C(32));
}

SYSAPI uint64_t _Chela_Date_Now()
{
    TIME_ZONE_INFORMATION tz;
    GetTimeZoneInformation(&tz);
    return _Chela_Date_UtcNow() - tz.Bias;
}

#endif
