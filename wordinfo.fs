\ WORDINFO.FS  V1.0                                    17may93jaw

\ May be cross-compiled
\ If you want check values then exclude comments,
\ but keep in mind that this can't be cross-compiled

INCLUDE look.fs

\ Wordinfo is a tool that checks a nfa
\ and finds out what wordtype we have
\ it is used in SEE.FS

: alias? ( nfa -- flag )
        dup name> look
        0= ABORT" WINFO: CFA not found"
\       cell+
        2dup <>
        IF   nip dup 1 cells - here !
             count $1f and here cell+ place true
        ELSE 2drop false THEN ;

: var?  ( nfa -- flag )
        cell+ (name>)
        >code-address ['] udp >code-address = ;

: con?  ( nfa -- flag )
        cell+ (name>)
        >code-address ['] bl >code-address = ;

: user?  ( nfa -- flag )
        cell+ (name>)
        >code-address ['] s0 >code-address = ;

: does? ( nfa -- flag )
        cell+ (name>)
        >code-address ['] bits >code-address = ;

: defered? ( nfa -- flag )
        cell+ (name>)
        >code-address ['] source >code-address = ;

: colon? ( nfa -- flag )
        cell+ (name>)
        >code-address ['] does? >code-address = ;

\ VALUE VCheck

\ : value? ( nfa -- flag )
\         dup does?
\         IF here @ ['] VCheck cell+ @ =
\            dup IF swap (name>) >body @ here ! ELSE nip THEN
\         ELSE drop false THEN ;

: prim? ( nfa -- flag )
        name>
        forthstart u< ;

\ None nestable IDs:

1 CONSTANT Pri#         \ Primitives
2 CONSTANT Con#         \ Constants
3 CONSTANT Var#         \ Variables
4 CONSTANT Val#         \ Values

\ Nestabe IDs:

5 CONSTANT Doe#         \ Does part
6 CONSTANT Def#         \ Defer
7 CONSTANT Col#         \ Colon def
8 CONSTANT Use#         \ User variable

\ Nobody knows:

9 CONSTANT Ali#         \ Alias

10 CONSTANT Str#         \ Structure words

11 CONSTANT Com#        \ Compiler directives : ; POSTPONE

CREATE InfoTable
        ' Prim?    A, Pri# ,
        ' Alias?   A, Ali# ,
        ' Con?     A, Con# ,
        ' Var?     A, Var# ,
\        ' Value?  A, Val# ,
        ' Defered? A, Def# ,
        ' Does?    A, Doe# ,
        ' Colon?   A, Col# ,
	' User?    A, Use# ,
        0 ,

: WordInfo ( nfa --- code )
        InfoTable
        BEGIN  dup @ dup
        WHILE  swap 2 cells + swap
               2 pick swap execute
        UNTIL
        1 cells - @ nip
        ELSE
        2drop drop 0
        THEN ;

