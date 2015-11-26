#
# spec file for package gforth
#
# Copyright (c) 2015 SUSE LINUX GmbH, Nuernberg, Germany.
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


Name:            gforth
Version:         0.7.9_20151126
Release:         0
Summary:         GNU Forth
License:         GFDL-1.2 and GPL-2.0+ and GPL-3.0+
Group:           Development/Languages/Other
Url:             http://www.gnu.org/software/gforth/
Source0:         http://www.complang.tuwien.ac.at/forth/gforth/gforth-%{version}.tar.gz
Source1:         http://www.complang.tuwien.ac.at/forth/gforth/gforth-%{version}.tar.gz.sig
Source2:	 http://savannah.gnu.org/people/viewgpg.php?user_id=9629#/%{name}.keyring
BuildRequires:   emacs-nox
BuildRequires:   libffi-devel
Requires(post):  %{install_info_prereq}
Requires(preun): %{install_info_prereq}
BuildRoot:       %{_tmppath}/%{name}-%{version}-build

%description
Gforth is a fast and portable implementation of the ANS Forth language.

%prep
%setup -q

%build
%configure
make --jobs 1
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
make check --jobs 1

%install
make DESTDIR=%{buildroot} install %{?_smp_mflags}
install -d %{buildroot}%{_datadir}/emacs/site-lisp
install -m 644 gforth.el gforth.elc suse-start-gforth.el %{buildroot}%{_datadir}/emacs/site-lisp

%post
%install_info --info-dir=%{_infodir} %{_infodir}/gforth.info.gz

%preun
%install_info_delete --info-dir=%{_infodir} %{_infodir}/gforth.info.gz

%files
%defattr(-,root,root)
%doc README BUGS NEWS
%{_bindir}/*
%{_includedir}/gforth
%{_libdir}/gforth
%{_datadir}/emacs/site-lisp/*.el*
%dir %{_datadir}/gforth
%{_datadir}/gforth/%{version}
%dir %{_datadir}/gforth/site-forth
%config %{_datadir}/gforth/site-forth/siteinit.fs
%doc %{_infodir}/*.gz
%doc %{_mandir}/man?/*

%changelog
