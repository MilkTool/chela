; This is the trap used to dynamically invoke methods according to the SystemV ABI(loosely)
; for AMD64 architecture.
default rel ; Use always a relative displacement.
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

; Local variables in the stack
functionPointer  equ -8
numParameters    equ -16
signature        equ -24
parameters       equ -32
memParams        equ -40

%define signPos r10
%define integersSeen r11
%define floatsSeen r12
%define curParam r13

; Invoke trap function.
global _Chela_Reflection_InvokeTrap
_Chela_Reflection_InvokeTrap:
    ; Function call prologue.
    push rbp
    push rbx
    push r12
    push r13
    push r14
    mov rbp, rsp

    ; Store the arguments
    push rdi ; Function pointer
    push rsi ; Number of parameters
    push rdx ; Signature
    push rcx ; Parameters
    sub rsp, 8 ; Memory parameters count

    ; Ignore the return value signature component
    mov al, [rdx]
    inc rdx
    
    ; Ignore the components of the argument.
    cmp al, PT_ValueStruct
    je .ignore_vstruct
    cmp al, PT_Vector
    je .ignore_vector
    ; Push arguments passed in registers.
    jmp .push_register

.ignore_vector: ; Ignore vector components.
    ; Ignore the number of components and the vector type.
    add rdx, byte 2

    ; Push the arguments in registers
    jmp .push_register    

.ignore_vstruct: ; Ignore value components
    ; Load the structure components.
    xor rax, rax
    mov al, [rdx]
    inc rdx

    ; Ignore the structure components
    add rdx, rax

    ; Push the arguments in registers
    jmp .push_register

    ; Begin pushing arguments in registers.
.push_register:
    ; Store the current signature position in r10.
    mov signPos, rdx
    
    ; Initialize the integer and float count.
    xor integersSeen, integersSeen
    xor floatsSeen, floatsSeen

    ; Initialize the argument index to zero.
    xor curParam, curParam
    cmp curParam, [rbp+numParameters]

.push_register_loop:
    jge .push_stack ; Done pushing arguments in registers

    ; Read the argument type.
    xor rax, rax
    mov al, [signPos]
    inc signPos

    ; Load the offset
    lea rbx, [.push_register_table]
    imul rax, byte 8
    add rax, rbx
    mov rbx, [rax]

    ; Compute the indirect jump address.
    lea rax, [.push_register_table]
    add rax, rbx

    ; Load the parameters base pointer.
    mov rbx, [rbp + parameters]
    add rbx, 8 ; Ignore the return value pointer.

    ; Perform the indirect jump
    jmp rax

    ; Jump table for individual type codes.
.push_register_table:
    dq .push_register_void - .push_register_table
    dq .push_register_bool - .push_register_table
    dq .push_register_byte - .push_register_table
    dq .push_register_short - .push_register_table
    dq .push_register_int - .push_register_table
    dq .push_register_long - .push_register_table
    dq .push_register_pointer - .push_register_table
    dq .push_register_reference - .push_register_table
    dq .push_register_float - .push_register_table
    dq .push_register_double - .push_register_table
    dq .push_register_long_double - .push_register_table
    dq .push_register_vector - .push_register_table
    dq .push_register_memory_struct - .push_register_table
    dq .push_register_value_struct - .push_register_table

.push_register_void:
    ; Ignore void parameters
    jmp .push_register_continue
.push_register_bool:
.push_register_byte:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_register_integer
    mov al, [r14]
    jmp .push_register_integer
.push_register_short:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_register_integer
    mov ax, [r14]
    jmp .push_register_integer
.push_register_int:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_register_integer
    mov eax, [r14]
    jmp .push_register_integer
.push_register_long:
.push_register_pointer:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_register_integer
    mov rax, [r14]
    jmp .push_register_integer
.push_register_reference:
    mov rax, [rbx + curParam*8]
    jmp .push_register_integer
.push_register_float:
    mov rax, [rbx + curParam*8]
    xorpd xmm8, xmm8
    test rax, rax
    jz .push_register_sse
    movss xmm8, [rax]
    jmp .push_register_sse
.push_register_double:
    mov rax, [rbx + curParam*8]
    xorpd xmm8, xmm8
    test rax, rax
    jz .push_register_sse
    movsd xmm8, [rax]
    jmp .push_register_sse
.push_register_long_double:
.push_register_vector:
.push_register_memory_struct:
    inc qword [rbp + memParams]
    jmp .push_register_continue
.push_register_value_struct:
    ; Shouldn't reach here.
    jmp .push_register_continue

.push_register_integer:
    ; Check remaining integer registers
    cmp integersSeen, 6
    jge .push_register_integer_memory ; Push in the stack.

    ; Load the offset
    lea rbx, [.push_register_integer_table]
    imul r14, integersSeen, byte 8
    add r14, rbx
    mov rbx, [r14]
    inc integersSeen

    ; Perform the indirect jump
    lea r14, [.push_register_integer_table]
    add rbx, r14
    jmp rbx

.push_register_integer_memory:
    ; Keep track of the memory parameters count.
    inc qword [rbp + memParams]
    inc integersSeen
    jmp .push_register_continue
    
    ; Jump table for integer register saving.
.push_register_integer_table:
    dq .push_register_integer_rdi - .push_register_integer_table
    dq .push_register_integer_rsi - .push_register_integer_table
    dq .push_register_integer_rdx - .push_register_integer_table
    dq .push_register_integer_rcx - .push_register_integer_table
    dq .push_register_integer_r8 - .push_register_integer_table
    dq .push_register_integer_r9 - .push_register_integer_table

.push_register_integer_rdi:
    mov rdi, rax
    jmp .push_register_continue
.push_register_integer_rsi:
    mov rsi, rax
    jmp .push_register_continue
.push_register_integer_rdx:
    mov rdx, rax
    jmp .push_register_continue
.push_register_integer_rcx:
    mov rcx, rax
    jmp .push_register_continue
.push_register_integer_r8:
    mov r8, rax
    jmp .push_register_continue
.push_register_integer_r9:
    mov r9, rax
    jmp .push_register_continue

.push_register_sse:
    ; Check remaining integer registers
    cmp floatsSeen, 7
    jg .push_register_sse_memory ; Push in the stack.

    ; Load the offset
    lea rbx, [.push_register_sse_table]
    imul rax, floatsSeen, byte 8
    add rax, rbx
    mov rbx, [rax]
    inc floatsSeen

    ; Perform the indirect jump
    lea rax, [.push_register_sse_table]
    add rbx, rax
    jmp rbx

.push_register_sse_memory:
    ; Keep track of the memory parameters count.
    inc qword [rbp + memParams]
    inc floatsSeen
    jmp .push_register_continue

.push_register_sse_table:
    dq .push_register_xmm0 - .push_register_sse_table
    dq .push_register_xmm1 - .push_register_sse_table
    dq .push_register_xmm2 - .push_register_sse_table
    dq .push_register_xmm3 - .push_register_sse_table
    dq .push_register_xmm4 - .push_register_sse_table
    dq .push_register_xmm5 - .push_register_sse_table
    dq .push_register_xmm6 - .push_register_sse_table
    dq .push_register_xmm7 - .push_register_sse_table

.push_register_xmm0:
    movdqa xmm0, xmm8
    jmp .push_register_continue
.push_register_xmm1:
    movdqa xmm1, xmm8
    jmp .push_register_continue
.push_register_xmm2:
    movdqa xmm2, xmm8
    jmp .push_register_continue
.push_register_xmm3:
    movdqa xmm3, xmm8
    jmp .push_register_continue
.push_register_xmm4:
    movdqa xmm4, xmm8
    jmp .push_register_continue
.push_register_xmm5:
    movdqa xmm5, xmm8
    jmp .push_register_continue
.push_register_xmm6:
    movdqa xmm6, xmm8
    jmp .push_register_continue
.push_register_xmm7:
    movdqa xmm7, xmm8
    jmp .push_register_continue

.push_register_continue:
    ; Increase the index and run back the loop.
    inc curParam
    cmp curParam, [rbp+numParameters]
    jmp .push_register_loop

.push_stack:
    ; First align the stack into a 16 byte boundary.
    test qword [rbp+memParams], 1
    jz .push_stack_begin_loop

.push_stack_padding:
    sub rsp, 8 ; Append an extra eightbyte to align the stack.
    
.push_stack_begin_loop:
    ; Decrease the parameter index.
    dec curParam
    dec signPos

    ; Until(curParam < 0)
    cmp curParam, 0
    jl .perform_call

    ; Read the argument type.
    xor rax, rax
    mov al, [signPos]

    ; Load the offset
    lea rbx, [.push_stack_table]
    imul rax, byte 8
    add rax, rbx
    mov rbx, [rax]

    ; Compute the indirect jump address
    lea rax, [.push_stack_table]
    add rax, rbx

    ; Load the parameters base pointer.
    mov rbx, [rbp + parameters]
    add rbx, 8 ; Ignore the return value pointer.

    ; Perform the jump.
    jmp rax

    ; Jump table for individual type codes.
.push_stack_table:
    dq .push_stack_void - .push_stack_table
    dq .push_stack_bool - .push_stack_table
    dq .push_stack_byte - .push_stack_table
    dq .push_stack_short - .push_stack_table
    dq .push_stack_int - .push_stack_table
    dq .push_stack_long - .push_stack_table
    dq .push_stack_pointer - .push_stack_table
    dq .push_stack_reference - .push_stack_table
    dq .push_stack_float - .push_stack_table
    dq .push_stack_double - .push_stack_table
    dq .push_stack_long_double - .push_stack_table
    dq .push_stack_vector - .push_stack_table
    dq .push_stack_memory_struct - .push_stack_table
    dq .push_stack_value_struct - .push_stack_table

.push_stack_void:
    jmp .push_stack_continue
.push_stack_bool:
.push_stack_byte:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_stack_integer
    mov al, [r14]
    jmp .push_stack_integer
.push_stack_short:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_stack_integer
    mov ax, [r14]
    jmp .push_stack_integer
.push_stack_int:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_stack_integer
    mov eax, [r14]
    jmp .push_stack_integer
.push_stack_long:
.push_stack_pointer:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_stack_integer
    mov rax, [r14]
    jmp .push_stack_integer
.push_stack_reference:
    mov rax, [rbx + curParam*8]
    jmp .push_stack_integer
.push_stack_float:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_stack_floating
    mov eax, [r14]
    jmp .push_stack_floating
.push_stack_double:
    mov r14, [rbx + curParam*8]
    xor rax, rax
    test r14, r14
    jz .push_stack_floating
    mov rax, [r14]
    jmp .push_stack_floating
.push_stack_long_double:    
.push_stack_vector:
.push_stack_memory_struct:
.push_stack_value_struct:
    ; Shouldn't reach here.
    jmp .push_stack_continue

.push_stack_integer:
    dec integersSeen
    cmp integersSeen, 6    
    jl .push_stack_continue
    push rax
    jmp .push_stack_continue
    
.push_stack_floating:
    dec floatsSeen
    cmp floatsSeen, 7    
    jle .push_stack_continue
    push rax
    jmp .push_stack_continue
    
.push_stack_continue:
    jmp .push_stack_begin_loop

.perform_call:
    ; Call the function
    call [rbp+functionPointer]

    ; Now r13 is free to use.
    ; Reset the integer and float count.
    xor integersSeen, integersSeen
    xor floatsSeen, floatsSeen

    ; Reset the signature position
    mov signPos, [rbp + signature]

    ; Load the return value pointer.
    mov rcx, [rbp+parameters]
    mov rsi, [rcx]

    ; Read the type id
    xor rcx, rcx
    mov cl, [signPos]
    inc signPos

    ; Load the offset
    lea rbx, [.copy_ret_table]
    imul rcx, 8
    add rcx, rbx
    mov rbx, [rcx]

    ; Perform the indirect call
    lea rcx, [.copy_ret_table]
    add rcx, rbx
    call rcx

.finish:
    ; Function return epilogue.
    mov rsp, rbp
    pop r14
    pop r13
    pop r12
    pop rbx
    pop rbp
    ret

.copy_ret_table:
    dq copy_ret_void - .copy_ret_table
    dq copy_ret_bool - .copy_ret_table
    dq copy_ret_byte - .copy_ret_table
    dq copy_ret_short - .copy_ret_table
    dq copy_ret_int - .copy_ret_table
    dq copy_ret_long - .copy_ret_table
    dq copy_ret_pointer - .copy_ret_table
    dq copy_ret_reference - .copy_ret_table
    dq copy_ret_float - .copy_ret_table
    dq copy_ret_double - .copy_ret_table
    dq copy_ret_long_double - .copy_ret_table
    dq copy_ret_vector - .copy_ret_table
    dq copy_ret_memory_struct - .copy_ret_table
    dq copy_ret_value_struct - .copy_ret_table

; Void function, ignore return value
copy_ret_void:
copy_ret_memory_struct:
    ret

; Byte sized return values.
copy_ret_bool:
copy_ret_byte:
    test integersSeen, integersSeen
    jnz .second
    mov [rsi], al
.return:    
    inc rsi
    inc integersSeen
    ret
.second:
    mov [rsi], dl
    jmp .return

; Short sized return values
copy_ret_short:
    test integersSeen, integersSeen
    jnz .second
    mov [rsi], ax
.return:    
    add rsi, 2
    inc integersSeen
    ret
.second:
    mov [rsi], dx
    jmp .return

; Int sized return values
copy_ret_int:
    test integersSeen, integersSeen
    jnz .second
    mov [rsi], eax
.return:    
    add rsi, 4
    inc integersSeen
    ret
.second:
    mov [rsi], edx
    jmp .return

; Long sized return values
copy_ret_long:
copy_ret_pointer:
copy_ret_reference:
    test integersSeen, integersSeen
    jnz .second
    mov [rsi], eax
.return:    
    add rsi, 8
    inc integersSeen
    ret
.second:
    mov [rsi], edx
    jmp .return

; Single precision floating point.
copy_ret_float:
    test floatsSeen, floatsSeen
    jnz .second
    movss [rsi], xmm0
.return:    
    add rsi, 8
    inc integersSeen
    ret
.second:
    movss [rsi], xmm1
    jmp .return

; Double precision floating point.
copy_ret_double:
    test floatsSeen, floatsSeen
    jnz .second
    movsd [rsi], xmm0
.return:    
    add rsi, 8
    inc integersSeen
    ret
.second:
    movsd [rsi], xmm1
    jmp .return

copy_ret_long_double:
copy_ret_vector:
copy_ret_value_struct:
    ret

