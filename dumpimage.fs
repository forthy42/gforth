\ image dump                                           15nov94py

Create magic  s" gforth00" here over allot swap move

'1 1 cells + 0 pad ! -1 pad c! pad @ 0< +  magic 7 chars + c!

: save-string-dict { addr1 u -- addr2 u }
    here { addr2 }
    u allot
    addr1 addr2 u move
    addr2 u ;

: update-image-included-files ( -- )
    included-files 2@ { addr cnt }
    image-included-files 2@ { old-addr old-cnt }
    align here { new-addr }
    cnt 2* cells allot
    new-addr cnt image-included-files 2!
    old-addr new-addr old-cnt 2* cells move
    cnt old-cnt
    U+DO
        addr i 2* cells + 2@ save-string-dict
	new-addr i 2* cells + 2!
    LOOP ;

: dump-fi ( addr u -- )
    w/o bin create-file throw >r
    magic 8 r@ write-file throw
    update-image-included-files
    forthstart here over - dup forthstart cell+ !
                         r@ write-file throw
\  relinfo here forthstart - 1- 8 cells / 1+ r@ write-file throw
  r> close-file throw ;

: savesystem ( "name" -- ) \ gforth
    name dump-fi ;
