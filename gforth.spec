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
Version:        0.7.9_20250305
Release:        1.1
Summary:        GNU Forth
License:        GFDL-1.2-only AND GPL-2.0-or-later AND GPL-3.0-or-later
Group:          Development/Languages/Other
Url:            http://www.gnu.org/software/gforth/
Source0:        http://www.complang.tuwien.ac.at/forth/gforth/Snapshots/current/gforth.tar.xz
Source1:        http://www.complang.tuwien.ac.at/forth/gforth/Snapshots/current/gforth.tar.xz.sig
Source2:        http://savannah.gnu.org/people/viewgpg.php?user_id=9629#/%{name}.keyring
BuildRequires:  emacs-nox
BuildRequires:  libffi-devel
BuildRequires:  libX11-devel
%if 0%{?rhel_version}
BuildRequires:  libtool mesa-libGL-devel mesa-libEGL-devel
BuildRequires:  libpng-devel freetype-devel harfbuzz-devel
BuildRequires:  pulseaudio-libs-devel glibc-devel libxkbcommon-devel
BuildRequires:  libtool-ltdl libtool-ltdl-devel wayland-devel wayland-protocols-devel
BuildRequires:  libXrandr-devel libXext-devel texinfo
%endif
%if 0%{?centos_version}
BuildRequires:  libtool mesa-libGL-devel mesa-libEGL-devel glew-devel
BuildRequires:  libpng-devel freetype-devel harfbuzz-devel libxkbcommon-devel
BuildRequires:  pulseaudio-libs-devel opus-devel libva-devel glibc-devel
BuildRequires:  libtool-ltdl libtool-ltdl-devel wayland-devel wayland-protocols-devel
BuildRequires:  libXrandr-devel libXext-devel texinfo texinfo-tex texi2html
%endif
%if 0%{?fedora}
BuildRequires:  libtool mesa-libGL-devel mesa-libEGL-devel glew-devel vulkan-devel gpsd-devel
BuildRequires:  libpng-devel stb-devel freetype-devel harfbuzz-devel libxkbcommon-devel
BuildRequires:  pulseaudio-libs-devel opus-devel libva-devel glibc-devel
BuildRequires:  libtool-ltdl libtool-ltdl-devel wayland-devel wayland-protocols-devel
BuildRequires:  libXrandr-devel libXext-devel texinfo texinfo-tex texi2html
%endif
BuildRequires:  m4
%if 0%{?suse_version}
BuildRequires:   libtool libltdl7 Mesa-libGL-devel Mesa-libglapi-devel glew-devel vulkan-devel gpsd-devel
BuildRequires:   Mesa-libGLESv2-devel Mesa-libGLESv3-devel libpng16-devel stb-devel freetype2-devel harfbuzz-devel
BuildRequires:   libpulse-devel libopus-devel libva-devel libva-gl-devel linux-glibc-devel libxkbcommon-devel
BuildRequires:   makeinfo texinfo info wayland-devel wayland-protocols-devel
Requires(post):  %{install_info_prereq}
Requires(preun): %{install_info_prereq}
%endif
BuildRoot:      %{_tmppath}/%{name}-%{version}-build

%description
Gforth is a fast and portable implementation of the ANS Forth language.

%post
/sbin/ldconfig
%postun
/sbin/ldconfig

%package html
Summary:        GNU Forth documentation in HTML format
License:        GFDL-1.2-only
Group:          Development/Languages/Other
BuildArch:      noarch
%description html
Gforth manual in HTML format

%package pdf
Summary:        GNU Forth documentation in PDF format
License:        GFDL-1.2-only
Group:          Development/Languages/Other
BuildArch:      noarch
%description pdf
Gforth manual in PDF format

%package devel
Summary:        GNU Forth development files
License:        GPL-3.0-or-later
Group:          Development/Languages/Other
BuildArch:      noarch
%description devel
Gforth header and include files

%package buildlog
Summary:        GNU Forth build log
License:        GPL-3.0-or-later
Group:          Development/Languages/Other
BuildArch:      noarch
%description buildlog
Gforth build log files

%prep
%setup -q

%build
%configure \
        --with-extra-libs
make %{?_smp_mflags}

%check
make check %{?_smp_mflags}
cat /proc/cpuinfo
make onebench

%install
make DESTDIR=%{buildroot} install install-html install-pdf --jobs 1
cp config.log %{buildroot}%{_datadir}/doc/gforth-config.log
rm -f `find %{buildroot}%{_libdir} -name '*.a' -or -name '*.so'`
%if 0%{?centos_version}
rm -f %{buildroot}%{_infodir}/dir
%endif
%if 0%{?fedora_version}
rm -f %{buildroot}%{_infodir}/dir
%endif

%install_info --info-dir=%{_infodir} %{_infodir}/gforth.info.gz

%preun
%install_info_delete --info-dir=%{_infodir} %{_infodir}/gforth.info.gz

%files
%defattr(-,root,root)
%doc README BUGS NEWS
%{_bindir}/*
%{_libdir}/gforth
%{_libdir}/libgforth*
%{_datadir}/gforth/site-forth
%exclude %{_datadir}/gforth/%{version}/wayland
%{_datadir}/gforth
%dir %{_datadir}/emacs/site-lisp
%{_datadir}/emacs/site-lisp/gforth.el
%{_datadir}/emacs/site-lisp/gforth.elc
%{_datadir}/emacs/site-lisp/site-start.d/start-gforth.el
%doc %{_infodir}/*.gz
%doc %{_mandir}/man?/*
%dir %{_datadir}/doc/gforth
%dir %{_datadir}/doc/vmgen

%files html
%doc %{_datadir}/doc/gforth/html
%doc %{_datadir}/doc/vmgen/html

%files pdf
%doc %{_datadir}/doc/gforth/gforth.pdf
%doc %{_datadir}/doc/vmgen/vmgen.pdf

%files devel
%{_includedir}/gforth.h
%{_includedir}/gforth
%{_includedir}/wayland
%{_includedir}/freetype-gl
%{_datadir}/gforth/%{version}/wayland

%files buildlog
%{_datadir}/doc/gforth-config.log

%changelog
* Wed Mar 05 2025 <bernd@net2o.de>
- Bump version to 0.7.9_20250305