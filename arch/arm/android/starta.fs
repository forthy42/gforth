\ startup stuff
: ++fpath ( addr u -- )
    open-fpath-file 0=
    IF fpath also-path close-file throw
    ELSE 2drop drop THEN ;
s" unix" ++fpath
s" minos2" ++fpath
." load terminal-server" cr stdout flush-file throw
require unix/terminal-server.fs
." load android" cr stdout flush-file throw
require unix/android.fs
." load gl-terminal" cr stdout flush-file throw
require minos2/gl-terminal.fs
." done loading" cr stdout flush-file throw
>screen
: t get-connection ;
