\ vertical file

Variable firstindex
2Variable lastindex
: vertical: ( -- )
    Create
  DOES> ( index <tag> -- )
    dup lastindex @ tuck <> and IF
	lastindex cell+ @ 1+ firstindex @ tuck - swap hex. hex.
	." .." lastindex @ name>string type cr
	over firstindex !
    THEN  lastindex 2! ;

: included-pipe ( addr u -- )
    r/o open-pipe throw ['] read-loop execute-parsing-file ;

Vocabulary verticals also verticals definitions

vertical: R
vertical: U
vertical: Tr
vertical: Tu

: .. ( start end <token> -- )
    ' { xt: token }  1+ swap ?DO  I token  LOOP ;

next-arg s" input" replaces
hex
s" sed -e 's/#.*U+\([0-9A-F]*\)..U+\([0-9A-F]*\)/\1 \2 .. U/g' -e 's/#.*U+\([0-9A-F]*\)/\1 U/g' -e 's/#.*//g' -e 's/; //g' -e 's/\([0-9A-F]*\)\.\.\([0-9A-F]*\)/\1 \2 ../g' %input% | tr ';' ' '" $substitute drop included-pipe
