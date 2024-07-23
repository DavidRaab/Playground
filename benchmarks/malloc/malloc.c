#include <stdio.h>
#include <stdlib.h>

struct Vec2 {
    int x;
    int y;
};

struct Vec2* new_vec(int x, int y) {
    struct Vec2 *v = malloc(sizeof (struct Vec2));
    v->x = x;
    v->y = y;
    return v;
}

int main() {
    for (int i=0; i < 100000000; i++) {
        struct Vec2 *v = new_vec(10,10);
        free(v);
    }
    return 0;
}
