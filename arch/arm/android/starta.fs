\ startup stuff
." load terminal-server" cr stdout flush-file throw
require ansi.fs
require unix/terminal-server.fs
: t get-connection ;
." load android" cr stdout flush-file throw
require unix/android.fs
." load gl-terminal" cr stdout flush-file throw
require minos2/gl-terminal.fs
." done loading" cr stdout flush-file throw
>screen
