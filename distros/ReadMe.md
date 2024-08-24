# Distro folder

this folder is intended to house distribution or build-system specific processes.

## HowTo:
Each folder needs to habe a `build.sh` file which is to be run locally and cannot require any additional parameters.
For now it is fine if building only works on devs' laptops. In the future this should be in a CI/CD.

## Contents

### archlinux

Archlinux uses aur-packages, we maintain (currently: fiz/Gerald Wodni) [gforth-git](https://aur.archlinux.org/packages/gforth-git)

