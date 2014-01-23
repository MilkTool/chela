BITS 32
section .text

; Parameter type enumeration constants.
PT_Void         equ 0
PT_Bool         equ 1
PT_Byte         equ 2
PT_Short        equ 3
PT_Int          equ 4
PT_Long         equ 5
PT_Pointer      equ 6
PT_Reference    equ 7
PT_Float        equ 8
PT_Double       equ 9
PT_LongDouble   equ 10
PT_Vector       equ 11
PT_MemoryStruct equ 12
PT_ValueStruct  equ 13


; Argument indices.
functionPointer  equ 8
numParameters    equ 12
signature        equ 16
parameters       equ 20
returnValue      equ 24
argumentTypePointer equ -4

; Invoke trap entry function.
global _Chela_Reflection_InvokeTrap
_Chela_Reflection_InvokeTrap:
    ; Function prolog.
    push ebp
    mov ebp, esp
    push ebx
    push esi
    push edi

    ; Read the signature pointer in esi, the remaining parameters in ecx.
    xor edx, edx
    mov ecx, [ebp + numParameters]
    mov esi, [ebp + signature]

.nextArg: 
    ; Check if an argument is remaining.
    test ecx, ecx
    jz .docall
    dec ecx

    ; Read the argument type
    mov dl, [esi]
    inc esi

    ; Read the argument pointer.
    mov ebx, [ebp + parameters]
    mov edi, [ebx + ecx*4]    

    ; Push it according to his type.
    lea ebx, [.pushArgumentTable]
    mov eax, [.pushArgumentTable + edx*4]
    add ebx, eax
    jmp ebx

    ; Push argument jump table.
.pushArgumentTable:
    dd .pushVoid         - .pushArgumentTable
    dd .pushBool         - .pushArgumentTable
    dd .pushByte         - .pushArgumentTable
    dd .pushShort        - .pushArgumentTable
    dd .pushInt          - .pushArgumentTable
    dd .pushLong         - .pushArgumentTable
    dd .pushPointer      - .pushArgumentTable
    dd .pushReference    - .pushArgumentTable
    dd .pushFloat        - .pushArgumentTable
    dd .pushDouble       - .pushArgumentTable
    dd .pushLongDouble   - .pushArgumentTable
    dd .pushVector       - .pushArgumentTable
    dd .pushMemoryStruct - .pushArgumentTable
    dd .pushValueStruct  - .pushArgumentTable

.pushVoid: ; Nop argument.
    jmp .nextArg

.pushBool: ; Push a boolean argument.
    xor eax, eax
    mov al, [edi]
    push eax
    jmp .nextArg

.pushByte: ; Push a byte argument.
    xor eax, eax
    mov al, [edi]
    push eax
    jmp .nextArg

.pushShort: ; Push a short argument.
    xor eax, eax
    mov ax, [edi]
    push eax
    jmp .nextArg

.pushInt:     ; Push an int argument.
.pushPointer: ; Push a pointer.
    mov eax, [edi]
    push eax
    jmp .nextArg

.pushReference: ; Push a reference.
    push edi
    jmp .nextArg

.pushLong:
    sub esp, 8
    fild qword [edi]
    fistp qword [esp]
    jmp .nextArg

.pushFloat:   ; Push a single precision floating point.
    sub esp, 4
    fld dword [edi]
    fstp dword [esp]
    jmp .nextArg

.pushDouble:
    sub esp, 8
    fld qword [edi]
    fstp qword [esp]
    jmp .nextArg

.pushLongDouble:
    sub esp, 16
    fld tword [edi]
    fstp tword [esp]
    jmp .nextArg

.pushVector:
    jmp .nextArg

.pushMemoryStruct:
    jmp .nextArg

.pushValueStruct:
    ; Store the number of bytes in eax.
    mov eax, [esi]
    add esi, 4

    ; Store the argument type pointer.
    mov [ebp + argumentTypePointer], esi

    ; Store the structure pointer and allocate space for it.
    mov esi, [edi]
    sub esp, ecx
    mov edi, esp

    ; Copy the dwords.
    mov ecx, eax
    shr ecx, 2
    cld
    rep
    movsd

    ; Copy the remaining bytes.
    mov ecx, eax
    and ecx, 0x03
    rep
    movsb

    ; Restore some registers
    mov esi, [ebp + argumentTypePointer]
    xor ecx, ecx
    jmp .nextArg
    
.docall:

    ; Perform the function call
    mov eax, [ebp + functionPointer]
    call eax

    ; Read the return type.
    xor ecx, ecx
    mov cl, [esi]

    ; Store the return value writer.
    lea edi, [.retValueTable]
    mov ebx, [.retValueTable + 4*ecx]
    add ebx, edi

    ; Store return pointer in edi
    mov edi, [ebp + returnValue]

    ; Jump to the return value writer.
    jmp ebx
    
.retValueTable:
    dd .retVoid         - .retValueTable
    dd .retBool         - .retValueTable
    dd .retByte         - .retValueTable
    dd .retShort        - .retValueTable
    dd .retInt          - .retValueTable
    dd .retLong         - .retValueTable
    dd .retPointer      - .retValueTable
    dd .retReference    - .retValueTable
    dd .retFloat        - .retValueTable
    dd .retDouble       - .retValueTable
    dd .retLongDouble   - .retValueTable
    dd .retVector       - .retValueTable
    dd .retMemoryStruct - .retValueTable
    dd .retValueStruct  - .retValueTable

.retVoid: ; Nothing returned.
    jmp .return

.retBool: ; Return a boolean.
    mov [edi], eax
    jmp .return

.retByte: ; Return a byte.
    mov [edi], al
    jmp .return

.retShort: ; Return a short.
    mov [edi], ax
    jmp .return

.retInt:       ; Return an integer.
.retPointer:   ; Return a pointer.
.retReference: ; Return a reference.
    mov [edi], eax
    jmp .return

.retLong: ; Return a long
    mov [edi], eax
    mov [edi + 4], edx
    jmp .return

.retFloat:
    fstp dword [edi]
    jmp .return

.retDouble:
    fstp qword [edi]
    jmp .return

.retLongDouble:
    fstp tword [edi]
    jmp .return

.retVector:
.retMemoryStruct:
.retValueStruct:
    jmp .return

.return:
    ; Function epilog.
    mov esp, ebp
    sub esp, 12
    pop edi
    pop esi
    pop ebx
    pop ebp
    ret
