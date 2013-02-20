#
# spec file for package gforth (Version 0.7.0)
#
# Copyright (c) 2009 SUSE LINUX Products GmbH, Nuernberg, Germany.
#
# All modifications and additions to the file contributed by third parties
# remain the property of their copyright owners, unless otherwise agreed
# upon. The license for this file, and modifications and additions to the
# file, is the same license as for the pristine package itself (unless the
# license for the pristine package is not an Open Source License, in which
# case the license is the MIT License). An "Open Source License" is a
# license that conforms to the Open Source Definition (Version 1.9)
# published by the Open Source Initiative.

# Please submit bugfixes or comments via http://bugs.opensuse.org/
#



Name:           gforth
%if 0%{?rhel_version}
BuildRequires:  emacs-nox automake autoconf libtool libtool-ltdl
%endif
%if 0%{?centos_version}
BuildRequires:  emacs-nox automake autoconf libtool libtool-ltdl libtool-ltdl-devel
%endif
%if 0%{?fedora}
BuildRequires:  emacs-nox automake autoconf libtool libtool-ltdl
%endif
%if 0%{?suse_version}
BuildRequires:  emacs-nox automake autoconf
%endif
Url:            http://www.gnu.org/software/gforth/
License:        GNU Free Documentation License, Version 1.2 (GFDL 1.2); GPL v2 or later; GPL v3 or later
Group:          Development/Languages/Other
AutoReqProv:    on
Version:        0.7.1
Release:        41.1
Summary:        GNU Forth
Source:         gforth-%{version}.tar.gz
BuildRoot:      %{_tmppath}/%{name}-%{version}-build

%description
Gforth is a fast and portable implementation of the ANS Forth language.



Authors:
--------
    Anton Ertl <anton@mips.complang.tuwien.ac.at>
    Bernd Paysan <bernd.paysan@gmx.de>
    Jens Wilke <jens.wilke@headissue.com>

%prep
%setup -q

%build
%{?suse_update_config}
#autoreconf -fi
CFLAGS="$RPM_OPT_FLAGS -D_GNU_SOURCE" \
./configure --prefix=/usr --mandir=%{_mandir} --infodir=%{_infodir} \
            --build=%{_target_cpu}-suse-linux
#make stamp-h.in
# Fixup timestamps. Can't rebuild them without working gforth
#touch prim*.b
#sleep 1
#touch engine/*.i
make
emacs --batch --no-site-file -f batch-byte-compile gforth.el
echo > gforth-autoloads.el
emacs --batch -l autoload --eval "(setq generated-autoload-file \"$PWD/gforth-autoloads.el\")" -f batch-update-autoloads .
{
  printf ';;; suse-start-gforth.el
;;
;;; Code:\n
(add-to-list '\''auto-mode-alist '\''("\\\\.fs\\\\'\''" . forth-mode))\n\n'
  sed -n '/^;;; Generated/,/^;;;\*\*\*/p' gforth-autoloads.el
  printf '\n;;; suse-start-gforth.el ends here\n'
} > suse-start-gforth.el
make install.TAGS gforth.fi

%check
make check

%install
make install prefix=$RPM_BUILD_ROOT/usr mandir=$RPM_BUILD_ROOT%{_mandir} \
	     infodir=$RPM_BUILD_ROOT%{_infodir}
install -d $RPM_BUILD_ROOT/usr/share/emacs/site-lisp
install -m 644 gforth.el gforth.elc suse-start-gforth.el $RPM_BUILD_ROOT/usr/share/emacs/site-lisp

%clean
rm -rf $RPM_BUILD_ROOT

%post
%install_info --info-dir=%{_infodir} %{_infodir}/gforth.info.gz

%postun
%install_info_delete --info-dir=%{_infodir} %{_infodir}/gforth.info.gz

%files
%defattr(-,root,root)
%doc README BUGS NEWS
/usr/bin/*
/usr/include/gforth
/usr/lib/gforth
/usr/share/emacs/site-lisp/*.el*
/usr/share/gforth
%dir /usr/share/gforth/site-forth
%config /usr/share/gforth/site-forth/siteinit.fs
%doc %{_infodir}/*.gz
%if 0%{?suse_version}
%else
%doc %{_infodir}/dir
%endif
%doc %{_mandir}/man?/*

%changelog
* Thu Nov 06 2008 schwab@suse.de
- Update to gforth 0.7.0.
  Requirements:
  At run-time requires libtool and gcc (for the libcc C interface) and
  gdb (for the disassembler (SEE)) on some platforms.
  Installation:
  support for DESTDIR, POST_INSTALL, INSTALL_SCRIPT
  automatic performance tuning on building (--enable-force-reg unnecessary)
  report performance and functionality problems at end of "make"
  autogen.sh now exists
  License:
  Changed to GPLv3
  Bug fixes
  Now works with address-space randomization.
  The single-step debugger works again in some engines.
  Many others.
  Ports:
  AMD64, ARM, IA-64 (Itanium): better performance
  PPC, PPC64: disassembler and assembler
  Gforth EC: R8C, 4stack, misc, 8086 work
  MacOS X: better support
  Invocation:
  New flags --ignore-async-signals, --vm-commit (default overcommit)
	      --print-sequences
  Forth 200x:
  X:extension-query: produce true for all implemented extensions
  X:required REQUIRED etc. (not new)
  X:defined: [DEFINED] and [UNDEFINED]
  X:parse-name: PARSE-NAME (new name)
  X:deferred: deferred words (new: DEFER@ DEFER! ACTION-OF)
  X:structures: +FIELD FIELD: FFIELD: CFIELD: etc.
  X:ekeys: new: EKEY>FKEY K-SHIFT-MASK K-CTRL-MASK K-ALT-MASK K-F1...K-F12
  X:fp-stack (not new)
  X:number-prefixes (partially new, see below)
  Number prefixes:
  0x is a hex prefix: 0xff and 0XfF now produces (decimal) 255
  [#] is a decimal prefix: #10 now produces (decimal) 10
  Signs after the number prefix are now accepted, e.g, #-50.
  ' now only handles a single (x)char: 'ab is no longer accepted,
  'a' now produces (decimal) 97
  Unicode support (currently supports only uniform encoding):
  added xchars words for dealing with variable-width multi-byte characters
  provide 8bit (ISO Latin 1) and UTF-8 support for xchars
  New words:
  \C C-FUNCTION C-LIBRARY END-C-LIBRARY C-LIBRARY-NAME (libcc C interface)
  LIB-ERROR (complements OPEN-LIB)
  OUTFILE-EXECUTE INFILE-EXECUTE BASE-EXECUTE (limited change of global state)
  16-bit and 32-bit memory acces: UW@ UL@ SW@ SL@ W! L! W@ L@ /W /L
  NEXT-ARG SHIFT-ARGS (OS command-line argument processing)
  NOTHROW (for backtrace control)
  FTRUNC FMOD (undocumented)
  SEE-CODE SEE-CODE-RANGE (show generated dynamic native code)
  Improvements/changes of existing words:
  S\", .\" now support \l, \m, \z, and limits hex and octal character specs.
  OPEN-FILE with W/O no longer creates or truncates files (no compat. file)
  OPEN-LIB now understands ~ at the start, like OPEN-FILE.
  TRY...ENDTRY changed significantly, compatibility files available (see docs).
  The disassembler (DISCODE) can now use gdb to disassemble code
  Uninitialized defered words now give a warning when executed
  Division is floored (disable with "configure --enable-force-cdiv")
  Gforth (not gforth-fast) reports division by zero and overflow on division
  on all platforms.
  Newly documented words:
  S>NUMBER? S>UNUMBER?
  EKEY keypress names: K-LEFT  K-RIGHT K-UP K-DOWN K-HOME K-END K-PRIOR
  K-NEXT K-INSERT K-DELETE
  CLEARSTACKS
  FORM
  Environment variable GFORTHSYSTEMPREFIX (used by word SYSTEM and friends)
  C interface:
  exported symbols now start with "gforth_" (for referencing them from C code)
  libcc C function call interface (requires libtool and gcc at run-time)
  alternative: undocumented libffi-based interface
  Libraries:
  depth-changes.fs: report stack depth changes during interpretation
  ans-report.fs now reports CfV extensions
  fsl-util.4th: FSL support files (undocumented)
  regexp.fs for regular expressions (undocumented)
  complex.fs for complex numbers (undocumented)
  fft.fs for Fast Fourier Transform (undocumented)
  wf.fs, a Wiki implementation (undocumented)
  httpd.fs, a web server (undocumented)
  status.fs, show interpreter status in separate xterm (undocumented)
  profile.fs for profiling (undocumented, incomplete)
  endtry-iferror.fs, recover-endtry.fs to ease the TRY change transition
  test/tester.fs: Now works with FP numbers (undocumented)
  test/ttester.fs: Version of tester.fs with improved interface (T{...}T).
  compat library:
  compat/execute-parsing.fs
  Speed improvements:
  automatic performance tuning on building
  static stack caching (good speedup on PPC)
  mixed-precision division is now faster
  support for int128 types on AMD64
  workarounds for gcc performance bugs (in particular, PR 15242)
  branch target alignment (good speedup on Alpha).
* Wed Jul 09 2008 schwab@suse.de
- Fix last change.
* Sat Jul 05 2008 schwab@suse.de
- Fix use of undocumented autoconf variable.
* Thu Oct 11 2007 schwab@suse.de
- Remove obsolete options.
* Wed Jan 25 2006 mls@suse.de
- converted neededforbuild to BuildRequires
* Thu Jan 19 2006 schwab@suse.de
- Don't strip binaries.
* Tue Dec 20 2005 dmueller@suse.de
- fix file list
* Wed Jan 21 2004 schwab@suse.de
- Workaround gcc 3.3 bug.
* Fri Jan 09 2004 schwab@suse.de
- Update to gforth 0.6.2.
* Tue Oct 28 2003 schwab@suse.de
- Fix quoting in configure script.
* Wed Jul 02 2003 schwab@suse.de
- Fix references to build root.
* Tue May 13 2003 schwab@suse.de
- Fix filelist.
* Thu Apr 24 2003 ro@suse.de
- fix install_info --delete call and move from preun to postun
* Wed Apr 23 2003 schwab@suse.de
- Enable use of long long on ppc.
- Fix and compile gforth.el.
- Add suse-start-gforth.el.
- Include all gforth variants.
* Tue Apr 22 2003 schwab@suse.de
- Use BuildRoot.
* Mon Apr 07 2003 schwab@suse.de
- Only delete info entries when removing last version.
* Thu Mar 27 2003 schwab@suse.de
- Update to gforth 0.6.1.
* Thu Feb 06 2003 schwab@suse.de
- Use %%install_info.
* Tue Sep 17 2002 ro@suse.de
- removed bogus self-provides
* Thu Apr 18 2002 schwab@suse.de
- Fix alpha port for gcc3.
* Thu Apr 18 2002 schwab@suse.de
- Fix i386 port for gcc3.
* Mon Feb 25 2002 schwab@suse.de
- Fix permissions.
* Tue May 08 2001 mfabian@suse.de
- bzip2 sources
* Sun Apr 15 2001 schwab@suse.de
- Fix pointer <-> int clash.
* Fri Oct 06 2000 schwab@suse.de
- Update to version 0.5.0.
* Thu Aug 17 2000 schwab@suse.de
- Fix ia64 configuration.
* Thu Aug 17 2000 schwab@suse.de
- Basic support for ia64.
* Tue Jan 18 2000 schwab@suse.de
- /usr/{info,man} -> /usr/share/{info,man}
* Mon Sep 13 1999 bs@suse.de
- ran old prepare_spec on spec file to switch to new prepare_spec.
* Mon Aug 30 1999 schwab@suse.de
- specfile cleanup
- fix prims2x.fs:read-whole-file for make check
* Fri May 21 1999 ro@suse.de
- update to 0.4.0
* Sat Jun 07 1997 florian@suse.de
- first version of GNU forth 0.3 for S.u.S.E.
