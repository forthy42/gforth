\ startup stuff
." load terminal-server" cr stdout flush-file
require unix/terminal-server.fs
." load android" cr stdout flush-file
require android.fs
." load gl-terminal" cr stdout flush-file
require gl-terminal.fs
." done loading" cr stdout flush-file
>screen
: t get-connection ;