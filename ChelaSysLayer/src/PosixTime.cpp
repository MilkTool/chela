#if defined(__unix__)
#include <sys/time.h>
#include <time.h>
#include "SysLayer.hpp"

uint64_t TicksPerSecond = UINT64_C(10000000);
uint64_t EpochAddition = UINT64_C(62135586000);

// Convert a time value into ticks.
inline uint64_t ConvertTime(timeval &tv)
{
    return uint64_t(tv.tv_usec)*UINT64_C(10) + uint64_t(tv.tv_sec)*TicksPerSecond + EpochAddition;
}

SYSAPI uint64_t _Chela_Date_Now()
{
    // Get the time of the day.
    timeval tv;
    struct timezone tz;
    gettimeofday(&tv, &tz);

    // Convert the time and return.
    uint64_t ret = ConvertTime(tv);
    ret += uint64_t(tz.tz_minuteswest)*TicksPerSecond*UINT64_C(60);
    return ret;
}

SYSAPI uint64_t _Chela_Date_UtcNow()
{
    timeval tv;
    gettimeofday(&tv, NULL);
    return ConvertTime(tv);
}

#endif //__unix__
