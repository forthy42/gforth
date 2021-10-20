\ Bidirectional Unicode database (one byte per codepoint table)

$Variable bidi-db

$110000 bidi-db $!len
bidi-db $@ erase

: bidis: ( n -- n+1 )
    Create dup , 1+
  DOES>  @ >r
    bidi-db $@ 2swap >r safe/string r> umin r> fill ;
: flags: ( n -- n )
    Create dup ,
  DOES>  @ { flag }
    bidi-db $@ 2swap >r safe/string r> umin
    bounds ?DO  flag I c+!  LOOP ;

Vocabulary bidis  also bidis definitions
0
bidis: ..L
bidis: ..AL
bidis: ..AN
bidis: ..B
bidis: ..BN
bidis: ..CS
bidis: ..EN
bidis: ..ES
bidis: ..ET
bidis: ..FSI
bidis: ..LRE
bidis: ..LRI
bidis: ..LRO
bidis: ..NSM
bidis: ..ON
bidis: ..PDF
bidis: ..PDI
bidis: ..R
bidis: ..RLE
bidis: ..RLI
bidis: ..RLO
bidis: ..S
bidis: ..WS
drop
include minos2/unicode/bidis.fs

previous definitions
Vocabulary verticals also verticals definitions
0
flags: ..R  $20 +
flags: ..U  $20 +
flags: ..Tr $20 +
flags: ..Tu $20 +
drop
include minos2/unicode/verticals.fs
previous definitions

Vocabulary mirrors also mirrors definitions
0
flags: ..N  $80 +
flags: ..Y
drop

include minos2/unicode/mirrors.fs
previous definitions
