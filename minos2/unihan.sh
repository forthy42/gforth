#!/bin/bash
cat >unihan.fs <<EOF
\ automatically generated from Unihan_Variants.txt
EOF
curl -s https://raw.githubusercontent.com/mahiuchun/zh-hk-data/master/Unihan_Variants.txt | grep -E 'kSimplifiedVariant|kTraditionalVariant' | sed -e 's/U+\([0-9A-F]*\)[ 	]*kSimplifiedVariant[ 	]*U+\([0-9A-F]*\).*/$\1 $\2 >sc/g' -e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\) U+\([0-9A-F]*\) U+\([0-9A-F]*\) U+\([0-9A-F]*\)/$\1 $\2 >tc	$\1 $\3 >tc	$\1 $\4 >tc	$\1 $\5 >tc/g' -e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\) U+\([0-9A-F]*\) U+\([0-9A-F]*\)/$\1 $\2 >tc	$\1 $\3 >tc	$\1 $\4 >tc/g' -e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\) U+\([0-9A-F]*\)/$\1 $\2 >tc	$\1 $\3 >tc/g'  -e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\).*/$\1 $\2 >tc/g' | grep -v '^#' >>unihan.fs
