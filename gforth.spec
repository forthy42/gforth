#
# spec file for package gforth
#
# Copyright (c) 2018 SUSE LINUX GmbH, Nuernberg, Germany.
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
Version:        0.7.9_20210202
Release:        0
Summary:        GNU Forth
License:        GFDL-1.2-only AND GPL-2.0-or-later AND GPL-3.0-or-later
Group:          Development/Languages/Other
Url:            http://www.gnu.org/software/gforth/
Source0:        http://www.complang.tuwien.ac.at/forth/gforth/Snapshot/current/gforth.tar.xz
Source1:        http://www.complang.tuwien.ac.at/forth/gforth/Snapshot/current/gforth.tar.xz.sig
Source2:        http://savannah.gnu.org/people/viewgpg.php?user_id=9629#/%{name}.keyring
BuildRequires:  emacs-nox
BuildRequires:  libffi-devel
BuildRequires:  libX11-devel
%if 0%{?rhel_version}
BuildRequires:  libtool
BuildRequires:  libtool-ltdl
BuildRequires:  libtool-ltdl-devel
%endif
%if 0%{?centos_version}
BuildRequires:  libtool
BuildRequires:  libtool-ltdl
BuildRequires:  libtool-ltdl-devel
%endif
%if 0%{?fedora}
BuildRequires:  libtool
BuildRequires:  libtool-ltdl
BuildRequires:  libtool-ltdl-devel
%endif
BuildRequires:  m4
%if 0%{?suse_version}
BuildRequires:   libtool libltdl7 Mesa-libGL-devel vulkan-devel gpsd-devel
BuildRequires:   Mesa-libGLESv2-devel libpng16-devel stb-devel freetype2-devel harfbuzz-devel
BuildRequires:   libpulse-devel libopus-devel libva-devel libva-gl-devel
Requires(post):  %{install_info_prereq}
Requires(preun): %{install_info_prereq}
%endif
BuildRoot:      %{_tmppath}/%{name}-%{version}-build

%description
Gforth is a fast and portable implementation of the ANS Forth language.

%prep
%setup -q

%build
%configure
#make %{?_smp_mflags}
make --jobs 4

%check
make check %{?_smp_mflags}
#make check --jobs 1

%install
make DESTDIR=%{buildroot} install --jobs 1
rm -f `find %{buildroot}%{_libdir} -name '*.a' -or -name '*.so'`
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
%{_includedir}/gforth.h
%{_includedir}/gforth
%{_libdir}/gforth
%{_libdir}/libgforth*
%{_includedir}/freetype-gl
%{_datadir}/gforth
%dir %{_datadir}/emacs/site-lisp
%{_datadir}/emacs/site-lisp/gforth.el
%{_datadir}/emacs/site-lisp/gforth.elc
%{_datadir}/emacs/site-lisp/site-start.d/start-gforth.el
%doc %{_infodir}/*.gz
%doc %{_mandir}/man?/*

%changelog
