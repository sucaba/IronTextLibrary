TextReader --> buffer --> lexer DFA -> lexer action -> parser machine -> parser action 


1. Lexer can predict min count of characters to buffer before reaching next key state
   Basic instruction for lexer language:
    require(n)

2. Data can be very lage (for instance log file). in such situations buffer does not contain all data

3. Lexer should not limit token length in any other way except system memory limitation.
   Token should fit in buffer. Should we resize buffer for larger tokens?
   In non-accepting states buffer refill should not drop part of buffer containing buffer start.
   Alternatives:
   1. Resize buffer (as in re2c c example)
   2. Deliver text to action through the "Rope" instance which contains multiple string chunks.
   3? Have actions able to process big tokens piece by piece as they arrive without wating for 
      pattern parsing completion. Note: in this case data thransfer should start when action
      ambiguity disappears which can happen after memory exhausted.
   
   In any case small tokens should be handled effectively without performance loss.

Questions:
1. when lexer should check for a buffer refill ?
   A: in key states (see SCCs).
2. when lexer should check for EOI ? 
   A: this information should be available when buffer is refilled.
3. Do we need pull lexer or push lexer?
   A: pull is more effective when input source can provide data faster than lexer can consume it 
   (lexer is counted with async functionality overhead).
   1. speed(provider) vs speed(consumer) + async-overhead
   2. speed(provider) vs speed(consumer) + blocking-overhead
4. do we need Lexer class or just a method?
   A: method is good for pull lexer while class is good for a push lexer (keeps current state).
   Lexer State: current file path or url, current buffer, current token start, current pos, current DFA state.

Lexer language:
- require <N>
- fetch
- is-a <value> <label>
- is-inrange <from> <to>
- goto <State>
- goto <State>

Prototype:
1. No buffer refill. All data in single buffer
2. Pull lexer

Full version features:
0. Buffer resize when token is longer than buffer len
1. Big tokens support via ropes (optimization which replaces buffer resizing i.e. there is some max-buffer-size).
   Should reduce memory consumption (?).
2. Buffer refill in key states (SCCs)
3. Push lexer based on IAsyncResult logic ?
