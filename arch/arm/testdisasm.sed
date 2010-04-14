#
# testdisasm.sed: cleanup rules for comparing testdisasm.fs output
#

# drop instruction addresses
s/( \$[0-9A-F]\+ )//g

# drop the target addresses of branch instructions
s/\$[0-9A-F0]\+ \(BL\?X\?,\)/\1/g
