\ table fomerly in search.fs

require hash.fs

\ table (case-sensitive wordlist)

: table-find ( addr len wordlist -- nfa / false )
    >r 2dup r> bucket @ (tablefind) ;

Create tablesearch-map ( -- wordlist-map )
    ' table-find A, ' hash-reveal A, ' (rehash) A, ' (rehash) A,

: table ( -- wid )
    \g create a case-sensitive wordlist
    tablesearch-map mappedwordlist ;

