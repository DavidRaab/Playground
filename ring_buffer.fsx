#!/usr/bin/env -S dotnet fsi

#load "Lib/RingBuffer.fs"
#load "Lib/Test.fs"
open RingBuffer
open Test

let bufferIs (buffer:RingBuffer<'a>) expected name =
    Test.is (buffer.ToArray()) expected name

let buf = RingBuffer(5)
buf.PushMany [5;10;15;20;25]
bufferIs buf [|5;10;15;20;25|] "First Push Many"
buf.PushMany [30]
bufferIs buf [|10;15;20;25;30|] "Push on full Buffer 1"
buf.PushMany [35]
bufferIs buf [|15;20;25;30;35|] "Push on full Buffer 2"
buf.PushMany [40;45;50]
bufferIs buf [|30;35;40;45;50|] "Push 3 items"
Test.is (buf.Pop()) (ValueSome 50) "1st pop"
Test.is (buf.Pop()) (ValueSome 45) "2nd pop"
bufferIs buf [|30;35;40|] "after 2 pop"
buf.PushMany [55;60;65]

bufferIs buf [|35;40;55;60;65|] "push 3 after 2 pop"
Test.is (buf.Pop()) (ValueSome 65) "Pop 1"
bufferIs buf [|35;40;55;60|] "4 entries"
Test.is (buf.Pop()) (ValueSome 60) "Pop 2"
Test.is (buf.Pop()) (ValueSome 55) "Pop 3"
Test.is (buf.Pop()) (ValueSome 40) "Pop 4"
Test.is (buf.Pop()) (ValueSome 35) "Pop 5"
Test.is (buf.Pop()) (ValueNone)    "Pop 6"

buf.PushMany [70;75;80]

bufferIs buf [|70;75;80|]
Test.is (buf.Shift()) (ValueSome 70) "Shift 1"
Test.is (buf.Shift()) (ValueSome 75) "Shift 2"
bufferIs buf [|80|] "one element"

buf.PushMany [85;90]

bufferIs buf [|80;85;90|] "3 elements"
Test.is (buf.Pop())   (ValueSome 90) "pop on 3 elements"
Test.is (buf.Shift()) (ValueSome 80) "shift on 2 elements"
bufferIs buf [|85|] "middle element"

buf.PushMany [95;100]
bufferIs buf [|85;95;100|] "push after shift"
buf.Unshift 105
bufferIs buf [|105;85;95;100|] "unshift 1"
buf.Unshift 110
bufferIs buf [|110;105;85;95;100|] "unshift 2"
buf.Unshift 115
bufferIs buf [|115;110;105;85;95|] "unshift 3"
buf.UnshiftMany [120;125]
bufferIs buf [|125;120;115;110;105|] "unshiftMany"

let copy = buf.Copy()
copy.PushMany [130;135]

bufferIs buf  [|125;120;115;110;105|] "unshiftMany"
bufferIs copy [|115;110;105;130;135|] "PushMany on Copy"

Test.is [|for x in buf  -> x|] [|125;120;115;110;105|] "for on buf"
Test.is [|for x in copy -> x|] [|115;110;105;130;135|] "for on copy"
Test.is (buf.FoldBack  (fun x xs -> x :: xs) []) [125;120;115;110;105] "foldBack on buf"
Test.is (copy.FoldBack (fun x xs -> x :: xs) []) [115;110;105;130;135] "foldBack on copy"
Test.is [for i= -3 to 3 do buf.[i]] [115;110;105;125;120;115;110] "indexer"
Test.is (buf.Fold  (fun state x   -> x :: state) []) [105;110;115;120;125] "Fold"
Test.is (buf.Foldi (fun state x i -> (i,x) :: state) []) [(4,105);(3,110);(2,115);(1,120);(0,125)] "Foldi"

Test.doneTesting ()