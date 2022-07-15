\ Forth package manager api
\ (c)copyright 2015-2016 by Gerald Wodni <gerald.wodni@gmail.com>

\ TODO : wrap this into a vocubulary, add sub-vacabularies for each response-type for maximum security

15 constant name-length

\ -- package info --

\ TODO : load wordlist for packages-response
: forth-packages ;

\ TODO : unload wordlist for packages-response
: end-forth-packages ;

\ show package name and description
: name-description ( -- )
    \ name
    vt-bold vt-magenta
    parse-name dup >r type
    \ space
    name-length r> - 1 max spaces
    \ description
    vt-normal
    10 parse type
    vt-color-off
    cr ;

\ -- package download --

: package-content ( <parse-name> <parse-version> -- c-addr-directory n-directory )
    vt-magenta vt-bold ." package-content: "
    vt-normal
    parse-name
    parse-name
    2over type
    ."  v:"
    2dup type

    fdirectory 2@
    $prefix$name$version+

    \ create package directory
    vt-bold
    2dup create-directories ?dup if
        vt-red ." ERROR" vt-default throw
    else
        vt-green ." ok"
    then
    vt-default cr ;

: end-package-content ( c-addr-directory n-directory -- )
    drop freet
    vt-magenta vt-bold ." end-package-content" cr
    vt-default ;

\ TODO: hide in final wordset
\ merge prefix and path, keep copy of address for easy free
: _merge-path ( c-addr-pre n-pre c-addr-path n-path -- c-addr-pre n-pre c-addr-merged c-addr-merged n-merged )
    2over 2swap \ copy prefix
    $+          \ add strings
    over -rot ; \ save address

: directory ( <parse-directory> -- )
    vt-bold vt-magenta
    ." directory "
    vt-normal
    parse-name \ parse dirname
    2dup type bl emit

    vt-bold
    _merge-path create-directories swap freet
    ?dup if
        vt-red  ." ERROR" vt-default throw
    else
        vt-green  ." ok"
    then
    vt-default cr ;

\ TODO: hide in final wordset
\ write content into file
: _burp-file ( c-addr-content n-content c-addr-filename n-filename -- )
    w/o create-file throw >r
    r@ write-file throw
    r> close-file throw ;

: file ( <parse-filename> <parse-link> -- )
    vt-bold vt-magenta
    ." file "
    parse-name \ parse filename
    parse-name \ parse link
    2over
    vt-normal
    type bl emit

    api-host http-slurp 200 = if
        vt-default
        2>r \ content
        _merge-path 2r> 2swap \ get merged path and content
        _burp-file \ store in file
        freet

        vt-bold vt-green ." ok"
    else
        vt-bold vt-red
        . type
        2drop
    then
    vt-default cr ;

\ parse name and immediately drop it
: parse-drop ( <parse-name> -- )
    parse-name 2drop ;
: parse-line ( <parse-line> -- c-addr n )
    10 parse ;
: parse-line-drop ( <parse-line> -- )
    parse-line 2drop ;

\ join two paths
: path-join ( c-addr1 n1 caddr2 n2 -- caddr3 n3 )
    {: c-addr1 n1 c-addr2 n2 :}
    n1 n2 + 1+ dup allocate throw >r
    c-addr1 r@ n1 cmove         \ 1st path
    [CHAR] / r@ n1 + c!         \ separator
    c-addr2 r@ n1 + 1+ n2 cmove \ 2nd path
    r> swap ;

    \ TODO: make path-join work and use it in "Main Found"
    \ TODO: free path afterwards

\ finclude package.4th handling
vocabulary finclude-words
also finclude-words definitions
    : forth-package ( -- f )
        0 ;
    : key-value ( <parse-name> <parse-line> -- )
        parse-name s" main" compare 0= if
            drop \ drop false-flag
            2over parse-name path-join \ get path
            -1 \ add true flag
        else
            parse-line-drop
        then ;
    : key-list ( <parse-name> <parse-line> -- )
        parse-line-drop ; \ no need to inspect any lines in finlude
    : end-forth-package ; ( -- )
previous definitions

