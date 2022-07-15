\ VT100 escape command helpers
\ (c)copyright 2015-2016 by Gerald Wodni <gerald.wodni@gmail.com>

\ TODO : implement in 2 tables (color - no color), implement toggle switch
\ sequence helpers
: <esc> ( -- )
    $1B emit ;
: <esc>-type ( c-addr n -- )
    <esc> type ;
: <esc>[ ( c-addr n -- )
    <esc> [CHAR] [ emit type ;
: <esc>[m ( c-addr n -- )
    <esc>[ [CHAR] m emit ;

\ font
: vt-bold
    s" 1m" <esc>[ ;
: vt-normal
    s" [22m" <esc>-type ;

\ colors
: vt-color-off  s" [39m" <esc>-type ;
: vt-white      s" 37" <esc>[m ;
: vt-cyan       s" 36" <esc>[m ;
: vt-magenta    s" 35" <esc>[m ;
: vt-blue       s" 34" <esc>[m ;
: vt-yellow     s" 33" <esc>[m ;
: vt-green      s" 32" <esc>[m ;
: vt-red        s" 31" <esc>[m ;

\ backgrounds
: vt-bg-off   s" 49m" <esc>[ ;
: vt-bg-white s" 47m" <esc>[ ;
: vt-bg-black s" 40m" <esc>[ ;

\ combined
: vt-default
    vt-normal
    vt-bg-off
    vt-color-off ;
