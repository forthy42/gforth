#!/usr/bin/env python

# makes objdumps unique
# $1 specifies the form to make unique

import commands
from optparse import OptionParser

HOSTNAME = commands.getoutput("hostname")
MNEMONIC_TEST='/home/complang/micrev/praktikum/test/mnemonic'

def get_opc_code_hex(line) :
  l = line.split()
  return l[1]+l[2]+l[3]+l[4]

if __name__ == '__main__' :
  parser = OptionParser("%prog <form>")
  (options, args) = parser.parse_args()
  if len(args) < 1 or len(args) > 1 :
    parser.error("incorrect number of arguments")
  dir = MNEMONIC_TEST + '/' + args[0]
  for k in commands.getoutput('ls %s/%s*'%(dir, HOSTNAME)).split('\n') :
    print k
    f = open(k,'r')
    lines = f.readlines()
    new_file = open(k+'.unique', 'w') 
    tmp_list = []
    new_file_list = []
    for l in lines :
      opc_code_hex = get_opc_code_hex(l)
      if opc_code_hex not in tmp_list :
        tmp_list.append(opc_code_hex)
        new_file_list.append(l)
    new_file.writelines(new_file_list)
    new_file.close()
    print commands.getoutput('mv %s.unique %s' %(k,k))
