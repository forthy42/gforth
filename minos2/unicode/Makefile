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

# Run this Makefile whenever a new Unicode standard is published.

UNIDATA = https://unicode.org/Public/UNIDATA

PREFORTH = gforth

FETCH = UnicodeData.txt VerticalOrientation.txt BidiBrackets.txt Blocks.txt \
	Unihan.zip
EXTRACT = Unihan_Variants.txt unihan.in
GEN = unihan.db brackets.db mirrors.fs bidis.fs verticals.fs

all: $(GEN)

clean:
	rm $(GEN)

realclean:
	rm $(FETCH) $(EXTRACT) $(GEN)

$(FETCH):
	curl -s $(UNIDATA)/$@ -o $@

Unihan_Variants.txt: Unihan.zip
	unzip $< $@
	touch $@

unihan.in: Unihan_Variants.txt
	echo "\ automatically generated from $^" >$@
	grep -E 'kSimplifiedVariant|kTraditionalVariant' $< | \
	sed -e 's/U+\([0-9A-F]*\)[ 	]*kSimplifiedVariant[ 	]*U+\([0-9A-F]*\).*/$$\1 $$\2 >sc/g' \
	-e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\) U+\([0-9A-F]*\) U+\([0-9A-F]*\) U+\([0-9A-F]*\)/$$\1 $$\2 >tc	$$\3 >tc2	$$\4 >tc2	$$\5 >tc2/g' \
	-e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\) U+\([0-9A-F]*\) U+\([0-9A-F]*\)/$$\1 $$\2 >tc	$$\3 >tc2	$$\4 >tc2/g' \
	-e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\) U+\([0-9A-F]*\)/$$\1 $$\2 >tc	$$\3 >tc2/g' \
	-e 's/U+\([0-9A-F]*\)[ 	]*kTraditionalVariant[ 	]*U+\([0-9A-F]*\).*/$$\1 $$\2 >tc/g' | \
	grep -v '^#' >>$@

unihan.db: unihan.in
	$(PREFORTH) -e ': >sc 2drop ; : >tc space swap xemit xemit ; : >tc2 xemit ;' $< -e bye >$@

brackets.db: BidiBrackets.txt
	grep '; o #' $< | cut -f1 -d'#' | tr ';' ' ' | \
	$(PREFORTH) -e ": o swap xemit xemit space ; hex stdin ' read-loop execute-parsing-file bye" >$@

mirrors.fs: mirror.fs UnicodeData.txt
	echo "\ automatically generated from $^" >$@
	$(PREFORTH) $^ -e bye >>$@

bidis.fs: bidi.fs UnicodeData.txt
	echo "\ automatically generated from $^" >$@
	$(PREFORTH) $^ -e bye >>$@

verticals.fs: vertical.fs VerticalOrientation.txt
	echo "\ automatically generated from $^" >$@
	$(PREFORTH) $^ -e bye >>$@

