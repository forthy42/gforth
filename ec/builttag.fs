\ builttag.fs

variable builtnr
create linebuf 200 chars allot
create filename 200 chars allot
0 value fd

: s'
  [char] ' parse postpone sliteral ; immediate

: builttag
  sourcefilename filename place
  'n filename count + 1 chars - c!
  filename count r/o bin open-file
  IF   drop 0 builtnr !
  ELSE 	>r linebuf 100 r@ read-line drop drop
	linebuf swap 0 -rot 0 -rot >number 2drop drop 1+
	builtnr ! r> close-file throw
  THEN
  filename count r/w bin create-file throw to fd
  base @ >r decimal
  builtnr @ s>d <# #S #> fd write-file throw
  s"  constant built#" fd write-line throw
  s' const create builtdate ," ' fd write-file throw
  time&date >r >r >r
  s>d <# ': hold # # #> fd write-file throw
  s>d <# bl hold # # #> fd write-file throw
  drop
  r> s>d <# '. hold # # #> fd write-file throw
  r> s>d <# '. hold # # #> fd write-file throw
  r> s>d <# # # # # #> fd write-file throw
  s' "' fd write-line throw
  s' : .built cr ." Built #" built# . ." Date " builtdate count type cr ;'
  fd write-line throw
  fd close-file throw
  filename count included 
  r> base ! ;
