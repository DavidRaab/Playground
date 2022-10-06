#!/usr/bin/env -S csharp

class Greeter {
    private int Counter = 0;

    public void Greet(string name) {
        Console.WriteLine("Hello {0}, I have greeted {1} times.", name, this.Counter);
        this.Counter = this.Counter + 1;
    }
}

var greeterA = new Greeter();
var greeterB = new Greeter();

greeterA.Greet("David");
greeterA.Greet("Vivi");
greeterA.Greet("Alice");
greeterB.Greet("Markus");
