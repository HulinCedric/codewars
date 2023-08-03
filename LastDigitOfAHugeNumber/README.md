# Last digit of a huge number

For a given list `[x1, x2, x3, ..., xn]` compute the last (decimal) digit of `x1 ^ (x2 ^ (x3 ^ (... ^ xn)))`.

E. g., with the input `[3, 4, 2]`, your code should return `1` because `3 ^ (4 ^ 2) = 3 ^ 16 = 43046721`.

_Beware:_ powers grow incredibly fast. For example, `9 ^ (9 ^ 9)` has more than 369 millions of digits. `lastDigit` has
to deal with such numbers efficiently.

_Corner cases:_ we assume that `0 ^ 0 = 1` and that `lastDigit` of an empty list equals to 1.

This kata generalizes [Last digit of a large number](http://www.codewars.com/kata/last-digit-of-a-large-number/haskell);
you may find useful to solve it beforehand.