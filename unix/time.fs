

\ lib libc.so.5

\ libc.so.5 1 1 proc time ( returns seconds after 1.1.70 utc... )

library libc libc.so.5
library libm libm.so.5
1 (int) libc time time ( ptr/0 -- seconds_after_1.1.70 )
1 (void) libc printf0 printf ( ptr -- )
2 (void) libc printf1 printf ( ptr n1 -- )
3 (void) libc printf2 printf ( ptr n1 n2 -- )
1 (int...) libc printf printf ( ptr n1 .. nm m -- len )
2 (float) libm cos cos ( float -- cos )
(addr) libc errno errno
