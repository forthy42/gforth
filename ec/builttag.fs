\ builttag.fs

0 [IF]

This is a cross compiler extension.

[THEN]

base @ decimal

variable builtnr
create linebuf 200 chars allot
create filename 200 chars allot
0 value btfd

: s'
  [char] ' parse postpone sliteral ; immediate

[IFDEF] project-name
: extractproject ( -- adr len ) project-name ;
[ELSE]

defined? sourcefilename 0= [IF]
	  cr ." I need project-name defined for builttag" abort
[THEN]

: extractproject ( -- adr len )
  sourcefilename 2dup >r >r
  BEGIN dup WHILE 1-
        2dup + c@ [char] . = IF r> drop r> drop EXIT THEN
  REPEAT 2drop r> r> ;

[THEN]

get-current >MINIMAL

: builttag
  base @ >r decimal
  extractproject filename place
  s" .n" filename +place
  filename count r/o open-file 
  IF   drop 0 builtnr !
  ELSE 	>r linebuf 100 r@ read-line drop drop
	linebuf swap 0 -rot 0 -rot >number 2drop drop 1+
	builtnr ! r> close-file throw
  THEN
  filename count r/w create-file throw to btfd
  builtnr @ s>d <# #S #> btfd write-file throw
  s"  constant built#" btfd write-line throw
  s' const create builtdate ," ' btfd write-file throw
  time&date >r >r >r
  s>d <# [char] : hold # # #> btfd write-file throw
  s>d <# bl hold # # #> btfd write-file throw
  drop
  r> s>d <# [char] . hold # # #> btfd write-file throw
  r> s>d <# [char] . hold # # #> btfd write-file throw
  r> s>d <# # # # # #> btfd write-file throw
  s' "' btfd write-line throw
  s' : .built cr ." Built #" built# . ." Date " builtdate count type cr ;'
  btfd write-line throw
  btfd close-file throw
  filename count included 
  r> base ! ;

set-current
base !
