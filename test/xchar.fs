\ xchar word test suite - UTF-8 only

base @ hex

{ 0 xc-size -> 1 }
{ 7f xc-size -> 1 }
{ 80 xc-size -> 2 }
{ 7ff xc-size -> 2 }
{ 800 xc-size -> 3 }
{ ffff xc-size -> 3 }
{ 10000 xc-size -> 4 }
{ 1fffff xc-size -> 4 }

: test-string s" 恭喜发财!" ;

{ test-string drop xc@+ swap xc@+ swap xc@+ swap xc@+ swap xc@+ nip
  -> 606D 559C 53D1 8D22 21 }
{ ffff pad 4 xc!+? -> pad 3 + 1 true }
{ test-string drop xchar+ -> test-string drop 3 + }
{ test-string drop xchar+ xchar- -> test-string drop }
{ test-string +x/string -> test-string 3 /string }
{ test-string x\string- x\string- -> test-string 4 - }
{ test-string x-size -> 3 }
{ test-string -trailing-garbage -> test-string }
{ test-string 2 - -trailing-garbage -> test-string 4 - }

{ 0. <# s" Test" holds #> s" Test" compare -> 0 }
{ 0. <# 606D xhold #> s" 恭" compare -> 0 }

{ 606D xc-width -> 2 }
{ 41 xc-width -> 1 }
{ 2060 xc-width -> 0 }
{ test-string x-width -> 9 }

base !