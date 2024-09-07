#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>

typedef struct Array {
    int* data;
    int  count;
    int  allocated;
} Array;

// Header
Array* array_new(int size);
void  array_free   (Array* array);
void  array_push   (Array* array, int value);
int   array_pop    (Array* array, int* out);
void  array_expand (Array* array);
void  array_foreach(const Array* array, void body(int));
void  array_item   (const Array* array, int index, int* out);

// Implementation
Array* array_new(int size) {
    size = size <= 0 ? 1 : size;

    Array* array     = malloc(sizeof(Array));
    array->data      = calloc(size, sizeof(size));
    array->count     = 0;
    array->allocated = size;

    return array;
}

void array_free(Array* array) {
    free(array->data);
    free(array);
}

void array_expand(Array* array) {
    int* old   = array->data;
    int  count = array->count;

    int new_size = array->allocated * 2;
    int* new     = calloc(new_size, sizeof(int));

    // copy old values to new array
    for (int i=0; i < count; i++) {
        new[i] = old[i];
    }

    array->data      = new;
    array->allocated = new_size;
    free(old);

    return;
}

void array_push(Array* array, int value) {
    if ( array->count >= array->allocated ) {
        array_expand(array);
    }

    array->data[array->count] = value;
    array->count += 1;

    return;
}

int array_pop(Array* array, int* out) {
    if ( array->count > 0 ) {
        *out = array->data[array->count-1];
        array->count -= 1;
        return 1;
    }
    return 0;
}

void array_foreach(const Array* array, void body(int)) {
    for ( int i=0; i < array->count; i++ ) {
        body(array->data[i]);
    }
}

void array_item(const Array* array, int index, int* out) {
    *out = array->data[index];
    return;
}

// Main Program
void print_nums(int value) {
    printf("%d ", value);
}

int main(int argc, char** argv) {
    Array* nums = array_new(1);
    printf("Allocated: %d\n", nums->allocated);

    // Push 100 ints on array
    for (int i=0; i < 100; i++) {
        array_push(nums, i+100);
    }

    // iterate
    printf("Iterate:\n");
    array_foreach(nums, &print_nums);
    printf("\n");

    // index 10
    int item;
    array_item(nums, 10, &item);
    printf("item 10: %d\n", item);

    // pop all values from array
    printf("Popping: \n");
    int value;
    while ( array_pop(nums, &value) ) {
        printf("%d ", value);
    }
    printf("\n");

    printf("Allocated: %d\n", nums->allocated);
    array_free(nums);
    return 0;
}
