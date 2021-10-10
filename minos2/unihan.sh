#!/bin/bash
cat >unihan.fs <<EOF
\ automatically generated from Unihan_Variants.txt
EOF
grep -E 'kSimplifiedVariant|kTraditionalVariant' ~/Downloads/Unihan_Variants.txt | sed -e 's/U+\([0-9A-F]*\)[ 	]*kSimplifiedVariant[ 	]*U+\([0-9A-F]*\).*/$\1 $\2 >sc/g' -e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\).*/$\1 $\2 >tc/g' | grep -v '^#' >>unihan.fs
