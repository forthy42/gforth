#!/bin/sh

# checkout f into temporary directory
CURRENT_DIR=$(pwd)
TARGET_DIR=$CURRENT_DIR/f
TEMP_F_GIT=$(mktemp -d -t f-XXXXXXXXXX)
cd $TEMP_F_GIT
git clone https://github.com/GeraldWodni/f.git .

# copy files
FILES="api.4th compat-common.4th compat-gforth.4th f.4th vt100.4th"
for FILE in ${FILES}; do
    cp $FILE $TARGET_DIR/$FILE
done

# back to origin
cd $CURRENT_DIR
rm -Rf $TEMP_F_GIT
