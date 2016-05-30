;*****************************************
;* Este codigo ha sido generado por:     *
;* Juan Pablo Avila Garza                *
;* Oscar Ivan Rivera Mendoza             *
;* Yololtzin Villarreal Arellano         *
;*****************************************
; PPPPP OOOOO NN   N GGGGG  AAA  MM MM EEEEE      1    00000  00000
; P   P O   O NNN  N G     A   A M M M E         11    0  00  0  00
; PPPPP O   O N NN N G GGG A   A M   M EEE      1 1    0 0 0  0 0 0
; P     O   O N  NNN G   G AAAAA M   M E          1    00  0  00  0
; P     OOOOO N   NN GGGGG A   A M   M EEEEE    11111  00000  00000

MODEL SMALL
STACK 64
DATASEG
ID01 db "",'$'
ID02 db "",'$'
CD01 db "",'$'
CD02 db "hola",'$'

;Termina declaracion de variables

CODESEG
INICIO:
mov ax,@data
mov ds,ax
mov es,ax

;GUARDAR CD01 EN ID01
mov cx, 100
mov di,offset ID01
mov si,offset CD01
rep movsb

;GUARDAR CD02 EN ID02
mov cx, 100
mov di,offset ID02
mov si,offset CD02
rep movsb

;GUARDAR ID02 EN ID01
mov ID01,al

;IMPRIMIR ID01
mov ah,09h
lea dx,ID01
int 21h
mov ah,02h ;Saltar linea
mov dl,0ah ;Saltar linea
int 21h ;Saltar linea


END INICIO