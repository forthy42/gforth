\ mirror.fs mirrors ram in rom and copies back at startup

0 [IF]

For the romable feature:

We need to save the ram area (there might be initialized variables,
and code-fields...) into rom and copy it back at system startup.

[THEN]

\ save ram area

>rom
unlock ram-dictionary extent lock		
Constant ram-len
Constant ram-start
Variable ram-origin
ram-start ram-origin ram-len unlock tcmove lock 
ram-len allot align
>auto

: mirrorram
  ram-origin ram-start ram-len cmove ;

