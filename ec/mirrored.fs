\ mirror.fs mirrors ram in rom and copies back at startup

0 [IF]

For the romable feature:

We need to save the ram area (there might be initialized variables,
and code-fields...) into rom and copy it back at system startup.

[THEN]

\ save ram area

unlock >CROSS

: saveram
  mirror-link 
  BEGIN @ dup WHILE
	>r r@ >rstart @ r@ >rdp @ over - tuck
	2dup X , X , X here swap tcmove
	X allot X align

>rom
unlock sramdp @ lock		constant ram-start
unlock ramdp @ sramdp @ - lock	constant ram-len
variable ram-origin
ram-start ram-origin ram-len unlock tcmove lock 
ram-len allot align
>auto

: mirrorram
  ram-origin ram-start ram-len cmove ;

