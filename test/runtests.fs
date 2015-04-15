\ ANS Forth tests - run all tests

\ Adjust the file paths as appropriate to your system
\ Select the appropriate test harness, either the simple tester.fr
\ or the more complex ttester.fs 

CR .( Running ANS Forth test programs, version 0.11) CR

\ S" tester.fr" INCLUDED
S" ttester.fs" INCLUDED
S" core.fs" INCLUDED
S" coreplustest.fs" INCLUDED
S" errorreport.fs" INCLUDED
S" coreexttest.fs" INCLUDED
S" doubletest.fs" INCLUDED
S" exceptiontest.fs" INCLUDED
S" facilitytest.fs" INCLUDED
S" filetest.fs" INCLUDED
S" memorytest.fs" INCLUDED
S" toolstest.fs" INCLUDED
S" searchordertest.fs" INCLUDED
S" stringtest.fs" INCLUDED
REPORT-ERRORS

CR CR .( Forth tests completed ) CR CR


