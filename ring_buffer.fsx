#!/usr/bin/env -S dotnet fsi

#load "Lib/RingBuffer.fsx"
#load "Lib/Test.fsx"
open RingBuffer
open Test

let bufferIs (buffer:RingBuffer<'a>) expected name =
    Test.is (buffer.ToArray()) expected name

let buf = RingBuffer(5)

Test.is buf.Capacity 5 "Capacity 1"
Test.is buf.Count    0 "Count 1"
buf.Push 1
Test.is buf.Capacity 5 "Capacity 2"
Test.is buf.Count    1 "Count 2"
buf.Push 1
Test.is buf.Count    2 "Count 3"
buf.PushMany [1..10]
Test.is buf.Count    5 "Count 4"


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

Test.is (buf.Shift()) (ValueSome 80) "Shift 3"
Test.is (buf.Shift()) (ValueNone)    "Shift 4"

buf.Unshift 80
bufferIs buf [|80|] "back to 1 element"
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

buf.[0]  <- 200
bufferIs buf [|200;120;115;110;105|] "Item Setter 1"

buf.[-1] <- 300
bufferIs buf [|200;120;115;110;300|] "Item Setter 2"
buf.[-3] <- 400
bufferIs buf [|200;120;400;110;300|] "Item Setter 3"
buf.[6]  <- 500
bufferIs buf [|200;500;400;110;300|] "Item Setter 4"
buf.[-7] <- 600
bufferIs buf [|200;500;400;600;300|] "Item Setter 5"

buf.Set 0 700
bufferIs buf [|700;500;400;600;300|] "Set"
Test.is (buf.Get -1) 300 "Get"

buf.Shift()
buf.Shift()

bufferIs buf [|400;600;300|] "3 Element Buffer"
Test.is (buf.[0])  400 "NFB Get 1"
Test.is (buf.[2])  300 "NFB Get 2"
Test.is (buf.[3])  400 "NFB Get 3"
Test.is (buf.[-1]) 300 "NFB Get 4"
Test.is (buf.[-3]) 400 "NFB Get 5"
Test.is (buf.[-4]) 300 "NFB Get 6"
Test.is (buf.[-7]) 300 "NFB Get 7"

buf.[0] <- 1
bufferIs buf [|1;600;300|] "NFB Set 1"
buf.[1] <- 2
bufferIs buf [|1;2;300|]   "NFB Set 2"
buf.[2] <- 3
bufferIs buf [|1;2;3|]     "NFB Set 3"

Test.doneTesting ()