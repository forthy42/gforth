#!/bin/bash
./snapcraft.yaml.in
snapcraft build --use-lxd --debug --verbose
