.DATA 

calcOffset QWORD ?
particleX QWORD ?
particleY QWORD ?

xG QWORD 0
yG QWORD 0

four QWORD 4
five qword -5

testValue DWORD 10

xSobelDword DWORD -1,0,1,-2,0,2,-1,0,1
ySobelDword DWORD 1,2,1,0,0,0,-1,-2,-1

xSobel QWORD -1,0,1,-2,0,2,-1,0,1
ySobel QWORD 1,2,1,0,0,0,-1,-2,-1
filterOS DWORD ?
.CODE

putSobelMasksH proc array: PTR DWORD
MOV RAX ,offset xSobelDword
MOVDQU XMM0,[RAX];move 4 mask numbers to xmm
MOVDQU XMM1,  [RCX];move 4 pixels to xmm

VPMULLD XMM2,XMM0,XMM1;multiply mask * pixels

MOVDQU XMM3,[RAX+16];move 4 mask numbers to xmm
MOVDQU XMM4,  [RCX+16];move next 4 pixels to xmm

VPMULLD XMM5,XMM3,XMM4;multiply mask * pixels

MOVDQU XMM6,[RAX+32];move last mask number to xmm
MOVDQU XMM7,  [RCX+32];move last pixel to xmm

VPMULLD XMM8,XMM6,XMM7;multiply mask * pixel

VPADDD XMM9,XMM5,XMM2;add calculated values
MOVD EAX,XMM8

MOVD EDX,XMM9
ADD EAX,EDX
PSRLDQ XMM9,4;move to lower dword

MOVD EDX,XMM9
ADD EAX,EDX
PSRLDQ XMM9,4;move to lower dword

MOVD EDX,XMM9
ADD EAX,EDX
PSRLDQ XMM9,4;move to lower dword

MOVD EDX,XMM9
ADD EAX,EDX

MOVD XMM15,EAX;save xGradient value in xmm15

MOV RAX ,offset ySobelDword
MOVDQU XMM0,[RAX];move 4 mask numbers to xmm

VPMULLD XMM2,XMM0,XMM1;multiply mask * pixels

MOVDQU XMM3,[RAX+16];move 4 mask numbers to xmm

VPMULLD XMM5,XMM3,XMM4;multiply mask * pixels

MOVDQU XMM6,[RAX+32];move last mask number to xmm

VPMULLD XMM8,XMM6,XMM7;multiply mask * last pixel

VPADDD XMM9,XMM5,XMM2;add calculated values
MOVD EBX,XMM8

MOVD EDX,XMM9
ADD EBX,EDX
PSRLDQ XMM9,4;move to lower dword

MOVD EDX,XMM9
ADD EBX,EDX
PSRLDQ XMM9,4;move to lower dword

MOVD EDX,XMM9
ADD EBX,EDX
PSRLDQ XMM9,4;move to lower dword

MOVD EDX,XMM9
ADD EBX,EDX;calculated yGradient

MOVD EAX, XMM15;sum xGradient^2 + yGradient^2

IMUL EAX,EAX
IMUL EBX,EBX

ADD EAX,EBX

RET

putSobelMasksH endp

END