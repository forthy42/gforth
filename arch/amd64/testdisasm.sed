#
# testdisasm.sed: cleanup rules for comparing testasm.fs output
#

# drop instruction addresses
s/ 0x[0-9a-f]\+://

# drop branch target addresses
s/\([ \t]\+call.*\|j.*\|loop.*\)[ \t]\+0x[0-9a-f]\+/\1 <addr...>/

