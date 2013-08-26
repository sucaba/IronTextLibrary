1. clist, nlist represented as arrays with maxsize equal to regexp size (number of states)
2. FORK n
   is
   CADD n
3. JMP n 
   is equivalent to 
   CADD n
   NEXT
4. IS <ch>
   ...
   should be rewritten as 
       NADD label-0
       NEXT
    label-0:
       IS <ch>

   where WAIT will block execution until new character will arrive

Algorithm:
    if has items in CLIST:
		put current label-index on stack
		switch (NFA-state jump-table)
	else if not has items in NLIST:
		FAIL <current-position>
	else
		wait for the next char

    

