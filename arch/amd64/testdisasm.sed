#
# testdisasm.sed: cleanup rules for comparing testasm.fs output
#

# drop instruction addresses
s/ 0x[0-9a-f]\+://g

