#!/usr/bin/env -S csharp

class Fn {
    public static List<int> Double(List<int> numbers) {
        var result = new List<int>();
        foreach (var x in numbers ) {
            result.Add(x * 2);
        }
        return result;
    }

    public static List<int> Square(List<int> numbers) {
        var result = new List<int>();
        foreach (var x in numbers ) {
            result.Add(x * x);
        }
        return result;
    }

    public static List<int> Add10(List<int> numbers) {
        var result = new List<int>();
        foreach (var x in numbers ) {
            result.Add(x + 10);
        }
        return result;
    }

    public static string Show(List<int> numbers) {
        return "[" + String.Join(',', numbers) + "]";
    }
}

var numbers = new List<int> {1,2,3,4,5,6,7,8,9,10};

Console.WriteLine("Double: {0}", Fn.Show(Fn.Double(numbers)));
Console.WriteLine("Square: {0}", Fn.Show(Fn.Square(numbers)));
Console.WriteLine("Add10:  {0}", Fn.Show(Fn.Add10(numbers)));



abstract class Mappable<T> {
    public List<T> Execute(List<T> numbers) {
        var result = new List<T>();
        foreach (var x in numbers ) {
            result.Add(this.Map(x));
        }
        return result;
    }
    public abstract T Map(T x);
}

class Double : Mappable<int> {
    public override int Map(int x) {
        return x * 2;
    }
}

var dbl = new Double();
Console.WriteLine("Double: {0}", Fn.Show(dbl.Execute(numbers)));


// One Method interface -- function
// interface Function<A,B> {
//     B Execute(A a);
// }

// class List {
//     public static List<B> map<A,B>(Function<A,B> f, List<A> xs) {
//         var result = new List<B>();
//         foreach (var x in xs) {
//             result.Add( f.Execute(x) );
//         }
//         return result;
//     }
// }

// class Double : Function<int,int> {
//     public int Execute(int x) {
//         return x * 2;
//     }
// }

// var doubles = List.map(new Double(), numbers);
// Console.WriteLine("{0}", Fn.Show(doubles));


class List {
    public static List<B> map<A,B>(Func<A,B> f, List<A> xs) {
        var result = new List<B>();
        foreach (var x in xs) {
            result.Add( f.Invoke(x) );
        }
        return result;
    }
}

var doubles = List.map(x => x * 2, numbers);
Console.WriteLine("Invoke: {0}", Fn.Show(doubles));