: x1 ( u -- u u1 ... un )
    case
	dup
	dup 1 = ?of drop endof
        dup 1 and ?of 3 * 1+ contof
        2/
    next-case ;

t{ 7 x1 -> 7 22 11 34 17 52 26 13 40 20 10 5 16 8 4 2 1 }t
