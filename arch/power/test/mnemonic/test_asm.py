#!/usr/bin/env python
# when using this script you should uncomment the wid stuff from the asm.fs
# stuff and add a dup hex. in the first line of the h, word
import re, commands, string
from optparse import OptionParser

# $1 specifies form to use

HEX = '[0-9abcdef]'
ASM='/home/complang/micrev/praktikum/ppc/asm.fs'
MNEMONIC_TEST='/home/complang/micrev/praktikum/test/mnemonic'
# XXX hack
HOSTNAME='north' #commands.getoutput("hostname")
addr_pat = re.compile('%s+(?:)'%HEX,re.I)
args_pat = re.compile('[\$0-9- ]*', re.I)
mnem_pat = re.compile('[0-9a-z\.]*$', re.I)

def get_addr (str) :
  return addr_pat.search(str).group()

def remove_parenthesis(str) :
  tmp = str.split('(')
  for k in range(len(tmp)) :
    if tmp[k][len(tmp[k])-1] == ')' :
      tmp[k] = tmp[k][:len(tmp[k])-1]
  s = ''
  for k in range(len(tmp)) :
    if s == '' : s = tmp[k]
    else : s = s + ',' + tmp[k]
  return s

def parse_args(args) :
  args = remove_parenthesis(args)
  list = args.split(',')
  for k in range(len(list)) :
    if list[k][0] == 'r' or list[k][0] == 'f' :
      list[k] = list[k][1:]
    elif list[k][0] == 'c' and list[k][1] == 'r' :
      list[k] = list[k][2:]
  return list

def parse_line(line) :
  l = line.split()
  try :
    t = (get_addr(line), l[1] + l[2] + l[3] + l[4], l[5], parse_args(l[6]))
  except IndexError :
    t = (get_addr(line), l[1] + l[2] + l[3] + l[4], l[5], [])
  return t

def is_branch_i_form(mnem) :
  return mnem == "b" or mnem == "ba" or mnem == "bl" or mnem == "bla"

def is_branch_b_form(mnem) :
  return mnem == "bc" or mnem == "bca" or mnem == "bcl" or mnem == "bcla"

def get_lines(file) :
  f = open(file, 'r')
  lines = f.readlines()
  f.close()
  return lines

def call_asm_inst(line) :
  args = ""
  if line[3] == [] : args = ""
  else : 
    for k in line[3] :
      args = args + " " + k

  inst = "%s %s" %(args, line[2])
  result = commands.getoutput('gforth %s -e "%s %s bye"' %(ASM, args, line[2]))
  l = result.strip().split()
  return (inst, l[len(l)-1])

def compare_result(reference, out) :
  return string.upper(reference[1]) == out[1][1:]

def test_mnemonic_args(args) :
  print "Testing mnemonic and its arguments:"
  count_tested = 0
  count_failed = 0
  test_dir = MNEMONIC_TEST + '/' + args[0]
  print "whole form from %s" % test_dir
  print '='*80
  for k in commands.getoutput('ls %s/%s*'%(test_dir, HOSTNAME)).split('\n') :
    for l in get_lines(k) :
      count_tested = count_tested + 1
      line = parse_line(l)
      print "testing: " + str(line),
      out = call_asm_inst(line)
      if compare_result(line, out) :
        print "OK"
        print "asm-inst: " + str(out)
      else :
        count_failed = count_failed + 1
        print "FAILED"
        print "asm-inst: " + str(out)
      print '-'*80
  print "Compared mnemonic and returned args"
  print "Testcases: %s, Failed: %s" %(count_tested, count_failed)

if __name__ == '__main__' :
  parser = OptionParser("%prog <form>")
  (options, args) = parser.parse_args()
  if len(args) != 1 :
    parser.error("incorrect number of arguments")
  test_mnemonic_args(args)
    
