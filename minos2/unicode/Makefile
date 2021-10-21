# Unicode database extract makefile

#Authors: Bernd Paysan
#Copyright (C) 2021 Free Software Foundation, Inc.

#This file is part of Gforth.

#Gforth is free software; you can redistribute it and/or
#modify it under the terms of the GNU General Public License
#as published by the Free Software Foundation, either version 3
#of the License, or (at your option) any later version.

#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#GNU General Public License for more details.

#You should have received a copy of the GNU General Public License
#along with this program. If not, see http://www.gnu.org/licenses/.

UNIDATA = https://unicode.org/Public/UNIDATA
UNIHAN = https://raw.githubusercontent.com/XUJiahua/ischinese/master

PREFORTH = gforth

FETCH = UnicodeData.txt VerticalOrientation.txt Unihan.zip
EXTRACT = Unihan_Variants.txt
GEN = unihan.fs mirrors.fs bidis.fs verticals.fs

all: $(GEN)

clean:
	rm $(GEN)

realclean:
	rm $(FETCH) $(EXTRACT) $(GEN)

UnicodeData.txt VerticalOrientation.txt Unihan.zip:
	curl -s $(UNIDATA)/$@ -o $@

Unihan_Variants.txt: Unihan.zip
	unzip $< $@
	touch $@

unihan.fs: Unihan_Variants.txt
	echo "\ automatically generated from $^" >$@
	grep -E 'kSimplifiedVariant|kTraditionalVariant' $< | \
	sed -e 's/U+\([0-9A-F]*\)[ 	]*kSimplifiedVariant[ 	]*U+\([0-9A-F]*\).*/$$\1 $$\2 >sc/g' \
	-e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\) U+\([0-9A-F]*\) U+\([0-9A-F]*\) U+\([0-9A-F]*\)/$$\1 $$\2 >tc	$$\3 >tc2	$$\4 >tc2	$$\5 >tc2/g' \
	-e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\) U+\([0-9A-F]*\) U+\([0-9A-F]*\)/$$\1 $$\2 >tc	$$\3 >tc2	$$\4 >tc2/g' \
	-e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\) U+\([0-9A-F]*\)/$$\1 $$\2 >tc	$$\3 >tc2/g' \
	-e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\).*/$$\1 $$\2 >tc/g' | \
	grep -v '^#' >>unihan.fs

mirrors.fs: mirror.fs UnicodeData.txt
	echo "\ automatically generated from $^" >$@
	$(PREFORTH) $^ -e bye >>$@

bidis.fs: bidi.fs UnicodeData.txt
	echo "\ automatically generated from $^" >$@
	$(PREFORTH) $^ -e bye >>$@

verticals.fs: vertical.fs VerticalOrientation.txt
	echo "\ automatically generated from $^" >$@
	$(PREFORTH) $^ -e bye >>$@

