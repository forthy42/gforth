\ generic mach file for pc gforth				03sep97jaw

true Constant NIL  \ relocating

>ENVIRON

true Constant file		\ controls the presence of the
				\ file access wordset
true Constant OS		\ flag to indicate a operating system

true Constant prims		\ true: primitives are c-code

true Constant floating		\ floating point wordset is present

true Constant glocals		\ gforth locals are present
				\ will be loaded
true Constant dcomps		\ double number comparisons

true Constant hash		\ hashing primitives are loaded/present

true Constant xconds		\ used together with glocals,
				\ special conditionals supporting gforths'
				\ local variables
true Constant header		\ save a header information

false Constant ec
false Constant crlf