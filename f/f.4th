\ Forth package manager for theForthNet
\ (c)copyright 2015-2016 by Gerald Wodni <gerald.wodni@gmail.com>

\ --- HTTP Client ---
\ if you want your system to support f, define the following words:
\ http-slurp ( c-addr-path n-path c-addr-host n-host -- c-addr-response n-response n-status )
\ create-directories ( c-addr n -- ior ) create recurive directories

\ until the major systems support us, we are stuck with this ugly include mess:

\ gforth
: compat-gforth if s" compat-gforth.4th" required then ;
[DEFINED] gforth compat-gforth

\ vfx
: compat-vfx    if s" compat-vfx.4th"    required then ;
[DEFINED] vfxforth compat-vfx

: try-n-die" ( f parse-until-" -- )
    >r [CHAR] " parse r> 0= if cr ." SYSTEM NOT SUPPORTED, " type quit else 2drop then ;

\ check if the http-client is now defined, if not there isn't much we can do about it :(
[DEFINED] http-slurp try-n-die" HTTP-client not implemented"
[DEFINED] create-directories try-n-die" create-directories not implemented"

\ constants
: api-host s" theforth.net" ;
: default-fdirectory s" ./forth-packages/" ;

\ configurable variables
2variable fdirectory                \ prefix for packages, must end with '/'
default-fdirectory fdirectory 2!


\ utils

\ add name and version separated by to prefix '/'
: $prefix$name$version+ ( c-addr-name n-name c-addr-version n-version c-addr-prefix n-prefix -- c-addr4 n4 )
    {: c-name n-name c-version n-version c-prefix n-prefix :}
    n-name n-version + n-prefix + 1+ dup            \ total length
    allocate throw                                  \ receiving buffer
    swap
    2>r
    c-prefix 2r@ drop n-prefix cmove                \ write prefix
    c-name 2r@ drop n-prefix + >r r@ n-name cmove   \ write name
    [CHAR] / r> n-name + >r r@ c!                   \ write slash
    c-version r> 1+ n-version cmove                 \ write version
    2r> ; \ final path

\ free and throw :P
: freet free throw ;

\ colors & api-response words
include vt100.4th   \ colors
include api.4th     \ evaluated words within api-responses

\ perform http-get on url and evaluate result
: api-get ( c-addr n xt-ok xt-err -- )
    >r >r
    api-host http-slurp dup 200 <> if
        cr ." HTTP-Error: " . cr
        over -rot rdrop r> execute freet
    else
        drop \ response code
        cr
        over -rot r> execute rdrop freet
    then ;

: api-get-eval ( c-addr n -- )
    ['] evaluate ['] type api-get ;

\ list all packages
: fall ( -- )
    s" /api/packages/forth" api-get-eval ;

\ search package name and descriptions
: api-parse-name ( c-addr n xt <parse-name> -- )
    >r parse-name ?dup 0= if
        2drop drop rdrop
        vt-red ." ERROR: no string given" vt-color-off
    else
        $+
        over -rot r> execute
        freet \ free constructed url
    then ;

: fsearch ( <parse-name> -- )
    s" /api/packages/search/forth/"
    ['] api-get-eval api-parse-name ;

\ display information about a package
: finfo-err ( c-addr n -- )
    2drop vt-red ." ERROR: not found" vt-color-off ;
: finfo-ok ( c-addr n -- )
    vt-magenta type vt-color-off ;
: finfo-get ( c-addr n -- )
    ['] finfo-ok ['] finfo-err api-get ;

: finfo ( <parse-name> -- )
    s" /api/packages/info/forth/"
    ['] finfo-get api-parse-name ;

\ download a package

\ search package name and descriptions
: api-parse-name-version ( c-addr n xt <parse-name> <parse-version> -- )
    >r 2>r parse-name ?dup 0= if \ parse name
        drop rdrop 2rdrop
        vt-red ." ERROR: no name given" vt-color-off
    else
        parse-name ?dup 0= if \ parse version
            2drop drop rdrop 2rdrop
            vt-red ." ERROR: no version given" vt-color-off
        else
            2r>
            $prefix$name$version+
            over -rot r> execute
            freet \ free constructed url
        then
    then ;

: fget ( <parse-name> <parse-version> -- )
    s" /api/packages/content/forth/"
    ['] api-get-eval api-parse-name-version ;


\ include a package
: finclude-parse-package.4th ( c-addr n -- )
    2dup s" /package.4th" $+ 2dup
    also finclude-words
        included
    previous
    if
        vt-magenta vt-bold
        ." package main file: " vt-normal 2dup type cr
        vt-default
        2swap 2>r 2swap 2>r rot >r \ super ugly way to keep datastack empty during include
        included
        r> 2r> 2r> \ restore datastack, any items pushed by the main file remain beneath
    else
        vt-red vt-bold
        ." No main file found!"
        vt-default
    then
    drop freet
    2drop ;

: finclude ( <parse-name> <parse-version> -- )
    fdirectory 2@
    ['] finclude-parse-package.4th api-parse-name-version ;


\ finclude stringstack x.x.x
\ finclude euler303 1.0.0
