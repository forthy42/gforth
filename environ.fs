\ ENVIRON.FS   Answer environmental queries            20may93jaw

\ May be cross-compiled

decimal

AVARIABLE EnvLink 0 EnvLink !

: (env)
       EnvLink linked
       dup c, here over chars allot swap move align
       , ;

: (2env)
       EnvLink linked
       dup $80 or
       c, here over chars allot swap move align
       , , ; 

: env" ( n -- )
       State @
       IF   postpone S" postpone (env)
       ELSE [char] " parse (env) THEN ; immediate

: 2env" ( d -- )
       State @
       IF   postpone S" postpone (2env)
       ELSE [char] " parse (2env) THEN ; immediate


: environment?  EnvLink
                BEGIN   @ dup
                WHILE   dup cell+ count $1f and
                        4 pick 4 pick compare 0=
                        IF      nip nip cell+ count dup -rot
                                $1f and + aligned
                                swap $80 and IF 2@ ELSE @ THEN
                                EXIT
                        THEN
                REPEAT
                drop 2drop false ;

