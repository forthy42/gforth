;;; gforth.el --- major mode for editing (G)Forth sources

;; Copyright (C) 1995,1996,1997,1998,2000,2001 Free Software Foundation, Inc.

;; This file is part of Gforth.

;; GForth is distributed in the hope that it will be useful,
;; but WITHOUT ANY WARRANTY.  No author or distributor
;; accepts responsibility to anyone for the consequences of using it
;; or for whether it serves any particular purpose or works at all,
;; unless he says so in writing.  Refer to the GNU Emacs General Public
;; License for full details.

;; Everyone is granted permission to copy, modify and redistribute
;; GNU Emacs, but only under the conditions described in the
;; GNU Emacs General Public License.   A copy of this license is
;; supposed to have been given to you along with Gforth so you
;; can know your rights and responsibilities.  It should be in a
;; file named COPYING.  Among other things, the copyright notice
;; and this notice must be preserved on all copies.

;; Author: Goran Rydqvist <gorry@ida.liu.se>
;; Maintainer: David Kühling <dvdkhlng@gmx.de>
;; Created: 16 July 88 by Goran Rydqvist
;; Keywords: forth, gforth

;; Changes by anton
;; This is a variant of forth.el that came with TILE.
;; I left most of this stuff untouched and made just a few changes for 
;; the things I use (mainly indentation and syntax tables).
;; So there is still a lot of work to do to adapt this to gforth.

;; Changes by David
;; Added a syntax-hilighting engine, rewrote auto-indentation engine.
;; Added support for block files.
 
;;-------------------------------------------------------------------
;; A Forth indentation, documentation search and interaction library
;;-------------------------------------------------------------------
;;
;; Written by Goran Rydqvist, gorry@ida.liu.se, Summer 1988
;; Started:	16 July 88
;; Version:	2.10
;; Last update:	5 December 1989 by Mikael Patel, mip@ida.liu.se
;; Last update:	25 June 1990 by Goran Rydqvist, gorry@ida.liu.se
;;
;; Documentation: See forth-mode (^HF forth-mode)
;;-------------------------------------------------------------------

;;; Code:

 

;;; Hilighting and indentation engine				(dk)
;;;

(defvar forth-words nil 
  "List of words for hilighting and recognition of parsed text areas. 
You can enable hilighting of object-oriented Forth code, by appending either
`forth-objects-words' or `forth-oof-words' to the list, depending on which
OOP package you're using. After `forth-words' changed, `forth-compile-words' 
must be called to make the changes take effect.

Each item of `forth-words' has the form 
   (MATCHER TYPE HILIGHT . &optional PARSED-TEXT ...)

MATCHER is either a list of strings to match, or a REGEXP.
   If it's a REGEXP, it should not be surrounded by '\\<' or '\\>', since 
   that'll be done automatically by the search routines.

TYPE should be one of 'definiton-starter', 'definition-ender', 'compile-only',
   'immediate' or 'non-immediate'. Those information are required to determine
   whether a word actually parses (and whether that parsed text needs to be
   hilighted).

HILIGHT is a cons cell of the form (FACE . MINIMUM-LEVEL)
   Where MINIMUM-LEVEL specifies the minimum value of `forth-hilight-level',
   that's required for matching text to be hilighted.

PARSED-TEXT specifies whether and how a word parses following text. You can
   specify as many subsequent PARSED-TEXT as you wish, but that shouldn't be
   necessary very often. It has the following form:
   (DELIM-REGEXP SKIP-LEADING-FLAG PARSED-TYPE HILIGHT)

DELIM-REGEXP is a regular expression that should match strings of length 1,
   which are delimiters for the parsed text.

A non-nil value for PARSE-LEADING-FLAG means, that leading delimiter strings
   before parsed text should be skipped. This is the parsing behaviour of the
   Forth word WORD. Set it to t for name-parsing words, nil for comments and
   strings.

PARSED-TYPE specifies what kind of text is parsed. It should be on of 'name',
   'string' or 'comment'.")
(setq forth-words
      '(
	(("[") definition-ender (font-lock-keyword-face . 1))
	(("]" "]l") definition-starter (font-lock-keyword-face . 1))
	((":") definition-starter (font-lock-keyword-face . 1)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("immediate" "compile-only" "restrict")
	 immediate (font-lock-keyword-face . 1))
	(("does>") compile-only (font-lock-keyword-face . 1))
	((":noname") definition-starter (font-lock-keyword-face . 1))
	((";" ";code") definition-ender (font-lock-keyword-face . 1))
	(("include" "require" "needs" "use") 
	 non-immediate (font-lock-keyword-face . 1) 
	 "[\n\t ]" t string (font-lock-string-face . 1))
	(("included" "required" "thru" "load")
	 non-immediate (font-lock-keyword-face . 1))
	(("[char]") compile-only (font-lock-keyword-face . 1)
	 "[ \t\n]" t string (font-lock-string-face . 1))
	(("char") non-immediate (font-lock-keyword-face . 1)
	 "[ \t\n]" t string (font-lock-string-face . 1))
	(("s\"" "c\"") immediate (font-lock-string-face . 1)
	 "[\"\n]" nil string (font-lock-string-face . 1))
	((".\"") compile-only (font-lock-string-face . 1)
	 "[\"\n]" nil string (font-lock-string-face . 1))
	(("abort\"") compile-only (font-lock-keyword-face . 1)
	 "[\"\n]" nil string (font-lock-string-face . 1))
	(("{") compile-only (font-lock-variable-name-face . 1)
	 "[\n}]" nil name (font-lock-variable-name-face . 1))
	((".(" "(") immediate (font-lock-comment-face . 1)
	  ")" nil comment (font-lock-comment-face . 1))
	(("\\" "\\G") immediate (font-lock-comment-face . 1)
	 "[\n]" nil comment (font-lock-comment-face . 1))
	  
	(("[if]" "[?do]" "[do]" "[for]" "[begin]" 
	  "[endif]" "[then]" "[loop]" "[+loop]" "[next]" "[until]" "[repeat]"
	  "[again]" "[while]" "[else]")
	 immediate (font-lock-keyword-face . 2))
	(("[ifdef]" "[ifundef]") immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("if" "begin" "ahead" "do" "?do" "+do" "u+do" "-do" "u-do" "for" 
	  "case" "of" "?dup-if" "?dup-0=-if" "then" "until" "repeat" "again" 
	  "loop" "+loop" "-loop" "next" "endcase" "endof" "else" "while" "try"
	  "recover" "endtry" "assert(" "assert0(" "assert1(" "assert2(" 
	  "assert3(" ")" "<interpretation" "<compilation" "interpretation>" 
	  "compilation>")
	 compile-only (font-lock-keyword-face . 2))

	(("true" "false" "c/l" "bl" "cell" "pi" "w/o" "r/o" "r/w") 
	 non-immediate (font-lock-constant-face . 2))
	(("~~") compile-only (font-lock-warning-face . 2))
	(("postpone" "[is]" "defers" "[']" "[compile]") 
	 compile-only (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("is" "what's") immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("<is>" "'") non-immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("[to]") compile-only (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-variable-name-face . 3))
	(("to") immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-variable-name-face . 3))
	(("<to>") non-immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-variable-name-face . 3))

	(("create" "variable" "constant" "2variable" "2constant" "fvariable"
	  "fconstant" "value" "field" "user" "vocabulary" 
	  "create-interpret/compile")
	 non-immediate (font-lock-type-face . 2)
	 "[ \t\n]" t name (font-lock-variable-name-face . 3))
	("\\S-+%" non-immediate (font-lock-type-face . 2))
	(("defer" "alias" "create-interpret/compile:") 
	 non-immediate (font-lock-type-face . 1)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("end-struct") non-immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-type-face . 3))
	(("struct") non-immediate (font-lock-keyword-face . 2))
	("-?[0-9]+\\(\\.[0-9]*e\\(-?[0-9]+\\)?\\|\\.?[0-9a-f]*\\)" 
	 immediate (font-lock-constant-face . 3))
	))

(defvar forth-use-objects nil 
  "*Non-nil makes forth-mode also hilight words from the \"Objects\" package.")
(defvar forth-objects-words nil
  "Hilighting description for words of the \"Objects\" package")
(setq forth-objects-words 
      '(((":m") definition-starter (font-lock-keyword-face . 1)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("m:") definition-starter (font-lock-keyword-face . 1))
	((";m") definition-ender (font-lock-keyword-face . 1))
	(("[current]" "[parent]") compile-only (font-lock-keyword-face . 1)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("current" "overrides") non-immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("[to-inst]") compile-only (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-variable-name-face . 3))
	(("[bind]") compile-only (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-type-face . 3)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("bind") non-immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-type-face . 3)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("inst-var" "inst-value") non-immediate (font-lock-type-face . 2)
	 "[ \t\n]" t name (font-lock-variable-name-face . 3))
	(("method" "selector")
	 non-immediate (font-lock-type-face . 1)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("end-class" "end-interface")
	 non-immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-type-face . 3))
	(("public" "protected" "class" "exitm" "implementation" "interface"
	  "methods" "end-methods" "this") 
	 non-immediate (font-lock-keyword-face . 2))
	(("object") non-immediate (font-lock-type-face . 2))))

(defvar forth-use-oof nil 
  "*Non-nil makes forth-mode also hilight words from the \"OOF\" package.")
(defvar forth-oof-words nil
  "Hilighting description for words of the \"OOF\" package")
(setq forth-oof-words 
      '((("class") non-immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-type-face . 3))
	(("var") non-immediate (font-lock-type-face . 2)
	 "[ \t\n]" t name (font-lock-variable-name-face . 3))
	(("method") non-immediate (font-lock-type-face . 2)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("::" "super" "bind" "bound" "link") 
	 immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
	(("ptr" "asptr" "[]") 
	 immediate (font-lock-keyword-face . 2)
	 "[ \t\n]" t name (font-lock-variable-name-face . 3))
	(("class;" "how:" "self" "new" "new[]" "definitions" "class?" "with"
	  "endwith")
	 non-immediate (font-lock-keyword-face . 2))
	(("object") non-immediate (font-lock-type-face . 2))))

(defvar forth-local-words nil 
  "List of Forth words to prepend to `forth-words'. Should be set by a 
 forth source, using a local variables list at the end of the file 
 (\"Local Variables: ... forth-local-words: ... End:\" construct).") 

(defvar forth-custom-words nil
  "List of Forth words to prepend to `forth-words'. Should be set in your
 .emacs.")

(defvar forth-hilight-level 3 "*Level of hilighting of Forth code.")

(defvar forth-compiled-words nil "Compiled representation of `forth-words'.")


; todo:
;

; Wörter ordentlich hilighten, die nicht auf whitespace beginning ( ..)IF
; Additional `forth-use-objects' or
; `forth-use-oof' could be set to non-nil for automatical adding of those
; word-lists. Using local variable list?
;
; Konfiguration über customization groups
;
; Bereich nur auf Wortanfang/ende ausweiten, wenn anfang bzw ende in einem 
; Wort liegen (?) -- speed!
;
; User interface
;
; 'forth-word' property muss eindeutig sein!
;
; imenu support schlauer machen

(setq debug-on-error t)

;; Filter list by predicate. This is a somewhat standard function for 
;; functional programming languages. So why isn't it already implemented 
;; in Lisp??
(defun forth-filter (predicate list)
  (let ((filtered nil))
    (mapcar (lambda (item)
	      (when (funcall predicate item)
		(if filtered
		    (nconc filtered (list item))
		  (setq filtered (cons item nil))))
	      nil) list)
    filtered))

;; Helper function for `forth-compile-word': return whether word has to be
;; added to the compiled word list, for syntactic parsing and hilighting.
(defun forth-words-filter (word)
  (let* ((hilight (nth 2 word))
	 (level (cdr hilight))
	 (parsing-flag (nth 3 word)))
    (or parsing-flag 
	(<= level forth-hilight-level))))

;; Helper function for `forth-compile-word': translate one entry from 
;; `forth-words' into the form  (regexp regexp-depth word-description)
(defun forth-compile-words-mapper (word)
  (let* ((matcher (car word))
	 (regexp (if (stringp matcher) (concat "\\(" matcher "\\)")
		   (if (listp matcher) (regexp-opt matcher t)
		     (error "Invalid matcher (stringp or listp expected `%s'" 
			    matcher))))
	 (depth (regexp-opt-depth regexp))
	 (description (cdr word)))
    (list regexp depth description)))

;; Read `words' and create a compiled representation suitable for efficient
;; parsing of the form  
;; (regexp (subexp-count word-description) (subexp-count2 word-description2)
;;  ...)
(defun forth-compile-wordlist (words)
  (let* ((mapped (mapcar 'forth-compile-words-mapper words))
	 (regexp (concat "\\<\\(" 
			 (mapconcat 'car mapped "\\|")
			 "\\)\\>"))
	 (sub-count 2)
	 (sub-list (mapcar 
		    (lambda (i) 
		      (let ((sub (cons sub-count (nth 2 i))))
			(setq sub-count (+ sub-count (nth 1 i)))
			sub 
			)) 
		    mapped)))
    (let ((result (cons regexp sub-list)))
      (byte-compile 'result)
      result)))

(defun forth-compile-words ()
  "Compile the the words from `forth-words' and `forth-indent-words' into
 the format that's later used for doing the actual hilighting/indentation.
 Store the resulting compiled wordlists in `forth-compiled-words' and 
`forth-compiled-indent-words', respective"
  (setq forth-compiled-words 
	(forth-compile-wordlist 
	 (forth-filter 'forth-words-filter forth-words)))
  (setq forth-compiled-indent-words 
	(forth-compile-wordlist forth-indent-words)))

(defun forth-hack-local-variables ()
  "Parse and bind local variables, set in the contents of the current 
 forth-mode buffer. Prepend `forth-local-words' to `forth-words' and 
 `forth-local-indent-words' to `forth-indent-words'."
  (hack-local-variables)
  (setq forth-words (append forth-local-words forth-words))
  (setq forth-indent-words (append forth-local-indent-words 
				   forth-indent-words)))

(defun forth-customize-words ()
  "Add the words from `forth-custom-words' and `forth-custom-indent-words'
 to `forth-words' and `forth-indent-words', respective. Add 
 `forth-objects-words' and/or `forth-oof-words' to `forth-words', if
 `forth-use-objects' and/or `forth-use-oof', respective is set."
  (setq forth-words (append forth-custom-words forth-words
			    (if forth-use-oof forth-oof-words nil)
			    (if forth-use-objects forth-objects-words nil)))
  (setq forth-indent-words (append 
			    forth-custom-indent-words forth-indent-words)))



;; get location of first character of previous forth word that's got 
;; properties
(defun forth-previous-start (pos)
  (let* ((word (get-text-property pos 'forth-word))
	 (prev (previous-single-property-change 
		(min (point-max) (1+ pos)) 'forth-word 
		(current-buffer) (point-min))))
    (if (or (= (point-min) prev) word) prev
      (if (get-text-property (1- prev) 'forth-word)
	  (previous-single-property-change 
	   prev 'forth-word (current-buffer) (point-min))
	(point-min)))))

;; Get location of the last character of the current/next forth word that's
;; got properties, text that's parsed by the word is considered as parts of 
;; the word.
(defun forth-next-end (pos)
  (let* ((word (get-text-property pos 'forth-word))
	 (next (next-single-property-change pos 'forth-word 
					    (current-buffer) (point-max))))
    (if word next
      (if (get-text-property next 'forth-word)
	  (next-single-property-change 
	   next 'forth-word (current-buffer) (point-max))
	(point-max)))))

(defun forth-next-whitespace (pos)
  (save-excursion
    (goto-char pos)
    (skip-syntax-forward "-" (point-max))
    (point)))
(defun forth-previous-word (pos)
  (save-excursion
    (goto-char pos)
    (re-search-backward "\\<" pos (point-min) 1)
    (point)))

;; Delete all properties, used by Forth mode, from `from' to `to'.
(defun forth-delete-properties (from to)
  (remove-text-properties 
   from to '(face nil forth-parsed nil forth-word nil forth-state nil)))

;; Get the index of the branch of the most recently evaluated regular 
;; expression that matched. (used for identifying branches "a\\|b\\|c...")
(defun forth-get-regexp-branch ()
  (let ((count 2))
    (while (not (match-beginning count))
      (setq count (1+ count)))
    count))

;; seek to next forth-word and return its "word-description"
(defun forth-next-known-forth-word (to)
  (if (<= (point) to)
      (progn
	(let* ((regexp (car forth-compiled-words))
	       (pos (re-search-forward regexp to t)))
	  (if pos (let ((branch (forth-get-regexp-branch))
			(descr (cdr forth-compiled-words)))
		    (goto-char (match-beginning 0))
		    (cdr (assoc branch descr)))
	    'nil)))
    nil))

;; Set properties of forth word at `point', eventually parsing subsequent 
;; words, and parsing all whitespaces. Set point to delimiter after word.
;; The word, including it's parsed text gets the `forth-word' property, whose 
;; value is unique, and may be used for getting the word's start/end 
;; positions.
(defun forth-set-word-properties (state data)
  (let* ((start (point))
	 (end (progn (re-search-forward "[ \t]\\|$" (point-max) 1)
		     (point)))
	 (type (car data))
	 (hilight (nth 1 data))
	 (bad-word (and (not state) (eq type 'compile-only)))
	 (hlface (if bad-word font-lock-warning-face
		   (if (<= (cdr hilight) forth-hilight-level)
		       (car hilight) nil))))
    (when hlface (put-text-property start end 'face hlface))
    ;; if word parses in current state, process parsed range of text
    (when (or (not state) (eq type 'compile-only) (eq type 'immediate))
      (let ((parse-data (nthcdr 2 data)))
	(while parse-data
	  (let ((delim (nth 0 parse-data))
		(skip-leading (nth 1 parse-data))
		(parse-type (nth 2 parse-data))
		(parsed-hilight (nth 3 parse-data))
		(parse-start (point))
		(parse-end))
	    (when skip-leading
	      (while (and (looking-at delim) (> (match-end 0) (point))
			  (not (looking-at "\n")))
		(forward-char)))
	    (re-search-forward delim (point-max) 1)
	    (setq parse-end (point))
	    (forth-delete-properties end parse-end)
	    (when (<= (cdr parsed-hilight) forth-hilight-level)
	      (put-text-property 
	       parse-start parse-end 'face (car parsed-hilight)))
	    (put-text-property 
	     parse-start parse-end 'forth-parsed parse-type)
	    (setq end parse-end)
	    (setq parse-data (nthcdr 4 parse-data))))))
    (put-text-property start end 'forth-word start)))

;; Search for known Forth words in the range `from' to `to', using 
;; `forth-next-known-forth-word' and set their properties via 
;; `forth-set-word-properties'.
(defun forth-update-properties (from to &optional loudly)
  (save-excursion
    (let ((msg-count 0) (state) (word-descr) (last-location))
      (goto-char (forth-previous-word (forth-previous-start 
				       (max (point-min) (1- from)))))
      (setq to (forth-next-end (min (point-max) (1+ to))))
      ;; `to' must be on a space delimiter, if a parsing word was changed
      (setq to (forth-next-whitespace to))
      (setq state (get-text-property (point) 'forth-state))
      (setq last-location (point))
      (forth-delete-properties (point) to)
      ;; hilight loop...
      (while (setq word-descr (forth-next-known-forth-word to))
	(when loudly
	  (when (equal 0 (% msg-count 100))
	    (message "Parsing Forth code...%s"
		     (make-string (/ msg-count 100) ?.)))
	  (setq msg-count (1+ msg-count)))
	(forth-set-word-properties state word-descr)
	(when state (put-text-property last-location (point) 'forth-state t))
	(let ((type (car word-descr)))
	  (if (eq type 'definition-starter) (setq state t))
	  (if (eq type 'definition-ender) (setq state nil))
	  (setq last-location (point))))
      ;; update state property up to `to'
      (if (and state (< (point) to))
	  (put-text-property last-location to 'forth-state t))
      ;; extend search if following state properties differ from current state
      (if (< to (point-max))
	  (if (not (equal state (get-text-property (1+ to) 'forth-state)))
	      (let ((extend-to (next-single-property-change 
				to 'forth-state (current-buffer) (point-max))))
		(forth-update-properties to extend-to))
	    ))
      )))

;; save-buffer-state borrowed from `font-lock.el'
(eval-when-compile 
  (defmacro forth-save-buffer-state (varlist &rest body)
    "Bind variables according to VARLIST and eval BODY restoring buffer state."
    (` (let* ((,@ (append varlist
		   '((modified (buffer-modified-p)) (buffer-undo-list t)
		     (inhibit-read-only t) (inhibit-point-motion-hooks t)
		     before-change-functions after-change-functions
		     deactivate-mark buffer-file-name buffer-file-truename))))
	 (,@ body)
	 (when (and (not modified) (buffer-modified-p))
	   (set-buffer-modified-p nil))))))

;; Function that is added to the `change-functions' hook. Calls 
;; `forth-update-properties' and keeps care of disabling undo information
;; and stuff like that.
(defun forth-change-function (from to len &optional loudly)
  (save-match-data
    (forth-save-buffer-state () 
     (unwind-protect 
	 (progn 
	   (forth-update-properties from to loudly)
	   (forth-update-show-screen)
	   (forth-update-warn-long-lines))))))

(eval-when-compile
  (byte-compile 'forth-set-word-properties)
  (byte-compile 'forth-next-known-forth-word)
  (byte-compile 'forth-update-properties)
  (byte-compile 'forth-delete-properties)
  (byte-compile 'forth-get-regexp-branch)) 

;;; imenu support
;;;
(defvar forth-defining-words 
  '("VARIABLE" "CONSTANT" "2VARIABLE" "2CONSTANT" "FVARIABLE" "FCONSTANT"
   "USER" "VALUE" "field" "end-struct" "VOCABULARY" "CREATE" ":" "CODE"
   "DEFER" "ALIAS")
  "List of words, that define the following word.
 Used for imenu index generation")

 
(defun forth-next-definition-starter ()
  (progn
    (let* ((pos (re-search-forward forth-defining-words-regexp (point-max) t)))
      (if pos
	  (if (or (text-property-not-all (match-beginning 0) (match-end 0) 
					 'forth-parsed nil)
		  (text-property-not-all (match-beginning 0) (match-end 0)
					 'forth-state nil)) 
	      (forth-next-definition-starter)
	    t)
	nil))))

(defun forth-create-index ()
  (let* ((forth-defining-words-regexp 
	  (concat "\\<\\(" (regexp-opt forth-defining-words) "\\)\\>"))
	 (index nil))
    (goto-char (point-min))
    (while (forth-next-definition-starter)
      (if (looking-at "[ \t]*\\([^ \t\n]+\\)")
	  (setq index (cons (cons (match-string 1) (point)) index))))
    index))

(require 'speedbar)
(speedbar-add-supported-extension ".fs")
(speedbar-add-supported-extension ".fb")

;; (require 'profile)
;; (setq profile-functions-list '(forth-set-word-properties forth-next-known-forth-word forth-update-properties forth-delete-properties forth-get-regexp-branch))

;;; Indentation
;;;

(defvar forth-indent-words nil 
  "List of words that have indentation behaviour.
Each element of `forth-indent-words' should have the form
   (MATCHER INDENT1 INDENT2 &optional TYPE) 
  
MATCHER is either a list of strings to match, or a REGEXP.
   If it's a REGEXP, it should not be surrounded by `\\<` or `\\>`, since 
   that'll be done automatically by the search routines.

TYPE might be omitted. If it's specified, the only allowed value is 
   currently the symbol `non-immediate', meaning that the word will not 
   have any effect on indentation inside definitions. (:NONAME is a good 
   example for this kind of word).

INDENT1 specifies how to indent a word that's located at a line's begin,
   following any number of whitespaces.

INDENT2 specifies how to indent words that are not located at a line's begin.

INDENT1 and INDENT2 are indentation specifications of the form
   (SELF-INDENT . NEXT-INDENT), where SELF-INDENT is a numerical value, 
   specifying how the matching line and all following lines are to be 
   indented, relative to previous lines. NEXT-INDENT specifies how to indent 
   following lines, relative to the matching line.
  
   Even values of SELF-INDENT and NEXT-INDENT correspond to multiples of
   `forth-indent-level'. Odd values get an additional 
   `forth-minor-indent-level' added/substracted. Eg a value of -2 indents
   1 * forth-indent-level  to the left, wheras 3 indents 
   1 * forth-indent-level + forth-minor-indent-level  columns to the right.")

(setq forth-indent-words
      '((("if" "begin" "do" "?do" "+do" "-do" "u+do"
	  "u-do" "?dup-if" "?dup-0=-if" "case" "of" "try" 
	  "[if]" "[ifdef]" "[ifundef]" "[begin]" "[for]" "[do]" "[?do]")
	 (0 . 2) (0 . 2))
	((":" ":noname" "code" "struct" "m:" ":m" "class" "interface")
	 (0 . 2) (0 . 2) non-immediate)
	("\\S-+%$" (0 . 2) (0 . 0) non-immediate)
	((";" ";m") (-2 . 0) (0 . -2))
	(("again" "repeat" "then" "endtry" "endcase" "endof" 
	  "[then]" "[endif]" "[loop]" "[+loop]" "[next]" 
	  "[until]" "[repeat]" "[again]" "loop")
	 (-2 . 0) (0 . -2))
	(("end-code" "end-class" "end-interface" "end-class-noname" 
	  "end-interface-noname" "end-struct" "class;")
	 (-2 . 0) (0 . -2) non-immediate)
	(("protected" "public" "how:") (-1 . 1) (0 . 0) non-immediate)
	(("+loop" "-loop" "until") (-2 . 0) (-2 . 0))
	(("else" "recover" "[else]") (-2 . 2) (0 . 0))
	(("while" "does>" "[while]") (-1 . 1) (0 . 0))
	(("\\g") (-2 . 2) (0 . 0))))

(defvar forth-local-indent-words nil 
  "List of Forth words to prepend to `forth-indent-words', when a forth-mode
buffer is created. Should be set by a Forth source, using a local variables 
list at the end of the file (\"Local Variables: ... forth-local-words: ... 
End:\" construct).")

(defvar forth-custom-indent-words nil
  "List of Forth words to prepend to `forth-indent-words'. Should be set in
 your .emacs.")

(defvar forth-indent-level 4
  "Indentation of Forth statements.")
(defvar forth-minor-indent-level 2
  "Minor indentation of Forth statements.")
(defvar forth-compiled-indent-words nil)

;; Return, whether `pos' is the first forth word on its line
(defun forth-first-word-on-line-p (pos)
  (save-excursion
    (beginning-of-line)
    (skip-chars-forward " \t")
    (= pos (point))))

;; Return indentation data (SELF-INDENT . NEXT-INDENT) of next known 
;; indentation word, or nil if there is no word up to `to'. 
;; Position `point' at location just after found word, or at `to'. Parsed 
;; ranges of text will not be taken into consideration!
(defun forth-next-known-indent-word (to)
  (if (<= (point) to)
      (progn
	(let* ((regexp (car forth-compiled-indent-words))
	       (pos (re-search-forward regexp to t)))
	  (if pos
	      (let* ((start (match-beginning 0))
		     (end (match-end 0))
		     (branch (forth-get-regexp-branch))
		     (descr (cdr forth-compiled-indent-words))
		     (indent (cdr (assoc branch descr)))
		     (type (nth 2 indent)))
		;; skip words that are parsed (strings/comments) and 
		;; non-immediate words inside definitions
		(if (or (text-property-not-all start end 'forth-parsed nil)
			(and (eq type 'non-immediate) 
			     (text-property-not-all start end 
						    'forth-state nil)))
		    (forth-next-known-indent-word to)
		  (if (forth-first-word-on-line-p (match-beginning 0))
		      (nth 0 indent) (nth 1 indent))))
	    nil)))
    nil))
  
;; Translate indentation value `indent' to indentation column. Multiples of
;; 2 correspond to multiples of `forth-indent-level'. Odd numbers get an
;; additional `forth-minor-indent-level' added (or substracted).
(defun forth-convert-to-column (indent)
  (let* ((sign (if (< indent 0) -1 1))
	 (value (abs indent))
	 (major (* (/ value 2) forth-indent-level))
	 (minor (* (% value 2) forth-minor-indent-level)))
    (* sign (+ major minor))))

;; Return the column increment, that the current line of forth code does to
;; the current or following lines. `which' specifies which indentation values
;; to use. 0 means the indentation of following lines relative to current 
;; line, 1 means the indentation of the current line relative to the previous 
;; line. Return `nil', if there are no indentation words on the current line.
(defun forth-get-column-incr (which)
  (save-excursion
    (let ((regexp (car forth-compiled-indent-words))
	  (word-indent)
	  (self-indent nil)
	  (next-indent nil)
	  (to (save-excursion (end-of-line) (point))))
      (beginning-of-line)
      (while (setq word-indent (forth-next-known-indent-word to))
	(let* ((self-incr (car word-indent))
	       (next-incr (cdr word-indent))
	       (self-column-incr (forth-convert-to-column self-incr))
	       (next-column-incr (forth-convert-to-column next-incr)))
	  (setq next-indent (if next-indent next-indent 0))
	  (setq self-indent (if self-indent self-indent 0))
	  (if (or (and (> next-indent 0) (< self-column-incr 0))
		  (and (< next-indent 0) (> self-column-incr 0)))
	      (setq next-indent (+ next-indent self-column-incr))
	    (setq self-indent (+ self-indent self-column-incr)))
	  (setq next-indent (+ next-indent next-column-incr))))
      (nth which (list self-indent next-indent)))))

;; Find previous line that contains indentation words, return the column,
;; to which following text should be indented to.
(defun forth-get-anchor-column ()
  (save-excursion
    (if (/= 0 (forward-line -1)) 0
      (let ((indent))
	(while (not (or (setq indent (forth-get-column-incr 1))
			(<= (point) (point-min))))
	  (forward-line -1))
	(+ (current-indentation) (if indent indent 0))))))

(defun forth-indent-line (&optional flag)
  "Correct indentation of the current Forth line."
  (let* ((anchor (forth-get-anchor-column))
	 (column-incr (forth-get-column-incr 0)))
    (forth-indent-to (if column-incr (+ anchor column-incr) anchor))))

(defun forth-current-column ()
  (- (point) (save-excursion (beginning-of-line) (point))))
(defun forth-current-indentation ()
  (- (save-excursion (beginning-of-line) (forward-to-indentation 0) (point))
     (save-excursion (beginning-of-line) (point))))

(defun forth-indent-to (x)
  (let ((p nil))
    (setq p (- (forth-current-column) (forth-current-indentation)))
    (forth-delete-indentation)
    (beginning-of-line)
    (indent-to x)
    (if (> p 0) (forward-char p))))

(defun forth-delete-indentation ()
  (save-excursion
    (delete-region 
     (progn (beginning-of-line) (point)) 
     (progn (back-to-indentation) (point)))))

(defun forth-indent-command ()
  (interactive)
  (forth-indent-line t))

;; remove trailing whitespaces in current line
(defun forth-remove-trailing ()
  (save-excursion
    (end-of-line)
    (delete-region (point) (progn (skip-chars-backward " \t") (point)))))

;; insert newline, removing any trailing whitespaces in the current line
(defun forth-newline-remove-trailing ()
  (save-excursion
    (delete-region (point) (progn (skip-chars-backward " \t") (point))))
  (newline))
;  (let ((was-point (point-marker)))
;    (unwind-protect 
;	(progn (forward-line -1) (forth-remove-trailing))
;      (goto-char (was-point)))))

;; workaround for bug in `reindent-then-newline-and-indent'
(defun forth-reindent-then-newline-and-indent ()
  (interactive "*")
  (indent-according-to-mode)
  (forth-newline-remove-trailing)
  (indent-according-to-mode))

;;; end hilighting/indentation

;;; Block file encoding/decoding  (dk)
;;;

(defconst forth-c/l 64 "Number of characters per block line")
(defconst forth-l/b 16 "Number of lines per block")

;; Check whether the unconverted block file line, point is in, does not
;; contain `\n' and `\t' characters.
(defun forth-check-block-line (line)
  (let ((end (save-excursion (beginning-of-line) (forward-char forth-c/l)
			     (point))))
    (save-excursion 
      (beginning-of-line)
      (when (search-forward "\n" end t)
	(message "Warning: line %i contains newline character #10" line)
	(ding t))
      (beginning-of-line)
      (when (search-forward "\t" end t)
	(message "Warning: line %i contains tab character #8" line)
	(ding t)))))

(defun forth-convert-from-block (from to)
  "Convert block file format to stream source in current buffer."
  (let ((line (count-lines (point-min) from)))
    (save-excursion
      (goto-char from)
      (set-mark to)
      (while (< (+ (point) forth-c/l) (mark t))
	(setq line (1+ line))
	(forth-check-block-line line)
	(forward-char forth-c/l)
	(forth-newline-remove-trailing))
      (when (= (+ (point) forth-c/l) (mark t))
	(forth-remove-trailing))
      (mark t))))

;; Pad a line of a block file up to `forth-c/l' characters, positioning `point'
;; at the end of line.
(defun forth-pad-block-line ()
  (save-excursion
    (end-of-line)
    (if (<= (current-column) forth-c/l)
	(move-to-column forth-c/l t)
      (message "Line %i longer than %i characters, truncated"
	       (count-lines (point-min) (point)) forth-c/l)
      (ding t)
      (move-to-column forth-c/l t)
      (delete-region (point) (progn (end-of-line) (point))))))

;; Replace tab characters in current line by spaces.
(defun forth-convert-tabs-in-line ()
  (save-excursion
    (beginning-of-line)
    (while (search-forward "\t" (save-excursion (end-of-line) (point)) t)
      (backward-char)
      (delete-region (point) (1+ (point)))
      (insert-char ?\  (- tab-width (% (current-column) tab-width))))))

;; Delete newline at end of current line, concatenating it with the following
;; line. Place `point' at end of newly formed line.
(defun forth-delete-newline ()
  (end-of-line)
  (delete-region (point) (progn (beginning-of-line 2) (point))))

(defun forth-convert-to-block (from to &optional original-buffer) 
  "Convert range of text to block file format in current buffer."
  (let* ((lines 0)) ; I have to count lines myself, since `count-lines' has
		    ; problems with trailing newlines...
    (save-excursion
      (goto-char from)
      (set-mark to)
      ;; pad lines to full length (`forth-c/l' characters per line)
      (while (< (save-excursion (end-of-line) (point)) (mark t))
	(setq lines (1+ lines))
	(forth-pad-block-line)
	(forth-convert-tabs-in-line)
	(forward-line))
      ;; also make sure the last line is padded, if `to' is at its end
      (end-of-line)
      (when (= (point) (mark t))
	(setq lines (1+ lines))
	(forth-pad-block-line)
	(forth-convert-tabs-in-line))
      ;; remove newlines between lines
      (goto-char from)
      (while (< (save-excursion (end-of-line) (point)) (mark t))
	(forth-delete-newline))
      ;; append empty lines, until last block is complete
      (goto-char (mark t))
      (let* ((required (* (/ (+ lines (1- forth-l/b)) forth-l/b) forth-l/b))
	     (pad-lines (- required lines)))
	(while (> pad-lines 0)
	  (insert-char ?\  forth-c/l)
	  (setq pad-lines (1- pad-lines))))
      (point))))

(defun forth-detect-block-file-p ()
  "Return non-nil if the current buffer is in block file format. Detection is
done by checking whether the first line has 1024 characters or more."
  (save-restriction 
    (widen)
    (save-excursion
       (goto-char (point-min))
       (end-of-line)
       (>= (current-column) 1024))))

;; add block file conversion routines to `format-alist'
(defconst forth-block-format-description
  '(forth-blocks "Forth block source file" nil 
		 forth-convert-from-block forth-convert-to-block 
		 t normal-mode))
(unless (memq forth-block-format-description format-alist)
  (setq format-alist (cons forth-block-format-description format-alist)))

;;; End block file encoding/decoding

;;; Block file editing
;;;
(defvar forth-overlay-arrow-string ">>")
(defvar forth-block-base 1 "Number of first block in block file")
(defvar forth-show-screen nil
  "Non-nil means to show screen starts and numbers (for block files)")
(defvar forth-warn-long-lines nil
  "Non-nil means to warn about lines that are longer than 64 characters")

(defvar forth-screen-marker nil)

(defun forth-update-show-screen ()
  "If `forth-show-screen' is non-nil, put overlay arrow to start of screen, 
`point' is in. If arrow now points to different screen than before, display 
screen number."
  (if (not forth-show-screen)
      (setq overlay-arrow-string nil)
    (save-excursion
      (let* ((line (count-lines (point-min) (min (point-max) (1+ (point)))))
	     (first-line (1+ (* (/ (1- line) forth-l/b) forth-l/b)))
	     (scr (+ forth-block-base (/ first-line forth-l/b))))
	(setq overlay-arrow-string forth-overlay-arrow-string)
	(goto-line first-line)
	(setq overlay-arrow-position forth-screen-marker)
	(set-marker forth-screen-marker 
		    (save-excursion (goto-line first-line) (point)))
	(setq forth-screen-number-string (format "%d" scr))))))

(add-hook 'forth-motion-hooks 'forth-update-show-screen)

(defun forth-update-warn-long-lines ()
  "If `forth-warn-long-lines' is non-nil, display a warning whenever a line
exceeds 64 characters."
  (when forth-warn-long-lines
    (when (> (save-excursion (end-of-line) (current-column)) forth-c/l)
      (message "Warning: current line exceeds %i characters"
	       forth-c/l))))

(add-hook 'forth-motion-hooks 'forth-update-warn-long-lines)
    
;;; End block file editing


(defvar forth-mode-abbrev-table nil
  "Abbrev table in use in Forth-mode buffers.")

(define-abbrev-table 'forth-mode-abbrev-table ())

(defvar forth-mode-map nil
  "Keymap used in Forth mode.")

(if (not forth-mode-map)
    (setq forth-mode-map (make-sparse-keymap)))

;(define-key forth-mode-map "\M-\C-x" 'compile)
(define-key forth-mode-map "\C-x\\" 'comment-region)
(define-key forth-mode-map "\C-x~" 'forth-remove-tracers)
(define-key forth-mode-map "\e\C-m" 'forth-send-paragraph)
(define-key forth-mode-map "\eo" 'forth-send-buffer)
(define-key forth-mode-map "\C-x\C-m" 'forth-split)
(define-key forth-mode-map "\e " 'forth-reload)
(define-key forth-mode-map "\t" 'forth-indent-command)
(define-key forth-mode-map "\C-m" 'forth-reindent-then-newline-and-indent)
(define-key forth-mode-map "\M-q" 'forth-fill-paragraph)
(define-key forth-mode-map "\e." 'forth-find-tag)

;;; hook into motion events (realy ugly!)  (dk)
(define-key forth-mode-map "\C-n" 'forth-next-line)
(define-key forth-mode-map "\C-p" 'forth-previous-line)
(define-key forth-mode-map [down] 'forth-next-line)
(define-key forth-mode-map [up] 'forth-previous-line)
(define-key forth-mode-map "\C-f" 'forth-forward-char)
(define-key forth-mode-map "\C-b" 'forth-backward-char)
(define-key forth-mode-map [right] 'forth-forward-char)
(define-key forth-mode-map [left] 'forth-backward-char)
(define-key forth-mode-map "\M-f" 'forth-forward-word)
(define-key forth-mode-map "\M-b" 'forth-backward-word)
(define-key forth-mode-map [C-right] 'forth-forward-word)
(define-key forth-mode-map [C-left] 'forth-backward-word)
(define-key forth-mode-map "\M-v" 'forth-scroll-down)
(define-key forth-mode-map "\C-v" 'forth-scroll-up)
(define-key forth-mode-map [prior] 'forth-scroll-down)
(define-key forth-mode-map [next] 'forth-scroll-up)

(defun forth-next-line (arg) 
  (interactive "p") (next-line arg) (run-hooks 'forth-motion-hooks))
(defun forth-previous-line (arg)
  (interactive "p") (previous-line arg) (run-hooks 'forth-motion-hooks))
(defun forth-backward-char (arg)
  (interactive "p") (backward-char arg) (run-hooks 'forth-motion-hooks))
(defun forth-forward-char (arg)
  (interactive "p") (forward-char arg) (run-hooks 'forth-motion-hooks))
(defun forth-forward-word (arg)
  (interactive "p") (forward-word arg) (run-hooks 'forth-motion-hooks))
(defun forth-backward-word (arg)
  (interactive "p") (backward-word arg) (run-hooks 'forth-motion-hooks))
(defun forth-scroll-down (arg)
  (interactive "P") (scroll-down arg) (run-hooks 'forth-motion-hooks))
(defun forth-scroll-up (arg)
  (interactive "P") (scroll-up arg) (run-hooks 'forth-motion-hooks))

;setup for C-h C-i to work
(if (fboundp 'info-lookup-add-help)
    (info-lookup-add-help
     :topic 'symbol
     :mode 'forth-mode
     :regexp "[^ 	
]+"
     :ignore-case t
     :doc-spec '(("(gforth)Name Index" nil "`" "'  "))))

(load "etags")

(defun forth-find-tag (tagname &optional next-p regexp-p)
  (interactive (find-tag-interactive "Find tag: "))
  (unless (or regexp-p next-p)
    (setq tagname (concat "\\(^\\|\\s-\\)\\(" (regexp-quote tagname) 
			    "\\)\\(\\s-\\|$\\)")))
  (switch-to-buffer
   (find-tag-noselect tagname next-p t)))

(defvar forth-mode-syntax-table nil
  "Syntax table in use in Forth-mode buffers.")

;; Important: hilighting/indentation now depends on a correct syntax table.
;; All characters, except whitespace *must* belong to the "word constituent"
;; syntax class. If different behaviour is required, use of Categories might
;; help.
(if (not forth-mode-syntax-table)
    (progn
      (setq forth-mode-syntax-table (make-syntax-table))
      (let ((char 0))
	(while (< char ?!)
	  (modify-syntax-entry char " " forth-mode-syntax-table)
	  (setq char (1+ char)))
	(while (< char 256)
	  (modify-syntax-entry char "w" forth-mode-syntax-table)
	  (setq char (1+ char))))
      ))


(defun forth-mode-variables ()
  (set-syntax-table forth-mode-syntax-table)
  (setq local-abbrev-table forth-mode-abbrev-table)
  (make-local-variable 'paragraph-start)
  (setq paragraph-start (concat "^$\\|" page-delimiter))
  (make-local-variable 'paragraph-separate)
  (setq paragraph-separate paragraph-start)
  (make-local-variable 'indent-line-function)
  (setq indent-line-function 'forth-indent-line)
;  (make-local-variable 'require-final-newline)
;  (setq require-final-newline t)
  (make-local-variable 'comment-start)
  (setq comment-start "\\ ")
  ;(make-local-variable 'comment-end)
  ;(setq comment-end " )")
  (make-local-variable 'comment-column)
  (setq comment-column 40)
  (make-local-variable 'comment-start-skip)
  (setq comment-start-skip "\\ ")
  (make-local-variable 'comment-indent-hook)
  (setq comment-indent-hook 'forth-comment-indent)
  (make-local-variable 'parse-sexp-ignore-comments)
  (setq parse-sexp-ignore-comments t)
  (setq case-fold-search t)
  (make-local-variable 'forth-words)
  (make-local-variable 'forth-compiled-words)
  (make-local-variable 'forth-compiled-indent-words)
  (make-local-variable 'forth-hilight-level)
  (make-local-variable 'after-change-functions)
  (make-local-variable 'forth-show-screen)
  (make-local-variable 'forth-screen-marker)
  (make-local-variable 'forth-warn-long-lines)
  (make-local-variable 'forth-screen-number-string)
  (make-local-variable 'forth-use-oof)
  (make-local-variable 'forth-use-objects) 
  (setq forth-screen-marker (copy-marker 0))
  (add-hook 'after-change-functions 'forth-change-function)
  (setq imenu-create-index-function 'forth-create-index))

;;;###autoload
(defun forth-mode ()
  "
Major mode for editing Forth code. Tab indents for Forth code. Comments
are delimited with \\ and newline. Paragraphs are separated by blank lines
only. Block files are autodetected, when read, and converted to normal 
stream source format. See also `forth-block-mode'.
\\{forth-mode-map}
 Forth-split
    Positions the current buffer on top and a forth-interaction window
    below. The window size is controlled by the forth-percent-height
    variable (see below).
 Forth-reload
    Reloads the forth library and restarts the forth process.
 Forth-send-buffer
    Sends the current buffer, in text representation, as input to the
    forth process.
 Forth-send-paragraph
    Sends the previous or the current paragraph to the forth-process.
    Note that the cursor only need to be with in the paragraph to be sent.
 forth-documentation
    Search for documentation of forward adjacent to cursor. Note! To use
    this mode you have to add a line, to your .emacs file, defining the
    directories to search through for documentation files (se variable
    forth-help-load-path below) e.g. (setq forth-help-load-path '(nil)).

Variables controlling interaction and startup
 forth-percent-height
    Tells split how high to make the edit portion, in percent of the
    current screen height.
 forth-program-name
    Tells the library which program name to execute in the interation
    window.

Variables controlling syntax hilighting/recognition of parsed text:
 `forth-words'
    List of words that have a special parsing behaviour and/or should be
    hilighted. Add custom words by setting forth-custom-words in your
    .emacs, or by setting forth-local-words, in source-files' local 
    variables lists.
 forth-use-objects
    Set this variable to non-nil in your .emacs, or a local variables 
    list, to hilight and recognize the words from the \"Objects\" package 
    for object-oriented programming.
 forth-use-oof
    Same as above, just for the \"OOF\" package.
 forth-custom-words
    List of custom Forth words to prepend to `forth-words'. Should be set
    in your .emacs.
 forth-local-words
    List of words to prepend to `forth-words', whenever a forth-mode
    buffer is created. That variable should be set by Forth sources, using
    a local variables list at the end of file, to get file-specific
    hilighting.
    0 [IF]
       Local Variables: ... 
       forth-local-words: ...
       End:
    [THEN]
 forth-hilight-level
    Controls how much syntax hilighting is done. Should be in the range 
    0..3

Variables controlling indentation style:
 `forth-indent-words'
    List of words that influence indentation.
 forth-local-indent-words
    List of words to prepend to `forth-indent-words', similar to 
    forth-local-words. Should be used for specifying file-specific 
    indentation, using a local variables list.
 forth-custom-indent-words
    List of words to prepend to `forth-indent-words'. Should be set in your
    .emacs.    
 forth-indent-level
    Indentation increment/decrement of Forth statements.
 forth-minor-indent-level
    Minor indentation increment/decrement of Forth statemens.

Variables controlling block-file editing:
 forth-show-screen
    Non-nil means, that the start of the current screen is marked by an
    overlay arrow, and screen numbers are displayed in the mode line.
    This variable is by default nil for `forth-mode' and t for 
    `forth-block-mode'.
 forth-overlay-arrow-string
    String to display as the overlay arrow, when `forth-show-screen' is t.
    Setting this variable to nil disables the overlay arrow.
 forth-block-base
    Screen number of the first block in a block file. Defaults to 1.
 forth-warn-long-lines
    Non-nil means that a warning message is displayed whenever you edit or
    move over a line that is longer than 64 characters (the maximum line
    length that can be stored into a block file). This variable defaults to
    t for `forth-block-mode' and to nil for `forth-mode'.

Variables controling documentation search
 forth-help-load-path
    List of directories to search through to find *.doc
    (forth-help-file-suffix) files. Nil means current default directory.
    The specified directories must contain at least one .doc file. If it
    does not and you still want the load-path to scan that directory, create
    an empty file dummy.doc.
 forth-help-file-suffix
    The file names to search for in each directory specified by
    forth-help-load-path. Defaulted to '*.doc'. 
"
  (interactive)
  (kill-all-local-variables)
  (use-local-map forth-mode-map)
  (setq mode-name "Forth")
  (setq major-mode 'forth-mode)
  ;; convert buffer contents from block file format, if necessary
  (when (forth-detect-block-file-p)
    (widen)
    (message "Converting from Forth block source...")
    (forth-convert-from-block (point-min) (point-max))
    (message "Converting from Forth block source...done"))
  ;; if user switched from forth-block-mode to forth-mode, make sure the file
  ;; is now stored as normal strem source
  (when (equal buffer-file-format '(forth-blocks))
    (setq buffer-file-format nil))
  (forth-mode-variables)
;  (if (not (forth-process-running-p))
;      (run-forth forth-program-name))
  (run-hooks 'forth-mode-hook))

;;;###autoload
(define-derived-mode forth-block-mode forth-mode "Forth Block Source" 
  "Major mode for editing Forth block source files, derived from 
`forth-mode'. Differences to `forth-mode' are:
 * files are converted to block format, when written (`buffer-file-format' 
   is set to `(forth-blocks)')
 * `forth-show-screen' and `forth-warn-long-lines' are t by default
  
Note that the length of lines in block files is limited to 64 characters.
When writing longer lines to a block file, a warning is displayed in the
echo area and the line is truncated. 

Another problem is imposed by block files that contain newline or tab 
characters. When Emacs converts such files back to block file format, 
it'll translate those characters to a number of spaces. However, when
you read such a file, a warning message is displayed in the echo area,
including a line number that may help you to locate and fix the problem.

So have a look at the *Messages* buffer, whenever you hear (or see) Emacs' 
bell during block file read/write operations."
  (setq buffer-file-format '(forth-blocks))
  (setq forth-show-screen t)
  (setq forth-warn-long-lines t)
  (setq forth-screen-number-string (format "%d" forth-block-base))
  (setq mode-line-format (append (reverse (cdr (reverse mode-line-format)))
				 '("--S" forth-screen-number-string "-%-"))))

(add-hook 'forth-mode-hook
      '(lambda () 
	 (make-local-variable 'compile-command)
	 (setq compile-command "gforth ")
	 (forth-hack-local-variables)
	 (forth-customize-words)
	 (forth-compile-words)
	 (forth-change-function (point-min) (point-max) nil t)))

(defun forth-fill-paragraph () 
  "Fill comments (starting with '\'; do not fill code (block style
programmers who tend to fill code won't use emacs anyway:-)."
  ; Currently only comments at the start of the line are filled.
  ; Something like lisp-fill-paragraph may be better.  We cannot use
  ; fill-paragraph, because it removes the \ from the first comment
  ; line. Therefore we have to look for the first line of the comment
  ; and use fill-region.
  (interactive)
  (save-excursion
    (beginning-of-line)
    (while (and
	     (= (forward-line -1) 0)
	     (looking-at "[ \t]*\\\\g?[ \t]+")))
    (if (not (looking-at "[ \t]*\\\\g?[ \t]+"))
	(forward-line 1))
    (let ((from (point))
	  (to (save-excursion (forward-paragraph) (point))))
      (if (looking-at "[ \t]*\\\\g?[ \t]+")
	  (progn (goto-char (match-end 0))
		 (set-fill-prefix)
		 (fill-region from to nil))))))

(defun forth-comment-indent ()
  (save-excursion
    (beginning-of-line)
    (if (looking-at ":[ \t]*")
	(progn
	  (end-of-line)
	  (skip-chars-backward " \t\n")
	  (1+ (current-column)))
      comment-column)))


;; Forth commands

(defun forth-remove-tracers ()
  "Remove tracers of the form `~~ '. Queries the user for each occurrence."
  (interactive)
  (query-replace-regexp "\\(~~ \\| ~~$\\)" "" nil))

(defvar forth-program-name "gforth"
  "*Program invoked by the `run-forth' command.")

(defvar forth-band-name nil
  "*Band loaded by the `run-forth' command.")

(defvar forth-program-arguments nil
  "*Arguments passed to the Forth program by the `run-forth' command.")

(defun run-forth (command-line)
  "Run an inferior Forth process. Output goes to the buffer `*forth*'.
With argument, asks for a command line. Split up screen and run forth 
in the lower portion. The current-buffer when called will stay in the
upper portion of the screen, and all other windows are deleted.
Call run-forth again to make the *forth* buffer appear in the lower
part of the screen."
  (interactive
   (list (let ((default
		 (or forth-process-command-line
		     (forth-default-command-line))))
	   (if current-prefix-arg
	       (read-string "Run Forth: " default)
	       default))))
  (setq forth-process-command-line command-line)
  (forth-start-process command-line)
  (forth-split)
  (forth-set-runlight forth-runlight:input))

(defun run-forth-if-not ()
  (if (not (forth-process-running-p))
      (run-forth forth-program-name)))

(defun reset-forth ()
  "Reset the Forth process."
  (interactive)
  (let ((process (get-process forth-program-name)))
    (cond ((or (not process)
	       (not (eq (process-status process) 'run))
	       (yes-or-no-p
"The Forth process is running, are you SURE you want to reset it? "))
	   (message "Resetting Forth process...")
	   (forth-reload)
	   (message "Resetting Forth process...done")))))

(defun forth-default-command-line ()
  (concat forth-program-name
	  (if forth-program-arguments
	      (concat " " forth-program-arguments)
	      "")))

;;;; Internal Variables

(defvar forth-process-command-line nil
  "Command used to start the most recent Forth process.")

(defvar forth-previous-send ""
  "Most recent expression transmitted to the Forth process.")

(defvar forth-process-filter-queue '()
  "Queue used to synchronize filter actions properly.")

(defvar forth-prompt "ok"
  "The current forth prompt string.")

(defvar forth-start-hook nil
  "If non-nil, a procedure to call when the Forth process is started.
When called, the current buffer will be the Forth process-buffer.")

(defvar forth-signal-death-message nil
  "If non-nil, causes a message to be generated when the Forth process dies.")

(defvar forth-percent-height 50
  "Tells run-forth how high the upper window should be in percent.")

(defconst forth-runlight:input ?I
  "The character displayed when the Forth process is waiting for input.")

(defvar forth-mode-string ""
  "String displayed in the mode line when the Forth process is running.")

;;;; Evaluation Commands

(defun forth-send-string (&rest strings)
  "Send the string arguments to the Forth process.
The strings are concatenated and terminated by a newline."
  (cond ((forth-process-running-p)
	 (forth-send-string-1 strings))
	((yes-or-no-p "The Forth process has died.  Reset it? ")
	 (reset-forth)
	 (goto-char (point-max))
	 (forth-send-string-1 strings))))

(defun forth-send-string-1 (strings)
  (let ((string (apply 'concat strings)))
    (forth-send-string-2 string)))

(defun forth-send-string-2 (string)
  (let ((process (get-process forth-program-name)))
    (if (not (eq (current-buffer) (get-buffer forth-program-name)))
	(progn
	 (forth-process-filter-output string)
	 (forth-process-filter:finish)))
    (send-string process (concat string "\n"))
    (if (eq (current-buffer) (process-buffer process))
	(set-marker (process-mark process) (point)))))


(defun forth-send-region (start end)
  "Send the current region to the Forth process.
The region is sent terminated by a newline."
  (interactive "r")
  (let ((process (get-process forth-program-name)))
    (if (and process (eq (current-buffer) (process-buffer process)))
	(progn (goto-char end)
	       (set-marker (process-mark process) end))))
  (forth-send-string "\n" (buffer-substring start end) "\n"))

(defun forth-end-of-paragraph ()
  (if (looking-at "[\t\n ]+") (skip-chars-backward  "\t\n "))
  (if (not (re-search-forward "\n[ \t]*\n" nil t))
      (goto-char (point-max))))

(defun forth-send-paragraph ()
  "Send the current or the previous paragraph to the Forth process"
  (interactive)
  (let (end)
    (save-excursion
      (forth-end-of-paragraph)
      (skip-chars-backward  "\t\n ")
      (setq end (point))
      (if (re-search-backward "\n[ \t]*\n" nil t)
	  (setq start (point))
	(goto-char (point-min)))
      (skip-chars-forward  "\t\n ")
      (forth-send-region (point) end))))
  
(defun forth-send-buffer ()
  "Send the current buffer to the Forth process."
  (interactive)
  (if (eq (current-buffer) (forth-process-buffer))
      (error "Not allowed to send this buffer's contents to Forth"))
  (forth-send-region (point-min) (point-max)))


;;;; Basic Process Control

(defun forth-start-process (command-line)
  (let ((buffer (get-buffer-create "*forth*")))
    (let ((process (get-buffer-process buffer)))
      (save-excursion
	(set-buffer buffer)
	(progn (if process (delete-process process))
	       (goto-char (point-max))
	       (setq mode-line-process '(": %s"))
	       (add-to-global-mode-string 'forth-mode-string)
	       (setq process
		     (apply 'start-process
			    (cons forth-program-name
				  (cons buffer
					(forth-parse-command-line
					 command-line)))))
	       (set-marker (process-mark process) (point-max))
	       (forth-process-filter-initialize t)
	       (forth-modeline-initialize)
	       (set-process-sentinel process 'forth-process-sentinel)
	       (set-process-filter process 'forth-process-filter)
	       (run-hooks 'forth-start-hook)))
    buffer)))

(defun forth-parse-command-line (string)
  (setq string (substitute-in-file-name string))
  (let ((start 0)
	(result '()))
    (while start
      (let ((index (string-match "[ \t]" string start)))
	(setq start
	      (cond ((not index)
		     (setq result
			   (cons (substring string start)
				 result))
		     nil)
		    ((= index start)
		     (string-match "[^ \t]" string start))
		    (t
		     (setq result
			   (cons (substring string start index)
				 result))
		     (1+ index))))))
    (nreverse result)))


(defun forth-process-running-p ()
  "True iff there is a Forth process whose status is `run'."
  (let ((process (get-process forth-program-name)))
    (and process
	 (eq (process-status process) 'run))))

(defun forth-process-buffer ()
  (let ((process (get-process forth-program-name)))
    (and process (process-buffer process))))

;;;; Process Filter

(defun forth-process-sentinel (proc reason)
  (let ((inhibit-quit nil))
    (forth-process-filter-initialize (eq reason 'run))
    (if (eq reason 'run)
	(forth-modeline-initialize)
	(setq forth-mode-string "")))
  (if (and (not (memq reason '(run stop)))
	   forth-signal-death-message)
      (progn (beep)
	     (message
"The Forth process has died!  Do M-x reset-forth to restart it"))))

(defun forth-process-filter-initialize (running-p)
  (setq forth-process-filter-queue (cons '() '()))
  (setq forth-prompt "ok"))


(defun forth-process-filter (proc string)
  (forth-process-filter-output string)
  (forth-process-filter:finish))

(defun forth-process-filter:enqueue (action)
  (let ((next (cons action '())))
    (if (cdr forth-process-filter-queue)
	(setcdr (cdr forth-process-filter-queue) next)
	(setcar forth-process-filter-queue next))
    (setcdr forth-process-filter-queue next)))

(defun forth-process-filter:finish ()
  (while (car forth-process-filter-queue)
    (let ((next (car forth-process-filter-queue)))
      (setcar forth-process-filter-queue (cdr next))
      (if (not (cdr next))
	  (setcdr forth-process-filter-queue '()))
      (apply (car (car next)) (cdr (car next))))))

;;;; Process Filter Output

(defun forth-process-filter-output (&rest args)
  (if (not (and args
		(null (cdr args))
		(stringp (car args))
		(string-equal "" (car args))))
      (forth-process-filter:enqueue
       (cons 'forth-process-filter-output-1 args))))

(defun forth-process-filter-output-1 (&rest args)
  (save-excursion
    (forth-goto-output-point)
    (apply 'insert-before-markers args)))

(defun forth-guarantee-newlines (n)
  (save-excursion
    (forth-goto-output-point)
    (let ((stop nil))
      (while (and (not stop)
		  (bolp))
	(setq n (1- n))
	(if (bobp)
	    (setq stop t)
	  (backward-char))))
    (forth-goto-output-point)
    (while (> n 0)
      (insert-before-markers ?\n)
      (setq n (1- n)))))

(defun forth-goto-output-point ()
  (let ((process (get-process forth-program-name)))
    (set-buffer (process-buffer process))
    (goto-char (process-mark process))))

(defun forth-modeline-initialize ()
  (setq forth-mode-string "  "))

(defun forth-set-runlight (runlight)
  (aset forth-mode-string 0 runlight)
  (forth-modeline-redisplay))

(defun forth-modeline-redisplay ()
  (save-excursion (set-buffer (other-buffer)))
  (set-buffer-modified-p (buffer-modified-p))
  (sit-for 0))

;;;; Process Filter Operations

(defun add-to-global-mode-string (x)
  (cond ((null global-mode-string)
	 (setq global-mode-string (list "" x " ")))
	((not (memq x global-mode-string))
	 (setq global-mode-string
	       (cons ""
		     (cons x
			   (cons " "
				 (if (equal "" (car global-mode-string))
				     (cdr global-mode-string)
				     global-mode-string))))))))


;; Misc

(setq auto-mode-alist (append auto-mode-alist
				'(("\\.fs$" . forth-mode))))

(defun forth-split ()
  (interactive)
  (forth-split-1 "*forth*"))

(defun forth-split-1 (buffer)
  (if (not (eq (window-buffer) (get-buffer buffer)))
      (progn
	(delete-other-windows)
	(split-window-vertically
	 (/ (* (screen-height) forth-percent-height) 100))
	(other-window 1)
	(switch-to-buffer buffer)
	(goto-char (point-max))
	(other-window 1))))
    
(defun forth-reload ()
  (interactive)
  (let ((process (get-process forth-program-name)))
    (if process (kill-process process t)))
  (sleep-for 0 100)
  (forth-mode))


;; Special section for forth-help

(defvar forth-help-buffer "*Forth-help*"
  "Buffer used to display the requested documentation.")

(defvar forth-help-load-path nil
  "List of directories to search through to find *.doc
 (forth-help-file-suffix) files. Nil means current default directory.
 The specified directories must contain at least one .doc file. If it
 does not and you still want the load-path to scan that directory, create
 an empty file dummy.doc.")

(defvar forth-help-file-suffix "*.doc"
  "The file names to search for in each directory.")

(setq forth-search-command-prefix "grep -n \"^    [^(]* ")
(defvar forth-search-command-suffix "/dev/null")
(defvar forth-grep-error-regexp ": No such file or directory")

(defun forth-function-called-at-point ()
  "Return the space delimited word a point."
  (save-excursion
    (save-restriction
      (narrow-to-region (max (point-min) (- (point) 1000)) (point-max))
      (skip-chars-backward "^ \t\n" (point-min))
      (if (looking-at "[ \t\n]")
	  (forward-char 1))
      (let (obj (p (point)))
	(skip-chars-forward "^ \t\n")
	(buffer-substring p (point))))))

(defun forth-help-names-extend-comp (path-list result)
  (cond ((null path-list) result)
	((null (car path-list))
	 (forth-help-names-extend-comp (cdr path-list) 
	       (concat result forth-help-file-suffix " ")))
	(t (forth-help-names-extend-comp
	    (cdr path-list) (concat result
				    (expand-file-name (car path-list)) "/"
				    forth-help-file-suffix " ")))))

(defun forth-help-names-extended ()
  (if forth-help-load-path
      (forth-help-names-extend-comp forth-help-load-path "")
    (error "forth-help-load-path not specified")))


;(define-key forth-mode-map "\C-hf" 'forth-documentation)

(defun forth-documentation (function)
  "Display the full documentation of FORTH word."
  (interactive
   (let ((fn (forth-function-called-at-point))
	 (enable-recursive-minibuffers t)	     
	 search-list
	 val)
     (setq val (read-string (format "Describe forth word (default %s): " fn)))
     (list (if (equal val "") fn val))))
  (forth-get-doc (concat forth-search-command-prefix
			 (grep-regexp-quote (concat function " ("))
			 "[^)]*\-\-\" " (forth-help-names-extended)
			 forth-search-command-suffix))
  (message "C-x C-m switches back to the forth interaction window"))

(defun forth-get-doc (command)
  "Display the full documentation of command."
  (let ((curwin (get-buffer-window (window-buffer)))
	reswin
	pointmax)
    (with-output-to-temp-buffer forth-help-buffer
      (progn
	(call-process "sh" nil forth-help-buffer t "-c" command)
	(setq reswin (get-buffer-window forth-help-buffer))))
    (setq reswin (get-buffer-window forth-help-buffer))
    (select-window reswin)
    (save-excursion
      (goto-char (setq pointmax (point-max)))
      (insert "--------------------\n\n"))
    (let (fd doc) 
      (while (setq fd (forth-get-file-data pointmax))
	(setq doc (forth-get-doc-string fd))
	(save-excursion
	  (goto-char (point-max))
	  (insert (substring (car fd) (string-match "[^/]*$" (car fd)))
		  ":\n\n" doc "\n")))
      (if (not doc)
	  (progn (goto-char (point-max)) (insert "Not found"))))
    (select-window curwin)))
  
(defun forth-skip-error-lines ()
  (let ((lines 0))
    (save-excursion
      (while (re-search-forward forth-grep-error-regexp nil t)
	(beginning-of-line)
	(forward-line 1)
	(setq lines (1+ lines))))
    (forward-line lines)))

(defun forth-get-doc-string (fd)
  "Find file (car fd) and extract documentation from line (nth 1 fd)."
  (let (result)
    (save-window-excursion
      (find-file (car fd))
      (goto-line (nth 1 fd))
      (if (not (eq (nth 1 fd) (1+ (count-lines (point-min) (point)))))
	  (error "forth-get-doc-string: serious error"))
      (if (not (re-search-backward "\n[\t ]*\n" nil t))
	  (goto-char (point-min))
	(goto-char (match-end 0)))
      (let ((p (point)))
	(if (not (re-search-forward "\n[\t ]*\n" nil t))
	    (goto-char (point-max)))
	(setq result (buffer-substring p (point))))
      (bury-buffer (current-buffer)))
    result))

(defun forth-get-file-data (limit)
  "Parse grep output and return '(filename line#) list. Return nil when
 passing limit."
  (forth-skip-error-lines)
  (if (< (point) limit)
      (let ((result (forth-get-file-data-cont limit)))
	(forward-line 1)
	(beginning-of-line)
	result)))

(defun forth-get-file-data-cont (limit)
  (let (result)
    (let ((p (point)))
      (skip-chars-forward "^:")
      (setq result (buffer-substring p (point))))
    (if (< (point) limit)
	(let ((p (1+ (point))))
	  (forward-char 1)
	  (skip-chars-forward "^:")
	  (list result (string-to-int (buffer-substring p (point))))))))

(defun grep-regexp-quote (str)
  (let ((i 0) (m 1) (res ""))
    (while (/= m 0)
      (setq m (string-to-char (substring str i)))
      (if (/= m 0)
	  (progn
	    (setq i (1+ i))
	    (if (string-match (regexp-quote (char-to-string m))
			      ".*\\^$[]")
		(setq res (concat res "\\")))
	    (setq res (concat res (char-to-string m))))))
    res))


(define-key forth-mode-map "\C-x\C-e" 'compile)
(define-key forth-mode-map "\C-x\C-n" 'next-error)
(require 'compile "compile")

(defvar forth-compile-command "gforth ")
;(defvar forth-compilation-window-percent-height 30)

(defun forth-compile (command)
  (interactive (list (setq forth-compile-command (read-string "Compile command: " forth-compile-command))))
  (forth-split-1 "*compilation*")
  (setq ctools-compile-command command)
  (compile1 ctools-compile-command "No more errors"))


;;; Forth menu
;;; Mikael Karlsson <qramika@eras70.ericsson.se>

(cond ((string-match "XEmacs\\|Lucid" emacs-version)
       (require 'func-menu)

  (defconst fume-function-name-regexp-forth
   "^\\(:\\)[ \t]+\\([^ \t]*\\)"
   "Expression to get word definitions in Forth.")

  (setq fume-function-name-regexp-alist
      (append '((forth-mode . fume-function-name-regexp-forth) 
             ) fume-function-name-regexp-alist))

  ;; Find next forth word in the buffer
  (defun fume-find-next-forth-function-name (buffer)
    "Searches for the next forth word in BUFFER."
    (set-buffer buffer)
    (if (re-search-forward fume-function-name-regexp nil t)
      (let ((beg (match-beginning 2))
            (end (match-end 2)))
        (cons (buffer-substring beg end) beg))))

  (setq fume-find-function-name-method-alist
  (append '((forth-mode    . fume-find-next-forth-function-name))))

  ))
;;; End Forth menu

;;; File folding of forth-files
;;; uses outline
;;; Toggle activation with M-x fold-f (when editing a forth-file) 
;;; Use f9 to expand, f10 to hide, Or the menubar in xemacs
;;;
;;; Works most of the times but loses sync with the cursor occasionally 
;;; Could be improved by also folding on comments

(require 'outline)

(defun f-outline-level ()
	(cond	((looking-at "\\`\\\\")
			0)
		((looking-at "\\\\ SEC")
			0)
		((looking-at "\\\\ \\\\ .*")
			0)
		((looking-at "\\\\ DEFS")
			1)
		((looking-at "\\/\\* ")
			1)
		((looking-at ": .*")
			1)
		((looking-at "\\\\G")
			2)
		((looking-at "[ \t]+\\\\")
			3))
)			

(defun fold-f  ()
   (interactive)
   (add-hook 'outline-minor-mode-hook 'hide-body)

   ; outline mode header start, i.e. find word definitions
;;;   (setq  outline-regexp  "^\\(:\\)[ \t]+\\([^ \t]*\\)")
   (setq  outline-regexp  "\\`\\\\\\|:\\|\\\\ SEC\\|\\\\G\\|[ \t]+\\\\\\|\\\\ DEFS\\|\\/\\*\\|\\\\ \\\\ .*")
   (setq outline-level 'f-outline-level)

   (outline-minor-mode)
   (define-key outline-minor-mode-map '(shift up) 'hide-sublevels)
   (define-key outline-minor-mode-map '(shift right) 'show-children)
   (define-key outline-minor-mode-map '(shift left) 'hide-subtree)
   (define-key outline-minor-mode-map '(shift down) 'show-subtree)

)

;;(define-key global-map '(shift up) 'fold-f)

;;; end file folding

;;; func-menu is a package that scans your source file for function definitions
;;; and makes a menubar entry that lets you jump to any particular function
;;; definition by selecting it from the menu.  The following code turns this on
;;; for all of the recognized languages.  Scanning the buffer takes some time,
;;; but not much.
;;;
(cond ((string-match "XEmacs\\|Lucid" emacs-version)
       (require 'func-menu)
;;       (define-key global-map 'f8 'function-menu)
       (add-hook 'find-fible-hooks 'fume-add-menubar-entry)
;       (define-key global-map "\C-cg" 'fume-prompt-function-goto)
;       (define-key global-map '(shift button3) 'mouse-function-menu)
))

;;; gforth.el ends here
