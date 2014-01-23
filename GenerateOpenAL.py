#!/usr/bin/python

from Tools.DefinesToEnum import *
from Tools.FunctionsToEntryPoints import *

def2enum = DefinesToEnum();
fun2ep = FunctionsToEntryPoints();

# Generate ALenum.chela
def2enum.Main("""
defs2enum.py -n Chela.OpenAL -e ALenum -v public
-o OpenAL/ALenum.chela OpenAL/Specs/al.h
""".split())

# Generate ALPointers.chela
fun2ep.Main("""
fun2ep.py -n Chela.OpenAL -c Pointers -v public
-p AL -pv public -o OpenAL/ALPointers.chela OpenAL/Specs/al.h
""".split())

# Generate ALCenum.chela
def2enum.Main(
"""defs2enum.py -n Chela.OpenAL -e ALCenum -v public
-o OpenAL/ALCenum.chela OpenAL/Specs/alc.h
""".split())

# Generate ALCEntryPoints.chela
fun2ep.Main("""
fun2ep.py -n Chela.OpenAL -c Pointers -v public
-p ALC -pv public -o OpenAL/ALCPointers.chela OpenAL/Specs/alc.h
""".split())

