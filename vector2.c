#include <stdio.h>
#include <stdlib.h>
#include <math.h>

struct Vector2 {
    int X;
    int Y;
};

struct Vector2* vector2_new(int x, int y) {
    struct Vector2 *obj = malloc(sizeof(struct Vector2));
    obj->X = x;
    obj->Y = y;
    return obj;
}

void vector2_show(struct Vector2 *this) {
    printf("X=%d; Y=%d\n", this->X, this->Y);
}

void vector2_set_x(struct Vector2 *this, int x) {
    this->X = x;
}

void vector2_set_y(struct Vector2 *this, int y) {
    this->Y = y;
}

double vector2_length(struct Vector2 *this) {
    return sqrt((this->X * this->X) + (this->Y * this->Y));
}

int main (int argc, char **argv) {
    struct Vector2 *v = vector2_new(1,1);

    // This prints X=1; Y=1
    vector2_show(v);

    // Setting X to 3
    vector2_set_x(v, 3);

    // prints X=3; Y=1
    vector2_show(v);

    // calculating length of vector2
    double length = vector2_length(v);

    // Prints: Length=3.162278
    printf("Length=%f\n", length);

    free(v);

    return 0;
}
