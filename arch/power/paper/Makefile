PROJNAME = asm_disasm_paper.tex
CHAPTERS = chapter1.tex \
		   chapter2.tex \
		   chapter3.tex \
		   chapter4.tex \
		   biblio.tex

all: view

asm_disasm_paper.dvi: $(PROJNAME) $(CHAPTERS)
	latex $<
	latex $<

view: asm_disasm_paper.dvi
	xdvi $<

allmost-clean:
	rm -f *toc *log *aux

clean:
	rm -f *dvi *toc *log *aux
