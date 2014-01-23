#!/bin/sh

mono --debug build/dist/Bind.exe -in:Bind/Specifications/ -out:OpenGL_ES -ns:Chela.Graphics -mode:es20
mono --debug build/dist/Bind.exe -in:Bind/Specifications/ -out:OpenGL -ns:Chela.Graphics.GL2 -mode:gl2
mono --debug build/dist/Bind.exe -in:Bind/Specifications/ -out:OpenCL -ns:Chela.OpenCL -mode:cl

