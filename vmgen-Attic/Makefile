#just a few notes:

all:
	cd ..; ./gforth --die-on-signal -p .:/usr/local/lib/gforth/site-forth:/usr/local/share/gforth/site-forth:/usr/local/lib/gforth/0.5.0:/usr/local/share/gforth/0.5.0:. -m 1000000 prims2x.fs -e "c-flag off s\" prim.b\" ' output-disasm process-file bye" >vmgen/prim-disasm.i ; cd vmgen
	gcc -O -c -I./../arch/386 disasm.c
