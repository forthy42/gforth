#!/bin/bash

sudo docker build -t forthy42/gforth:latest .
docker push forthy42/gforth:latest
