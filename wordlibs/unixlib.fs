
require ./../wordlib.fs

WordLibrary unixlib ./unixlib.so

: cconst"
  '" word count
  get_cconst abort" constant not found!"
  state @ 
  IF	postpone literal
  THEN ; immediate
