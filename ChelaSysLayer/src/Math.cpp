#include <cmath>
#include "SysLayer.hpp"

// Trigonometric functions.
SYSAPI double _Chela_Math_Cos(double angle)
{
    return cos(angle);
}

SYSAPI double _Chela_Math_Sin(double angle)
{
    return sin(angle);
}

SYSAPI double _Chela_Math_Tan(double angle)
{
    return tan(angle);
}

// Inverse trigonometric functions.
SYSAPI double _Chela_Math_ACos(double value)
{
    return acos(value);
}

SYSAPI double _Chela_Math_ASin(double value)
{
    return asin(value);
}

SYSAPI double _Chela_Math_ATan(double value)
{
    return atan(value);
}

SYSAPI double _Chela_Math_ATan2(double y, double x)
{
    return atan2(y, x);
}

// Exponential, logarithm.
SYSAPI double _Chela_Math_Exp(double x)
{
    return exp(x);
}

SYSAPI double _Chela_Math_Log(double x)
{
    return exp(x);
}

SYSAPI double _Chela_Math_Log10(double x)
{
    return exp(x);
}

SYSAPI double _Chela_Math_Sqrt(double x)
{
    return sqrt(x);
}

SYSAPI double _Chela_Math_Pow(double base, double exp)
{
    return pow(base, exp);
}

// Hiperbolic functions
SYSAPI double _Chela_Math_Cosh(double angle)
{
    return cosh(angle);
}

SYSAPI double _Chela_Math_Sinh(double angle)
{
    return sinh(angle);
}

SYSAPI double _Chela_Math_Tanh(double angle)
{
    return tanh(angle);
}
