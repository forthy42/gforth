\ xchar word test suite - UTF-8 only

base @ hex
utf-8 set-encoding

{ 0 xc-size -> 1 }
{ 7f xc-size -> 1 }
{ 80 xc-size -> 2 }
{ 7ff xc-size -> 2 }
{ 800 xc-size -> 3 }
{ ffff xc-size -> 3 }
{ 10000 xc-size -> 4 }
{ 10ffff xc-size -> 4 }

: test-string s" 恭喜发财!" ;
: test-string2 s" äöü你好吗？🤦🍌😐🤪😂" ;
: broken-string s\" \x9C\xC2\xC3\x40\xE6\xE4\x82\xE5\x25\xF1\xF2\x30\xF3\xA0\xF4\xA3\x50\xF5\xA4\xA5\x31\xF6\xA6\xA7\xF7\xA8\xA9\xAA\xFF\x81\x82\x83\x84\x85\x86\x87" ;

: string>xchars ( addr u -- xc1 .. xcn )
    bounds U+DO  I xc@+ swap I - +LOOP ;
: string>xsize ( addr u -- xs1 .. xsn )
    bounds U+DO  I I' over - x-size dup +LOOP ;
: string>-garbage ( addr u -- len0 .. lenn )
    0 U+DO  I -trailing-garbage swap  LOOP  drop ;
: hex.s ( n1 .. nn -- )
    [: depth dup 0 ?DO  dup i - pick .  LOOP  drop ;] $10 base-execute
    clearstack ;
    
{ test-string string>xchars -> 606D 559C 53D1 8D22 21 }
{ test-string string>xsize -> 3 3 3 3 1 }
{ ffff pad 4 xc!+? -> pad 3 + 1 true }
{ test-string string>-garbage -> 0 0 0 3 3 3 6 6 6 9 9 9 0C }

{ test-string2 string>xchars -> E4 F6 FC 4F60 597D 5417 FF1F 1F926 1F34C 1F610 1F92A 1F602 }
{ test-string2 string>xsize -> 2 2 2 3 3 3 3 4 4 4 4 4 }

{ test-string drop xchar+ -> test-string drop 3 + }
{ test-string drop xchar+ xchar- -> test-string drop }
{ test-string +x/string -> test-string 3 /string }
{ test-string x\string- x\string- -> test-string 4 - }
{ test-string x-size -> 3 }

{ broken-string string>xchars -> FFFD FFFD FFFD 40 FFFD FFFD FFFD 25 FFFD FFFD 30 FFFD FFFD 50 FFFD 31 FFFD FFFD FFFD FFFD FFFD FFFD FFFD }
{ broken-string string>xsize -> 1 1 1 1 1 2 1 1 1 1 1 2 2 1 3 1 3 4 4 1 1 1 1 }
{ broken-string string>-garbage -> 0 0 1 2 4 4 5 5 7 9 9 0A 0C 0C 0C 0E 0E 11 11 11 11 15 15 15 15 18 18 18 18 1C 1C 1C 1C 1C 1C 1C }
T{ test-string2 string>-garbage -> 0 0 2 2 4 4 6 6 6 9 9 9 0C 0C 0C 0F 0F 0F 12 12 12 12 16 16 16 16 1A 1A 1A 1A 1E 1E 1E 1E 22 22 22 22 }T

{ 123. <# #s s" Test" holds #> s" Test123" compare -> 0 }
{ 123. <# #s 606D xhold #> s" 恭123" compare -> 0 }

{ 606D xc-width -> 2 }
{ 41 xc-width -> 1 }
{ 2060 xc-width -> 0 }
{ test-string x-width -> 9 }

{ '√' parse abc√ "abc" str= -> true }
\ { '√' parse def
\ "def" str= -> true }

base !
