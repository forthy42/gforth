\ bidi file

Variable firstindex
2Variable lastindex
: bidi: ( -- )
    Create
  DOES> ( index <tag> -- )
    dup lastindex @ tuck <> and IF
	lastindex cell+ @ 1+ firstindex @ tuck - swap hex. hex.
	." .." lastindex @ name>string type cr
	over firstindex !
    THEN  lastindex 2! ;

: included-pipe ( addr u -- )
    r/o open-pipe throw ['] read-loop execute-parsing-file ;

Vocabulary bidis also bidis definitions

bidi: L
bidi: AL
bidi: AN
bidi: B
bidi: BN
bidi: CS
bidi: EN
bidi: ES
bidi: ET
bidi: FSI
bidi: LRE
bidi: LRI
bidi: LRO
bidi: NSM
bidi: ON
bidi: PDF
bidi: PDI
bidi: R
bidi: RLE
bidi: RLI
bidi: RLO
bidi: S
bidi: WS
      
hex
s" cut -f1,5 -d';' minos2/UnicodeData.txt | tr ';' ' '" included-pipe
