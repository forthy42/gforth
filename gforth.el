;; Forth mode for Emacs
;; This file is part of GForth.
;; Changes by anton
;; This is a variant of forth.el that came with TILE.
;; I left most of this stuff untouched and made just a few changes for 
;; the things I use (mainly indentation and syntax tables).
;; So there is still a lot of work to do to adapt this to gforth.

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


(defvar forth-positives
  " : :noname code interpretation: ;code does> begin do ?do +do -do u+do u-do while if ?dup-if ?dup-0=-if else case of struct [if] [ifdef] [ifundef] [else] with public: private: class "
  "Contains all words which will cause the indent-level to be incremented
on the next line.
OBS! All words in forth-positives must be surrounded by spaces.")

(defvar forth-negatives
  " ; end-code ;code does> until repeat while +loop loop -loop s+loop else then endif again endcase endof end-struct [then] [else] [endif] endwith class; how: "
  "Contains all words which will cause the indent-level to be decremented
on the current line.
OBS! All words in forth-negatives must be surrounded by spaces.")

(defvar forth-zeroes
  " : :noname code interpretation: public: private: how: class class; "
  "Contains all words which causes the indent to go to zero")

(setq forth-zero 0)

(defvar forth-zup
  " how: implements "
  "Contains all words which causes zero indent level to change")

(defvar forth-zdown
  " class; how: class public: private: "
  "Contains all words which causes zero indent level to change")

(defvar forth-prefixes
  " postpone [compile] ['] [char] "
  "words that prefix and escape other words")

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
(define-key forth-mode-map "\C-m" 'reindent-then-newline-and-indent)
(define-key forth-mode-map "\M-q" 'forth-fill-paragraph)
(define-key forth-mode-map "\e." 'forth-find-tag)

(load "etags")

(defun forth-find-tag (tagname &optional next-p regexp-p)
  (interactive (find-tag-interactive "Find tag: "))
  (switch-to-buffer
   (find-tag-noselect (concat " " tagname " ") next-p regexp-p)))

(defvar forth-mode-syntax-table nil
  "Syntax table in use in Forth-mode buffers.")

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
      (modify-syntax-entry ?\" "\"" forth-mode-syntax-table)
      (modify-syntax-entry ?\\ "<" forth-mode-syntax-table)
      (modify-syntax-entry ?\n ">" forth-mode-syntax-table)
      ))
;I do not define '(' and ')' as comment delimiters, because emacs
;only supports one comment syntax (and a hack to accomodate C++); I
;use '\' for natural language comments and '(' for formal comments
;like stack comments, so for me it's better to have emacs treat '\'
;comments as comments. If you want it different, make the appropriate
;changes (best in your .emacs file).
;
;Hmm, the C++ hack could be used to support both comment syntaxes: we
;can have different comment styles, if both comments start with the
;same character. we could use ' ' as first and '(' and '\' as second
;character. However this would fail for G\ comments.

(defconst forth-indent-level 4
  "Indentation of Forth statements.")

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
  (setq parse-sexp-ignore-comments t))
  
;;;###autoload
(defun forth-mode ()
  "
Major mode for editing Forth code. Tab indents for Forth code. Comments
are delimited with \\ and newline. Paragraphs are separated by blank lines
only.
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

Variables controlling indentation style:
 forth-positives
    A string containing all words which causes the indent-level of the
    following line to be incremented.
    OBS! Each word must be surronded by spaces.
 forth-negatives
    A string containing all words which causes the indentation of the
    current line to be decremented, if the word begin the line. These
    words also has a cancelling effect on the indent-level of the
    following line, independent of position.
    OBS! Each word must be surronded by spaces.
 forth-zeroes
    A string containing all words which causes the indentation of the
    current line to go to zero, if the word begin the line.
    OBS! Each word must be surronded by spaces.
 forth-indent-level
    Indentation increment/decrement of Forth statements.

 Note! A word which decrements the indentation of the current line, may
    also be mentioned in forth-positives to cause the indentation to
    resume the previous level.

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
  (forth-mode-variables)
;  (if (not (forth-process-running-p))
;      (run-forth forth-program-name))
  (run-hooks 'forth-mode-hook))

(setq forth-mode-hook
      '(lambda () 
	 (make-local-variable 'compile-command)
	 (setq compile-command "gforth ")))

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

(defun forth-current-indentation ()
  (save-excursion
    (beginning-of-line)
    (back-to-indentation)
    (current-column)))

(defun forth-delete-indentation ()
  (let ((b nil) (m nil))
    (save-excursion
      (beginning-of-line)
      (setq b (point))
      (back-to-indentation)
      (setq m (point)))
    (delete-region b m)))

(defun forth-indent-line (&optional flag)
  "Correct indentation of the current Forth line."
  (let ((x (forth-calculate-indent)))
    (forth-indent-to x)))
  
(defun forth-indent-command ()
  (interactive)
  (forth-indent-line t))

(defun forth-indent-to (x)
  (let ((p nil))
    (setq p (- (current-column) (forth-current-indentation)))
    (forth-delete-indentation)
    (beginning-of-line)
    (indent-to x)
    (if (> p 0) (forward-char p))))

;;Calculate indent
(defun forth-calculate-indent ()
  (let ((w1 nil) (indent 0) (centre 0))
    (save-excursion
      (beginning-of-line)
      (skip-chars-backward " \t\n")
      (beginning-of-line)
      (back-to-indentation)
      (setq indent (current-column))
      (setq centre indent)
      (setq indent (+ indent (forth-sum-line-indentation))))
    (save-excursion
      (beginning-of-line)
      (back-to-indentation)
      (let ((p (point)))
	(skip-chars-forward "^ \t\n")
	(setq w1 (buffer-substring p (point)))))
    (if (> (- indent centre) forth-indent-level)
	(setq indent (+ centre forth-indent-level)))
    (if (> (- centre indent) forth-indent-level)
	(setq indent (- centre forth-indent-level)))
    (if (< indent 0) (setq indent 0))
    (setq indent (- indent
		    (if (string-match 
			 (regexp-quote (concat " " w1 " "))
			 forth-negatives)
			forth-indent-level 0)))
    (if (string-match (regexp-quote (concat " " w1 " ")) forth-zdown)
	(setq forth-zero 0))
    (if (string-match (regexp-quote (concat " " w1 " ")) forth-zeroes)
	(setq indent forth-zero))
    (if (string-match (regexp-quote (concat " " w1 " ")) forth-zup)
	(setq forth-zero 4))
    indent))

(defun forth-sum-line-indentation ()
  "Add upp the positive and negative weights of all words on the current line."
  (let ((b (point)) (e nil) (sum 0) (w nil) (t1 nil) (t2 nil) (first t))
    (end-of-line) (setq e (point))
    (goto-char b)
    (while (< (point) e)
      (setq w (forth-next-word))
      (setq t1 (string-match (regexp-quote (concat " " w " "))
			     forth-positives))
      (setq t2 (string-match (regexp-quote (concat " " w " "))
			     forth-negatives))
      (if t1
	  (setq sum (+ sum forth-indent-level)))
      (if (and t2 (not first))
	  (setq sum (- sum forth-indent-level)))
      (skip-chars-forward " \t")
      (setq first nil))
    sum))


(defun forth-next-word ()
  "Return the next forth-word. Skip anything that the forth-word takes from
the input stream (comments, arguments, etc.)"
;actually, it would be better to use commands based on the
;syntax-table or comment-start etc.
  (let ((w1 nil))
    (while (not w1)
      (skip-chars-forward " \t\n")
      (let ((p (point)))
	(skip-chars-forward "^ \t\n")
	(setq w1 (buffer-substring p (point))))
      (cond ((string-match "\"" w1)
	     (progn
	       (skip-chars-forward "^\"\n")
	       (forward-char)))
	    ((string-match "\\\\" w1)
	     (progn
	       (end-of-line)
	       ))
	    ((or (equal "(" w1) (equal ".(" w1))
	     (progn
	       (skip-chars-forward "^)\n")
	       (forward-char)))
	    ((string-match (regexp-quote (concat " " w1 " ")) forth-prefixes)
	     (progn (skip-chars-forward " \t\n")
		    (skip-chars-forward "^ \t\n")))
	    (t nil)))
    w1))
      

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

;;(define-key outline-minor-mode-map 'f9 'show-entry)
;;(define-key outline-minor-mode-map 'f10 'hide-entry)

(defun fold-f  ()
   (interactive)
   (add-hook 'outline-minor-mode-hook 'hide-body)

   ; outline mode header start, i.e. find word definitions
   (setq  outline-regexp  "^\\(:\\)[ \t]+\\([^ \t]*\\)")

   (outline-minor-mode)
)
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
       (define-key global-map "\C-cg" 'fume-prompt-function-goto)
       (define-key global-map '(shift button3) 'mouse-function-menu)
       ))
