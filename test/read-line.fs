\ regression test of (read-line)

create buf 16 allot

[undefined] hex.r [if]
: hex.r ( u1 u2 -- )
    ['] u.r #16 base-execute ;
[then]

: show-line ( addr u -- )
    bounds ?do i c@ 2 hex.r space loop ;


: show-file {: f n -- :}
    begin
        buf 16 erase
        buf 1+ n f (read-line) throw cr . tuck . .
        cr buf 16 show-line cr
    0= until ;


: test ( -- )
    7 2 do
        cr cr i . cr
        s" read-line.input" open-fpath-file throw 2drop i show-file
    loop
    s" read-line2.input" open-fpath-file throw 2drop 3 show-file
;

test
