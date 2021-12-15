#!/bin/bash
./snapcraft.yaml.in
sudo /snap/bin/snapcraft --use-lxd --debug
