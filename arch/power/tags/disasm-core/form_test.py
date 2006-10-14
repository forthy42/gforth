#!/usr/bin/python

import commands

# testing of forms
f = "$7d8d7114"
gforth = "/home/complang/micrev/gforth-20050128-ppc64/bin/gforth-fast "
com = "disasm.fs -e \"1 " + f + " disasm-inst bye\""
print com


s = commands.getoutput(gforth + com)
b = s == "12 13 14 adde."
print s, b, f

