\ startup stuff
require unix/terminal-server.fs \ get-connection
require android.fs
require gl-terminal.fs
\ require omx-example.fs \ just load it
\ get-connection
\ require gl-sample.fs
\ require unix/pthread.fs
\ 2Variable dummyinput  dummyinput create_pipe
\ dummyinput @ to infile-id
>screen
: t get-connection >term ;