; Hello World

section .data
    hello:      db  'Hello world!',10  ; 'Hello world!' plus a linefeed character
    helloLen:   equ $-hello            ; Length of the 'Hello world!' string

section .text
    global _start

_start:
    ; sys_write(1,hello,helloLen)
    mov eax,4        ; The system call for write (sys_write)
    mov ebx,1        ; File descriptor 1 - STDOUT
    mov ecx,hello    ; Put the offset of hello in ecx
    mov edx,helloLen ; helloLen is a constant, so we don't need to say
                     ;  mov edx,[helloLen] to get it's actual value
    int 80h          ; Call the kernel
    
    ; exit(0)
    mov eax,1        ; The system call for exit (sys_exit)
    mov ebx,0        ; Exit with return code of 0 (no error)
    int 80h
