#include <stdio.h>
#include <math.h>

struct Vector2 {
    int X;
    int Y;
};

struct Vector2 vector2_create(int x, int y) {
    struct Vector2 v = { x, y };
    return v;
}

void vector2_show(struct Vector2 v) {
    printf("X=%d; Y=%d\n", v.X, v.Y);
}

void vector2_setX_1(struct Vector2 v, int x) {
    v.X = x;
}

void vector2_setX_2(struct Vector2 *v, int x) {
    v->X = x;
}

double vector2_length(struct Vector2 v) {
    return sqrt((v.X * v.X) + (v.Y * v.Y));
}

int main (int argc, char **argv) {
    struct Vector2 v = vector2_create(1,1);

    // This prints X=1; Y=1
    vector2_show(v);

    // What do you expect here?
    vector2_setX_1(v, 3);

    // still prints X=1; Y=1
    vector2_show(v);

    // structs are always copied. A .Net developer would say there are value tuples.
    // But in fact. Something like "refecrence types" doesn't exists. .Net introduced
    // this kind, as many other languages. So how do we get a function setting a value?
    // By passing not the value but instead create a pointer to the struct and pass
    // this pointer as a value. This technique is called passed-by-reference.

    // We get a pointer to every variable by putting the ampersan (&) before the variable.
    vector2_setX_2(&v, 3);

    // What you see here is what .Net calls a "reference type" and is an object in C#,F#,...
    // .Net creates a structure on the heap and you get the reference (pointer) to the struct.
    // In C this is explicit.

    // now prints X=3; Y=1
    vector2_show(v);

    // We don't need to pass a pointer if we don't want to change anything, but
    // then the struct is copied.
    double length = vector2_length(v);

    // Prints: Length=3.162278
    printf("Length=%f\n", length);

    return 0;
}
