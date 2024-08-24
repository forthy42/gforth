#!/usr/bin/env bash

# list of aur repos to build for
REPOS=gforth-git

set -e

function build {
    rm -Rf ./$REPO
    git clone ssh://aur@aur.archlinux.org/${REPO}.git

    PKGFILE=${REPO}/PKGBUILD

    AUR_REPO=$(grep "pkgver=" $PKGFILE | cut -d= -f2)
    BUILD_VERSION=$(git tag -l "0.7.9_*" | tail -n 1)
    echo "hi1 $BUILD_VERSION"
    BUILD_VERSION=${BUILD_VERSION:6}
    echo "hi2 $BUILD_VERSION"

    if [ "$AUR_REPO" == "$BUILD_VERSION" ]; then
        echo "Version up to date, nothing to do"
    else
        COMMENT="Bumping version from $AUR_REPO to $BUILD_VERSION"
        echo $COMMENT

        # bump version in PKGBUILD
        sed -i "s/$AUR_REPO/$BUILD_VERSION/" $PKGFILE

        cd $REPO
            # bump version in .SRCINFO
            makepkg --printsrcinfo > .SRCINFO

            # ship
            git commit -am "$COMMENT"
            git push
        cd ..
    fi

    rm -Rf ./$REPO
}

for REPO in $REPOS; do
    build
done
