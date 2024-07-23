#include <stdio.h>
#include <stdlib.h>
#include <time.h>

typedef struct Vector2 {
    float X;
    float Y;
} Vector2;

typedef struct Transform {
    Vector2 Position;
    Vector2 Scale;
    float   Rotation;
} Transform;

int main(int argc, char **argv) {
    // this stack allocates an array for 50000 transform structs
    Transform transforms[50000];

    // But just the memory is allocated, we also need to initialize fields
    for (int i=0; i<50000; i++) {
        transforms[i].Position.X = 1.0;
        transforms[i].Position.Y = (float) rand();
        transforms[i].Scale.X    = (float) rand();
        transforms[i].Scale.Y    = (float) rand();
        transforms[i].Rotation   = 0.0;
    }

    clock_t start = clock();
    float sum = 0;
    for (int i=0; i<50000; i++) {
        // we somehow also need to read from array, otherwise we just have a
        // loop adding numbers
        sum += transforms[i].Position.X;
    }
    clock_t stop  = clock();

    double total = (double) (stop - start) / CLOCKS_PER_SEC;
    printf("Sum: %f Time: %f\n", sum, total);

    return 0;
}
