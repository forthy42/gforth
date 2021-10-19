\ mirror file

Variable firstindex
2Variable lastindex
: mirror: ( -- )
    Create
  DOES> ( index <tag> -- )
    dup lastindex @ tuck <> and IF
	lastindex cell+ @ 1+ firstindex @ tuck - swap hex. hex.
	." .." lastindex @ name>string type cr
	over firstindex !
    THEN  lastindex 2! ;

: included-pipe ( addr u -- )
    r/o open-pipe throw ['] read-loop execute-parsing-file ;

Vocabulary mirrors also mirrors definitions

mirror: Y
mirror: N

hex
s" curl https://unicode.org/Public/UNIDATA/UnicodeData.txt | cut -f1,10 -d';' | tr ';' ' '" included-pipe
