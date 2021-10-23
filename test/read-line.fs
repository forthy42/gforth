\ regression test of (read-line)

create buf 16 allot

: hex.r ( u1 u2 -- )
    ['] u.r #16 base-execute ;

: show-line ( addr u -- )
    bounds ?do i c@ 2 hex.r space loop ;


: test ( -- )
    7 2 do
        cr cr i . cr
        s" read-line.input" open-fpath-file throw 2drop { f }
        begin
            buf 16 erase
            buf 1+ i f (read-line) throw cr . tuck . .
            cr buf 16 show-line cr
        0= until
    loop ;

test bye
