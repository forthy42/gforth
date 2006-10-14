#!/usr/bin/env python

import re, commands, string
from optparse import OptionParser

# $1 specifies form to use $2 optional mnemonic only

HEX = '[0-9abcdef]'
DISASM='/home/complang/micrev/praktikum/ppc/disasm.fs'
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

def call_disasm_inst(line) :
  out = commands.getoutput('gforth %s -e "%s %s disasm-inst bye"' 
                          %( DISASM, int(line[0], 16), int(line[1], 16))
                          )
  mnem = mnem_pat.search(out).group()
  if is_branch_i_form(mnem) :
    args = out.split()[0]
    args = string.lower(args[1:])
    args = args.split()
  else :
    args = args_pat.search(out).group().split()
  return (mnem, args, out)

def get_lines(file) :
  f = open(file, 'r')
  lines = f.readlines()
  f.close()
  return lines

def compare_mnemonic(reference, out) :
  return reference[2] == out[0]

def compare_args(reference, out) :
  return reference[3] == out[1]

def test_mnemonic_args(args) :
  print "Testing mnemonic and its arguments:"
  count_tested = 0
  count_failed = 0
  if len(args) == 2 :
    test_file = MNEMONIC_TEST + '/' + args[0] + '/' + HOSTNAME + '.' + args[1]
    print test_file
    print '='*80
    for k in get_lines(test_file) :
      count_tested = count_tested + 1
      line = parse_line(k)
      print "testing: " + str(line), 
      out = call_disasm_inst(line)
      if compare_mnemonic(line, out) and compare_args(line, out) :
        print "OK"
        print "disasm-inst: " + str(out)
      else :
        count_failed = count_failed + 1
        print "FAILED"
        print "disasm-inst: " + str(out)
      print '-'*80
  else :
    test_dir = MNEMONIC_TEST + '/' + args[0]
    print "whole form from %s" % test_dir
    print '='*80
    for k in commands.getoutput('ls %s/%s*'%(test_dir, HOSTNAME)).split('\n') :
      for l in get_lines(k) :
        count_tested = count_tested + 1
        line = parse_line(l)
        print "testing: " + str(line),
        out = call_disasm_inst(line)
        if compare_mnemonic(line, out) and compare_args(line, out) :
          print "OK"
          print "disasm-inst: " + str(out)
        else :
          count_failed = count_failed + 1
          print "FAILED"
          print "disasm-inst: " + str(out)
        print '-'*80
  print "Compared mnemonic and returned args"
  print "Testcases: %s, Failed: %s" %(count_tested, count_failed)

def test_mnemonic(args) :
  print "Testing mnemonic:"
  count_tested = 0
  count_failed = 0
  if len(args) == 2 :
    test_file = MNEMONIC_TEST + '/' + args[0] + '/' + HOSTNAME + '.' + args[1]
    print test_file
    print '='*80
    for k in get_lines(test_file) :
      count_tested = count_tested + 1
      line = parse_line(k)
      print "testing: " + str(line), 
      out = call_disasm_inst(line)
      if compare_mnemonic(line, out) :
        print "OK"
        print "disasm-inst: " + str(out)
      else :
        count_failed = count_failed + 1
        print "FAILED"
        print "disasm-inst: " + str(out)
      print '-'*80
  else :
    test_dir = MNEMONIC_TEST + '/' + args[0]
    print "whole form from %s" % test_dir
    print '='*80
    for k in commands.getoutput('ls %s/%s*'%(test_dir, HOSTNAME)).split('\n') :
      for l in get_lines(k) :
        count_tested = count_tested + 1
        line = parse_line(l)
        print "testing: " + str(line),
        out = call_disasm_inst(line)
        if compare_mnemonic(line, out) :
          print "OK"
          print "disasm-inst: " + str(out)
        else :
          count_failed = count_failed + 1
          print "FAILED"
          print "disasm-inst: " + str(out)
        print '-'*80
  print "Compared Mnemonic only"
  print "Testcases: %s, Failed: %s" %(count_tested, count_failed)

def test_arguments(args) :
  print "Testing mnemonic:"
  count_tested = 0
  count_failed = 0
  if len(args) == 2 :
    test_file = MNEMONIC_TEST + '/' + args[0] + '/' + HOSTNAME + '.' + args[1]
    print test_file
    print '='*80
    for k in get_lines(test_file) :
      count_tested = count_tested + 1
      line = parse_line(k)
      print "testing: " + str(line), 
      out = call_disasm_inst(line)
      if compare_args(line, out) :
        print "OK"
        print "disasm-inst: " + str(out)
      else :
        count_failed = count_failed + 1
        print "FAILED"
        print "disasm-inst: " + str(out)
      print '-'*80
  else :
    test_dir = MNEMONIC_TEST + '/' + args[0]
    print "whole form from %s" % test_dir
    print '='*80
    for k in commands.getoutput('ls %s/%s*'%(test_dir, HOSTNAME)).split('\n') :
      for l in get_lines(k) :
        count_tested = count_tested + 1
        line = parse_line(l)
        print "testing: " + str(line),
        out = call_disasm_inst(line)
        if compare_args(line, out) :
          print "OK"
          print "disasm-inst: " + str(out)
        else :
          count_failed = count_failed + 1
          print "FAILED"
          print "disasm-inst: " + str(out)
        print '-'*80
  print "Compared Arguments only"
  print "Testcases: %s, Failed: %s" %(count_tested, count_failed)

def show_disasm(args) :
  print "Runing disasm-inst"
  count_tested = 0
  if len(args) == 2 :
    test_file = MNEMONIC_TEST + '/' + args[0] + '/' + HOSTNAME + '.' + args[1]
    print test_file
    print '='*80
    for k in get_lines(test_file) :
      count_tested = count_tested + 1
      line = parse_line(k)
      print "testing: " + str(line)
      out = call_disasm_inst(line)
      print "disasm-inst: " + str(out)
      print '-'*80
  else :
    test_dir = MNEMONIC_TEST + '/' + args[0]
    print "whole form from %s" % test_dir
    print '='*80
    for k in commands.getoutput('ls %s/%s*'%(test_dir, HOSTNAME)).split('\n') :
      for l in get_lines(k) :
        count_tested = count_tested + 1
        line = parse_line(l)
        print "testing: " + str(line)
        out = call_disasm_inst(line)
        print "disasm-inst: " + str(out)
        print '-'*80
  print "Run disasm-inst compare the result by yourself!!"
  print "Testcases: %s" %(count_tested)

if __name__ == '__main__' :
  parser = OptionParser("%prog [-m] [-a] form [mnemonic]")
  parser.add_option("-m", "--mnemonic", action="store_true"
                   , dest="mnemonic", default=False
                   , help = "compare the returned mnemonic"
                   )
  parser.add_option("-a", "--args"
                   , action="store_true", dest="args", default=False
                   , help = "compare the result args"
                   )
  (options, args) = parser.parse_args()
  if len(args) < 1 or len(args) > 2 :
    parser.error("incorrect number of arguments")

  if options.mnemonic and options.args : 
    test_mnemonic_args(args)
  elif options.mnemonic : 
    test_mnemonic(args)
  elif options.args : 
    test_arguments(args)
  else : 
    show_disasm(args)
