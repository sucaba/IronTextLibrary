Scanner Generator state kinds
-----------------------------

1) state has no valid sentinel transition and has tunnel

ch1 => goto s1
ch2 => goto s2
...
chN => goto sN
else:
    goto tunnel // tunnel state will check for on-sentinel buffer-refilling

2) state has no valid sentinel transition and has no tunnel
ch1 => goto s1
ch2 => goto s2
...
chN => goto sN
'\0' => if (EOB) { saveState(); FILL(K) } else { goto FAIL }
else:
    goto FAIL

3) state has valid sentinel transition
ch1 => goto s1
ch2 => goto s2
...
chN => goto sN
'\0' => 
    if (EOB) { saveState(); FILL(K) } 
        // can be handled in tunnel but will cause character comparsions (runtime penalty)
    else { goto on-sentinel-transition-state }
else:
    goto tunnel or FAIL
