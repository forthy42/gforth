 cr .s
 RESET

 cr R1 , R2 MOV.W:G
 cr R1 , A0 MOV.W:G
 cr R1 , [A0] MOV.W:G
 cr R1 , [A1] MOV.W:G
 cr R1 , $12 [A0] MOV.W:G
 cr R1 , $345 [A0] MOV.W:G
cr .s
 cr R1 , $12 [SB] MOV.W:G
 cr R1 , $1234 [SB] MOV.W:G
 cr R1 , $12 [FB] MOV.W:G
 cr R1 , $1234 MOV.W:G
cr .s
 cr [A0] , R1 MOV.W:G
 cr $12 [A1] , R0 MOV.W:G
 cr [A0] , [A1] MOV.W:G
 cr $12 [A1] , [A0] MOV.W:G
 cr $12 [A0] , [A1] MOV.W:G
 cr $1234 [A0] , $5678 [A1] MOV.W:G
 cr $1234 [A1] , $5678 [A0] MOV.W:G
 cr $12 [A1] , $3456 [SB] MOV.W:G
 cr $12 [A1] , $34 [FB] MOV.W:G
 cr $1234 [A1] , $5678 MOV.W:G
\ cr  [SB] , R1 MOV.W:G              \ error
 cr $12 [SB] , R0 MOV.W:G
\ cr [SB] , [A1] MOV.W:G             \ error
 cr $12 [SB] , [A0] MOV.W:G
 cr $12 [SB] , [A1] MOV.W:G
 cr $1234 [SB] , $5678 [A1] MOV.W:G
 cr $1234 [SB] , $5678 [A0] MOV.W:G
 cr $12 [SB] , $3456 [SB] MOV.W:G
 cr $12 [SB] , $34 [FB] MOV.W:G
 cr $1234 [SB] , $5678 MOV.W:G
cr .s
\ cr [FB] , R1 MOV.W:G               \ error
 cr $12 [FB] , R0 MOV.W:G
\ cr [FB] , [A1] MOV.W:G             \ error
 cr $12 [FB] , [A0] MOV.W:G
 cr $12 [FB] , [A1] MOV.W:G
\ cr $1234 [FB] , $5678 [A1] MOV.W:G \ error
\ cr $1234 [FB] , $5678 [A0] MOV.W:G \ error
 cr $12 [FB] , $3456 [SB] MOV.W:G
 cr $12 [FB] , $34 [FB] MOV.W:G
\ cr $1234 [FB] , $5678 MOV.W:G      \ error
 cr .( vor mov.b:g ) .s
 cr R0L , R0H MOV.B:G
 cr R0H , A0 MOV.B:G
 cr R1L , [A0] MOV.B:G
 cr R1H , [A1] MOV.B:G
 cr R0L , $12 [A0] MOV.B:G
 cr R0H , $345 [A0] MOV.B:G
 cr R1L , $12 [SB] MOV.B:G
 cr R1H , $1234 [SB] MOV.B:G
 cr R1L , $12 [FB] MOV.B:G
 cr R1H , $1234 MOV.B:G
 cr .( vor mov.w:g ) .s
 cr  # $abcd , R2 MOV.W:G
 cr  # $abcd , A0 MOV.W:G
 cr  # $abcd , [A0] MOV.W:G
 cr  # $abcd , [A1] MOV.W:G
 cr  # $abcd , $12 [A0] MOV.W:G
 cr  # $abcd , $345 [A0] MOV.W:G
 cr  # $abcd , $12 [SB] MOV.W:G
 cr  # $abcd , $1234 [SB] MOV.W:G
 cr  # $abcd , $12 [FB] MOV.W:G
 cr  # $abcd , $1234 MOV.W:G
 cr .s
 cr  $1234 , R1 MOV.W:G
 cr  $1234 , $5678 MOV.W:G
 cr  $1234 , $56 [FB] MOV.W:G
 cr  $1234 , $5678 [SB] MOV.W:G
 cr
 cr  $1234 , [A1] MOV.W:G
 cr  $1234 , $56 [A0] MOV.W:G
 cr  $1234 , $5678 [A1] MOV.W:G
cr .s
 cr  $12 JNC
 cr  $1234 JMP.W
cr .s
 cr R0 JMPI.W
 cr R1 JMPI.W
 cr R2 JMPI.W
 cr R3 JMPI.W
 cr A0 JMPI.W
 cr [A0] JMPI.W
 cr $12 [A0] JMPI.W
 cr $1234 [A0] JMPI.W
 cr $12345 [A0] JMPI.W
 cr A1 JMPI.W
 cr [A1] JMPI.W
 cr $12 [A1] JMPI.W
 cr $1234 [A1] JMPI.W
 cr $12345 [A1] JMPI.W
 cr $12 [SB] JMPI.W
 cr $1234 [SB] JMPI.W
 cr $12 [FB] JMPI.W
\ cr $1234 [FB] JMPI.W      \ error
 cr $1234 JMPI.W
 cr
 cr R0 abs.b
 cr R1 abs.b
 cr R2 abs.b
 cr R3 abs.b
 cr A0 abs.b
 cr [A0] abs.b
 cr $12 [A0] abs.b
 cr $1234 [A0] abs.b
 cr $12345 [A0] abs.b
 cr A1 abs.b
 cr [A1] abs.b
 cr $12 [A1] abs.b
 cr $1234 [A1] abs.b
 cr $12345 [A1] abs.b
 cr $12 [SB] abs.b
 cr $1234 [SB] abs.b
 cr $12 [FB] abs.b
\ cr $1234 [FB] abs.b      \ error
 cr $1234 abs.b
 cr
 cr R0 div.b
 cr R1 div.b
 cr R2 div.b
 cr R3 div.b
 cr A0 div.b
 cr [A0] div.b
 cr $12 [A0] div.b
 cr $1234 [A0] div.b
 cr $12345 [A0] div.b
 cr A1 div.b
 cr [A1] div.b
 cr $12 [A1] div.b
 cr $1234 [A1] div.b
 cr $12345 [A1] div.b
 cr $12 [SB] div.b
 cr $1234 [SB] div.b
 cr $12 [FB] div.b
\ cr $1234 [FB] div.b      \ error
 cr $1234 div.b
 cr
 cr [A0] pop.w:g
 cr [A1] push.w:g
 cr $12 [A0] pop.w:g
 cr $345 [A1] push.w:g
 cr
 cr .( # push )
 cr # $12 push.b:g
 cr # $345 push.w:g
\ cr # $345 pop.w:g
 cr
 cr   # $ffff , ip mov.w:g            \ ip will be patched
 cr   # $fef0 , sp ldc                \ sp at $FD80...$FEF0
 cr   # $fd80 , rp mov.w:g            \ rp at $F.00...$FD80
 cr   # -2 , rp add.w:q
 cr   w , r2 mov.w:g
 cr   rp , w mov.w:g  ip , [w] mov.w:g
 cr   # 4 , r2 add.w:q  r2 , ip mov.w:g
 cr   rp , w mov.w:g  # 2 , rp add.w:q
 cr   [w] , ip mov.w:g
 cr   tos , w mov.w:g                  \ copy tos to w
 cr   tos pop.w:g                      \ get new tos
 cr   [w] jmpi.w                       \ execute
 cr   # 0 , R2 MOV.W:G
 cr  # 1 , A0 MOV.W:G
 cr  # -1 , [A0] MOV.W:G
 cr  # 2 , [A1] MOV.W:G
 cr  # -2 , $12 [A0] MOV.W:G
 cr  # 3 , $345 [A0] MOV.W:G
 cr  # -3 , $12 [SB] MOV.W:G
 cr  # 4 , $1234 [SB] MOV.W:G
 cr  # -8 , $12 [FB] MOV.W:G
 cr  # 7 , $1234 MOV.W:G
 cr
 cr  # 0 , R2 MOV.W:q
 cr  # 1 , A0 MOV.W:q
 cr  # -1 , [A0] MOV.W:q
 cr  # 2 , [A1] MOV.W:q
 cr  # -2 , $12 [A0] MOV.W:q
 cr  # 3 , $345 [A0] MOV.W:q
 cr  # -3 , $12 [SB] MOV.W:q
 cr  # 4 , $1234 [SB] MOV.W:q
 cr  # -8 , $12 [FB] MOV.W:q
 cr  # 7 , $1234 MOV.W:q
\ cr  # 8 , $1234 MOV.W:q      \ error
 cr  sp , $3456 [SB] stc
 cr  # $1234 , sp ldc
 cr  [A0] , sp ldc
 cr  $12 [A0] , sp ldc
 cr  [A1] , sp ldc
 cr  $12 [A1] , sp ldc
\ cr  [SB] , sp ldc            \ error
 cr  $12 [SB] , sp ldc
 cr  $1234 [SB] , sp ldc
 cr  $12 jle
 cr  u fclr
 cr  o fset
 cr .( vor mul.w:g ) .s
 cr  # $abcd , [A0] MUL.W:G
 cr  # $abcd , [A1] MUL.W:G
 cr  $1234 , [A0] MUL.W:G
 cr  $1234 , [A1] MUL.W:G
 cr  # $abcd , $1234 MUL.W:G
 cr .( vor mul.b:g ) .s
 cr  # $abcd , [A0] mul.b:G
 cr  # $abcd , [A1] mul.b:G
 cr  $1234 , [A0] mul.b:G
 cr  $1234 , [A1] mul.b:G
 cr  # $5678 , $1234 mul.b:G
 cr  $1234 , $5678 mul.b:G
 cr .s
 cr  # $5678 , $1234 mov.b:G
 cr  $1234 , $5678 mov.b:G
cr .s

\ for tests only                             hfs 07:47 04/24/92

