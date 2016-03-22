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
Version:         0.7.9_20160306
Release:         0
Summary:         GNU Forth
License:         GFDL-1.2 and GPL-2.0+ and GPL-3.0+
Group:           Development/Languages/Other
Url:             http://www.gnu.org/software/gforth/
Source0:         http://www.complang.tuwien.ac.at/forth/gforth/%{version}/gforth-%{version}.tar.xz
Source1:         http://www.complang.tuwien.ac.at/forth/gforth/%{version}/gforth-%{version}.tar.xz.sig
Source2:	 http://savannah.gnu.org/people/viewgpg.php?user_id=9629#/%{name}.keyring
BuildRequires:   emacs-nox
BuildRequires:   libffi-devel
BuildRequires:   libtool
%if 0%{?rhel_version}
BuildRequires:   libtool-ltdl libtool-ltdl-devel
%endif
%if 0%{?centos_version}
BuildRequires:   libtool-ltdl libtool-ltdl-devel
%endif
%if 0%{?fedora}
BuildRequires:   libtool-ltdl libtool-ltdl-devel
%endif
%if 0%{?suse_version}
BuildRequires:   libltdl7
Requires(post):  %{install_info_prereq}
Requires(preun): %{install_info_prereq}
%endif
BuildRoot:       %{_tmppath}/%{name}-%{version}-build

%description
Gforth is a fast and portable implementation of the ANS Forth language.

%prep
%setup -q

%build
%configure
make --jobs 1
emacs --batch --no-site-file -f batch-byte-compile gforth.el
make install.TAGS gforth.fi

%check
make check --jobs 1

%install
make --jobs 1 DESTDIR=%{buildroot} install
make start-gforth.el
touch siteinit.fs
install -d %{buildroot}%{_datadir}/emacs/site-lisp
install -m 644 gforth.el gforth.elc %{buildroot}%{_datadir}/emacs/site-lisp
install -d %{buildroot}%{_datadir}/emacs/site-lisp/site-start.d
install -m 644 start-gforth.el %{buildroot}%{_datadir}/emacs/site-lisp/site-start.d
install -m 644 siteinit.fs %{buildroot}%{_datadir}/gforth/site-forth
%if 0%{?centos_version}
rm -f %{buildroot}%{_infodir}/dir
%endif

%post
%install_info --info-dir=%{_infodir} %{_infodir}/gforth.info.gz

%preun
%install_info_delete --info-dir=%{_infodir} %{_infodir}/gforth.info.gz

%files
%defattr(-,root,root)
%doc README BUGS NEWS
%{_bindir}/*
%{_includedir}/gforth
%{_libdir}/*
%{_datadir}/emacs/site-lisp/*.el*
%{_datadir}/emacs/site-lisp/site-start.d/*.el*
%dir %{_datadir}/gforth
%{_datadir}/gforth/%{version}
%dir %{_datadir}/gforth/site-forth
%config %{_datadir}/gforth/site-forth/siteinit.fs
%doc %{_infodir}/*.gz
%doc %{_mandir}/man?/*

%changelog
